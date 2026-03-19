using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class MaQrDto
{
    [Range(1, int.MaxValue)]
    public int MaDiem { get; set; }

    [Required]
    [StringLength(255)]
    public string GiaTriQR { get; set; } = string.Empty;

    public bool TrangThaiHoatDong { get; set; } = true;

    public int? MaTaiKhoanTao { get; set; }
}
