using Microsoft.Maui.Storage;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        var baseUrl = Preferences.Get("api_url", "http://172.20.10.2:5000");
Console.WriteLine("BASE = " + baseUrl);
        ApiEntry.Text = Preferences.Get("api_url", "http://172.20.10.2:5000");
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Set("api_url", ApiEntry.Text);

        StatusLabel.Text = "✅ Da luu API URL";
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

            var res = await http.GetAsync(ApiEntry.Text + "/swagger");

            if (res.IsSuccessStatusCode)
            {
                StatusLabel.Text = "✅ Ket noi thanh cong";
                StatusLabel.TextColor = Colors.Green;
            }
            else
            {
                StatusLabel.Text = "⚠️ Server co phan hoi nhung khong OK";
                StatusLabel.TextColor = Colors.Orange;
            }
        }
        catch
        {
            StatusLabel.Text = "❌ Khong ket noi duoc";
            StatusLabel.TextColor = Colors.Red;
        }
    }
}