using System.ComponentModel.DataAnnotations;

namespace HeThongThuyetMinhDuLich.Api.Models.Dtos;

public class NoiDungThuyetMinhDto
{
    [Range(1, int.MaxValue)]
    public int MaDiem { get; set; }

    [Range(1, int.MaxValue)]
    public int MaNgonNgu { get; set; }

    [Required]
    [StringLength(200)]
    public string TieuDe { get; set; } = string.Empty;

    public string? NoiDungVanBan { get; set; }

    [StringLength(255)]
    public string? DuongDanAmThanh { get; set; }

    public bool ChoPhepTTS { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int? ThoiLuongGiay { get; set; }

    public int? MaTaiKhoanTao { get; set; }

    public int? MaTaiKhoanCapNhat { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
}
