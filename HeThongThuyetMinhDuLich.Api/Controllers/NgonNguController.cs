using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using HeThongThuyetMinhDuLich.Api.Services;
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
        var items = (await dbContext.NgonNgus
                .AsNoTracking()
                .ToListAsync())
            .OrderByDescending(x => x.LaMacDinh)
            .ThenBy(x => LanguageCatalog.GetSortOrder(x.MaNgonNguQuocTe))
            .ThenBy(x => x.TenNgonNgu)
            .ToList();

        return Ok(items);
    }

    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<object>>> GetUpdatedSince([FromQuery] DateTime? updatedSince)
    {
        var thresholdUtc = updatedSince?.ToUniversalTime() ?? DateTime.MinValue;

        var items = (await dbContext.NgonNgus
                .AsNoTracking()
                .Where(x => (x.NgayCapNhat ?? x.NgayTao) > thresholdUtc)
                .Select(x => new
                {
                    x.MaNgonNgu,
                    x.MaNgonNguQuocTe,
                    x.TenNgonNgu,
                    x.LaMacDinh,
                    x.TrangThaiHoatDong,
                    NgayCapNhat = x.NgayCapNhat ?? x.NgayTao
                })
                .ToListAsync())
            .OrderByDescending(x => x.LaMacDinh)
            .ThenBy(x => LanguageCatalog.GetSortOrder(x.MaNgonNguQuocTe))
            .ThenBy(x => x.TenNgonNgu)
            .ToList();

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
        var normalizedIsoCode = LanguageCatalog.NormalizeIsoCode(model.MaNgonNguQuocTe);
        var normalizedDisplayName = LanguageCatalog.NormalizeDisplayName(normalizedIsoCode, model.TenNgonNgu);

        if (model.LaMacDinh)
        {
            await ClearCurrentDefaultLanguageAsync();
        }

        var entity = new NgonNgu
        {
            MaNgonNguQuocTe = normalizedIsoCode,
            TenNgonNgu = normalizedDisplayName,
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

        var normalizedIsoCode = LanguageCatalog.NormalizeIsoCode(model.MaNgonNguQuocTe);
        var normalizedDisplayName = LanguageCatalog.NormalizeDisplayName(normalizedIsoCode, model.TenNgonNgu);

        if (model.LaMacDinh && !item.LaMacDinh)
        {
            await ClearCurrentDefaultLanguageAsync(id);
        }

        item.MaNgonNguQuocTe = normalizedIsoCode;
        item.TenNgonNgu = normalizedDisplayName;
        item.LaMacDinh = model.LaMacDinh;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task ClearCurrentDefaultLanguageAsync(int? excludeId = null)
    {
        var defaults = await dbContext.NgonNgus
            .Where(x => x.LaMacDinh && (!excludeId.HasValue || x.MaNgonNgu != excludeId.Value))
            .ToListAsync();

        foreach (var language in defaults)
        {
            language.LaMacDinh = false;
            language.NgayCapNhat = DateTime.UtcNow;
        }
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
