namespace HeThongThuyetMinhDuLich.Api.Services;

public class EdgeTtsSettings
{
    public string Executable { get; set; } = "edge-tts";
    public string Voice { get; set; } = "vi-VN-HoaiMyNeural";
    public string Rate { get; set; } = "+0%";
    public string Volume { get; set; } = "+0%";
    public string Pitch { get; set; } = "+0Hz";
    public string OutputFolder { get; set; } = "audio/tts";
    public bool AutoGenerateOnSave { get; set; }
}
