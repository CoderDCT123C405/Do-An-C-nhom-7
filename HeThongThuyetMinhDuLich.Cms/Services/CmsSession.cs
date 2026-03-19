namespace HeThongThuyetMinhDuLich.Cms.Services;

public class CmsSession
{
    public string? AccessToken { get; private set; }
    public string? TenDangNhap { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void SignIn(string tenDangNhap, string accessToken)
    {
        TenDangNhap = tenDangNhap;
        AccessToken = accessToken;
    }

    public void SignOut()
    {
        TenDangNhap = null;
        AccessToken = null;
    }
}
