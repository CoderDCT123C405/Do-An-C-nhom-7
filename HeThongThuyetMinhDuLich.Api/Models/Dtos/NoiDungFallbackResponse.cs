namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class NoiDungFallbackResponse
{
    public int MaDiem { get; set; }
    public int? MaNgonNguYeuCau { get; set; }
    public int? MaNgonNguThucTe { get; set; }
    public NgonNguFallbackItem? NgonNguYeuCau { get; set; }
    public NgonNguFallbackItem? NgonNguThucTe { get; set; }
    public List<NgonNguFallbackItem> NgonNguKhaDung { get; set; } = [];
    public List<NgonNguFallbackItem> NgonNguThayThe { get; set; } = [];
    public List<NoiDungFallbackContentItem> NoiDung { get; set; } = [];
}

public class NgonNguFallbackItem
{
    public int MaNgonNgu { get; set; }
    public string MaNgonNguQuocTe { get; set; } = string.Empty;
    public string TenNgonNgu { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
}

public class NoiDungFallbackContentItem
{
    public int MaNoiDung { get; set; }
    public int MaDiem { get; set; }
    public int MaNgonNgu { get; set; }
    public string? TenNgonNgu { get; set; }
    public string? TieuDe { get; set; }
    public string? NoiDungVanBan { get; set; }
    public string? DuongDanAmThanh { get; set; }
    public bool ChoPhepTTS { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public bool TrangThaiHoatDong { get; set; }
}