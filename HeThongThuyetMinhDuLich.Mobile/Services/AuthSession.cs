using HeThongThuyetMinhDuLich.Mobile.Models;
using Microsoft.Maui.Storage;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class AuthSession
{
    private const string KeyAccessToken = "auth.accessToken";
    private const string KeyTenDangNhap = "auth.tenDangNhap";
    private const string KeyHoTen = "auth.hoTen";
    private const string KeyLoaiTaiKhoan = "auth.loaiTaiKhoan";
    private const string KeyVaiTro = "auth.vaiTro";
    private const string KeyMaNguoiDung = "auth.maNguoiDung";
    private const string KeyHetHanLuc = "auth.hetHanLuc";

    public AuthSession()
    {
        AccessToken = Preferences.Default.Get(KeyAccessToken, string.Empty);
        TenDangNhap = Preferences.Default.Get(KeyTenDangNhap, string.Empty);
        HoTen = Preferences.Default.Get(KeyHoTen, string.Empty);
        LoaiTaiKhoan = Preferences.Default.Get(KeyLoaiTaiKhoan, string.Empty);
        VaiTro = Preferences.Default.Get(KeyVaiTro, string.Empty);

        var maNguoiDung = Preferences.Default.Get(KeyMaNguoiDung, 0);
        MaNguoiDung = maNguoiDung > 0 ? maNguoiDung : null;

        var expiryRaw = Preferences.Default.Get(KeyHetHanLuc, string.Empty);
        if (DateTime.TryParse(expiryRaw, out var expiry))
        {
            HetHanLuc = expiry;
        }
    }

    public string? AccessToken { get; private set; }
    public string? TenDangNhap { get; private set; }
    public string? HoTen { get; private set; }
    public string? LoaiTaiKhoan { get; private set; }
    public string? VaiTro { get; private set; }
    public int? MaNguoiDung { get; private set; }
    public DateTime? HetHanLuc { get; private set; }

    public bool IsAuthenticated =>
        !string.IsNullOrWhiteSpace(AccessToken)
        && (!HetHanLuc.HasValue || HetHanLuc.Value > DateTime.UtcNow);

    public string? DisplayName => string.IsNullOrWhiteSpace(HoTen) ? TenDangNhap : HoTen;

    public void SignIn(MobileLoginResponse login)
    {
        AccessToken = login.Token;
        TenDangNhap = login.TenDangNhap;
        HoTen = login.HoTen;
        LoaiTaiKhoan = login.LoaiTaiKhoan;
        VaiTro = login.VaiTro;
        MaNguoiDung = login.MaDinhDanh;
        HetHanLuc = login.HetHanLuc == default ? null : login.HetHanLuc;

        Preferences.Default.Set(KeyAccessToken, AccessToken ?? string.Empty);
        Preferences.Default.Set(KeyTenDangNhap, TenDangNhap ?? string.Empty);
        Preferences.Default.Set(KeyHoTen, HoTen ?? string.Empty);
        Preferences.Default.Set(KeyLoaiTaiKhoan, LoaiTaiKhoan ?? string.Empty);
        Preferences.Default.Set(KeyVaiTro, VaiTro ?? string.Empty);
        Preferences.Default.Set(KeyMaNguoiDung, MaNguoiDung ?? 0);
        Preferences.Default.Set(KeyHetHanLuc, HetHanLuc?.ToString("O") ?? string.Empty);
    }

    public void SignOut()
    {
        AccessToken = null;
        TenDangNhap = null;
        HoTen = null;
        LoaiTaiKhoan = null;
        VaiTro = null;
        MaNguoiDung = null;
        HetHanLuc = null;

        Preferences.Default.Remove(KeyAccessToken);
        Preferences.Default.Remove(KeyTenDangNhap);
        Preferences.Default.Remove(KeyHoTen);
        Preferences.Default.Remove(KeyLoaiTaiKhoan);
        Preferences.Default.Remove(KeyVaiTro);
        Preferences.Default.Remove(KeyMaNguoiDung);
        Preferences.Default.Remove(KeyHetHanLuc);
    }
}
