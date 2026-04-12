using HeThongThuyetMinhDuLich.Mobile.Models;
using HeThongThuyetMinhDuLich.Mobile.Services;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class AuthPage : ContentPage
{
    private readonly MobileApiClient _apiClient;
    private readonly AuthSession _authSession;
    private readonly LanguageService _languageService;
    private bool _isBusy;
    private bool _isLanguageSubscribed;

    public AuthPage(MobileApiClient apiClient, AuthSession authSession, LanguageService languageService)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _authSession = authSession;
        _languageService = languageService;
        ApplyLocalization();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SubscribeLanguageChanges();
        await LoadLanguagesAsync();
        UpdateSessionUi();
        ConfigureToolbar();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnsubscribeLanguageChanges();
    }

    private async Task LoadLanguagesAsync()
    {
        try
        {
            var languages = (await _apiClient.GetNgonNguAsync()).Where(x => x.TrangThaiHoatDong).ToList();
            RegisterLanguagePicker.ItemsSource = languages;
            RegisterLanguagePicker.SelectedItem = languages.FirstOrDefault(x => string.Equals(x.MaNgonNguQuocTe, _languageService.CurrentLanguageCode, StringComparison.OrdinalIgnoreCase))
                ?? languages.FirstOrDefault(x => x.LaMacDinh)
                ?? languages.FirstOrDefault();
        }
        catch
        {
            RegisterLanguagePicker.ItemsSource = Array.Empty<NgonNguItem>();
        }
    }

    private void UpdateSessionUi()
    {
        SessionStatusLabel.Text = _authSession.IsAuthenticated
            ? _languageService.Format("LoggedInSessionStatus", _authSession.DisplayName ?? _authSession.TenDangNhap ?? _languageService.GetText("AccountFallback"))
            : _languageService.GetText("GuestSessionStatus");
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
                ShowStatus(_languageService.GetText("MissingLoginFields"), false);
                return;
            }

            var result = await _apiClient.LoginUserAsync(new UserLoginRequest
            {
                TenDangNhap = username,
                MatKhau = password
            });

            if (!result.Success)
            {
                ShowStatus(result.ErrorMessage ?? _languageService.GetText("LoginFailed"), false);
                return;
            }

            UpdateSessionUi();
            ConfigureToolbar();
            ShowStatus(_languageService.GetText("LoginSuccess"), true);
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
                ShowStatus(_languageService.GetText("MissingRegisterFields"), false);
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
                ShowStatus(result.ErrorMessage ?? _languageService.GetText("RegisterFailed"), false);
                return;
            }

            UpdateSessionUi();
            ConfigureToolbar();
            ShowStatus(_languageService.GetText("RegisterSuccess"), true);
            await Shell.Current.GoToAsync("..");
        });
    }

    private void OnShowLoginClicked(object? sender, EventArgs e) => ShowLoginCard(true);

    private void OnShowRegisterClicked(object? sender, EventArgs e) => ShowLoginCard(false);

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        _authSession.SignOut();
        UpdateSessionUi();
        ConfigureToolbar();
        ShowStatus(_languageService.GetText("LogoutSuccess"), true);
    }

    private void ShowStatus(string message, bool success)
    {
        StatusLabel.IsVisible = true;
        StatusLabel.Text = message;
        StatusLabel.TextColor = success ? Color.FromArgb("#027A48") : Color.FromArgb("#B42318");
    }

    private void SubscribeLanguageChanges()
    {
        if (_isLanguageSubscribed)
        {
            return;
        }

        _languageService.LanguageChanged += OnLanguageChanged;
        _isLanguageSubscribed = true;
    }

    private void UnsubscribeLanguageChanges()
    {
        if (!_isLanguageSubscribed)
        {
            return;
        }

        _languageService.LanguageChanged -= OnLanguageChanged;
        _isLanguageSubscribed = false;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            ApplyLocalization();
            await LoadLanguagesAsync();
            UpdateSessionUi();
            ConfigureToolbar();
        });
    }

    private void ApplyLocalization()
    {
        Title = _languageService.GetText("AuthPageTitle");
        AuthHeroTitleLabel.Text = _languageService.GetText("AuthHeroTitle");
        LoginTabButton.Text = _languageService.GetText("LoginTab");
        RegisterTabButton.Text = _languageService.GetText("RegisterTab");
        LoginSectionTitleLabel.Text = _languageService.GetText("LoginSectionTitle");
        LoginUsernameEntry.Placeholder = _languageService.GetText("UsernamePlaceholder");
        LoginPasswordEntry.Placeholder = _languageService.GetText("PasswordPlaceholder");
        LoginButton.Text = _languageService.GetText("LoginTab");
        RegisterSectionTitleLabel.Text = _languageService.GetText("RegisterSectionTitle");
        RegisterUsernameEntry.Placeholder = _languageService.GetText("UsernamePlaceholder");
        RegisterPasswordEntry.Placeholder = _languageService.GetText("PasswordPlaceholder");
        RegisterFullNameEntry.Placeholder = _languageService.GetText("FullNamePlaceholder");
        RegisterEmailEntry.Placeholder = _languageService.GetText("EmailPlaceholder");
        RegisterPhoneEntry.Placeholder = _languageService.GetText("PhonePlaceholder");
        RegisterLanguagePicker.Title = _languageService.GetText("RegisterLanguageTitle");
        RegisterButton.Text = _languageService.GetText("RegisterAndLoginButton");
        LogoutButton.Text = _languageService.GetText("LogoutButton");
        UpdateSessionUi();
    }

    private void ConfigureToolbar()
    {
        ToolbarItems.Clear();
        ToolbarItems.Add(new ToolbarItem
        {
            Text = _languageService.CurrentLanguage.ShortLabel,
            Order = ToolbarItemOrder.Primary,
            Priority = 0,
            Command = new Command(async () => await ShowLanguageMenuAsync())
        });
    }

    private async Task ShowLanguageMenuAsync()
    {
        var actions = _languageService.SupportedLanguages
            .Select(x => $"{x.ShortLabel} - {x.NativeName}")
            .ToArray();

        var selectedAction = await Shell.Current.DisplayActionSheetAsync(
            _languageService.GetText("LanguageActionTitle"),
            _languageService.GetText("CancelAction"),
            null,
            actions);

        var selectedLanguage = _languageService.SupportedLanguages.FirstOrDefault(x => $"{x.ShortLabel} - {x.NativeName}" == selectedAction);
        if (selectedLanguage is null)
        {
            return;
        }

        _languageService.SetLanguage(selectedLanguage.Code);
    }
}
