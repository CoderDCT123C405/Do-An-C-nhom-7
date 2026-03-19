using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaQrController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await dbContext.MaQrs
            .AsNoTracking()
            .Include(x => x.DiemThamQuan)
            .OrderBy(x => x.MaQR)
            .Select(x => new
            {
                x.MaQR,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.GiaTriQR,
                x.TrangThaiHoatDong,
                x.NgayTao,
                x.MaTaiKhoanTao
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var item = await dbContext.MaQrs
            .AsNoTracking()
            .Include(x => x.DiemThamQuan)
            .Where(x => x.MaQR == id)
            .Select(x => new
            {
                x.MaQR,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.GiaTriQR,
                x.TrangThaiHoatDong,
                x.NgayTao,
                x.MaTaiKhoanTao
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("gia-tri/{giaTriQr}")]
    public async Task<ActionResult<object>> GetByGiaTriQr(string giaTriQr)
    {
        var item = await dbContext.MaQrs
            .AsNoTracking()
            .Include(x => x.DiemThamQuan)
            .Where(x => x.GiaTriQR == giaTriQr && x.TrangThaiHoatDong)
            .Select(x => new
            {
                x.MaQR,
                x.GiaTriQR,
                x.MaDiem,
                DiemThamQuan = x.DiemThamQuan == null ? null : new
                {
                    x.DiemThamQuan.MaDiem,
                    x.DiemThamQuan.MaDinhDanh,
                    x.DiemThamQuan.TenDiem,
                    x.DiemThamQuan.MoTaNgan,
                    x.DiemThamQuan.DiaChi,
                    x.DiemThamQuan.ViDo,
                    x.DiemThamQuan.KinhDo,
                    x.DiemThamQuan.BanKinhKichHoat
                }
            })
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound();
        }

        var noiDung = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.MaDiem == item.MaDiem && x.TrangThaiHoatDong)
            .OrderBy(x => x.MaNgonNgu)
            .Select(x => new
            {
                x.MaNoiDung,
                x.MaDiem,
                x.MaNgonNgu,
                x.TieuDe,
                x.NoiDungVanBan,
                x.DuongDanAmThanh,
                x.ChoPhepTTS,
                x.ThoiLuongGiay
            })
            .ToListAsync();

        return Ok(new
        {
            item.MaQR,
            item.GiaTriQR,
            item.MaDiem,
            item.DiemThamQuan,
            NoiDung = noiDung
        });
    }

    [HttpGet("/api/maqr/{giaTriQr}")]
    public Task<ActionResult<object>> GetByGiaTriQrAlias(string giaTriQr)
        => GetByGiaTriQr(giaTriQr);

    [HttpPost]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<MaQr>> Create(MaQrDto model)
    {
        var entity = new MaQr
        {
            MaDiem = model.MaDiem,
            GiaTriQR = model.GiaTriQR,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            MaTaiKhoanTao = model.MaTaiKhoanTao,
            NgayTao = DateTime.UtcNow
        };

        dbContext.MaQrs.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaQR }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Update(int id, MaQrDto model)
    {
        var item = await dbContext.MaQrs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.MaDiem = model.MaDiem;
        item.GiaTriQR = model.GiaTriQR;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.MaTaiKhoanTao = model.MaTaiKhoanTao;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.MaQrs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.MaQrs.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
