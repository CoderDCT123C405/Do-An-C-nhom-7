using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Networking;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class SyncService
{
    private readonly MobileApiClient _api;
    private readonly MobileCacheStore _cache;
    private readonly IDispatcher _dispatcher;

    private bool _isSyncing;
    private IDispatcherTimer? _timer;

    public SyncService(
        MobileApiClient api,
        MobileCacheStore cache,
        IDispatcher dispatcher)
    {
        _api = api;
        _cache = cache;
        _dispatcher = dispatcher;
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

        _timer = _dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(30);

        _timer.Tick += async (s, e) =>
        {
            await SyncAllAsync();
        };

        _timer.Start();
    }
}