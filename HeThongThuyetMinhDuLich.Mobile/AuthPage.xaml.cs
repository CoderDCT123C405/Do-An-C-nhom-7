using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class AuthPage : ContentPage
{
    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private bool _isBusy;

    public AuthPage(MobileApiClient apiClient, AuthSession authSession)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLanguagesAsync();
        UpdateSessionUi();
    }

    private async Task LoadLanguagesAsync()
    {
        try
        {
            var languages = (await _apiClient.GetNgonNguAsync()).Where(x => x.TrangThaiHoatDong).ToList();
            RegisterLanguagePicker.ItemsSource = languages;
            RegisterLanguagePicker.SelectedItem = languages.FirstOrDefault(x => x.LaMacDinh) ?? languages.FirstOrDefault();
        }
        catch
        {
            RegisterLanguagePicker.ItemsSource = Array.Empty<NgonNguItem>();
        }
    }

    private void UpdateSessionUi()
    {
        SessionStatusLabel.Text = _authSession.IsAuthenticated
            ? $"Da dang nhap: {_authSession.DisplayName ?? _authSession.TenDangNhap}"
            : "Dang su dung che do khach.";
        LogoutButton.IsVisible = _authSession.IsAuthenticated;
    }

    private void ShowLoginCard(bool showLogin)
    {
        LoginCard.IsVisible = showLogin;
        RegisterCard.IsVisible = !showLogin;
        LoginTabButton.BackgroundColor = showLogin ? Color.FromArgb("#2E5BCA") : Color.FromArgb("#E8EEF7");
        LoginTabButton.TextColor = showLogin ? Colors.White : Color.FromArgb("#1F3555");
        RegisterTabButton.BackgroundColor = showLogin ? Color.FromArgb("#E8EEF7") : Color.FromArgb("#2E5BCA");
        RegisterTabButton.TextColor = showLogin ? Color.FromArgb("#1F3555") : Colors.White;
        StatusLabel.IsVisible = false;
    }

    private async Task RunBusyAsync(Func<Task> action)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        LoginButton.IsEnabled = false;
        RegisterButton.IsEnabled = false;

        try
        {
            await action();
        }
        finally
        {
            _isBusy = false;
            LoginButton.IsEnabled = true;
            RegisterButton.IsEnabled = true;
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await RunBusyAsync(async () =>
        {
            var username = LoginUsernameEntry.Text?.Trim() ?? string.Empty;
            var password = LoginPasswordEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Vui long nhap day du ten dang nhap va mat khau.", false);
                return;
            }

            var result = await _apiClient.LoginUserAsync(new UserLoginRequest
            {
                TenDangNhap = username,
                MatKhau = password
            });

            if (!result.Success)
            {
                ShowStatus(result.ErrorMessage ?? "Dang nhap that bai.", false);
                return;
            }

            UpdateSessionUi();
            ShowStatus("Dang nhap thanh cong.", true);
            await Shell.Current.GoToAsync("..");
        });
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await RunBusyAsync(async () =>
        {
            var username = RegisterUsernameEntry.Text?.Trim() ?? string.Empty;
            var password = RegisterPasswordEntry.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Ten dang nhap va mat khau dang ky la bat buoc.", false);
                return;
            }

            var selectedLanguage = RegisterLanguagePicker.SelectedItem as NgonNguItem;
            var result = await _apiClient.RegisterUserAsync(new UserRegisterRequest
            {
                TenDangNhap = username,
                MatKhau = password,
                HoTen = RegisterFullNameEntry.Text?.Trim(),
                Email = RegisterEmailEntry.Text?.Trim(),
                SoDienThoai = RegisterPhoneEntry.Text?.Trim(),
                MaNgonNguMacDinh = selectedLanguage?.MaNgonNgu
            });

            if (!result.Success)
            {
                ShowStatus(result.ErrorMessage ?? "Dang ky that bai.", false);
                return;
            }

            UpdateSessionUi();
            ShowStatus("Dang ky thanh cong va da dang nhap.", true);
            await Shell.Current.GoToAsync("..");
        });
    }

    private void OnShowLoginClicked(object? sender, EventArgs e) => ShowLoginCard(true);

    private void OnShowRegisterClicked(object? sender, EventArgs e) => ShowLoginCard(false);

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        _authSession.SignOut();
        UpdateSessionUi();
        ShowStatus("Da dang xuat tai khoan.", true);
    }

    private void ShowStatus(string message, bool success)
    {
        StatusLabel.IsVisible = true;
        StatusLabel.Text = message;
        StatusLabel.TextColor = success ? Color.FromArgb("#027A48") : Color.FromArgb("#B42318");
    }
}
