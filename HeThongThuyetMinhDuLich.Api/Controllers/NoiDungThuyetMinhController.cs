using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NoiDungThuyetMinhController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet("diem/{maDiem:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByDiem(int maDiem)
    {
        var items = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.MaDiem == maDiem)
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

        return Ok(items);
    }

    [HttpGet("diem/{maDiem:int}/ngonngu/{maNgonNgu:int}")]
    public async Task<ActionResult<object>> GetByDiemAndNgonNgu(int maDiem, int maNgonNgu)
    {
        var item = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Include(x => x.NgonNgu)
            .Where(x => x.MaDiem == maDiem && x.MaNgonNgu == maNgonNgu)
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
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<NoiDungThuyetMinh>> Create(NoiDungThuyetMinhDto model)
    {
        var entity = new NoiDungThuyetMinh
        {
            MaDiem = model.MaDiem,
            MaNgonNgu = model.MaNgonNgu,
            TieuDe = model.TieuDe,
            NoiDungVanBan = model.NoiDungVanBan,
            DuongDanAmThanh = model.DuongDanAmThanh,
            ChoPhepTTS = model.ChoPhepTTS,
            ThoiLuongGiay = model.ThoiLuongGiay,
            MaTaiKhoanTao = model.MaTaiKhoanTao,
            MaTaiKhoanCapNhat = model.MaTaiKhoanCapNhat,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.NoiDungThuyetMinhs.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByDiemAndNgonNgu), new { maDiem = entity.MaDiem, maNgonNgu = entity.MaNgonNgu }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Update(int id, NoiDungThuyetMinhDto model)
    {
        var item = await dbContext.NoiDungThuyetMinhs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.MaNgonNgu = model.MaNgonNgu;
        item.MaDiem = model.MaDiem;
        item.TieuDe = model.TieuDe;
        item.NoiDungVanBan = model.NoiDungVanBan;
        item.DuongDanAmThanh = model.DuongDanAmThanh;
        item.ChoPhepTTS = model.ChoPhepTTS;
        item.ThoiLuongGiay = model.ThoiLuongGiay;
        item.MaTaiKhoanTao = model.MaTaiKhoanTao ?? item.MaTaiKhoanTao;
        item.MaTaiKhoanCapNhat = model.MaTaiKhoanCapNhat;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.NoiDungThuyetMinhs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.NoiDungThuyetMinhs.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
