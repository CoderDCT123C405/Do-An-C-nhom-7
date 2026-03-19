namespace HeThongThuyetMinhDuLich.Api.Models;

public class LoaiDiemThamQuan
{
    public int MaLoai { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public ICollection<DiemThamQuan> DiemThamQuans { get; set; } = [];
}
