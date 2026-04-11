using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class DiemThamQuanDto
{
    public int MaDiem { get; set; }

    [Required]
    [StringLength(50)]
    public string MaDinhDanh { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string TenDiem { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MoTaNgan { get; set; }

    [Range(-90, 90)]
    public decimal ViDo { get; set; }

    [Range(-180, 180)]
    public decimal KinhDo { get; set; }

    [Range(0.01, 1000000)]
    public decimal BanKinhKichHoat { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    [Range(1, int.MaxValue)]
    public int MaLoai { get; set; }

    public int? MaTaiKhoanTao { get; set; }

    [Range(1, int.MaxValue)]
    public int MaTaiKhoanCapNhat { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
