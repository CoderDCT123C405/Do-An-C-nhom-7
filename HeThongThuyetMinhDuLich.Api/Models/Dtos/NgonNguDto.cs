using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class NgonNguDto
{
    [Required]
    [StringLength(10)]
    public string MaNgonNguQuocTe { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TenNgonNgu { get; set; } = string.Empty;

    public bool LaMacDinh { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
