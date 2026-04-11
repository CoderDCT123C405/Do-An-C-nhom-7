using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HeThongThuyetMinhDuLich.Cms.Models;
using Microsoft.Extensions.Options;

namespace HeThongThuyetMinhDuLich.Cms.Services;

public class CmsApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<ApiSettings> apiOptions,
    CmsSession session)
{
    private static readonly object BypassLock = new();
    private const string BypassUsername = "admin";
    private const string BypassPassword = "Admin@123";

    private HttpClient CreateClient()
    {
        TryBootstrapBypassSession();

        var client = httpClientFactory.CreateClient("Api");
        client.BaseAddress = new Uri(apiOptions.Value.BaseUrl.TrimEnd('/') + "/");
        if (!string.IsNullOrWhiteSpace(session.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return client;
    }

    private void TryBootstrapBypassSession()
    {
        if (!session.IsAuthenticated || !string.IsNullOrWhiteSpace(session.AccessToken))
        {
            return;
        }

        lock (BypassLock)
        {
            if (!string.IsNullOrWhiteSpace(session.AccessToken))
            {
                return;
            }

            try
            {
                using var authClient = httpClientFactory.CreateClient("Api");
                authClient.BaseAddress = new Uri(apiOptions.Value.BaseUrl.TrimEnd('/') + "/");

                var response = authClient.PostAsJsonAsync("api/auth/admin/login", new AdminLoginRequest
                {
                    TenDangNhap = BypassUsername,
                    MatKhau = BypassPassword
                }).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var data = response.Content.ReadFromJsonAsync<LoginResponse>().GetAwaiter().GetResult();
                if (data is null || string.IsNullOrWhiteSpace(data.Token))
                {
                    return;
                }

                session.SignIn(data.MaDinhDanh, data.TenDangNhap, data.Token, data.HoTen, data.VaiTro, data.HetHanLuc);
            }
            catch
            {
                // ignore bootstrap failures in bypass mode
            }
        }
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
        ApplyAuditFields(model, isCreate: true);
        return await SendAsync(() => client.PostAsJsonAsync("api/diemthamquan", model, cancellationToken));
    }

    public async Task<ApiOperationResult> UpdateDiemAsync(int id, DiemThamQuanCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        ApplyAuditFields(model, isCreate: false);
        return await SendAsync(() => client.PutAsJsonAsync($"api/diemthamquan/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteDiemAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/diemthamquan/{id}", cancellationToken));
    }

    public async Task<IReadOnlyList<NoiDungThuyetMinhItem>> GetNoiDungTheoDiemAsync(int maDiem, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<NoiDungThuyetMinhItem>>($"api/noidungthuyetminh/diem/{maDiem}", cancellationToken) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<NgonNguItem>> GetNgonNguAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<NgonNguItem>>("api/ngonngu", cancellationToken) ?? new List<NgonNguItem>();
        }
        catch
        {
            return new List<NgonNguItem>();
        }
    }

    public async Task<ApiOperationResult> CreateNoiDungAsync(NoiDungThuyetMinhCreate model, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            ApplyAuditFields(model, isCreate: true);
            using var response = await client.PostAsJsonAsync("api/noidungthuyetminh", model, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                var id = TryExtractId(payload, "maNoiDung");
                return ApiOperationResult.Ok(id);
            }

            return await BuildFailResultAsync(response, cancellationToken);
        }
        catch
        {
            return ApiOperationResult.Fail("Khong the ket noi API. Vui long thu lai.");
        }
    }

    public async Task<ApiOperationResult> UpdateNoiDungAsync(int id, NoiDungThuyetMinhCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        ApplyAuditFields(model, isCreate: false);
        return await SendAsync(() => client.PutAsJsonAsync($"api/noidungthuyetminh/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteNoiDungAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/noidungthuyetminh/{id}", cancellationToken));
    }

    public async Task<ApiOperationResult> GenerateAudioForNoiDungAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsync($"api/noidungthuyetminh/{id}/generate-audio", null, cancellationToken));
    }

    public async Task<ApiOperationResult> GenerateAudioBatchAsync(bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        var overwriteValue = overwrite ? "true" : "false";
        return await SendAsync(() => client.PostAsync($"api/noidungthuyetminh/generate-audio?overwrite={overwriteValue}", null, cancellationToken));
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

    public async Task<IReadOnlyList<LichSuPhatItem>> GetLichSuPhatAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<LichSuPhatItem>>("api/lichsuphat", cancellationToken) ?? new List<LichSuPhatItem>();
        }
        catch
        {
            return new List<LichSuPhatItem>();
        }
    }

    public async Task<IReadOnlyList<MaQRItem>> GetMaQRAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateClient();
            return await client.GetFromJsonAsync<List<MaQRItem>>("api/maqr", cancellationToken) ?? new List<MaQRItem>();
        }
        catch
        {
            return new List<MaQRItem>();
        }
    }

    public async Task<ApiOperationResult> CreateMaQRAsync(MaQRCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("api/maqr", model, cancellationToken));
    }

    public async Task<ApiOperationResult> UpdateMaQRAsync(int id, MaQRCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"api/maqr/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteMaQRAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/maqr/{id}", cancellationToken));
    }

    public async Task<ApiOperationResult> CreateNgonNguAsync(NgonNguCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("api/ngonngu", model, cancellationToken));
    }

    public async Task<ApiOperationResult> UpdateNgonNguAsync(int id, NgonNguCreate model, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"api/ngonngu/{id}", model, cancellationToken));
    }

    public async Task<ApiOperationResult> DeleteNgonNguAsync(int id, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"api/ngonngu/{id}", cancellationToken));
    }

    private void ApplyAuditFields(DiemThamQuanCreate model, bool isCreate)
    {
        if (session.MaTaiKhoan is not int maTaiKhoan || maTaiKhoan <= 0)
        {
            return;
        }

        if (isCreate && (!model.MaTaiKhoanTao.HasValue || model.MaTaiKhoanTao <= 0))
        {
            model.MaTaiKhoanTao = maTaiKhoan;
        }

        model.MaTaiKhoanCapNhat = maTaiKhoan;
    }

    private void ApplyAuditFields(NoiDungThuyetMinhCreate model, bool isCreate)
    {
        if (session.MaTaiKhoan is not int maTaiKhoan || maTaiKhoan <= 0)
        {
            return;
        }

        if (isCreate && (!model.MaTaiKhoanTao.HasValue || model.MaTaiKhoanTao <= 0))
        {
            model.MaTaiKhoanTao = maTaiKhoan;
        }

        model.MaTaiKhoanCapNhat = maTaiKhoan;
    }

    public async Task<IEnumerable<NguoiDungItem>> GetNguoiDungAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<IEnumerable<NguoiDungItem>>("/api/nguoidung") ?? Array.Empty<NguoiDungItem>();
    }

    public async Task<ApiOperationResult> CreateNguoiDungAsync(NguoiDungCreate payload)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("/api/nguoidung", payload));
    }

    public async Task<ApiOperationResult> UpdateNguoiDungAsync(int id, NguoiDungCreate payload)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"/api/nguoidung/{id}", payload));
    }

    public async Task<ApiOperationResult> DeleteNguoiDungAsync(int id)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"/api/nguoidung/{id}"));
    }

    public async Task<IEnumerable<TaiKhoanItem>> GetTaiKhoanAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<IEnumerable<TaiKhoanItem>>("/api/taikhoan") ?? Array.Empty<TaiKhoanItem>();
    }

    public async Task<ApiOperationResult> CreateTaiKhoanAsync(TaiKhoanCreate payload)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PostAsJsonAsync("/api/taikhoan", payload));
    }

    public async Task<ApiOperationResult> UpdateTaiKhoanAsync(int id, TaiKhoanCreate payload)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.PutAsJsonAsync($"/api/taikhoan/{id}", payload));
    }

    public async Task<ApiOperationResult> DeleteTaiKhoanAsync(int id)
    {
        using var client = CreateClient();
        return await SendAsync(() => client.DeleteAsync($"/api/taikhoan/{id}"));
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

            return await BuildFailResultAsync(response);
        }
        catch
        {
            return ApiOperationResult.Fail("Khong the ket noi API. Vui long thu lai.");
        }
    }

    private static async Task<ApiOperationResult> BuildFailResultAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
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

    private static int? TryExtractId(string? payload, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var prop in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var id))
                {
                    return id;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
