using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.HinhAnh;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HinhAnhDiemThamQuanController(DuLichDbContext dbContext, IWebHostEnvironment environment) : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const string PoiImageRequestPrefix = "/images/poi/";

    [HttpGet("diem/{maDiem:int}")]
    public async Task<ActionResult<IEnumerable<object>>> GetByDiem(int maDiem)
    {
        var items = await dbContext.HinhAnhDiemThamQuans
            .AsNoTracking()
            .Where(x => x.MaDiem == maDiem)
            .OrderByDescending(x => x.LaAnhDaiDien)
            .ThenBy(x => x.ThuTuHienThi)
            .Select(x => new
            {
                x.MaHinhAnh,
                x.MaDiem,
                x.TenTepTin,
                x.DuongDanHinhAnh,
                x.LaAnhDaiDien,
                x.ThuTuHienThi,
                x.NgayTaiLen
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,BienTap")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] HinhAnhUploadRequest request)
    {
        var diem = await dbContext.DiemThamQuans.FindAsync(request.MaDiem);
        if (diem is null)
        {
            return NotFound(new { message = "Khong tim thay diem tham quan." });
        }

        if (request.TepTin is null || request.TepTin.Length == 0)
        {
            return BadRequest(new { message = "Tep tin khong hop le." });
        }

        var extension = Path.GetExtension(request.TepTin.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Chi ho tro file jpg, jpeg, png, webp." });
        }

        var poiFolderName = SanitizePathSegment(diem.MaDinhDanh);
        if (string.IsNullOrWhiteSpace(poiFolderName))
        {
            poiFolderName = $"POI{request.MaDiem}";
        }

        var uploadFolder = Path.Combine(GetSharedPoiImageRootPath(), poiFolderName);
        Directory.CreateDirectory(uploadFolder);

        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadFolder, safeFileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await request.TepTin.CopyToAsync(stream);
        }

        if (request.LaAnhDaiDien)
        {
            var oldRepresentativeImages = await dbContext.HinhAnhDiemThamQuans
                .Where(x => x.MaDiem == request.MaDiem && x.LaAnhDaiDien)
                .ToListAsync();

            foreach (var image in oldRepresentativeImages)
            {
                image.LaAnhDaiDien = false;
            }
        }

        var duongDan = $"{PoiImageRequestPrefix}{poiFolderName}/{safeFileName}";
        var entity = new HinhAnhDiemThamQuan
        {
            MaDiem = request.MaDiem,
            TenTepTin = request.TepTin.FileName,
            DuongDanHinhAnh = duongDan,
            LaAnhDaiDien = request.LaAnhDaiDien,
            ThuTuHienThi = request.ThuTuHienThi,
            NgayTaiLen = DateTime.UtcNow,
            MaTaiKhoanTao = request.MaTaiKhoanTao
        };

        dbContext.HinhAnhDiemThamQuans.Add(entity);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByDiem), new { maDiem = entity.MaDiem }, new
        {
            entity.MaHinhAnh,
            entity.MaDiem,
            entity.TenTepTin,
            entity.DuongDanHinhAnh,
            entity.LaAnhDaiDien,
            entity.ThuTuHienThi
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Update(int id, HinhAnhDiemThamQuanUpdateRequest request)
    {
        var item = await dbContext.HinhAnhDiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (request.LaAnhDaiDien)
        {
            var siblingImages = await dbContext.HinhAnhDiemThamQuans
                .Where(x => x.MaDiem == item.MaDiem && x.MaHinhAnh != item.MaHinhAnh && x.LaAnhDaiDien)
                .ToListAsync();

            foreach (var sibling in siblingImages)
            {
                sibling.LaAnhDaiDien = false;
            }
        }

        item.LaAnhDaiDien = request.LaAnhDaiDien;
        item.ThuTuHienThi = request.ThuTuHienThi;

        await EnsureRepresentativeImageAsync(item.MaDiem, preferredImageId: request.LaAnhDaiDien ? item.MaHinhAnh : null);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,BienTap")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await dbContext.HinhAnhDiemThamQuans.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (TryResolveImagePath(item.DuongDanHinhAnh, out var fullPath))
        {
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        dbContext.HinhAnhDiemThamQuans.Remove(item);
        await dbContext.SaveChangesAsync();

        await EnsureRepresentativeImageAsync(item.MaDiem);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task EnsureRepresentativeImageAsync(int maDiem, int? preferredImageId = null)
    {
        var images = await dbContext.HinhAnhDiemThamQuans
            .Where(x => x.MaDiem == maDiem)
            .OrderByDescending(x => x.LaAnhDaiDien)
            .ThenBy(x => x.ThuTuHienThi ?? int.MaxValue)
            .ThenBy(x => x.MaHinhAnh)
            .ToListAsync();

        if (images.Count == 0)
        {
            return;
        }

        HinhAnhDiemThamQuan representative = images.First();
        if (preferredImageId.HasValue)
        {
            representative = images.FirstOrDefault(x => x.MaHinhAnh == preferredImageId.Value) ?? representative;
        }
        else if (!images.Any(x => x.LaAnhDaiDien))
        {
            representative = images.First();
        }
        else
        {
            representative = images.First(x => x.LaAnhDaiDien);
        }

        foreach (var image in images)
        {
            image.LaAnhDaiDien = image.MaHinhAnh == representative.MaHinhAnh;
        }
    }

    private string GetSharedPoiImageRootPath()
    {
        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "wwwroot", "images", "poi"));
    }

    private string GetApiWebRootPath()
    {
        var rootPath = environment.WebRootPath;
        if (!string.IsNullOrWhiteSpace(rootPath))
        {
            return rootPath;
        }

        return Path.Combine(environment.ContentRootPath, "wwwroot");
    }

    private bool TryResolveImagePath(string? imagePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return false;
        }

        var normalizedRelativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var comparePath = imagePath.Replace('\\', '/');

        if (comparePath.StartsWith(PoiImageRequestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var sharedRoot = GetSharedPoiImageRootPath();
            return TryBuildPathUnderRoot(sharedRoot, normalizedRelativePath["images/poi/".Length..], out fullPath);
        }

        return TryBuildPathUnderRoot(GetApiWebRootPath(), normalizedRelativePath, out fullPath);
    }

    private static bool TryBuildPathUnderRoot(string rootPath, string relativePath, out string fullPath)
    {
        fullPath = string.Empty;
        var rootFullPath = Path.GetFullPath(rootPath);
        var candidatePath = Path.GetFullPath(Path.Combine(rootFullPath, relativePath));
        var rootPrefix = rootFullPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootFullPath
            : rootFullPath + Path.DirectorySeparatorChar;

        if (!candidatePath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = candidatePath;
        return true;
    }

    private static string SanitizePathSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim()
            .Select(ch => invalid.Contains(ch) || ch == '/' || ch == '\\' ? '-' : ch)
            .ToArray();

        return new string(chars).Trim('-', ' ');
    }
}
