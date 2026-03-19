using HeThongThuyetMinhDuLich.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace HeThongThuyetMinhDuLich.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(GetApiBaseUrl());
        });
        builder.Services.AddSingleton<MobileCacheStore>();
        builder.Services.AddSingleton<MobileApiClient>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

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
