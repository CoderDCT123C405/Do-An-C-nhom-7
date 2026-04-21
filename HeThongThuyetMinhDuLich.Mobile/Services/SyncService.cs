using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Networking;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class SyncService
{
    private readonly MobileApiClient _api;
    private readonly string _baseUrl;

    private readonly SemaphoreSlim _lock = new(1, 1);
    private IDispatcherTimer? _timer;
    private static readonly HttpClient _http = new();

    public SyncService(MobileApiClient api, string baseUrl)
    {
        _api = api;
        _baseUrl = baseUrl;
    }

    public async Task SyncAllAsync()
    {
        if (!await _lock.WaitAsync(0)) return;

        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return;

            var pois = await _api.GetDiemThamQuanAsync();

            var semaphore = new SemaphoreSlim(3);

            var tasks = pois.Select(async poi =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var images = await _api.GetHinhAnhByDiemAsync(poi.MaDiem);

                    foreach (var img in images)
                    {
                        var url = img.DuongDanHinhAnh;

                        if (string.IsNullOrWhiteSpace(url))
                            continue;

                        url = url.Replace("file:///", "")
                                 .Replace("file:/", "")
                                 .Replace("\\", "/");

                        if (!url.StartsWith("http"))
                            url = _baseUrl.TrimEnd('/') + "/" + url.TrimStart('/');

                        var version = poi.NgayCapNhat.Ticks;
                        var fileName = $"{poi.MaDiem}_{version}_{Path.GetFileName(url)}";
                        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                        // xoá file cũ
                        var prefix = $"{poi.MaDiem}_";
                        var oldFiles = Directory.GetFiles(FileSystem.AppDataDirectory, prefix + "*");

                        foreach (var file in oldFiles)
                        {
                            if (!file.Contains($"_{version}_"))
                            {
                                try { File.Delete(file); } catch { }
                            }
                        }

                        if (File.Exists(localPath))
                            continue;

                        try
                        {
                            var res = await _http.GetAsync(url);
                            if (!res.IsSuccessStatusCode) continue;

                            var bytes = await res.Content.ReadAsByteArrayAsync();
                            if (bytes.Length == 0) continue;

                            File.WriteAllBytes(localPath, bytes);
                        }
                        catch { }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void StartAutoSync()
    {
        if (_timer != null) return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null) return;
        _http.Timeout = TimeSpan.FromSeconds(10);
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMinutes(5);

        _timer.Tick += async (_, _) =>
        {
            try
            {
                await SyncAllAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TIMER SYNC ERROR: " + ex.Message);
            }
        };

        _timer.Start();
    }
}