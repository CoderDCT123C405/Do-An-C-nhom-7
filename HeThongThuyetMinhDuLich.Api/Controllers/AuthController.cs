using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Auth;
using HeThongThuyetMinhDuLich.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(DuLichDbContext dbContext, JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost]
    public Task<ActionResult<LoginResponse>> AdminLoginAlias(AdminLoginRequest request)
        => AdminLogin(request);

    [HttpPost("admin/login")]
    public async Task<ActionResult<LoginResponse>> AdminLogin(AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
        {
            return BadRequest(new { message = "Ten dang nhap va mat khau la bat buoc." });
        }

        var username = request.TenDangNhap.Trim();
        var password = request.MatKhau.Trim();

        var taiKhoan = await dbContext.TaiKhoans
            .FirstOrDefaultAsync(x => x.TenDangNhap == username && x.TrangThaiHoatDong);

        if (taiKhoan is null || !VerifyPassword(password, taiKhoan.MatKhauMaHoa))
        {
            return Unauthorized(new { message = "Ten dang nhap hoac mat khau khong dung." });
        }

        if (!IsCmsRole(taiKhoan.VaiTro))
        {
            return Forbid();
        }

        return Ok(jwtTokenService.TaoTokenChoTaiKhoan(taiKhoan));
    }

    [HttpPost("user/login")]
    public async Task<ActionResult<LoginResponse>> UserLogin(UserLoginRequest request)
    {
        var nguoiDung = await dbContext.NguoiDungs
            .FirstOrDefaultAsync(x => x.TenDangNhap == request.TenDangNhap && x.TrangThaiHoatDong);

        if (nguoiDung is null || string.IsNullOrWhiteSpace(nguoiDung.MatKhauMaHoa) ||
            !BCrypt.Net.BCrypt.Verify(request.MatKhau, nguoiDung.MatKhauMaHoa))
        {
            return Unauthorized(new { message = "Ten dang nhap hoac mat khau khong dung." });
        }

        return Ok(jwtTokenService.TaoTokenChoNguoiDung(nguoiDung));
    }

    [HttpPost("user/register")]
    public async Task<ActionResult<object>> UserRegister(UserRegisterRequest request)
    {
        var daTonTai = await dbContext.NguoiDungs.AnyAsync(x => x.TenDangNhap == request.TenDangNhap);
        if (daTonTai)
        {
            return Conflict(new { message = "Ten dang nhap da ton tai." });
        }

        var nguoiDung = new NguoiDung
        {
            TenDangNhap = request.TenDangNhap,
            MatKhauMaHoa = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
            HoTen = request.HoTen,
            Email = request.Email,
            SoDienThoai = request.SoDienThoai,
            MaNgonNguMacDinh = request.MaNgonNguMacDinh,
            TrangThaiHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        dbContext.NguoiDungs.Add(nguoiDung);
        await dbContext.SaveChangesAsync();

        return Created(string.Empty, new
        {
            nguoiDung.MaNguoiDung,
            nguoiDung.TenDangNhap,
            nguoiDung.HoTen,
            nguoiDung.Email
        });
    }

    private static bool VerifyPassword(string rawPassword, string storedHashOrRaw)
    {
        if (string.IsNullOrWhiteSpace(storedHashOrRaw))
        {
            return false;
        }

        if (storedHashOrRaw.StartsWith("$2", StringComparison.Ordinal))
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, storedHashOrRaw);
        }

        return string.Equals(rawPassword, storedHashOrRaw, StringComparison.Ordinal);
    }

    private static bool IsCmsRole(string? vaiTro)
        => string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(vaiTro, "BienTap", StringComparison.OrdinalIgnoreCase);
}
