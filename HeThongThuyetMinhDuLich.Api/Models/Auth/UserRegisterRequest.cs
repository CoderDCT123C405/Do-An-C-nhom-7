namespace HeThongThuyetMinhDuLich.Api.Models.Auth;

public class UserRegisterRequest
{
    public string TenDangNhap { get; set; } = string.Empty;
    public string MatKhau { get; set; } = string.Empty;
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int? MaNgonNguMacDinh { get; set; }
}
