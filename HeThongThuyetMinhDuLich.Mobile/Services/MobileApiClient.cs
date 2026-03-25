using System.Net.Http.Json;
using HeThongThuyetMinhDuLich.Mobile.Models;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class MobileApiClient(IHttpClientFactory httpClientFactory, MobileCacheStore cacheStore)
{
    // 🔊 Resolve URL audio
    public string ResolveAudioUrl(string? pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
            return string.Empty;

        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var absolute))
            return absolute.ToString();

        using var client = httpClientFactory.CreateClient("Api");
        return new Uri(client.BaseAddress!, pathOrUrl.TrimStart('/')).ToString();
    }

    // ================== DIEM THAM QUAN ==================

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetDiemThamQuanAsync()
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        // 🟢 ONLINE
        if (isOnline)
        {
            try
            {
                using var client = httpClientFactory.CreateClient("Api");

                // lấy toàn bộ từ server
                var serverData = await client.GetFromJsonAsync<List<DiemThamQuanItem>>("api/diemthamquan") ?? [];

                if (serverData.Any())
                {
                    // 🔥 chỉ cần gọi Save → đã tự clear + insert
                    await cacheStore.SavePoisAsync(serverData);
                }

                return serverData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("API ERROR: " + ex.Message);

                // fallback
                return await cacheStore.GetPoisAsync();
            }
        }

        // 🔴 OFFLINE
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
                using var client = httpClientFactory.CreateClient("Api");

                var payload = await client.GetFromJsonAsync<NoiDungByDiemResponse>($"api/noidung/{maDiem}");
                var items = payload?.NoiDung ?? [];

                // sync xuống sqlite
                await cacheStore.SaveNoiDungAsync(maDiem, items);

                return items;
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
            return null;

        try
        {
            using var client = httpClientFactory.CreateClient("Api");

            var response = await client.GetAsync($"api/maqr/{Uri.EscapeDataString(qrValue)}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<QrLookupResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("QR ERROR: " + ex.Message);
            return null;
        }
    }

    // ================== LICH SU PHAT ==================

    public async Task CreateLichSuPhatAsync(LichSuPhatCreateRequest request)
    {
        var isOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

        if (!isOnline)
            return;

        try
        {
            using var client = httpClientFactory.CreateClient("Api");
            await client.PostAsJsonAsync("api/lichsuphat", request);
        }
        catch (Exception ex)
        {
            Console.WriteLine("POST ERROR: " + ex.Message);
        }
    }
}