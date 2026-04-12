using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models.Dtos;
using HeThongThuyetMinhDuLich.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/noidung")]
public class NoiDungController(DuLichDbContext dbContext, AudioPathResolver audioPathResolver) : ControllerBase
{
    [HttpGet("{maDiem:int}/fallback")]
    public async Task<ActionResult<NoiDungFallbackResponse>> GetByMaDiemWithFallback(
        int maDiem,
        [FromQuery] int? maNgonNguUuTien,
        [FromQuery] int[]? fallbackNgonNgu)
    {
        var rows = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.MaDiem == maDiem && x.TrangThaiHoatDong)
            .Include(x => x.NgonNgu)
            .OrderBy(x => x.MaNgonNgu)
            .ToListAsync();

        if (rows.Count == 0)
        {
            return NotFound(new { message = "Khong tim thay noi dung cho diem tham quan nay." });
        }

        var availableLanguages = rows
            .Where(x => x.NgonNgu is not null)
            .Select(x => x.NgonNgu!)
            .GroupBy(x => x.MaNgonNgu)
            .Select(x => x.First())
            .OrderByDescending(x => x.LaMacDinh)
            .ThenBy(x => LanguageCatalog.GetSortOrder(x.MaNgonNguQuocTe))
            .ThenBy(x => x.TenNgonNgu)
            .ToList();

        var resolvedLanguageId = ResolveFallbackLanguageId(availableLanguages, maNgonNguUuTien, fallbackNgonNgu);
        var requestedLanguage = maNgonNguUuTien.HasValue
            ? await dbContext.NgonNgus.AsNoTracking().FirstOrDefaultAsync(x => x.MaNgonNgu == maNgonNguUuTien.Value)
            : null;
        var resolvedLanguage = availableLanguages.FirstOrDefault(x => x.MaNgonNgu == resolvedLanguageId);

        var contents = rows
            .Where(x => !resolvedLanguageId.HasValue || x.MaNgonNgu == resolvedLanguageId.Value)
            .Select(x => new NoiDungFallbackContentItem
            {
                MaNoiDung = x.MaNoiDung,
                MaDiem = x.MaDiem,
                MaNgonNgu = x.MaNgonNgu,
                TenNgonNgu = x.NgonNgu?.TenNgonNgu,
                TieuDe = x.TieuDe,
                NoiDungVanBan = x.NoiDungVanBan,
                DuongDanAmThanh = audioPathResolver.ResolveNoiDungAudioPath(x.MaNoiDung, x.DuongDanAmThanh),
                ChoPhepTTS = x.ChoPhepTTS,
                ThoiLuongGiay = x.ThoiLuongGiay,
                TrangThaiHoatDong = x.TrangThaiHoatDong
            })
            .ToList();

        var response = new NoiDungFallbackResponse
        {
            MaDiem = maDiem,
            MaNgonNguYeuCau = maNgonNguUuTien,
            MaNgonNguThucTe = resolvedLanguageId,
            NgonNguYeuCau = requestedLanguage is null ? null : MapLanguage(requestedLanguage),
            NgonNguThucTe = resolvedLanguage is null ? null : MapLanguage(resolvedLanguage),
            NgonNguKhaDung = availableLanguages.Select(MapLanguage).ToList(),
            NgonNguThayThe = availableLanguages
                .Where(x => !resolvedLanguageId.HasValue || x.MaNgonNgu != resolvedLanguageId.Value)
                .Select(MapLanguage)
                .ToList(),
            NoiDung = contents
        };

        return Ok(response);
    }

    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<object>>> GetUpdatedSince([FromQuery] DateTime? updatedSince, [FromQuery] int? maDiem)
    {
        var thresholdUtc = updatedSince?.ToUniversalTime() ?? DateTime.MinValue;

        var query = dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => (x.NgayCapNhat ?? x.NgayTao) > thresholdUtc);

        if (maDiem.HasValue)
        {
            query = query.Where(x => x.MaDiem == maDiem.Value);
        }

        var items = await query
            .Include(x => x.NgonNgu)
            .OrderBy(x => x.MaDiem)
            .ThenBy(x => x.MaNgonNgu)
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
                x.TrangThaiHoatDong,
                NgayCapNhat = x.NgayCapNhat ?? x.NgayTao
            })
            .ToListAsync();

        var result = items.Select(x => new
        {
            x.MaNoiDung,
            x.MaDiem,
            x.MaNgonNgu,
            x.TenNgonNgu,
            x.TieuDe,
            x.NoiDungVanBan,
            DuongDanAmThanh = audioPathResolver.ResolveNoiDungAudioPath(x.MaNoiDung, x.DuongDanAmThanh),
            x.ChoPhepTTS,
            x.ThoiLuongGiay,
            x.TrangThaiHoatDong,
            x.NgayCapNhat
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{maDiem:int}")]
    public async Task<ActionResult<object>> GetByMaDiem(int maDiem)
    {
        var items = await dbContext.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.MaDiem == maDiem && x.TrangThaiHoatDong)
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

        var result = items.Select(x => new
        {
            x.MaNoiDung,
            x.MaDiem,
            x.MaNgonNgu,
            x.TenNgonNgu,
            x.TieuDe,
            x.NoiDungVanBan,
            DuongDanAmThanh = audioPathResolver.ResolveNoiDungAudioPath(x.MaNoiDung, x.DuongDanAmThanh),
            x.ChoPhepTTS,
            x.ThoiLuongGiay,
            x.TrangThaiHoatDong
        }).ToList();

        if (result.Count == 0)
        {
            return NotFound(new { message = "Khong tim thay noi dung cho diem tham quan nay." });
        }

        return Ok(new
        {
            MaDiem = maDiem,
            NoiDung = result
        });
    }

    private static int? ResolveFallbackLanguageId(
        IReadOnlyCollection<HeThongThuyetMinhDuLich.Api.Models.NgonNgu> availableLanguages,
        int? requestedLanguageId,
        IReadOnlyCollection<int>? fallbackLanguageIds)
    {
        if (availableLanguages.Count == 0)
        {
            return null;
        }

        var availableLanguageIds = availableLanguages.Select(x => x.MaNgonNgu).ToHashSet();

        if (requestedLanguageId.HasValue && availableLanguageIds.Contains(requestedLanguageId.Value))
        {
            return requestedLanguageId.Value;
        }

        if (fallbackLanguageIds is not null)
        {
            foreach (var fallbackLanguageId in fallbackLanguageIds)
            {
                if (availableLanguageIds.Contains(fallbackLanguageId))
                {
                    return fallbackLanguageId;
                }
            }
        }

        var defaultLanguageId = availableLanguages.FirstOrDefault(x => x.LaMacDinh)?.MaNgonNgu;
        if (defaultLanguageId.HasValue)
        {
            return defaultLanguageId.Value;
        }

        var vietnameseLanguageId = availableLanguages
            .FirstOrDefault(x => string.Equals(x.MaNgonNguQuocTe, "vi", StringComparison.OrdinalIgnoreCase))
            ?.MaNgonNgu;
        if (vietnameseLanguageId.HasValue)
        {
            return vietnameseLanguageId.Value;
        }

        return availableLanguages.First().MaNgonNgu;
    }

    private static NgonNguFallbackItem MapLanguage(HeThongThuyetMinhDuLich.Api.Models.NgonNgu language)
    {
        return new NgonNguFallbackItem
        {
            MaNgonNgu = language.MaNgonNgu,
            MaNgonNguQuocTe = LanguageCatalog.NormalizeIsoCode(language.MaNgonNguQuocTe),
            TenNgonNgu = LanguageCatalog.NormalizeDisplayName(language.MaNgonNguQuocTe, language.TenNgonNgu),
            LaMacDinh = language.LaMacDinh
        };
    }
}
