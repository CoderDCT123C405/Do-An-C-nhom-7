namespace HeThongThuyetMinhDuLich.Api.Services;

public class EdgeTtsSettings
{
    public string Executable { get; set; } = "edge-tts";
    public string Voice { get; set; } = "vi-VN-HoaiMyNeural";
    public string[] FallbackVoices { get; set; } = ["vi-VN-NamMinhNeural"];
    public string Rate { get; set; } = "+0%";
    public string Volume { get; set; } = "+0%";
    public string Pitch { get; set; } = "+0Hz";
    public string OutputFolder { get; set; } = "audio/tts";
    public int TimeoutSeconds { get; set; } = 90;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1200;
    public int MinAudioBytes { get; set; } = 1024;
    public bool AutoGenerateOnSave { get; set; }
}
