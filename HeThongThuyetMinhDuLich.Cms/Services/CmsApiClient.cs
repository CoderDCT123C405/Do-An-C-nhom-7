using System.Net;
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
        client.BaseAddress = new Uri(apiOptions.Value.BaseUrl.TrimEnd('/') + "/");
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
            return await client.GetFromJsonAsync<List<LoaiDiemThamQuanItem>>("api/loaidiemthamquan", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ApiOperationResult> CreateLoaiDiemAsync(LoaiDiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("api/loaidiemthamquan", model, cancellationToken));
    }

    public async Task<ApiOperationResult> UpdateLoaiDiemAsync(int id, LoaiDiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"api/loaidiemthamquan/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteLoaiDiemAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/loaidiemthamquan/{id}", cancellationToken));
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetDiemAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<DiemThamQuanItem>>("api/diemthamquan", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ApiOperationResult> CreateDiemAsync(DiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("api/diemthamquan", model, cancellationToken));
    }

    public async Task<ApiOperationResult> UpdateDiemAsync(int id, DiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"api/diemthamquan/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteDiemAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/diemthamquan/{id}", cancellationToken));
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

    private static async Task<ApiOperationResult> SendAsync(Func<Task<HttpResponseMessage>> action)
    {
        try
        {
            using var response = await action();
            if (response.IsSuccessStatusCode)
            {
                return ApiOperationResult.Ok();
            }

            var payload = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return ApiOperationResult.Fail("Ban khong co quyen thuc hien thao tac nay.");
            }

            if (!string.IsNullOrWhiteSpace(payload))
            {
                return ApiOperationResult.Fail(payload);
            }

            return ApiOperationResult.Fail($"Yeu cau that bai ({(int)response.StatusCode}).");
        }
        catch
        {
            return ApiOperationResult.Fail("Khong the ket noi API. Vui long thu lai.");
        }
    }
}
