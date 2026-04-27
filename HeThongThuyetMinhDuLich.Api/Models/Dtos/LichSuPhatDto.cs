using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class LichSuPhatDto
{
    public int? MaNguoiDung { get; set; }

    [Range(1, int.MaxValue)]
    public int MaDiem { get; set; }

    [Range(1, int.MaxValue)]
    public int MaNoiDung { get; set; }

    [Required]
    [RegularExpression("^(gps|qr|manual)$")]
    public string CachKichHoat { get; set; } = string.Empty;

    public DateTime? ThoiGianBatDau { get; set; }

    [Range(0, int.MaxValue)]
    public int? ThoiLuongDaNghe { get; set; }

    [StringLength(128)]
    public string? DeviceId { get; set; }

    [StringLength(128)]
    public string? SessionId { get; set; }

    public DateTime? LastSeen { get; set; }
}
