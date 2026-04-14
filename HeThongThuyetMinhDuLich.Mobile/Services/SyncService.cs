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
        if (_isSyncing) return; // nếu đang đồng bộ thì không thực hiện đồng bộ mới

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) // nếu không có kết nối internet thì không thực hiện đồng bộ
            return;

        try
        {
            _isSyncing = true;

            await _api.GetDiemThamQuanAsync(); // lấy danh sách điểm tham quan

            var pois = await _cache.GetPoisAsync();// lấy dữ liệu đã lưu trong máy

            foreach (var poi in pois)
            {
                await _api.GetNoiDungByDiemAsync(poi.MaDiem); // đồng bộ nội dung thuyết minh của từng điểm tham quan
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SYNC ERROR: " + ex.Message); // ghi log lỗi nếu có lỗi xảy ra trong quá trình đồng bộ
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
        _timer.Interval = TimeSpan.FromSeconds(30); // tự động chạy định kì mỗi 30 giây

        _timer.Tick += async (s, e) =>
        {
            await SyncAllAsync();
        };

        _timer.Start();
    }
}
