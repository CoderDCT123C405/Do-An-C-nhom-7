using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class TaiKhoanDto
{
    [Required]
    [StringLength(50)]
    public string TenDangNhap { get; set; } = string.Empty;

    [StringLength(255)]
    public string? MatKhau { get; set; }

    [Required]
    [StringLength(100)]
    public string HoTen { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? VaiTro { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
