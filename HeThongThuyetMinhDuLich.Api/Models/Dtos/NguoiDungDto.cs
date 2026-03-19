using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class NguoiDungDto
{
    [StringLength(50)]
    public string? TenDangNhap { get; set; }

    [StringLength(255)]
    public string? MatKhau { get; set; }

    [StringLength(100)]
    public string? HoTen { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    public int? MaNgonNguMacDinh { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
