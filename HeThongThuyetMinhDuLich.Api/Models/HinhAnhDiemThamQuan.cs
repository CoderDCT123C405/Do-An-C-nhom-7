namespace HeThongThuyetMinhDuLich.Api.Models;

public class HinhAnhDiemThamQuan
{
    public int MaHinhAnh { get; set; }
    public int MaDiem { get; set; }
    public string TenTepTin { get; set; } = string.Empty;
    public string DuongDanHinhAnh { get; set; } = string.Empty;
    public bool LaAnhDaiDien { get; set; }
    public int? ThuTuHienThi { get; set; }
    public DateTime NgayTaiLen { get; set; } = DateTime.UtcNow;
    public int? MaTaiKhoanTao { get; set; }

    public DiemThamQuan? DiemThamQuan { get; set; }
    public TaiKhoan? TaiKhoanTao { get; set; }
}
