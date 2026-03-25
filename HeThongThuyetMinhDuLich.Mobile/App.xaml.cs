using Microsoft.Maui;
using Microsoft.Maui.Controls;
using HeThongThuyetMinhDuLich.Mobile.Services;

namespace HeThongThuyetMinhDuLich.Mobile;

public partial class App : Application
{
    private readonly AppShell _appShell;
    private readonly SyncService _sync;

    public App(AppShell appShell, SyncService sync)
    {
        InitializeComponent();

        _appShell = appShell;
        _sync = sync;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_appShell);
    }

    protected override void OnStart()
    {
        base.OnStart();
        _sync.StartAutoSync();
    }
}