using HeThongThuyetMinhDuLich.Mobile.Services;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class AppShell : Shell
{
    private readonly LanguageService _languageService;

    public AppShell(LanguageService languageService)
    {
        InitializeComponent();
        _languageService = languageService;
        Routing.RegisterRoute(nameof(AuthPage), typeof(AuthPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        Routing.RegisterRoute(nameof(UsageHistoryPage), typeof(UsageHistoryPage));
        _languageService.LanguageChanged += OnLanguageChanged;
        ApplyLocalization();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(ApplyLocalization);
    }

    private void ApplyLocalization()
    {
        HomeShellContent.Title = _languageService.GetText("ShellHomeTitle");
    }
}
