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
public class NoiDungThuyetMinhController(
    DuLichDbContext dbContext,
    EdgeTtsService edgeTtsService) : ControllerBase
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
        await TryAutoGenerateAudioAsync(entity);

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
        await TryAutoGenerateAudioAsync(item);
        return NoContent();
    }

    [HttpPost("{id:int}/generate-audio")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<object>> GenerateAudio(int id, CancellationToken cancellationToken)
    {
        var item = await dbContext.NoiDungThuyetMinhs.FindAsync([id], cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!edgeTtsService.IsConfigured)
        {
            return BadRequest(new { message = "edge-tts chưa được cấu hình. Hãy kiểm tra EdgeTts:Executable." });
        }

        var oldPath = item.DuongDanAmThanh;
        item.DuongDanAmThanh = await edgeTtsService.GenerateAudioAsync(item, cancellationToken);
        item.NgayCapNhat = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldPath, item.DuongDanAmThanh, StringComparison.OrdinalIgnoreCase))
        {
            edgeTtsService.DeleteManagedAudio(oldPath);
        }

        return Ok(new
        {
            item.MaNoiDung,
            item.TieuDe,
            item.DuongDanAmThanh
        });
    }

    [HttpPost("generate-audio")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<object>> GenerateAudioBatch([FromQuery] bool overwrite = false, CancellationToken cancellationToken = default)
    {
        if (!edgeTtsService.IsConfigured)
        {
            return BadRequest(new { message = "edge-tts chưa được cấu hình. Hãy kiểm tra EdgeTts:Executable." });
        }

        var items = await dbContext.NoiDungThuyetMinhs
            .Where(x => x.TrangThaiHoatDong && !string.IsNullOrWhiteSpace(x.NoiDungVanBan))
            .OrderBy(x => x.MaNoiDung)
            .ToListAsync(cancellationToken);

        var generated = 0;
        foreach (var item in items)
        {
            if (!overwrite && !string.IsNullOrWhiteSpace(item.DuongDanAmThanh))
            {
                continue;
            }

            var oldPath = item.DuongDanAmThanh;
            item.DuongDanAmThanh = await edgeTtsService.GenerateAudioAsync(item, cancellationToken);
            item.NgayCapNhat = DateTime.UtcNow;
            generated++;

            if (overwrite && !string.Equals(oldPath, item.DuongDanAmThanh, StringComparison.OrdinalIgnoreCase))
            {
                edgeTtsService.DeleteManagedAudio(oldPath);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new
        {
            TongNoiDung = items.Count,
            DaSinhAudio = generated
        });
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

        edgeTtsService.DeleteManagedAudio(item.DuongDanAmThanh);
        dbContext.NoiDungThuyetMinhs.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task TryAutoGenerateAudioAsync(NoiDungThuyetMinh item)
    {
        if (!edgeTtsService.IsConfigured ||
            !string.IsNullOrWhiteSpace(item.DuongDanAmThanh) ||
            string.IsNullOrWhiteSpace(item.NoiDungVanBan))
        {
            return;
        }

        var settings = HttpContext?.RequestServices.GetService<IConfiguration>();
        if (!bool.TryParse(settings?["EdgeTts:AutoGenerateOnSave"], out var autoGenerate) || !autoGenerate)
        {
            return;
        }

        item.DuongDanAmThanh = await edgeTtsService.GenerateAudioAsync(item);
        item.NgayCapNhat = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }
}
