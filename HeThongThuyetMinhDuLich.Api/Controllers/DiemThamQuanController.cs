using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiemThamQuanController(
    DuLichDbContext dbContext,
    ILogger<DiemThamQuanController> logger) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách điểm tham quan có phân trang (paging)
    /// </summary>
    /// <param name="page">Trang hiện tại (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số lượng bản ghi mỗi trang (mặc định 20, tối đa 100)</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiemThamQuanDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 100 : (pageSize > 100 ? 100 : pageSize);
        var query = dbContext.DiemThamQuans
            .AsNoTracking()
            .OrderBy(x => x.TenDiem)
            .Select(x => new DiemThamQuanDto
            {
                MaDiem = x.MaDiem,
                AnhDaiDienUrl = x.HinhAnhDiemThamQuans
                    .OrderByDescending(h => h.LaAnhDaiDien)
                    .ThenBy(h => h.ThuTuHienThi ?? int.MaxValue)
                    .Select(h => h.DuongDanHinhAnh)
                    .FirstOrDefault(),
                MaDinhDanh = x.MaDinhDanh,
                TenDiem = x.TenDiem,
                MoTaNgan = x.MoTaNgan,
                ViDo = x.ViDo,
                KinhDo = x.KinhDo,
                BanKinhKichHoat = x.BanKinhKichHoat,
                DiaChi = x.DiaChi,
                MaLoai = x.MaLoai,
                TrangThaiHoatDong = x.TrangThaiHoatDong
            });
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(items);
    }

    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<object>>> GetUpdatedSince([FromQuery] DateTime? updatedSince)
    {
        var thresholdUtc = updatedSince?.ToUniversalTime() ?? DateTime.MinValue;

        var items = await dbContext.DiemThamQuans
            .AsNoTracking()
            .OrderBy(x => x.MaDiem)
            .Where(x => (x.NgayCapNhat ?? x.NgayTao) > thresholdUtc)
            .Select(x => new
            {
                x.MaDiem,
                AnhDaiDienUrl = x.HinhAnhDiemThamQuans
                    .OrderByDescending(h => h.LaAnhDaiDien)
                    .ThenBy(h => h.ThuTuHienThi ?? int.MaxValue)
                    .Select(h => h.DuongDanHinhAnh)
                    .FirstOrDefault(),
                x.MaDinhDanh,
                x.TenDiem,
                x.MoTaNgan,
                x.ViDo,
                x.KinhDo,
                x.BanKinhKichHoat,
                x.DiaChi,
                x.MaLoai,
                x.TrangThaiHoatDong,
                NgayCapNhat = x.NgayCapNhat ?? x.NgayTao
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
                    .OrderByDescending(h => h.LaAnhDaiDien)
                    .ThenBy(h => h.ThuTuHienThi ?? int.MaxValue)
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

    /// <summary>
    /// Lấy 10 điểm tham quan gần nhất vị trí (tối ưu bằng bounding box)
    /// </summary>
    [HttpGet("gan-day")]
    public async Task<ActionResult<IEnumerable<object>>> GetNearby(
        [FromQuery] decimal? vido,
        [FromQuery] decimal? kinhdo,
        [FromQuery] decimal? lat,
        [FromQuery] decimal? lng,
        [FromQuery] double radius = 0.1) // radius theo độ, mặc định ~11km
    {
        var viDoThamChieu = lat ?? vido;
        var kinhDoThamChieu = lng ?? kinhdo;
        if (viDoThamChieu is null || kinhDoThamChieu is null)
        {
            return BadRequest(new { message = "Can cung cap toa do qua cap vido/kinhdo hoac lat/lng." });
        }

        // Lọc sơ bộ theo bounding box trước khi tính khoảng cách
        var minLat = (double)viDoThamChieu.Value - radius;
        var maxLat = (double)viDoThamChieu.Value + radius;
        var minLng = (double)kinhDoThamChieu.Value - radius;
        var maxLng = (double)kinhDoThamChieu.Value + radius;

        var items = await dbContext.DiemThamQuans
            .AsNoTracking()
            .Where(x => x.TrangThaiHoatDong
                && (double)x.ViDo >= minLat && (double)x.ViDo <= maxLat
                && (double)x.KinhDo >= minLng && (double)x.KinhDo <= maxLng)
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
        var loaiTonTai = await dbContext.LoaiDiemThamQuans.AnyAsync(x => x.MaLoai == model.MaLoai);
        if (!loaiTonTai)
        {
            return BadRequest(new { message = "Ma loai khong hop le." });
        }

        var entity = new DiemThamQuan
        {
            MaDinhDanh = model.MaDinhDanh.Trim(),
            TenDiem = model.TenDiem.Trim(),
            MoTaNgan = model.MoTaNgan?.Trim(),
            ViDo = model.ViDo,
            KinhDo = model.KinhDo,
            BanKinhKichHoat = model.BanKinhKichHoat,
            DiaChi = model.DiaChi?.Trim(),
            MaLoai = model.MaLoai,
            MaTaiKhoanTao = model.MaTaiKhoanTao,
            MaTaiKhoanCapNhat = model.MaTaiKhoanCapNhat,
            TrangThaiHoatDong = model.TrangThaiHoatDong,
            NgayTao = DateTime.UtcNow
        };

        dbContext.DiemThamQuans.Add(entity);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("DiemThamQuan created: {MaDiem} - {TenDiem}", entity.MaDiem, entity.TenDiem);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "Ma dinh danh da ton tai." });
        }

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

        var loaiTonTai = await dbContext.LoaiDiemThamQuans.AnyAsync(x => x.MaLoai == model.MaLoai);
        if (!loaiTonTai)
        {
            return BadRequest(new { message = "Ma loai khong hop le." });
        }

        item.MaDinhDanh = model.MaDinhDanh.Trim();
        item.TenDiem = model.TenDiem.Trim();
        item.MoTaNgan = model.MoTaNgan?.Trim();
        item.ViDo = model.ViDo;
        item.KinhDo = model.KinhDo;
        item.BanKinhKichHoat = model.BanKinhKichHoat;
        item.DiaChi = model.DiaChi?.Trim();
        item.MaLoai = model.MaLoai;
        item.MaTaiKhoanCapNhat = model.MaTaiKhoanCapNhat;
        item.TrangThaiHoatDong = model.TrangThaiHoatDong;
        item.NgayCapNhat = DateTime.UtcNow;

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("DiemThamQuan updated: {MaDiem} - {TenDiem}", item.MaDiem, item.TenDiem);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new { message = "Ma dinh danh da ton tai." });
        }

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

        if (!item.TrangThaiHoatDong)
        {
            return Conflict(new { message = "Diem tham quan da o trang thai tam dung." });
        }

        item.TrangThaiHoatDong = false;
        item.NgayCapNhat = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        logger.LogInformation("DiemThamQuan soft-deleted (inactive): {MaDiem}", id);
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
