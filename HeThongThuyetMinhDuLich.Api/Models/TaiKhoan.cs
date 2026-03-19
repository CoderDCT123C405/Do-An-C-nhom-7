namespace HeThongThuyetMinhDuLich.Api.Models;

public class TaiKhoan
{
    public int MaTaiKhoan { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhauMaHoa { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? VaiTro { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<DiemThamQuan> DiemThamQuanDaTaos { get; set; } = [];
    public ICollection<DiemThamQuan> DiemThamQuanDaCapNhats { get; set; } = [];
    public ICollection<NoiDungThuyetMinh> NoiDungDaTaos { get; set; } = [];
    public ICollection<NoiDungThuyetMinh> NoiDungDaCapNhats { get; set; } = [];
    public ICollection<HinhAnhDiemThamQuan> HinhAnhDaTaos { get; set; } = [];
    public ICollection<MaQr> MaQrDaTaos { get; set; } = [];
}
