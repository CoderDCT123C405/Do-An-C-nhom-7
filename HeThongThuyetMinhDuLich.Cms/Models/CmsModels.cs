using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Cms.Models;

public class AdminLoginRequest
{
    [Required(ErrorMessage = "Vui long nhap ten dang nhap.")]
    [StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui long nhap mat khau.")]
    [StringLength(100)]
    public string MatKhau { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime HetHanLuc { get; set; }
    public string LoaiTaiKhoan { get; set; } = string.Empty;
    public int MaDinhDanh { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string? HoTen { get; set; }
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
    [Required(ErrorMessage = "Ten loai la bat buoc.")]
    [StringLength(100)]
    public string TenLoai { get; set; } = string.Empty;

    [StringLength(255)]
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
    [Required(ErrorMessage = "Ma dinh danh la bat buoc.")]
    [StringLength(50)]
    public string MaDinhDanh { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ten diem la bat buoc.")]
    [StringLength(200)]
    public string TenDiem { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MoTaNgan { get; set; }

    [Range(-90, 90, ErrorMessage = "Vi do khong hop le.")]
    public decimal ViDo { get; set; }

    [Range(-180, 180, ErrorMessage = "Kinh do khong hop le.")]
    public decimal KinhDo { get; set; }

    [Range(typeof(decimal), "0.01", "1000000", ErrorMessage = "Ban kinh kich hoat phai > 0.")]
    public decimal BanKinhKichHoat { get; set; } = 150;

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui long chon loai diem.")]
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

public class ApiOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static ApiOperationResult Ok() => new() { Success = true };
    public static ApiOperationResult Fail(string? message) => new() { Success = false, ErrorMessage = message ?? "Co loi xay ra." };
}
