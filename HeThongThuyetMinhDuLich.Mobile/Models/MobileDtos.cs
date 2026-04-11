namespace HeThongThuyetMinhDuLich.Mobile.Models;

public class UserLoginRequest
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
}

public class UserRegisterRequest
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int? MaNgonNguMacDinh { get; set; }
}

public class MobileLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime HetHanLuc { get; set; }
    public string LoaiTaiKhoan { get; set; } = string.Empty;
    public int MaDinhDanh { get; set; }
    public string TenDangNhap { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? VaiTro { get; set; }
}

public class MobileAuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public MobileLoginResponse? Login { get; set; }

    public static MobileAuthResult Ok(MobileLoginResponse? login = null) => new() { Success = true, Login = login };
    public static MobileAuthResult Fail(string? error) => new() { Success = false, ErrorMessage = error ?? "Co loi xay ra." };
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
    public DateTime NgayCapNhat { get; set; }
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
    public string? TepAmThanhNoiBo { get; set; }
    public bool ChoPhepTTS { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayCapNhat { get; set; }
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

public class QrSummaryItem
{
    public int MaQR { get; set; }
    public int MaDiem { get; set; }
    public string GiaTriQR { get; set; } = string.Empty;
    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
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

public class NgonNguItem
{
    public int MaNgonNgu { get; set; }
    public string MaNgonNguQuocTe { get; set; } = string.Empty;
    public string TenNgonNgu { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}
