using System.Net.Http.Headers;
using System.Net.Http.Json;
using HeThongThuyetMinhDuLich.Cms.Models;
using Microsoft.Extensions.Options;

namespace HeThongThuyetMinhDuLich.Cms.Services;

public class CmsApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<ApiSettings> apiOptions,
    CmsSession session)
{
    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient("Api");
        client.BaseAddress = new Uri(apiOptions.Value.BaseUrl);
        if (!string.IsNullOrWhiteSpace(session.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }
        return client;
    }

    public async Task<LoginResponse?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/auth/admin/login", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<LoaiDiemThamQuanItem>> GetLoaiDiemAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<LoaiDiemThamQuanItem>>("api/loaidiemthamquan", cancellationToken)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> CreateLoaiDiemAsync(LoaiDiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/loaidiemthamquan", model, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetDiemAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<DiemThamQuanItem>>("api/diemthamquan", cancellationToken)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> CreateDiemAsync(DiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/diemthamquan", model, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<ThongKeTheoDiemItem>> GetThongKeTheoDiemAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<ThongKeTheoDiemItem>>("api/lichsuphat/thong-ke/luot-nghe-theo-diem", cancellationToken)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<ThongKeTheoKichHoatItem>> GetThongKeTheoKichHoatAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<ThongKeTheoKichHoatItem>>("api/lichsuphat/thong-ke/luot-nghe-theo-kich-hoat", cancellationToken)
                ?? [];
        }
        catch
        {
            return [];
        }
    }
}
