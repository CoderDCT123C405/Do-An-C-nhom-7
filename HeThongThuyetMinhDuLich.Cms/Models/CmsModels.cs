namespace HeThongThuyetMinhDuLich.Cms.Models;

public class AdminLoginRequest
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public string? VaiTro { get; set; }
}

public class LoaiDiemThamQuanItem
{
    public int MaLoai { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThaiHoatDong { get; set; }
}

public class LoaiDiemThamQuanCreate
{
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
}

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

public class DiemThamQuanCreate
{
    public string MaDinhDanh { get; set; } = string.Empty;
    public string TenDiem { get; set; } = string.Empty;
    public string? MoTaNgan { get; set; }
    public decimal ViDo { get; set; }
    public decimal KinhDo { get; set; }
    public decimal BanKinhKichHoat { get; set; } = 150;
    public string? DiaChi { get; set; }
    public int MaLoai { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
}

public class ThongKeTheoDiemItem
{
    public int MaDiem { get; set; }
    public string? TenDiem { get; set; }
    public int SoLuotNghe { get; set; }
    public int TongThoiLuongDaNghe { get; set; }
}

public class ThongKeTheoKichHoatItem
{
    public string CachKichHoat { get; set; } = string.Empty;
    public int SoLuotPhat { get; set; }
    public int TongThoiLuongDaNghe { get; set; }
}
