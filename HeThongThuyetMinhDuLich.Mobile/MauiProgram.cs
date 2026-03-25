using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;
namespace HeThongThuyetMinhDuLich.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

#if !WINDOWS
        builder.UseMauiMaps();
#endif

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        // 🔥 API
        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(GetApiBaseUrl());
        });

        // 🔥 SERVICES
        builder.Services.AddSingleton<MobileCacheStore>();
        builder.Services.AddSingleton<MobileApiClient>();
        builder.Services.AddSingleton<SyncService>();

        // 🔥 UI
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

        // 🔥 DISPATCHER (QUAN TRỌNG)
        builder.Services.AddSingleton<IDispatcher>(sp =>
            Application.Current?.Dispatcher ?? throw new Exception("Dispatcher not available"));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static string GetApiBaseUrl()
    {
        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5000/"
            : "http://localhost:5000/";
    }
}