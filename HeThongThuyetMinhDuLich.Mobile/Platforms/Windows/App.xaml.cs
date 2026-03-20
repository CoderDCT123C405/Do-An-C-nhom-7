using Microsoft.UI.Xaml;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HeThongThuyetMinhDuLich.Mobile.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        TryWriteCrashLog("WinUI.UnhandledException", e.Exception);
    }

    private static void OnCurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        TryWriteCrashLog("AppDomain.UnhandledException", e.ExceptionObject as Exception);
    }

    private static void TryWriteCrashLog(string source, Exception? ex)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "HeThongThuyetMinhDuLich.Mobile.windows.crash.log");
            var sb = new StringBuilder();
            sb.AppendLine($"Time(UTC): {DateTime.UtcNow:O}");
            sb.AppendLine($"Source: {source}");
            sb.AppendLine(ex?.ToString() ?? "Unknown exception");
            sb.AppendLine(new string('-', 80));
            File.AppendAllText(path, sb.ToString());
        }
        catch
        {
            // no-op
        }
    }
}
