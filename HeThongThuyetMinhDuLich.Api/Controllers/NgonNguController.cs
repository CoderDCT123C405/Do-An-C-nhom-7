using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NgonNguController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NgonNgu>>> GetAll()
    {
        var items = await dbContext.NgonNgus
            .AsNoTracking()
            .OrderBy(x => x.TenNgonNgu)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<object>>> GetUpdatedSince([FromQuery] DateTime? updatedSince)
    {
        var thresholdUtc = updatedSince?.ToUniversalTime() ?? DateTime.MinValue;

        var items = await dbContext.NgonNgus
            .AsNoTracking()
            .Where(x => (x.NgayCapNhat ?? x.NgayTao) > thresholdUtc)
            .OrderBy(x => x.MaNgonNgu)
            .Select(x => new
            {
                x.MaNgonNgu,
                x.MaNgonNguQuocTe,
                x.TenNgonNgu,
                x.LaMacDinh,
                x.TrangThaiHoatDong,
                NgayCapNhat = x.NgayCapNhat ?? x.NgayTao
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NgonNgu>> GetById(int id)
    {
        var item = await dbContext.NgonNgus.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NgonNgu>> Create(NgonNguDto model)
    {
        var entity = new NgonNgu
        {
            MaNgonNguQuocTe = model.MaNgonNguQuocTe,
            TenNgonNgu = model.TenNgonNgu,
            LaMacDinh = model.LaMacDinh,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        dbContext.NgonNgus.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaNgonNgu }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, NgonNguDto model)
    {
        var item = await dbContext.NgonNgus.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.MaNgonNguQuocTe = model.MaNgonNguQuocTe;
        item.TenNgonNgu = model.TenNgonNgu;
        item.LaMacDinh = model.LaMacDinh;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.NgonNgus.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.NgonNgus.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
