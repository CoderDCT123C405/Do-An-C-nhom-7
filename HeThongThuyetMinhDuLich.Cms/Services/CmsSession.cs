namespace HeThongThuyetMinhDuLich.Cms.Services;

public class CmsSession
{
    private const bool BypassLogin = false; // FIX Ở ĐÂY

    public int? MaTaiKhoan { get; private set; }
    public string? AccessToken { get; private set; }
    public string? TenDangNhap { get; private set; }
    public string? HoTen { get; private set; }
    public string? VaiTro { get; private set; }
    public DateTime? HetHanLuc { get; private set; }

    public bool IsAuthenticated =>
        BypassLogin
        || (!string.IsNullOrWhiteSpace(AccessToken)
        && (!HetHanLuc.HasValue || HetHanLuc.Value > DateTime.UtcNow));

    public bool IsAdmin => string.Equals(VaiTro, "Admin", StringComparison.OrdinalIgnoreCase);

    public bool IsBienTap => string.Equals(VaiTro, "BienTap", StringComparison.OrdinalIgnoreCase);

    public void SignIn(int? maTaiKhoan, string tenDangNhap, string accessToken, string? hoTen, string? vaiTro, DateTime? hetHanLuc)
    {
        MaTaiKhoan = maTaiKhoan;
        TenDangNhap = tenDangNhap;
        AccessToken = accessToken;
        HoTen = hoTen;
        VaiTro = vaiTro;
        HetHanLuc = hetHanLuc;
    }

    public void SignOut()
    {
        MaTaiKhoan = null;
        TenDangNhap = null;
        AccessToken = null;
        HoTen = null;
        VaiTro = null;
        HetHanLuc = null;
    }
}