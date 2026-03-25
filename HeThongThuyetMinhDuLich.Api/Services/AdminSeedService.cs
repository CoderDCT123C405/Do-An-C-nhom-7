using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Services;

public static class AdminSeedService
{
    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration, string dbProvider)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DuLichDbContext>();

        if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            try
            {
                await db.Database.MigrateAsync();
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Existing SQL Server schema without migration history: keep app running.
            }
        }

        var section = configuration.GetSection("SeedAdmin");
        var username = section["Username"] ?? "admin";
        var password = section["Password"] ?? "Admin@123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var fullName = section["FullName"] ?? "System Admin";
        var role = section["Role"] ?? "Admin";
        var resetIfExists = bool.TryParse(section["ResetIfExists"], out var reset) && reset;

        var existing = await db.TaiKhoans.FirstOrDefaultAsync(x => x.TenDangNhap == username);
        if (existing is null)
        {
            db.TaiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = username,
                MatKhauMaHoa = passwordHash,
                HoTen = fullName,
                Email = "admin@local",
                VaiTro = role,
                TrangThaiHoatDong = true,
                NgayTao = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else if (resetIfExists)
        {
            existing.MatKhauMaHoa = passwordHash;
            existing.HoTen = fullName;
            existing.VaiTro = role;
            existing.TrangThaiHoatDong = true;
            existing.NgayCapNhat = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        await EnsureDemoDataAsync(db);
    }

    private static async Task EnsureDemoDataAsync(DuLichDbContext db)
    {
        if (await db.DiemThamQuans.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var adminId = await db.TaiKhoans
            .Where(x => x.TenDangNhap == "admin")
            .Select(x => (int?)x.MaTaiKhoan)
            .FirstOrDefaultAsync();

        var vi = await db.NgonNgus.FirstOrDefaultAsync(x => x.MaNgonNguQuocTe == "vi");
        if (vi is null)
        {
            vi = new NgonNgu
            {
                MaNgonNguQuocTe = "vi",
                TenNgonNgu = "Tieng Viet",
                LaMacDinh = true,
                TrangThaiHoatDong = true
            };
            db.NgonNgus.Add(vi);
            await db.SaveChangesAsync();
        }

        var loaiLichSu = new LoaiDiemThamQuan
        {
            TenLoai = "Di tich lich su",
            MoTa = "Cac diem den mang gia tri lich su",
            TrangThaiHoatDong = true,
            NgayTao = now
        };
        var loaiVanHoa = new LoaiDiemThamQuan
        {
            TenLoai = "Van hoa - bao tang",
            MoTa = "Khong gian van hoa va bao tang",
            TrangThaiHoatDong = true,
            NgayTao = now
        };
        db.LoaiDiemThamQuans.AddRange(loaiLichSu, loaiVanHoa);
        await db.SaveChangesAsync();

        var poi1 = new DiemThamQuan
        {
            MaDinhDanh = "POI001",
            TenDiem = "Ben Nha Rong",
            MoTaNgan = "Dia diem lich su noi tieng tai TP.HCM.",
            ViDo = 10.769969m,
            KinhDo = 106.704849m,
            BanKinhKichHoat = 150,
            DiaChi = "1 Nguyen Tat Thanh, Quan 4, TP.HCM",
            MaLoai = loaiLichSu.MaLoai,
            MaTaiKhoanTao = adminId,
            TrangThaiHoatDong = true,
            NgayTao = now
        };
        var poi2 = new DiemThamQuan
        {
            MaDinhDanh = "POI002",
            TenDiem = "Bao tang Lich su TP.HCM",
            MoTaNgan = "Bao tang trung bay nhieu hien vat gia tri.",
            ViDo = 10.787116m,
            KinhDo = 106.705405m,
            BanKinhKichHoat = 180,
            DiaChi = "2 Nguyen Binh Khiem, Quan 1, TP.HCM",
            MaLoai = loaiVanHoa.MaLoai,
            MaTaiKhoanTao = adminId,
            TrangThaiHoatDong = true,
            NgayTao = now
        };
        db.DiemThamQuans.AddRange(poi1, poi2);
        await db.SaveChangesAsync();

        db.NoiDungThuyetMinhs.AddRange(
            new NoiDungThuyetMinh
            {
                MaDiem = poi1.MaDiem,
                MaNgonNgu = vi.MaNgonNgu,
                TieuDe = "Gioi thieu Ben Nha Rong",
                NoiDungVanBan = "Ben Nha Rong la mot dia danh lich su gan voi hanh trinh ra di tim duong cuu nuoc cua Chu tich Ho Chi Minh.",
                DuongDanAmThanh = null,
                ChoPhepTTS = true,
                ThoiLuongGiay = 45,
                MaTaiKhoanTao = adminId,
                TrangThaiHoatDong = true,
                NgayTao = now
            },
            new NoiDungThuyetMinh
            {
                MaDiem = poi2.MaDiem,
                MaNgonNgu = vi.MaNgonNgu,
                TieuDe = "Gioi thieu Bao tang Lich su",
                NoiDungVanBan = "Bao tang Lich su TP.HCM luu giu nhieu bo suu tap hien vat quy, the hien qua trinh phat trien van hoa Viet Nam.",
                DuongDanAmThanh = null,
                ChoPhepTTS = true,
                ThoiLuongGiay = 50,
                MaTaiKhoanTao = adminId,
                TrangThaiHoatDong = true,
                NgayTao = now
            });

        db.MaQrs.AddRange(
            new MaQr
            {
                MaDiem = poi1.MaDiem,
                GiaTriQR = "QR_POI001",
                TrangThaiHoatDong = true,
                NgayTao = now,
                MaTaiKhoanTao = adminId
            },
            new MaQr
            {
                MaDiem = poi2.MaDiem,
                GiaTriQR = "QR_POI002",
                TrangThaiHoatDong = true,
                NgayTao = now,
                MaTaiKhoanTao = adminId
            });

        await db.SaveChangesAsync();
    }
}
