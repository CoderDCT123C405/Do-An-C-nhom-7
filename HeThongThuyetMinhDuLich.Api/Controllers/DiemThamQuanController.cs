using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiemThamQuanController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
public async Task<ActionResult<IEnumerable<DiemThamQuanDto>>> GetAll()
{
    var items = await dbContext.DiemThamQuans
        .AsNoTracking()
        .Include(x => x.LoaiDiemThamQuan)
        .OrderBy(x => x.TenDiem)
        .Select(x => new DiemThamQuanDto
        {
            MaDiem = x.MaDiem,
            MaDinhDanh = x.MaDinhDanh,
            TenDiem = x.TenDiem,
            MoTaNgan = x.MoTaNgan,
            ViDo = x.ViDo,
            KinhDo = x.KinhDo,
            BanKinhKichHoat = x.BanKinhKichHoat,
            DiaChi = x.DiaChi,
            MaLoai = x.MaLoai,
            TrangThaiHoatDong = x.TrangThaiHoatDong
        })
        .ToListAsync();

    return Ok(items);
}

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var item = await dbContext.DiemThamQuans
            .AsNoTracking()
            .Include(x => x.LoaiDiemThamQuan)
            .Include(x => x.HinhAnhDiemThamQuans)
            .Include(x => x.NoiDungThuyetMinhs)
            .Where(x => x.MaDiem == id)
            .Select(x => new
            {
                x.MaDiem,
                x.MaDinhDanh,
                x.TenDiem,
                x.MoTaNgan,
                x.ViDo,
                x.KinhDo,
                x.BanKinhKichHoat,
                x.DiaChi,
                x.MaLoai,
                TenLoai = x.LoaiDiemThamQuan != null ? x.LoaiDiemThamQuan.TenLoai : null,
                x.TrangThaiHoatDong,
                HinhAnh = x.HinhAnhDiemThamQuans
                    .OrderBy(h => h.ThuTuHienThi)
                    .Select(h => new
                    {
                        h.MaHinhAnh,
                        h.TenTepTin,
                        h.DuongDanHinhAnh,
                        h.LaAnhDaiDien,
                        h.ThuTuHienThi
                    }),
                NoiDung = x.NoiDungThuyetMinhs
                    .Select(n => new
                    {
                        n.MaNoiDung,
                        n.MaNgonNgu,
                        n.TieuDe,
                        n.DuongDanAmThanh,
                        n.ChoPhepTTS,
                        n.ThoiLuongGiay
                    })
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("gan-day")]
    public async Task<ActionResult<IEnumerable<object>>> GetNearby(
        [FromQuery] decimal? vido,
        [FromQuery] decimal? kinhdo,
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lng)
    {
        var viDoThamChieu = lat ?? vido;
        var kinhDoThamChieu = lng ?? kinhdo;
        if (viDoThamChieu is null || kinhDoThamChieu is null)
        {
            return BadRequest(new { message = "Can cung cap toa do qua cặp vido/kinhdo hoac lat/lng." });
        }

        var items = await dbContext.DiemThamQuans
            .AsNoTracking()
            .Where(x => x.TrangThaiHoatDong)
            .Select(x => new
            {
                x.MaDiem,
                x.TenDiem,
                x.ViDo,
                x.KinhDo,
                x.BanKinhKichHoat,
                KhoangCachUocTinh = Math.Sqrt(
                    Math.Pow((double)(x.ViDo - viDoThamChieu.Value), 2) +
                    Math.Pow((double)(x.KinhDo - kinhDoThamChieu.Value), 2))
            })
            .OrderBy(x => x.KhoangCachUocTinh)
            .Take(10)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<DiemThamQuan>> Create(DiemThamQuanDto model)
    {
        var entity = new DiemThamQuan
        {
            MaDinhDanh = model.MaDinhDanh,
            TenDiem = model.TenDiem,
            MoTaNgan = model.MoTaNgan,
            ViDo = model.ViDo,
            KinhDo = model.KinhDo,
            BanKinhKichHoat = model.BanKinhKichHoat,
            DiaChi = model.DiaChi,
            MaLoai = model.MaLoai,
            MaTaiKhoanTao = model.MaTaiKhoanTao,
            MaTaiKhoanCapNhat = model.MaTaiKhoanCapNhat,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.DiemThamQuans.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaDiem }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Update(int id, DiemThamQuanDto model)
    {
        var item = await dbContext.DiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.MaDinhDanh = model.MaDinhDanh;
        item.TenDiem = model.TenDiem;
        item.MoTaNgan = model.MoTaNgan;
        item.ViDo = model.ViDo;
        item.KinhDo = model.KinhDo;
        item.BanKinhKichHoat = model.BanKinhKichHoat;
        item.DiaChi = model.DiaChi;
        item.MaLoai = model.MaLoai;
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
        var item = await dbContext.DiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.DiemThamQuans.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
