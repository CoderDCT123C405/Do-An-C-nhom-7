namespace HeThongThuyetMinhDuLich.Mobile.Models;

public class DiemThamQuanItem
{
    public int MaDiem { get; set; }
    public string MaDinhDanh { get; set; } = string.Empty;
    public string TenDiem { get; set; } = string.Empty;
    public string? MoTaNgan { get; set; }
    public decimal ViDo { get; set; }
    public decimal KinhDo { get; set; }
    public decimal BanKinhKichHoat { get; set; }
    public string? DiaChi { get; set; }
    public int MaLoai { get; set; }
    public bool TrangThaiHoatDong { get; set; }
}

public class NoiDungItem
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
}

public class NoiDungByDiemResponse
{
    public int MaDiem { get; set; }
    public List<NoiDungItem> NoiDung { get; set; } = [];
}

public class QrLookupResponse
{
    public int MaQR { get; set; }
    public string GiaTriQR { get; set; } = string.Empty;
    public int MaDiem { get; set; }
    public DiemThamQuanItem? DiemThamQuan { get; set; }
    public List<NoiDungItem> NoiDung { get; set; } = [];
}

public class LichSuPhatCreateRequest
{
    public int? MaNguoiDung { get; set; }
    public int MaDiem { get; set; }
    public int MaNoiDung { get; set; }
    public string CachKichHoat { get; set; } = string.Empty;
    public DateTime? ThoiGianBatDau { get; set; }
    public int? ThoiLuongDaNghe { get; set; }
}
