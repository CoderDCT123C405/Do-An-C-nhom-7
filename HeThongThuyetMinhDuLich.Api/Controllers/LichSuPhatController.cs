using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LichSuPhatController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.NguoiDung)
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .OrderByDescending(x => x.ThoiGianBatDau)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaNguoiDung,
                HoTenNguoiDung = x.NguoiDung != null ? x.NguoiDung.HoTen : null,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<object>> GetById(long id)
    {
        var item = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.NguoiDung)
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .Where(x => x.MaLichSuPhat == id)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaNguoiDung,
                HoTenNguoiDung = x.NguoiDung != null ? x.NguoiDung.HoTen : null,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("nguoidung/{maNguoiDung:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByNguoiDung(int maNguoiDung)
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .Where(x => x.MaNguoiDung == maNguoiDung)
            .OrderByDescending(x => x.ThoiGianBatDau)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("thong-ke/luot-nghe-theo-diem")]
    public async Task<ActionResult<IEnumerable<object>>> ThongKeTheoDiem()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .GroupBy(x => new { x.MaDiem, x.DiemThamQuan!.TenDiem })
            .Select(g => new
            {
                g.Key.MaDiem,
                g.Key.TenDiem,
                SoLuotNghe = g.Count(),
                TongThoiLuongDaNghe = g.Sum(x => x.ThoiLuongDaNghe ?? 0)
            })
            .OrderByDescending(x => x.SoLuotNghe)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("thong-ke/luot-nghe-theo-kich-hoat")]
    public async Task<ActionResult<IEnumerable<object>>> ThongKeTheoCachKichHoat()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .GroupBy(x => x.CachKichHoat)
            .Select(g => new
            {
                CachKichHoat = g.Key,
                SoLuotPhat = g.Count(),
                TongThoiLuongDaNghe = g.Sum(x => x.ThoiLuongDaNghe ?? 0)
            })
            .OrderByDescending(x => x.SoLuotPhat)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<LichSuPhat>> Create(LichSuPhatDto model)
    {
        var entity = new LichSuPhat
        {
            MaNguoiDung = model.MaNguoiDung,
            MaDiem = model.MaDiem,
            MaNoiDung = model.MaNoiDung,
            CachKichHoat = model.CachKichHoat,
            ThoiGianBatDau = model.ThoiGianBatDau ?? DateTime.UtcNow,
            ThoiLuongDaNghe = model.ThoiLuongDaNghe
        };

        dbContext.LichSuPhats.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaLichSuPhat }, entity);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, LichSuPhatDto model)
    {
        var item = await dbContext.LichSuPhats.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.MaNguoiDung = model.MaNguoiDung;
        item.MaDiem = model.MaDiem;
        item.MaNoiDung = model.MaNoiDung;
        item.CachKichHoat = model.CachKichHoat;
        item.ThoiGianBatDau = model.ThoiGianBatDau ?? item.ThoiGianBatDau;
        item.ThoiLuongDaNghe = model.ThoiLuongDaNghe;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await dbContext.LichSuPhats.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.LichSuPhats.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
