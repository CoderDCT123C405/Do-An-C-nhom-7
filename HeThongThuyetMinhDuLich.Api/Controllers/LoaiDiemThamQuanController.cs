using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoaiDiemThamQuanController(DuLichDbContext dbContext, ILogger<LoaiDiemThamQuanController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoaiDiemThamQuan>>> GetAll()
    {
        var items = await dbContext.LoaiDiemThamQuans
            .OrderBy(x => x.TenLoai)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoaiDiemThamQuan>> GetById(int id)
    {
        var item = await dbContext.LoaiDiemThamQuans.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<LoaiDiemThamQuan>> Create(LoaiDiemThamQuanDto model)
    {
        var tenLoai = model.TenLoai.Trim();

        var entity = new LoaiDiemThamQuan
        {
            TenLoai = tenLoai,
            MoTa = model.MoTa?.Trim(),
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.LoaiDiemThamQuans.Add(entity);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("LoaiDiem created: {MaLoai} - {TenLoai}", entity.MaLoai, entity.TenLoai);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "Ten loai da ton tai." });
        }

        return CreatedAtAction(nameof(GetById), new { id = entity.MaLoai }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Update(int id, LoaiDiemThamQuanDto model)
    {
        var item = await dbContext.LoaiDiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.TenLoai = model.TenLoai.Trim();
        item.MoTa = model.MoTa?.Trim();
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("LoaiDiem updated: {MaLoai} - {TenLoai}", item.MaLoai, item.TenLoai);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "Ten loai da ton tai." });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.LoaiDiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (!item.TrangThaiHoatDong)
        {
            return Conflict(new { message = "Loai diem da o trang thai tam dung." });
        }

        var dangDuocSuDung = await dbContext.DiemThamQuans.AnyAsync(x => x.MaLoai == id && x.TrangThaiHoatDong);
        if (dangDuocSuDung)
        {
            return Conflict(new { message = "Khong the an loai diem khi van con diem tham quan dang hoat dong." });
        }

        item.TrangThaiHoatDong = false;
        item.NgayCapNhat = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("LoaiDiem soft-deleted (inactive): {MaLoai}", id);
        return NoContent();
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || message.Contains("2601", StringComparison.OrdinalIgnoreCase)
            || message.Contains("2627", StringComparison.OrdinalIgnoreCase);
    }
}
