using Microsoft.Extensions.Hosting;

namespace HeThongThuyetMinhDuLich.Api.Services;

public class AudioPathResolver(IWebHostEnvironment environment)
{
    public string? ResolveNoiDungAudioPath(int maNoiDung, string? duongDanAmThanh)
    {
        if (!string.IsNullOrWhiteSpace(duongDanAmThanh))
        {
            return duongDanAmThanh;
        }

        var relativePath = $"/audio/tts/noidung-{maNoiDung}.mp3";
        var filePath = Path.Combine(environment.WebRootPath ?? string.Empty, "audio", "tts", $"noidung-{maNoiDung}.mp3");
        return File.Exists(filePath) ? relativePath : null;
    }
}