using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class LoaiDiemThamQuanDto
{
    [Required]
    [StringLength(100)]
    public string TenLoai { get; set; } = string.Empty;

    [StringLength(255)]
    public string? MoTa { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
