using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace HeThongThuyetMinhDuLich.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseBarcodeReader();

#if !WINDOWS
        builder.UseMauiMaps();
#endif

        builder.ConfigureFonts(_ => { });

        builder.Services.AddHttpClient("Api", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        builder.Services.AddSingleton<AuthSession>();
        builder.Services.AddSingleton<LanguageService>();
        builder.Services.AddSingleton<MobileCacheStore>();
        builder.Services.AddSingleton<MobileApiClient>();
        builder.Services.AddSingleton<SyncService>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<UsageHistoryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
