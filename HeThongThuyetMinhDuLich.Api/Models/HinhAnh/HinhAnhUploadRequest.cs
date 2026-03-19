using Microsoft.AspNetCore.Http;

namespace HeThongThuyetMinhDuLich.Api.Models.HinhAnh;

public class HinhAnhUploadRequest
{
    public int MaDiem { get; set; }
    public bool LaAnhDaiDien { get; set; }
    public int? ThuTuHienThi { get; set; }
    public int? MaTaiKhoanTao { get; set; }
    public IFormFile TepTin { get; set; } = default!;
}
