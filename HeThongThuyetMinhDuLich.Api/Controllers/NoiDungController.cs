using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/noidung")]
public class NoiDungController(DuLichDbContext dbContext, AudioPathResolver audioPathResolver) : ControllerBase
{
    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<object>>> GetUpdatedSince([FromQuery] DateTime? updatedSince, [FromQuery] int? maDiem)
    {
        var thresholdUtc = updatedSince?.ToUniversalTime() ?? DateTime.MinValue;

        var query = dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => (x.NgayCapNhat ?? x.NgayTao) > thresholdUtc);

        if (maDiem.HasValue)
        {
            query = query.Where(x => x.MaDiem == maDiem.Value);
        }

        var items = await query
            .Include(x => x.NgonNgu)
            .OrderBy(x => x.MaDiem)
            .ThenBy(x => x.MaNgonNgu)
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
                x.TrangThaiHoatDong,
                NgayCapNhat = x.NgayCapNhat ?? x.NgayTao
            })
            .ToListAsync();

        var result = items.Select(x => new
        {
            x.MaNoiDung,
            x.MaDiem,
            x.MaNgonNgu,
            x.TenNgonNgu,
            x.TieuDe,
            x.NoiDungVanBan,
            DuongDanAmThanh = audioPathResolver.ResolveNoiDungAudioPath(x.MaNoiDung, x.DuongDanAmThanh),
            x.ChoPhepTTS,
            x.ThoiLuongGiay,
            x.TrangThaiHoatDong,
            x.NgayCapNhat
        }).ToList();

        return Ok(result);
    }

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

        var result = items.Select(x => new
        {
            x.MaNoiDung,
            x.MaDiem,
            x.MaNgonNgu,
            x.TenNgonNgu,
            x.TieuDe,
            x.NoiDungVanBan,
            DuongDanAmThanh = audioPathResolver.ResolveNoiDungAudioPath(x.MaNoiDung, x.DuongDanAmThanh),
            x.ChoPhepTTS,
            x.ThoiLuongGiay,
            x.TrangThaiHoatDong
        }).ToList();

        if (result.Count == 0)
        {
            return NotFound(new { message = "Khong tim thay noi dung cho diem tham quan nay." });
        }

        return Ok(new
        {
            MaDiem = maDiem,
            NoiDung = result
        });
    }
}
