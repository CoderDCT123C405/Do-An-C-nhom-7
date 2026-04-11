namespace HeThongThuyetMinhDuLich.Api.Models;

public class NoiDungThuyetMinh
{
    public int MaNoiDung { get; set; }
    public int MaDiem { get; set; }
    public int MaNgonNgu { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? NoiDungVanBan { get; set; }
    public string? DuongDanAmThanh { get; set; }
    public bool ChoPhepTTS { get; set; } = true;
    public int? ThoiLuongGiay { get; set; }
    public int? MaTaiKhoanTao { get; set; }
    public int MaTaiKhoanCapNhat { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public DiemThamQuan? DiemThamQuan { get; set; }
    public NgonNgu? NgonNgu { get; set; }
    public TaiKhoan? TaiKhoanTao { get; set; }
    public TaiKhoan? TaiKhoanCapNhat { get; set; }
    public ICollection<LichSuPhat> LichSuPhats { get; set; } = [];
}
