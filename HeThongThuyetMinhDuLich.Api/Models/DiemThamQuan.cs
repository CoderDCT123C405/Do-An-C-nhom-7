namespace HeThongThuyetMinhDuLich.Api.Models;

public class DiemThamQuan
{
    public int MaDiem { get; set; }
    public string MaDinhDanh { get; set; } = string.Empty;
    public string TenDiem { get; set; } = string.Empty;
    public string? MoTaNgan { get; set; }
    public decimal ViDo { get; set; }
    public decimal KinhDo { get; set; }
    public decimal BanKinhKichHoat { get; set; }
    public string? DiaChi { get; set; }
    public int MaLoai { get; set; }
    public int? MaTaiKhoanTao { get; set; }
    public int MaTaiKhoanCapNhat { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public LoaiDiemThamQuan? LoaiDiemThamQuan { get; set; }
    public TaiKhoan? TaiKhoanTao { get; set; }
    public TaiKhoan? TaiKhoanCapNhat { get; set; }
    public ICollection<NoiDungThuyetMinh> NoiDungThuyetMinhs { get; set; } = [];
    public ICollection<HinhAnhDiemThamQuan> HinhAnhDiemThamQuans { get; set; } = [];
    public ICollection<MaQr> MaQrs { get; set; } = [];
    public ICollection<LichSuPhat> LichSuPhats { get; set; } = [];
}
