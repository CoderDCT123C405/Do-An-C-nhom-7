using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NguoiDungController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await dbContext.NguoiDungs
            .AsNoTracking()
            .Include(x => x.NgonNguMacDinh)
            .OrderBy(x => x.HoTen)
            .Select(x => new
            {
                x.MaNguoiDung,
                x.TenDangNhap,
                x.HoTen,
                x.Email,
                x.SoDienThoai,
                x.MaNgonNguMacDinh,
                TenNgonNguMacDinh = x.NgonNguMacDinh != null ? x.NgonNguMacDinh.TenNgonNgu : null,
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
        var item = await dbContext.NguoiDungs
            .AsNoTracking()
            .Include(x => x.NgonNguMacDinh)
            .Where(x => x.MaNguoiDung == id)
            .Select(x => new
            {
                x.MaNguoiDung,
                x.TenDangNhap,
                x.HoTen,
                x.Email,
                x.SoDienThoai,
                x.MaNgonNguMacDinh,
                TenNgonNguMacDinh = x.NgonNguMacDinh != null ? x.NgonNguMacDinh.TenNgonNgu : null,
                x.TrangThaiHoatDong,
                x.NgayTao,
                x.NgayCapNhat
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<NguoiDung>> Create(NguoiDungDto model)
    {
        var entity = new NguoiDung
        {
            TenDangNhap = model.TenDangNhap,
            MatKhauMaHoa = string.IsNullOrWhiteSpace(model.MatKhau) ? null : BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
            HoTen = model.HoTen,
            Email = model.Email,
            SoDienThoai = model.SoDienThoai,
            MaNgonNguMacDinh = model.MaNgonNguMacDinh,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.NguoiDungs.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaNguoiDung }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, NguoiDungDto model)
    {
        var item = await dbContext.NguoiDungs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.TenDangNhap = model.TenDangNhap;
        item.HoTen = model.HoTen;
        item.Email = model.Email;
        item.SoDienThoai = model.SoDienThoai;
        item.MaNgonNguMacDinh = model.MaNgonNguMacDinh;
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
        var item = await dbContext.NguoiDungs.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.NguoiDungs.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
