namespace HeThongThuyetMinhDuLich.Api.Models.Auth;

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
