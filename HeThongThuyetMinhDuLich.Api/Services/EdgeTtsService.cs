using System.Diagnostics;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.Extensions.Options;

namespace HeThongThuyetMinhDuLich.Api.Services;

public class EdgeTtsService(
    IOptions<EdgeTtsSettings> settingsOptions,
    IWebHostEnvironment environment,
    ILogger<EdgeTtsService> logger)
{
    private readonly EdgeTtsSettings _settings = settingsOptions.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.Executable);

    public async Task<string?> GenerateAudioAsync(NoiDungThuyetMinh item, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(item.NoiDungVanBan))
        {
            throw new InvalidOperationException("Không có nội dung văn bản để sinh audio.");
        }

        var relativePath = BuildRelativeAudioPath(item.MaNoiDung);
        var fullPath = Path.Combine(GetWebRootPath(), relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var startInfo = new ProcessStartInfo
        {
            FileName = _settings.Executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--text");
        startInfo.ArgumentList.Add(item.NoiDungVanBan);
        startInfo.ArgumentList.Add("--voice");
        startInfo.ArgumentList.Add(_settings.Voice);
        startInfo.ArgumentList.Add("--rate");
        startInfo.ArgumentList.Add(_settings.Rate);
        startInfo.ArgumentList.Add("--volume");
        startInfo.ArgumentList.Add(_settings.Volume);
        startInfo.ArgumentList.Add("--pitch");
        startInfo.ArgumentList.Add(_settings.Pitch);
        startInfo.ArgumentList.Add("--write-media");
        startInfo.ArgumentList.Add(fullPath);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to start edge-tts.");
            throw new InvalidOperationException(
                "Không khởi động được edge-tts. Hãy cài Python và edge-tts, hoặc cấu hình đúng EdgeTts:Executable.");
        }

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;
        if (process.ExitCode != 0)
        {
            logger.LogWarning("edge-tts failed for MaNoiDung {MaNoiDung}: {ExitCode} - {StdErr}", item.MaNoiDung, process.ExitCode, stdErr);
            throw new InvalidOperationException(
                $"edge-tts thất bại: {process.ExitCode}. {stdErr}".Trim());
        }

        if (!System.IO.File.Exists(fullPath))
        {
            throw new InvalidOperationException("edge-tts chạy xong nhưng không tạo được file audio.");
        }

        return relativePath;
    }

    public void DeleteManagedAudio(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!path.StartsWith("/", StringComparison.Ordinal) ||
            !path.StartsWith($"/{NormalizeFolder(_settings.OutputFolder)}/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fullPath = Path.Combine(GetWebRootPath(), path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private string BuildRelativeAudioPath(int maNoiDung)
    {
        var folder = NormalizeFolder(_settings.OutputFolder);
        return $"/{folder}/noidung-{maNoiDung}.mp3";
    }

    private string GetWebRootPath()
    {
        return string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
    }

    private static string NormalizeFolder(string folder)
    {
        return folder.Trim().Trim('/').Replace('\\', '/');
    }
}
