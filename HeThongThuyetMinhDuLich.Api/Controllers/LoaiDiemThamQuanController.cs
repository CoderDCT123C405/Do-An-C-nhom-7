using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoaiDiemThamQuanController(DuLichDbContext dbContext) : ControllerBase
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
        var entity = new LoaiDiemThamQuan
        {
            TenLoai = model.TenLoai,
            MoTa = model.MoTa,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.LoaiDiemThamQuans.Add(entity);
        await dbContext.SaveChangesAsync();

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

        item.TenLoai = model.TenLoai;
        item.MoTa = model.MoTa;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
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

        dbContext.LoaiDiemThamQuans.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
