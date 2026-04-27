using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LichSuPhatController(DuLichDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.NguoiDung)
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .OrderByDescending(x => x.ThoiGianBatDau)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaNguoiDung,
                TenNguoiDung = x.NguoiDung != null ? x.NguoiDung.HoTen : null,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.DeviceId,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<object>> GetById(long id)
    {
        var item = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.NguoiDung)
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .Where(x => x.MaLichSuPhat == id)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaNguoiDung,
                TenNguoiDung = x.NguoiDung != null ? x.NguoiDung.HoTen : null,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.DeviceId,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("nguoidung/{maNguoiDung:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByNguoiDung(int maNguoiDung)
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .Include(x => x.DiemThamQuan)
            .Include(x => x.NoiDungThuyetMinh)
            .Where(x => x.MaNguoiDung == maNguoiDung)
            .OrderByDescending(x => x.ThoiGianBatDau)
            .Select(x => new
            {
                x.MaLichSuPhat,
                x.MaDiem,
                TenDiem = x.DiemThamQuan != null ? x.DiemThamQuan.TenDiem : null,
                x.MaNoiDung,
                TieuDeNoiDung = x.NoiDungThuyetMinh != null ? x.NoiDungThuyetMinh.TieuDe : null,
                x.CachKichHoat,
                x.ThoiGianBatDau,
                x.ThoiLuongDaNghe
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("thong-ke/luot-nghe-theo-diem")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<IEnumerable<object>>> ThongKeTheoDiem()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .GroupBy(x => new { x.MaDiem, x.DiemThamQuan!.TenDiem })
            .Select(g => new
            {
                g.Key.MaDiem,
                g.Key.TenDiem,
                SoLuotNghe = g.Count(),
                TongThoiLuongDaNghe = g.Sum(x => x.ThoiLuongDaNghe ?? 0)
            })
            .OrderByDescending(x => x.SoLuotNghe)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("thong-ke/luot-nghe-theo-kich-hoat")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<IEnumerable<object>>> ThongKeTheoCachKichHoat()
    {
        var items = await dbContext.LichSuPhats
            .AsNoTracking()
            .GroupBy(x => x.CachKichHoat)
            .Select(g => new
            {
                CachKichHoat = g.Key,
                SoLuotPhat = g.Count(),
                TongThoiLuongDaNghe = g.Sum(x => x.ThoiLuongDaNghe ?? 0)
            })
            .OrderByDescending(x => x.SoLuotPhat)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("thong-ke/nguoi-dung-dang-hoat-dong")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<ActionResult<object>> ThongKeNguoiDungDangHoatDong([FromQuery] int withinMinutes = 15)
    {
        var normalizedMinutes = Math.Clamp(withinMinutes, 1, 24 * 60);
        var thresholdUtc = DateTime.UtcNow.AddMinutes(-normalizedMinutes);

        var recentRows = await dbContext.LichSuPhats
            .AsNoTracking()
            .Where(x => (x.LastSeen ?? x.ThoiGianBatDau) >= thresholdUtc)
            .Select(g => new
            {
                g.MaLichSuPhat,
                g.MaNguoiDung,
                g.DeviceId,
                g.SessionId,
                g.IpAddress,
                LanHoatDong = g.LastSeen ?? g.ThoiGianBatDau
            })
            .ToListAsync();

        var authenticatedDeviceIds = recentRows
            .Where(x => x.MaNguoiDung.HasValue && !string.IsNullOrWhiteSpace(x.DeviceId))
            .Select(x => x.DeviceId!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var userGroups = recentRows
            .Where(x => x.MaNguoiDung.HasValue)
            .GroupBy(x => x.MaNguoiDung!.Value)
            .Select(g => new
            {
                LoaiDinhDanh = "user",
                DinhDanh = $"user:{g.Key}",
                MaNguoiDung = (int?)g.Key,
                DeviceId = g
                    .Where(x => !string.IsNullOrWhiteSpace(x.DeviceId))
                    .Select(x => x.DeviceId!.Trim())
                    .FirstOrDefault(),
                LanHoatDongGanNhat = g.Max(x => x.LanHoatDong),
                SoLuotNghe = g.Count()
            });

        var guestRows = recentRows
            .Where(x => !x.MaNguoiDung.HasValue)
            .Where(x =>
                string.IsNullOrWhiteSpace(x.DeviceId) ||
                !authenticatedDeviceIds.Contains(x.DeviceId.Trim()));

        var deviceGroups = guestRows
            .GroupBy(x =>
            {
                var deviceId = x.DeviceId?.Trim();
                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    return $"device:{deviceId}";
                }

                var sessionId = x.SessionId?.Trim();
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    return $"session:{sessionId}";
                }

                var ipAddress = x.IpAddress?.Trim();
                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    return $"ip:{ipAddress}";
                }

                return $"history:{x.MaLichSuPhat}";
            })
            .Select(g => new
            {
                LoaiDinhDanh = "device",
                DinhDanh = g.Key,
                MaNguoiDung = (int?)null,
                DeviceId = g
                    .Where(x => !string.IsNullOrWhiteSpace(x.DeviceId))
                    .Select(x => x.DeviceId!.Trim())
                    .FirstOrDefault(),
                LanHoatDongGanNhat = g.Max(x => x.LanHoatDong),
                SoLuotNghe = g.Count()
            });

        var activeRows = userGroups
            .Concat(deviceGroups)
            .OrderByDescending(x => x.LanHoatDongGanNhat)
            .ToList();

        var activeUserCount = activeRows.Count(x => string.Equals(x.LoaiDinhDanh, "user", StringComparison.OrdinalIgnoreCase));
        var activeDeviceCount = activeRows.Count(x => string.Equals(x.LoaiDinhDanh, "device", StringComparison.OrdinalIgnoreCase));

        return Ok(new
        {
            WithinMinutes = normalizedMinutes,
            MocThoiGianUtc = thresholdUtc,
            SoNguoiDungDangHoatDong = activeRows.Count,
            SoDoiTuongDangHoatDong = activeRows.Count,
            SoNguoiDungDaDangNhapDangHoatDong = activeUserCount,
            SoThietBiKhachDangHoatDong = activeDeviceCount,
            TongLuotNgheTrongKhoang = activeRows.Sum(x => x.SoLuotNghe),
            DanhSach = activeRows
        });
    }

    [HttpPost]
    public async Task<ActionResult<LichSuPhat>> Create(LichSuPhatDto model)
    {
        var deviceId = string.IsNullOrWhiteSpace(model.DeviceId) ? null : model.DeviceId.Trim();
        var sessionId = string.IsNullOrWhiteSpace(model.SessionId) ? null : model.SessionId.Trim();

        var entity = new LichSuPhat
        {
            MaNguoiDung = model.MaNguoiDung,
            MaDiem = model.MaDiem,
            MaNoiDung = model.MaNoiDung,
            CachKichHoat = model.CachKichHoat,
            ThoiGianBatDau = model.ThoiGianBatDau ?? DateTime.UtcNow,
            ThoiLuongDaNghe = model.ThoiLuongDaNghe,
            DeviceId = deviceId,
            SessionId = sessionId,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            LastSeen = model.LastSeen ?? DateTime.UtcNow
        };

        dbContext.LichSuPhats.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.MaLichSuPhat }, entity);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, LichSuPhatDto model)
    {
        var item = await dbContext.LichSuPhats.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var deviceId = string.IsNullOrWhiteSpace(model.DeviceId) ? item.DeviceId : model.DeviceId.Trim();
        var sessionId = string.IsNullOrWhiteSpace(model.SessionId) ? item.SessionId : model.SessionId.Trim();

        item.MaNguoiDung = model.MaNguoiDung;
        item.MaDiem = model.MaDiem;
        item.MaNoiDung = model.MaNoiDung;
        item.CachKichHoat = model.CachKichHoat;
        item.ThoiGianBatDau = model.ThoiGianBatDau ?? item.ThoiGianBatDau;
        item.ThoiLuongDaNghe = model.ThoiLuongDaNghe;
        item.DeviceId = deviceId;
        item.SessionId = sessionId;
        item.IpAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        item.LastSeen = model.LastSeen ?? DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await dbContext.LichSuPhats.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.LichSuPhats.Remove(item);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
