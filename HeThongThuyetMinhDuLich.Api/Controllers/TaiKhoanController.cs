using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaiKhoanController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await dbContext.TaiKhoans
            .AsNoTracking()
            .OrderBy(x => x.TenDangNhap)
            .Select(x => new
            {
                x.MaTaiKhoan,
                x.TenDangNhap,
                x.HoTen,
                x.Email,
                x.VaiTro,
                x.TrangThaiHoatDong,
                x.NgayTao,
                x.NgayCapNhat
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var item = await dbContext.TaiKhoans
            .AsNoTracking()
            .Where(x => x.MaTaiKhoan == id)
            .Select(x => new
            {
                x.MaTaiKhoan,
                x.TenDangNhap,
                x.HoTen,
                x.Email,
                x.VaiTro,
                x.TrangThaiHoatDong,
                x.NgayTao,
                x.NgayCapNhat
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(TaiKhoanDto model)
    {
        if (string.IsNullOrWhiteSpace(model.MatKhau))
        {
            return BadRequest(new { message = "Mat khau khong duoc de trong." });
        }

        var entity = new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap,
            MatKhauMaHoa = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
            HoTen = model.HoTen,
            Email = model.Email,
            VaiTro = model.VaiTro,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.TaiKhoans.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaTaiKhoan }, new
        {
            entity.MaTaiKhoan,
            entity.TenDangNhap,
            entity.HoTen,
            entity.Email,
            entity.VaiTro
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaiKhoanDto model)
    {
        var item = await dbContext.TaiKhoans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.TenDangNhap = model.TenDangNhap;
        item.HoTen = model.HoTen;
        item.Email = model.Email;
        item.VaiTro = model.VaiTro;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.MatKhau))
        {
            item.MatKhauMaHoa = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
        }

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.TaiKhoans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.TaiKhoans.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
