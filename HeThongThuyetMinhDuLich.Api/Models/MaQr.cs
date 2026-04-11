namespace HeThongThuyetMinhDuLich.Api.Models;

public class MaQr
{
    public int MaQR { get; set; }
    public int MaDiem { get; set; }
    public string GiaTriQR { get; set; } = string.Empty;
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }
    public int? MaTaiKhoanTao { get; set; }

    public DiemThamQuan? DiemThamQuan { get; set; }
    public TaiKhoan? TaiKhoanTao { get; set; }
}
