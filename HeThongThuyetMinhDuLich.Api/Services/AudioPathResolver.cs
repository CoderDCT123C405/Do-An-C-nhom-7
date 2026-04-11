using Microsoft.Extensions.Hosting;

namespace HeThongThuyetMinhDuLich.Api.Services;

public class AudioPathResolver(IWebHostEnvironment environment)
{
    public string? ResolveNoiDungAudioPath(int maNoiDung, string? duongDanAmThanh)
    {
        if (TryResolveManagedAudioPath(duongDanAmThanh, out var resolvedPath))
        {
            return resolvedPath;
        }

        if (!string.IsNullOrWhiteSpace(duongDanAmThanh) && !IsAppRelativePath(duongDanAmThanh))
        {
            return duongDanAmThanh;
        }

        var relativePath = $"/audio/tts/noidung-{maNoiDung}.mp3";
        return TryResolveManagedAudioPath(relativePath, out resolvedPath) ? resolvedPath : null;
    }

    private bool TryResolveManagedAudioPath(string? relativePath, out string? normalizedPath)
    {
        normalizedPath = null;
        if (!IsAppRelativePath(relativePath))
        {
            return false;
        }

        var webRootPath = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            return false;
        }

        var segments = relativePath!
            .Trim()
            .TrimStart('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var filePath = Path.Combine(webRootPath, Path.Combine(segments));
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists || fileInfo.Length <= 0)
        {
            return false;
        }

        normalizedPath = "/" + string.Join('/', segments);
        return true;
    }

    private static bool IsAppRelativePath(string? path)
    {
        return !string.IsNullOrWhiteSpace(path) && path.StartsWith('/');
    }
}