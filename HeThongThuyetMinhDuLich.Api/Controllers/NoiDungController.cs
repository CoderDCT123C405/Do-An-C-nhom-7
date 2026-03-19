using HeThongThuyetMinhDuLich.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/noidung")]
public class NoiDungController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet("{maDiem:int}")]
    public async Task<ActionResult<object>> GetByMaDiem(int maDiem)
    {
        var items = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.MaDiem == maDiem && x.TrangThaiHoatDong)
            .Include(x => x.NgonNgu)
            .OrderBy(x => x.MaNgonNgu)
            .Select(x => new
            {
                x.MaNoiDung,
                x.MaDiem,
                x.MaNgonNgu,
                TenNgonNgu = x.NgonNgu != null ? x.NgonNgu.TenNgonNgu : null,
                x.TieuDe,
                x.NoiDungVanBan,
                x.DuongDanAmThanh,
                x.ChoPhepTTS,
                x.ThoiLuongGiay,
                x.TrangThaiHoatDong
            })
            .ToListAsync();

        if (items.Count == 0)
        {
            return NotFound(new { message = "Khong tim thay noi dung cho diem tham quan nay." });
        }

        return Ok(new
        {
            MaDiem = maDiem,
            NoiDung = items
        });
    }
}
