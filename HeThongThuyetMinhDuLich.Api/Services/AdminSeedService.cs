using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Services;

public static class AdminSeedService
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DuLichDbContext>();

        await db.Database.MigrateAsync();

        var section = configuration.GetSection("SeedAdmin");
        var username = section["Username"] ?? "admin";
        var password = section["Password"] ?? "Admin@123";
        var fullName = section["FullName"] ?? "System Admin";
        var role = section["Role"] ?? "Admin";
        var resetIfExists = bool.TryParse(section["ResetIfExists"], out var reset) && reset;

        var existing = await db.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == username);
        if (existing is null)
        {
            db.TaiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = username,
                MatKhauMaHoa = password,
                HoTen = fullName,
                Email = "admin@local",
                VaiTro = role,
                TrangThaiHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return;
        }

        if (!resetIfExists)
        {
            return;
        }

        existing.MatKhauMaHoa = password;
        existing.HoTen = fullName;
        existing.VaiTro = role;
        existing.TrangThaiHoatDong = true;
        existing.NgayCapNhat = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }
}
