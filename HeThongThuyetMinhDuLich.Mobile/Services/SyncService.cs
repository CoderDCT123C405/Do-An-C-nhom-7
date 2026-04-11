using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Networking;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class SyncService
{
    private readonly MobileApiClient _api;
    private readonly MobileCacheStore _cache;

    private bool _isSyncing;
    private IDispatcherTimer? _timer;

    public SyncService(
        MobileApiClient api,
        MobileCacheStore cache)
    {
        _api = api;
        _cache = cache;
    }

    public async Task SyncAllAsync()
    {
        if (_isSyncing) return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return;

        try
        {
            _isSyncing = true;

            await _api.GetDiemThamQuanAsync();

            var pois = await _cache.GetPoisAsync();

            foreach (var poi in pois)
            {
                await _api.GetNoiDungByDiemAsync(poi.MaDiem);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SYNC ERROR: " + ex.Message);
        }
        finally
        {
            _isSyncing = false;
        }
    }

    public void StartAutoSync()
    {
        if (_timer != null) return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null) return;

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);

        _timer.Tick += async (s, e) =>
        {
            await SyncAllAsync();
        };

        _timer.Start();
    }
}
