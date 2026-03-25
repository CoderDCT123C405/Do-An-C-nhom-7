using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Auth;

public class AdminLoginRequest
{
    [Required]
    [StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string MatKhau { get; set; } = string.Empty;
}
