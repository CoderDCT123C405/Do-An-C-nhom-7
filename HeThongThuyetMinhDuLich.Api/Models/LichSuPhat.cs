namespace HeThongThuyetMinhDuLich.Api.Models;

public class LichSuPhat
{
    public long MaLichSuPhat { get; set; }
    public int? MaNguoiDung { get; set; }
    public int MaDiem { get; set; }
    public int MaNoiDung { get; set; }
    public string CachKichHoat { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; } = DateTime.UtcNow;
    public int? ThoiLuongDaNghe { get; set; }
    public string? DeviceId { get; set; }
    public string? SessionId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? LastSeen { get; set; }

    public NguoiDung? NguoiDung { get; set; }
    public DiemThamQuan? DiemThamQuan { get; set; }
    public NoiDungThuyetMinh? NoiDungThuyetMinh { get; set; }
}
