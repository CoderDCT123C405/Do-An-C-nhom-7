using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text.Json;
using HeThongThuyetMinhDuLich.Mobile.Models;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class MobileApiClient(IHttpClientFactory httpClientFactory, MobileCacheStore cacheStore, AuthSession authSession)
{
    private string? _resolvedBaseUrl;

    private static readonly HashSet<string> AllowedPlaybackTriggers = new(StringComparer.OrdinalIgnoreCase)
    {
        "gps",
        "qr",
        "manual"
    };

    private static readonly string[] AndroidApiBaseUrls =
    [
        "http://10.0.2.2:5000/",
        "http://10.0.2.2:61397/"
    ];

    private static readonly string[] DesktopApiBaseUrls =
    [
        "http://localhost:5000/",
        "http://localhost:61397/"
    ];

    private const string SyncKeyPois = MobileCacheStore.SyncKeyPois;
    private const string SyncKeyNoiDung = MobileCacheStore.SyncKeyNoiDung;
    private const string SyncKeyNgonNgu = MobileCacheStore.SyncKeyNgonNgu;
    private const string SyncKeyQr = MobileCacheStore.SyncKeyQr;

    // 🔊 Resolve URL audio
    public string ResolveAudioUrl(string? pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            return string.Empty;

        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var absolute))
        {
            // Some APIs may return file:// URLs (e.g. file:///audio/tts/x.mp3).
            // On mobile we must convert them back to HTTP endpoint.
            if (string.Equals(absolute.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(absolute.LocalPath))
                {
                    return absolute.ToString();
                }

                var filePath = absolute.AbsolutePath;
                if (string.IsNullOrWhiteSpace(filePath))
                    return string.Empty;

                var fallbackBaseUrl = _resolvedBaseUrl ?? GetCandidateBaseUrls().First();
                return new Uri(new Uri(fallbackBaseUrl), filePath.TrimStart('/')).ToString();
            }

            if (DeviceInfo.Platform == DevicePlatform.Android &&
                (string.Equals(absolute.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(absolute.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(absolute.Host, "::1", StringComparison.OrdinalIgnoreCase)))
            {
                if (!string.IsNullOrWhiteSpace(_resolvedBaseUrl) &&
                    Uri.TryCreate(_resolvedBaseUrl, UriKind.Absolute, out var resolvedBase))
                {
                    var resolvedBuilder = new UriBuilder(resolvedBase)
                    {
                        Path = absolute.AbsolutePath,
                        Query = absolute.Query.TrimStart('?')
                    };
                    return resolvedBuilder.Uri.ToString();
                }

                var androidBuilder = new UriBuilder(absolute)
                {
                    Host = "10.0.2.2"
                };
                return androidBuilder.Uri.ToString();
            }

            return absolute.ToString();
        }

        var baseUrl = _resolvedBaseUrl ?? GetCandidateBaseUrls().First();
        return new Uri(new Uri(baseUrl), pathOrUrl.TrimStart('/')).ToString();
    }

    public string ResolveAudioUrl(NoiDungItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.TepAmThanhNoiBo))
        {
            var localUrl = ResolveAudioUrl(item.TepAmThanhNoiBo);
            if (!string.IsNullOrWhiteSpace(localUrl))
            {
                return localUrl;
            }
        }

        return ResolveAudioUrl(item.DuongDanAmThanh);
    }

    public async Task<string> GetPlayableAudioSourceAsync(NoiDungItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.TepAmThanhNoiBo) &&
            Uri.TryCreate(item.TepAmThanhNoiBo, UriKind.Absolute, out var localUri) &&
            string.Equals(localUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) &&
            File.Exists(localUri.LocalPath))
        {
            var bytes = await File.ReadAllBytesAsync(localUri.LocalPath);
            if (bytes.Length > 0)
            {
                return $"data:audio/mpeg;base64,{Convert.ToBase64String(bytes)}";
            }
        }

        return ResolveAudioUrl(item);
    }

    // ================== DIEM THAM QUAN ==================

    public async Task<MobileAuthResult> LoginUserAsync(UserLoginRequest request)
    {
        try
        {
            using var client = await CreateApiClientAsync();
            using var response = await client.PostAsJsonAsync("api/auth/user/login", request);
            if (!response.IsSuccessStatusCode)
            {
                return MobileAuthResult.Fail(await ExtractErrorMessageAsync(response));
            }

            var login = await response.Content.ReadFromJsonAsync<MobileLoginResponse>();
            if (login is null || string.IsNullOrWhiteSpace(login.Token))
            {
                return MobileAuthResult.Fail("Dang nhap that bai.");
            }

            authSession.SignIn(login);
            return MobileAuthResult.Ok(login);
        }
        catch (Exception ex)
        {
            return MobileAuthResult.Fail(ex.Message);
        }
    }

    public async Task<MobileAuthResult> RegisterUserAsync(UserRegisterRequest request)
    {
        try
        {
            using var client = await CreateApiClientAsync();
            using var response = await client.PostAsJsonAsync("api/auth/user/register", request);
            if (!response.IsSuccessStatusCode)
            {
                return MobileAuthResult.Fail(await ExtractErrorMessageAsync(response));
            }

            return await LoginUserAsync(new UserLoginRequest
            {
                TenDangNhap = request.TenDangNhap,
                MatKhau = request.MatKhau
            });
        }
        catch (Exception ex)
        {
            return MobileAuthResult.Fail(ex.Message);
        }
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetDiemThamQuanAsync()
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (isOnline)
        {
            try
            {
                await SyncPoisAsync();
                return await cacheStore.GetPoisAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("API ERROR: " + ex.Message);
                return await cacheStore.GetPoisAsync();
            }
        }

        return await cacheStore.GetPoisAsync();
    }

    // ================== NOI DUNG ==================

    public async Task<IReadOnlyList<NoiDungItem>> GetNoiDungByDiemAsync(int maDiem)
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (isOnline)
        {
            try
            {
                await SyncNoiDungAsync(maDiem);
                return await cacheStore.GetNoiDungAsync(maDiem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("API ERROR: " + ex.Message);
                return await cacheStore.GetNoiDungAsync(maDiem);
            }
        }

        return await cacheStore.GetNoiDungAsync(maDiem);
    }

    // ================== QR ==================

    public async Task<QrLookupResponse?> LookupQrAsync(string qrValue)
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (!isOnline)
            return await LookupQrFromCacheAsync(qrValue);

        try
        {
            using var client = await CreateApiClientAsync();

            var response = await client.GetAsync($"api/maqr/{Uri.EscapeDataString(qrValue)}");

            if (!response.IsSuccessStatusCode)
                return await LookupQrFromCacheAsync(qrValue);

            var result = await response.Content.ReadFromJsonAsync<QrLookupResponse>();
            if (result is null)
            {
                return await LookupQrFromCacheAsync(qrValue);
            }

            if (result.DiemThamQuan is not null)
            {
                result.DiemThamQuan.TrangThaiHoatDong = true;
                if (result.DiemThamQuan.NgayCapNhat == default)
                {
                    result.DiemThamQuan.NgayCapNhat = DateTime.UtcNow;
                }

                await cacheStore.SavePoiAsync(result.DiemThamQuan);
            }

            var cachedContents = await PrepareOfflineReadyNoiDungAsync(result.NoiDung);
            foreach (var item in cachedContents)
            {
                item.TrangThaiHoatDong = true;
                if (item.NgayCapNhat == default)
                {
                    item.NgayCapNhat = DateTime.UtcNow;
                }
            }

            result.NoiDung = cachedContents.ToList();
            await cacheStore.SaveNoiDungAsync(result.MaDiem, cachedContents);
            await cacheStore.SaveQrMappingAsync(new QrSummaryItem
            {
                MaQR = result.MaQR,
                MaDiem = result.MaDiem,
                GiaTriQR = result.GiaTriQR,
                TrangThaiHoatDong = true,
                NgayCapNhat = DateTime.UtcNow
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("QR ERROR: " + ex.Message);
            return await LookupQrFromCacheAsync(qrValue);
        }
    }

    // ================== NGON NGU ==================

    public async Task<IReadOnlyList<NgonNguItem>> GetNgonNguAsync()
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (!isOnline)
            return await cacheStore.GetNgonNguAsync();

        try
        {
            await SyncNgonNguAsync();
            return await cacheStore.GetNgonNguAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("LANG ERROR: " + ex.Message);
            return await cacheStore.GetNgonNguAsync();
        }
    }

    // ================== LICH SU PHAT ==================

    public async Task CreateLichSuPhatAsync(LichSuPhatCreateRequest request)
    {
        var normalizedRequest = NormalizePlaybackHistoryRequest(request);
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (!isOnline)
        {
            await cacheStore.EnqueuePlaybackHistoryAsync(normalizedRequest);
            return;
        }

        try
        {
            using var client = await CreateApiClientAsync();
            var response = await client.PostAsJsonAsync("api/lichsuphat", normalizedRequest);
            if (!response.IsSuccessStatusCode)
            {
                await cacheStore.EnqueuePlaybackHistoryAsync(normalizedRequest);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("POST ERROR: " + ex.Message);
            await cacheStore.EnqueuePlaybackHistoryAsync(normalizedRequest);
        }
    }

    public async Task SyncOfflineStateAsync(IEnumerable<int>? poiIds = null)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }

        await SyncNgonNguAsync();
        await SyncPoisAsync();
        await SyncQrMappingsAsync();
        await SyncPendingPlaybackHistoryAsync();
        await SyncNoiDungAsync();
    }

    public async Task SyncPendingPlaybackHistoryAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }

        var pendingItems = await cacheStore.GetPendingPlaybackHistoryAsync();
        if (pendingItems.Count == 0)
        {
            return;
        }

        using var client = await CreateApiClientAsync();
        var completedIds = new List<long>();

        foreach (var item in pendingItems)
        {
            var normalizedRequest = NormalizePlaybackHistoryRequest(new LichSuPhatCreateRequest
            {
                MaNguoiDung = item.MaNguoiDung,
                MaDiem = item.MaDiem,
                MaNoiDung = item.MaNoiDung,
                CachKichHoat = item.CachKichHoat,
                ThoiGianBatDau = item.ThoiGianBatDau,
                ThoiLuongDaNghe = item.ThoiLuongDaNghe
            });

            var response = await client.PostAsJsonAsync("api/lichsuphat", normalizedRequest);

            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            completedIds.Add(item.Id);
        }

        await cacheStore.RemovePendingPlaybackHistoryAsync(completedIds);
    }

    public async Task SyncQrMappingsAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }

        try
        {
            using var client = await CreateApiClientAsync();
            var requestStartedAtUtc = DateTime.UtcNow;
            var lastSyncUtc = await cacheStore.GetSyncCheckpointAsync(SyncKeyQr);
            var query = BuildUpdatedSinceQuery("api/maqr/sync", lastSyncUtc);
            var items = await client.GetFromJsonAsync<List<QrSummaryItem>>(query) ?? [];
            await cacheStore.SaveQrMappingsAsync(items, replaceMissing: lastSyncUtc is null);
            await cacheStore.SetSyncCheckpointAsync(SyncKeyQr, ComputeNextSyncCheckpoint(items.Select(x => x.NgayCapNhat), requestStartedAtUtc));
        }
        catch (Exception ex)
        {
            Console.WriteLine("QR SYNC ERROR: " + ex.Message);
        }
    }

    private async Task<HttpClient> CreateApiClientAsync()
    {
        if (!string.IsNullOrWhiteSpace(_resolvedBaseUrl))
        {
            return CreateClientForBaseUrl(_resolvedBaseUrl);
        }

        foreach (var baseUrl in GetCandidateBaseUrls())
        {
            var client = CreateClientForBaseUrl(baseUrl);
            try
            {
                using var response = await client.GetAsync("swagger/index.html", HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    _resolvedBaseUrl = baseUrl;
                    return client;
                }
            }
            catch
            {
                client.Dispose();
                continue;
            }

            client.Dispose();
        }

        throw new InvalidOperationException("Khong ket noi duoc API o cac cong da cau hinh.");
    }

    private HttpClient CreateClientForBaseUrl(string baseUrl)
    {
        var client = httpClientFactory.CreateClient("Api");
        client.BaseAddress = new Uri(baseUrl);
        if (!string.IsNullOrWhiteSpace(authSession.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authSession.AccessToken);
        }
        return client;
    }

    private static async Task<string> ExtractErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(payload))
            {
                return $"API loi {(int)response.StatusCode}.";
            }

            using var json = JsonDocument.Parse(payload);
            if (json.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (json.RootElement.TryGetProperty("message", out var messageElement))
                {
                    return messageElement.GetString() ?? payload;
                }

                if (json.RootElement.TryGetProperty("title", out var titleElement))
                {
                    return titleElement.GetString() ?? payload;
                }
            }

            return payload;
        }
        catch
        {
            return $"API loi {(int)response.StatusCode}.";
        }
    }

    private static IReadOnlyList<string> GetCandidateBaseUrls()
    {
        return DeviceInfo.Platform == DevicePlatform.Android
            ? AndroidApiBaseUrls
            : DesktopApiBaseUrls;
    }

    private static LichSuPhatCreateRequest NormalizePlaybackHistoryRequest(LichSuPhatCreateRequest request)
    {
        var trigger = string.IsNullOrWhiteSpace(request.CachKichHoat)
            ? "manual"
            : request.CachKichHoat.Trim().ToLowerInvariant();

        if (!AllowedPlaybackTriggers.Contains(trigger))
        {
            trigger = "manual";
        }

        return new LichSuPhatCreateRequest
        {
            MaNguoiDung = request.MaNguoiDung,
            MaDiem = request.MaDiem,
            MaNoiDung = request.MaNoiDung,
            CachKichHoat = trigger,
            ThoiGianBatDau = request.ThoiGianBatDau,
            ThoiLuongDaNghe = request.ThoiLuongDaNghe
        };
    }

    private async Task SyncPoisAsync()
    {
        using var client = await CreateApiClientAsync();
        var requestStartedAtUtc = DateTime.UtcNow;
        var lastSyncUtc = await cacheStore.GetSyncCheckpointAsync(SyncKeyPois);
        var query = BuildUpdatedSinceQuery("api/diemthamquan/sync", lastSyncUtc);
        var items = await client.GetFromJsonAsync<List<DiemThamQuanItem>>(query) ?? [];
        await cacheStore.SavePoisAsync(items, replaceMissing: lastSyncUtc is null);
        var nextCheckpointUtc = ComputeNextSyncCheckpoint(items.Select(x => x.NgayCapNhat), requestStartedAtUtc);
        await cacheStore.SetSyncCheckpointAsync(SyncKeyPois, nextCheckpointUtc);
        LogSyncResult("pois", items.Count, lastSyncUtc, nextCheckpointUtc);
    }

    private async Task SyncNgonNguAsync()
    {
        using var client = await CreateApiClientAsync();
        var requestStartedAtUtc = DateTime.UtcNow;
        var lastSyncUtc = await cacheStore.GetSyncCheckpointAsync(SyncKeyNgonNgu);
        var query = BuildUpdatedSinceQuery("api/ngonngu/sync", lastSyncUtc);
        var items = await client.GetFromJsonAsync<List<NgonNguItem>>(query) ?? [];
        await cacheStore.SaveNgonNguAsync(items, replaceMissing: lastSyncUtc is null);
        var nextCheckpointUtc = ComputeNextSyncCheckpoint(items.Select(x => x.NgayCapNhat), requestStartedAtUtc);
        await cacheStore.SetSyncCheckpointAsync(SyncKeyNgonNgu, nextCheckpointUtc);
        LogSyncResult("ngonngu", items.Count, lastSyncUtc, nextCheckpointUtc);
    }

    private async Task SyncNoiDungAsync(int? maDiem = null)
    {
        using var client = await CreateApiClientAsync();
        var requestStartedAtUtc = DateTime.UtcNow;
        var syncKey = maDiem.HasValue ? $"{SyncKeyNoiDung}:{maDiem.Value}" : SyncKeyNoiDung;
        var lastSyncUtc = await cacheStore.GetSyncCheckpointAsync(syncKey);
        var query = BuildUpdatedSinceQuery("api/noidung/sync", lastSyncUtc, maDiem.HasValue ? $"maDiem={maDiem.Value}" : null);
        var items = await client.GetFromJsonAsync<List<NoiDungItem>>(query) ?? [];
        var preparedItems = await PrepareOfflineReadyNoiDungAsync(items);

        foreach (var groupedItems in preparedItems.GroupBy(x => x.MaDiem))
        {
            await cacheStore.SaveNoiDungAsync(groupedItems.Key, groupedItems, replaceMissing: lastSyncUtc is null && maDiem.HasValue);
        }

        var nextCheckpointUtc = ComputeNextSyncCheckpoint(preparedItems.Select(x => x.NgayCapNhat), requestStartedAtUtc);
        await cacheStore.SetSyncCheckpointAsync(syncKey, nextCheckpointUtc);
        LogSyncResult(syncKey, preparedItems.Count, lastSyncUtc, nextCheckpointUtc);
    }

    private static string BuildUpdatedSinceQuery(string path, DateTime? updatedSinceUtc, string? extraQuery = null)
    {
        var queryParts = new List<string>();

        if (updatedSinceUtc.HasValue)
        {
            queryParts.Add($"updatedSince={Uri.EscapeDataString(updatedSinceUtc.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture))}");
        }

        if (!string.IsNullOrWhiteSpace(extraQuery))
        {
            queryParts.Add(extraQuery);
        }

        return queryParts.Count == 0 ? path : $"{path}?{string.Join("&", queryParts)}";
    }

    private static DateTime ComputeNextSyncCheckpoint(IEnumerable<DateTime> timestamps, DateTime requestStartedAtUtc)
    {
        var maxTimestampUtc = timestamps.DefaultIfEmpty(DateTime.MinValue).Max();
        return maxTimestampUtc > requestStartedAtUtc ? maxTimestampUtc : requestStartedAtUtc;
    }

    private static void LogSyncResult(string area, int deltaCount, DateTime? lastSyncUtc, DateTime nextCheckpointUtc)
    {
        var previousCheckpoint = lastSyncUtc?.ToString("O", CultureInfo.InvariantCulture) ?? "FULL";
        var nextCheckpoint = nextCheckpointUtc.ToString("O", CultureInfo.InvariantCulture);
        Console.WriteLine($"SYNC [{area}] delta={deltaCount} from={previousCheckpoint} to={nextCheckpoint}");
    }

    private async Task<IReadOnlyList<NoiDungItem>> PrepareOfflineReadyNoiDungAsync(IEnumerable<NoiDungItem> items)
    {
        var result = new List<NoiDungItem>();

        foreach (var item in items)
        {
            var offlineItem = new NoiDungItem
            {
                MaNoiDung = item.MaNoiDung,
                MaDiem = item.MaDiem,
                MaNgonNgu = item.MaNgonNgu,
                TenNgonNgu = item.TenNgonNgu,
                TieuDe = item.TieuDe,
                NoiDungVanBan = item.NoiDungVanBan,
                DuongDanAmThanh = item.DuongDanAmThanh,
                ChoPhepTTS = item.ChoPhepTTS,
                ThoiLuongGiay = item.ThoiLuongGiay,
                TrangThaiHoatDong = item.TrangThaiHoatDong,
                NgayCapNhat = item.NgayCapNhat,
                TepAmThanhNoiBo = await CacheAudioFileAsync(item)
            };

            result.Add(offlineItem);
        }

        return result;
    }

    private async Task<string?> CacheAudioFileAsync(NoiDungItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.TepAmThanhNoiBo) &&
            Uri.TryCreate(item.TepAmThanhNoiBo, UriKind.Absolute, out var localUri) &&
            string.Equals(localUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase) &&
            File.Exists(localUri.LocalPath))
        {
            return item.TepAmThanhNoiBo;
        }

        var audioUrl = ResolveAudioUrl(item.DuongDanAmThanh);
        if (string.IsNullOrWhiteSpace(audioUrl) ||
            !Uri.TryCreate(audioUrl, UriKind.Absolute, out var absoluteAudioUri) ||
            string.Equals(absoluteAudioUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
        {
            return item.TepAmThanhNoiBo;
        }

        try
        {
            var extension = Path.GetExtension(absoluteAudioUri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".mp3";
            }

            var folderPath = Path.Combine(FileSystem.Current.AppDataDirectory, "audio-cache");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, $"noidung-{item.MaNoiDung}{extension}");
            if (File.Exists(filePath))
            {
                return new Uri(filePath).AbsoluteUri;
            }

            using var client = httpClientFactory.CreateClient("Api");
            using var response = await client.GetAsync(absoluteAudioUri, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                return item.TepAmThanhNoiBo;
            }

            await using var input = await response.Content.ReadAsStreamAsync();
            await using var output = File.Create(filePath);
            await input.CopyToAsync(output);
            return new Uri(filePath).AbsoluteUri;
        }
        catch (Exception ex)
        {
            Console.WriteLine("AUDIO CACHE ERROR: " + ex.Message);
            return item.TepAmThanhNoiBo;
        }
    }

    private async Task<QrLookupResponse?> LookupQrFromCacheAsync(string qrValue)
    {
        var cachedQr = await cacheStore.GetQrMappingAsync(qrValue);
        if (cachedQr is null)
        {
            return null;
        }

        var poi = await cacheStore.GetPoiAsync(cachedQr.MaDiem);
        if (poi is null)
        {
            return null;
        }

        var noiDung = await cacheStore.GetNoiDungAsync(cachedQr.MaDiem);
        return new QrLookupResponse
        {
            MaQR = cachedQr.MaQR,
            GiaTriQR = cachedQr.GiaTriQR,
            MaDiem = cachedQr.MaDiem,
            DiemThamQuan = poi,
            NoiDung = noiDung.ToList()
        };
    }
}
