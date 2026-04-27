using Microsoft.Maui.Storage;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class SettingsPage : ContentPage
{
    private const string ApiUrlPreferenceKey = "api_url";

    public SettingsPage()
    {
        InitializeComponent();
        var baseUrl = Preferences.Get(ApiUrlPreferenceKey, GetDefaultApiUrl());
        ApiEntry.Text = NormalizeApiUrl(baseUrl);
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        var normalizedUrl = NormalizeApiUrl(ApiEntry.Text);
        Preferences.Set(ApiUrlPreferenceKey, normalizedUrl);
        ApiEntry.Text = normalizedUrl;

        StatusLabel.Text = "Da luu API URL";
        StatusLabel.TextColor = Colors.Green;
    }

    private async void OnTestClicked(object sender, EventArgs e)
    {
        try
        {
            var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            var baseUrl = NormalizeApiUrl(ApiEntry.Text);
            var res = await http.GetAsync(baseUrl + "/swagger/index.html");

            if (res.IsSuccessStatusCode)
            {
                StatusLabel.Text = "Ket noi thanh cong";
                StatusLabel.TextColor = Colors.Green;
            }
            else
            {
                StatusLabel.Text = "Server co phan hoi nhung khong OK";
                StatusLabel.TextColor = Colors.Orange;
            }
        }
        catch
        {
            StatusLabel.Text = "Khong ket noi duoc";
            StatusLabel.TextColor = Colors.Red;
        }
    }

    private static string GetDefaultApiUrl()
    {
        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000"
            : "http://localhost:5000";
    }

    private static string NormalizeApiUrl(string? value)
    {
        var raw = string.IsNullOrWhiteSpace(value) ? GetDefaultApiUrl() : value.Trim();
        return raw.TrimEnd('/');
    }
}
