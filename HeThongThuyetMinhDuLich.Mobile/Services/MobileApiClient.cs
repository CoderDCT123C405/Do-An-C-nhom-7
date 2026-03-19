using System.Net.Http.Json;
using HeThongThuyetMinhDuLich.Mobile.Models;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class MobileApiClient(IHttpClientFactory httpClientFactory, MobileCacheStore cacheStore)
{
    public string ResolveAudioUrl(string? pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        using var client = httpClientFactory.CreateClient("Api");
        return new Uri(client.BaseAddress!, pathOrUrl.TrimStart('/')).ToString();
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetDiemThamQuanAsync()
    {
        try
        {
            using var client = httpClientFactory.CreateClient("Api");
            var items = await client.GetFromJsonAsync<List<DiemThamQuanItem>>("api/diemthamquan") ?? [];
            await cacheStore.SavePoisAsync(items);
            return items;
        }
        catch
        {
            return await cacheStore.GetPoisAsync();
        }
    }

    public async Task<IReadOnlyList<NoiDungItem>> GetNoiDungByDiemAsync(int maDiem)
    {
        try
        {
            using var client = httpClientFactory.CreateClient("Api");
            var payload = await client.GetFromJsonAsync<NoiDungByDiemResponse>($"api/noidung/{maDiem}");
            var items = payload?.NoiDung ?? [];
            await cacheStore.SaveNoiDungAsync(maDiem, items);
            return items;
        }
        catch
        {
            return await cacheStore.GetNoiDungAsync(maDiem);
        }
    }

    public async Task<QrLookupResponse?> LookupQrAsync(string qrValue)
    {
        using var client = httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"api/maqr/{Uri.EscapeDataString(qrValue)}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<QrLookupResponse>();
    }

    public async Task CreateLichSuPhatAsync(LichSuPhatCreateRequest request)
    {
        using var client = httpClientFactory.CreateClient("Api");
        await client.PostAsJsonAsync("api/lichsuphat", request);
    }
}
