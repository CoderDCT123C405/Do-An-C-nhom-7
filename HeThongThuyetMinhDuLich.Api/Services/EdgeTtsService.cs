using System.Diagnostics;
using System.Globalization;
using System.Text;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.Extensions.Options;

namespace HeThongThuyetMinhDuLich.Api.Services;

public class EdgeTtsService(
    IOptions<EdgeTtsSettings> settingsOptions,
    IWebHostEnvironment environment,
    ILogger<EdgeTtsService> logger)
{
    private readonly EdgeTtsSettings _settings = settingsOptions.Value;
    private string? _resolvedExecutable;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ResolveExecutable());

    public async Task<(string? path, int? durationSeconds)> GenerateAudioAsync(NoiDungThuyetMinh item, CancellationToken cancellationToken = default)
    {
        return await GenerateAudioAsync(item, languageCode: null, cancellationToken);
    }

    public async Task<(string? path, int? durationSeconds)> GenerateAudioAsync(
        NoiDungThuyetMinh item,
        string? languageCode,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return (null, null);
        }

        var text = NormalizeText(item.NoiDungVanBan);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Khong co noi dung van ban de sinh audio.");
        }

        var effectiveLanguageCode = string.IsNullOrWhiteSpace(languageCode)
            ? item.NgonNgu?.MaNgonNguQuocTe
            : languageCode;

        var relativePath = BuildRelativeAudioPath(item, effectiveLanguageCode);
        var fullPath = Path.Combine(GetWebRootPath(), relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var maxRetries = Math.Clamp(_settings.MaxRetries, 1, 6);
        var timeoutSeconds = Math.Clamp(_settings.TimeoutSeconds, 15, 300);
        var retryDelayMs = Math.Clamp(_settings.RetryDelayMs, 200, 10000);
        var minAudioBytes = Math.Max(_settings.MinAudioBytes, 1);
        var voices = BuildVoiceCandidates(effectiveLanguageCode);

        Exception? lastException = null;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            var voice = voices[(attempt - 1) % voices.Count];
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                var runResult = await RunEdgeTtsAsync(text, voice, fullPath, timeoutSeconds, cancellationToken);
                if (runResult.ExitCode != 0)
                {
                    throw new InvalidOperationException($"edge-tts that bai: {runResult.ExitCode}. {runResult.StdErr}".Trim());
                }

                if (!File.Exists(fullPath))
                {
                    throw new InvalidOperationException("edge-tts chay xong nhung khong tao duoc file audio.");
                }

                var fileInfo = new FileInfo(fullPath);
                if (fileInfo.Length < minAudioBytes)
                {
                    throw new InvalidOperationException($"File audio tao ra qua nho ({fileInfo.Length} bytes).");
                }

                var duration = GetAudioDuration(fullPath);
                return (relativePath, duration);
            }
            catch (Exception ex) when (IsRetryable(ex) && attempt < maxRetries)
            {
                lastException = ex;
                logger.LogWarning(ex,
                    "edge-tts loi tam thoi cho MaNoiDung {MaNoiDung}, lan {Attempt}/{MaxRetries}, voice {Voice}. Thu lai...",
                    item.MaNoiDung, attempt, maxRetries, voice);
                await Task.Delay(retryDelayMs, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                logger.LogWarning(ex,
                    "edge-tts that bai cho MaNoiDung {MaNoiDung}, lan {Attempt}/{MaxRetries}, voice {Voice}.",
                    item.MaNoiDung, attempt, maxRetries, voice);
                break;
            }
        }

        throw new InvalidOperationException($"Khong tao duoc audio sau {maxRetries} lan thu.", lastException);
    }

    private int? GetAudioDuration(string fullPath)
    {
        try
        {
            using var file = TagLib.File.Create(fullPath);
            var seconds = (int)Math.Round(file.Properties.Duration.TotalSeconds);
            return seconds > 0 ? seconds : null;
        }
        catch
        {
            return null;
        }
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
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string BuildRelativeAudioPath(NoiDungThuyetMinh item, string? languageCode)
    {
        var folder = NormalizeFolder(_settings.OutputFolder);
        var pointName = item.DiemThamQuan?.TenDiem;
        if (string.IsNullOrWhiteSpace(pointName))
        {
            pointName = ExtractPointNameFromTitle(item.TieuDe);
        }

        var safePointName = SlugifyFilePart(pointName);
        var safeLanguageCode = SlugifyFilePart(languageCode ?? item.NgonNgu?.MaNgonNguQuocTe) ?? item.MaNgonNgu.ToString(CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(safePointName))
        {
            safePointName = $"noidung-{item.MaNoiDung}";
        }

        return $"/{folder}/{safePointName}-{safeLanguageCode}.mp3";
    }

    private static string? ExtractPointNameFromTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        var trimmed = title.Trim();
        var prefixes = new[]
        {
            "Thuyết minh ",
            "Thuyet minh ",
            "Audio Guide - ",
            "语音导览 - "
        };

        foreach (var prefix in prefixes)
        {
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed[prefix.Length..].Trim();
            }
        }

        return trimmed;
    }

    private static string? SlugifyFilePart(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var pendingDash = false;

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var mapped = ch switch
            {
                'đ' => 'd',
                'Đ' => 'd',
                _ => char.ToLowerInvariant(ch)
            };

            if ((mapped >= 'a' && mapped <= 'z') || (mapped >= '0' && mapped <= '9'))
            {
                if (pendingDash && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(mapped);
                pendingDash = false;
            }
            else if (builder.Length > 0)
            {
                pendingDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }

    private async Task<(int ExitCode, string StdOut, string StdErr)> RunEdgeTtsAsync(
        string text,
        string voice,
        string fullPath,
        int timeoutSeconds,
        CancellationToken cancellationToken)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var startInfo = new ProcessStartInfo
        {
            FileName = ResolveExecutable(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        startInfo.ArgumentList.Add("--text");
        startInfo.ArgumentList.Add(text);
        startInfo.ArgumentList.Add("--voice");
        startInfo.ArgumentList.Add(voice);
        startInfo.ArgumentList.Add($"--rate={_settings.Rate}");
        startInfo.ArgumentList.Add($"--volume={_settings.Volume}");
        startInfo.ArgumentList.Add($"--pitch={_settings.Pitch}");
        startInfo.ArgumentList.Add("--write-media");
        startInfo.ArgumentList.Add(fullPath);

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Khong khoi dong duoc edge-tts. Hay cai Python + edge-tts, hoac cau hinh dung EdgeTts:Executable.", ex);
        }

        var stdOutTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);
        var stdErrTask = process.StandardError.ReadToEndAsync(linkedCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException($"edge-tts vuot qua timeout {timeoutSeconds}s.");
        }

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        return (process.ExitCode, stdOut, stdErr);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
        }
        catch
        {
            // best effort
        }
    }

    private static string NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
        return string.Join('\n', normalized
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));
    }

    private bool IsRetryable(Exception ex)
    {
        if (ex is TimeoutException or TaskCanceledException)
        {
            return true;
        }

        var message = ex.ToString();
        return message.Contains("NoAudioReceived", StringComparison.OrdinalIgnoreCase)
               || message.Contains("timed out", StringComparison.OrdinalIgnoreCase)
               || message.Contains("timeout", StringComparison.OrdinalIgnoreCase)
               || message.Contains("ClientConnectorError", StringComparison.OrdinalIgnoreCase)
               || message.Contains("Temporary failure", StringComparison.OrdinalIgnoreCase)
               || message.Contains("WebSocket", StringComparison.OrdinalIgnoreCase)
               || message.Contains("No audio was received", StringComparison.OrdinalIgnoreCase);
    }

    private List<string> BuildVoiceCandidates(string? languageCode = null)
    {
        var normalizedLanguageCode = languageCode?.Trim().ToLowerInvariant();
        if (string.Equals(normalizedLanguageCode, "en", StringComparison.OrdinalIgnoreCase))
        {
            return ["en-US-JennyNeural", "en-US-GuyNeural"];
        }

        if (string.Equals(normalizedLanguageCode, "ko", StringComparison.OrdinalIgnoreCase))
        {
            return ["ko-KR-SunHiNeural", "ko-KR-InJoonNeural"];
        }

        if (string.Equals(normalizedLanguageCode, "zh", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedLanguageCode, "zh-cn", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedLanguageCode, "zh-hans", StringComparison.OrdinalIgnoreCase))
        {
            return ["zh-CN-XiaoxiaoNeural", "zh-CN-YunxiNeural"];
        }

        var voices = new List<string>();
        if (!string.IsNullOrWhiteSpace(_settings.Voice))
        {
            voices.Add(_settings.Voice.Trim());
        }

        foreach (var fallback in _settings.FallbackVoices ?? [])
        {
            if (string.IsNullOrWhiteSpace(fallback))
            {
                continue;
            }

            var voice = fallback.Trim();
            if (!voices.Contains(voice, StringComparer.OrdinalIgnoreCase))
            {
                voices.Add(voice);
            }
        }

        if (voices.Count == 0)
        {
            voices.Add("vi-VN-HoaiMyNeural");
        }

        return voices;
    }

    private string ResolveExecutable()
    {
        if (!string.IsNullOrWhiteSpace(_resolvedExecutable))
        {
            return _resolvedExecutable;
        }

        var configured = _settings.Executable?.Trim();
        if (string.IsNullOrWhiteSpace(configured))
        {
            return string.Empty;
        }

        _resolvedExecutable = configured;
        return _resolvedExecutable;
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
