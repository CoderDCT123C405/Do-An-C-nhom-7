namespace HeThongThuyetMinhDuLich.Api.Models;

public class NguoiDung
{
    public int MaNguoiDung { get; set; }
    public string? TenDangNhap { get; set; }
    public string? MatKhauMaHoa { get; set; }
    public string? HoTen { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public int? MaNgonNguMacDinh { get; set; }
    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    public DateTime? NgayCapNhat { get; set; }

    public NgonNgu? NgonNguMacDinh { get; set; }
    public ICollection<LichSuPhat> LichSuPhats { get; set; } = [];
}
