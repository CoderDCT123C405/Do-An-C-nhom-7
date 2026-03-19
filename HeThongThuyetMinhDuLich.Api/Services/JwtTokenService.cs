using HeThongThuyetMinhDuLich.Api.Models;
using HeThongThuyetMinhDuLich.Api.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HeThongThuyetMinhDuLich.Api.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public LoginResponse TaoTokenChoTaiKhoan(TaiKhoan taiKhoan)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name, taiKhoan.TenDangNhap),
            new(ClaimTypes.Role, taiKhoan.VaiTro ?? "Admin"),
            new("loai_tai_khoan", "admin")
        };

        var expires = DateTime.UtcNow.AddMinutes(LaySoPhutHetHan());
        var token = TaoJwt(claims, expires);

        return new LoginResponse
        {
            Token = token,
            HetHanLuc = expires,
            LoaiTaiKhoan = "admin",
            MaDinhDanh = taiKhoan.MaTaiKhoan,
            TenDangNhap = taiKhoan.TenDangNhap,
            HoTen = taiKhoan.HoTen,
            VaiTro = taiKhoan.VaiTro
        };
    }

    public LoginResponse TaoTokenChoNguoiDung(NguoiDung nguoiDung)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
            new(ClaimTypes.Name, nguoiDung.TenDangNhap ?? string.Empty),
            new(ClaimTypes.Role, "NguoiDung"),
            new("loai_tai_khoan", "nguoi_dung")
        };

        var expires = DateTime.UtcNow.AddMinutes(LaySoPhutHetHan());
        var token = TaoJwt(claims, expires);

        return new LoginResponse
        {
            Token = token,
            HetHanLuc = expires,
            LoaiTaiKhoan = "nguoi_dung",
            MaDinhDanh = nguoiDung.MaNguoiDung,
            TenDangNhap = nguoiDung.TenDangNhap ?? string.Empty,
            HoTen = nguoiDung.HoTen,
            VaiTro = "NguoiDung"
        };
    }

    private string TaoJwt(IEnumerable<Claim> claims, DateTime expires)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int LaySoPhutHetHan()
    {
        var value = configuration.GetSection("Jwt")["ExpireMinutes"];
        return int.TryParse(value, out var minutes) ? minutes : 120;
    }
}
