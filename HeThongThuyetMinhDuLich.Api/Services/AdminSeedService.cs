using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Services;

public static class AdminSeedService
{
    private sealed record DemoNoiDungSeed(int MaNgonNgu, string TieuDe, string NoiDungVanBan);
    private sealed record OnlineSampleContentSeed(string IsoCode, int MaNgonNgu, string TieuDe, string NoiDungVanBan);

    public static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration configuration, string dbProvider)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<DuLichDbContext>();
        var edgeTtsService = scope.ServiceProvider.GetRequiredService<EdgeTtsService>();

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

        await EnsureDemoDataAsync(db, edgeTtsService, dbProvider);
    }

    private static async Task EnsureDemoDataAsync(DuLichDbContext db, EdgeTtsService edgeTtsService, string dbProvider)
    {
        var now = DateTime.UtcNow;
        var adminId = await db.TaiKhoans
            .Where(x => x.TenDangNhap == "admin")
            .Select(x => (int?)x.MaTaiKhoan)
            .FirstOrDefaultAsync();
        if (!adminId.HasValue)
        {
            throw new InvalidOperationException("Khong tim thay tai khoan admin de seed du lieu mau.");
        }

        var vi = await EnsureLanguageAsync(db, "vi", "Tiếng Việt", isDefault: true);
        var en = await EnsureLanguageAsync(db, "en", "Tiếng Anh");
        var zhCn = await EnsureLanguageAsync(db, "zh-CN", "Tiếng Trung");

        if (!await db.DiemThamQuans.AnyAsync())
        {
            var loaiLichSu = new LoaiDiemThamQuan
            {
                TenLoai = "Di tích lịch sử",
                MoTa = "Các điểm đến mang giá trị lịch sử",
                TrangThaiHoatDong = true,
                NgayTao = now
            };
            var loaiVanHoa = new LoaiDiemThamQuan
            {
                TenLoai = "Văn hóa - bảo tàng",
                MoTa = "Không gian văn hóa và bảo tàng",
                TrangThaiHoatDong = true,
                NgayTao = now
            };
            db.LoaiDiemThamQuans.AddRange(loaiLichSu, loaiVanHoa);
            await db.SaveChangesAsync();

            var poi1 = new DiemThamQuan
            {
                MaDinhDanh = "POI001",
                TenDiem = "Bến Nhà Rồng",
                MoTaNgan = "Địa điểm lịch sử nổi tiếng tại TP.HCM.",
                ViDo = 10.769969m,
                KinhDo = 106.704849m,
                BanKinhKichHoat = 150,
                DiaChi = "1 Nguyen Tat Thanh, Quan 4, TP.HCM",
                MaLoai = loaiLichSu.MaLoai,
                MaTaiKhoanTao = adminId,
                MaTaiKhoanCapNhat = adminId.Value,
                TrangThaiHoatDong = true,
                NgayTao = now
            };
            var poi2 = new DiemThamQuan
            {
                MaDinhDanh = "POI002",
                TenDiem = "Bảo tàng Lịch sử TP.HCM",
                MoTaNgan = "Bảo tàng trưng bày nhiều hiện vật giá trị.",
                ViDo = 10.787116m,
                KinhDo = 106.705405m,
                BanKinhKichHoat = 180,
                DiaChi = "2 Nguyen Binh Khiem, Quan 1, TP.HCM",
                MaLoai = loaiVanHoa.MaLoai,
                MaTaiKhoanTao = adminId,
                MaTaiKhoanCapNhat = adminId.Value,
                TrangThaiHoatDong = true,
                NgayTao = now
            };
            db.DiemThamQuans.AddRange(poi1, poi2);
            await db.SaveChangesAsync();
        }

        await EnsureAuditFieldsAsync(db, adminId.Value, now);
        await EnsureDemoNoiDungAsync(db, adminId.Value, now, vi, en, zhCn);
        await EnsureDemoQrAsync(db, adminId, now);

        if (string.Equals(dbProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureOnlineSampleTranslationsAsync(db, edgeTtsService, adminId.Value, now, vi, en, zhCn);
        }
    }

    private static async Task EnsureAuditFieldsAsync(DuLichDbContext db, int adminId, DateTime now)
    {
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE DiemThamQuan SET MaTaiKhoanCapNhat = COALESCE(MaTaiKhoanTao, {0}), NgayCapNhat = COALESCE(NgayCapNhat, {1}) WHERE MaTaiKhoanCapNhat IS NULL",
            adminId,
            now);

        await db.Database.ExecuteSqlRawAsync(
            "UPDATE NoiDungThuyetMinh SET MaTaiKhoanCapNhat = COALESCE(MaTaiKhoanTao, {0}), NgayCapNhat = COALESCE(NgayCapNhat, {1}) WHERE MaTaiKhoanCapNhat IS NULL",
            adminId,
            now);
    }

    private static async Task<NgonNgu> EnsureLanguageAsync(
        DuLichDbContext db,
        string isoCode,
        string displayName,
        bool isDefault = false)
    {
        var language = await db.NgonNgus.FirstOrDefaultAsync(x => x.MaNgonNguQuocTe == isoCode);
        if (language is null)
        {
            language = new NgonNgu
            {
                MaNgonNguQuocTe = isoCode,
                TenNgonNgu = displayName,
                LaMacDinh = isDefault,
                TrangThaiHoatDong = true
            };
            db.NgonNgus.Add(language);
            await db.SaveChangesAsync();
            return language;
        }

        var changed = false;
        if (!language.TrangThaiHoatDong)
        {
            language.TrangThaiHoatDong = true;
            changed = true;
        }

        if (!string.Equals(language.TenNgonNgu, displayName, StringComparison.Ordinal))
        {
            language.TenNgonNgu = displayName;
            changed = true;
        }

        if (isDefault && !language.LaMacDinh)
        {
            language.LaMacDinh = true;
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }

        return language;
    }

    private static async Task EnsureDemoNoiDungAsync(
        DuLichDbContext db,
        int adminId,
        DateTime now,
        NgonNgu vi,
        NgonNgu en,
        NgonNgu zhCn)
    {
        var poiSeeds = new Dictionary<string, DemoNoiDungSeed[]>
        {
            ["POI001"] =
            [
                new DemoNoiDungSeed(vi.MaNgonNgu, "Giới thiệu Bến Nhà Rồng", "Bến Nhà Rồng là di tích lịch sử gắn với hành trình ra đi tìm đường cứu nước của Chủ tịch Hồ Chí Minh và là điểm dừng chân quan trọng trong hành trình khám phá Sài Gòn."),
                new DemoNoiDungSeed(en.MaNgonNgu, "Ben Nha Rong Audio Guide", "Ben Nha Rong is a historic landmark closely associated with President Ho Chi Minh's journey to seek a path for national liberation."),
                new DemoNoiDungSeed(zhCn.MaNgonNgu, "芽龙码头语音导览", "芽龙码头是与胡志明主席寻找民族解放道路历程紧密相连的重要历史遗迹，也是游客了解胡志明市历史的重要地点。")
            ],
            ["POI002"] =
            [
                new DemoNoiDungSeed(vi.MaNgonNgu, "Giới thiệu Bảo tàng Lịch sử", "Bảo tàng Lịch sử TP.HCM lưu giữ nhiều bộ sưu tập hiện vật quý, phản ánh tiến trình hình thành và phát triển của văn hóa Việt Nam qua nhiều thời kỳ."),
                new DemoNoiDungSeed(en.MaNgonNgu, "History Museum Audio Guide", "The Ho Chi Minh City Museum of History preserves valuable collections that reflect the development of Vietnamese culture."),
                new DemoNoiDungSeed(zhCn.MaNgonNgu, "历史博物馆语音导览", "胡志明市历史博物馆珍藏了大量珍贵文物，系统展示了越南文化发展的历史脉络。")
            ]
        };

        var poiIdentifiers = poiSeeds.Keys.ToList();
        var pois = await db.DiemThamQuans
            .Where(x => poiIdentifiers.Contains(x.MaDinhDanh))
            .ToDictionaryAsync(x => x.MaDinhDanh);

        var changed = false;
        foreach (var (maDinhDanh, seeds) in poiSeeds)
        {
            if (!pois.TryGetValue(maDinhDanh, out var poi))
            {
                continue;
            }

            foreach (var seed in seeds)
            {
                var exists = await db.NoiDungThuyetMinhs.AnyAsync(x => x.MaDiem == poi.MaDiem && x.MaNgonNgu == seed.MaNgonNgu);
                if (exists)
                {
                    continue;
                }

                db.NoiDungThuyetMinhs.Add(new NoiDungThuyetMinh
                {
                    MaDiem = poi.MaDiem,
                    MaNgonNgu = seed.MaNgonNgu,
                    TieuDe = seed.TieuDe,
                    NoiDungVanBan = seed.NoiDungVanBan,
                    DuongDanAmThanh = null,
                    ChoPhepTTS = true,
                    ThoiLuongGiay = 45,
                    MaTaiKhoanTao = adminId,
                    MaTaiKhoanCapNhat = adminId,
                    TrangThaiHoatDong = true,
                    NgayTao = now
                });
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureDemoQrAsync(DuLichDbContext db, int? adminId, DateTime now)
    {
        var pois = await db.DiemThamQuans
            .Where(x => x.MaDinhDanh == "POI001" || x.MaDinhDanh == "POI002")
            .Select(x => new { x.MaDiem, x.MaDinhDanh })
            .ToListAsync();

        var changed = false;
        foreach (var poi in pois)
        {
            var qrValue = $"QR_{poi.MaDinhDanh}";
            var exists = await db.MaQrs.AnyAsync(x => x.MaDiem == poi.MaDiem || x.GiaTriQR == qrValue);
            if (exists)
            {
                continue;
            }

            db.MaQrs.Add(new MaQr
            {
                MaDiem = poi.MaDiem,
                GiaTriQR = qrValue,
                TrangThaiHoatDong = true,
                NgayTao = now,
                MaTaiKhoanTao = adminId
            });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureOnlineSampleTranslationsAsync(
        DuLichDbContext db,
        EdgeTtsService edgeTtsService,
        int adminId,
        DateTime now,
        NgonNgu vi,
        NgonNgu en,
        NgonNgu zhCn)
    {
        var baseContents = await db.NoiDungThuyetMinhs
            .AsNoTracking()
            .Where(x => x.TrangThaiHoatDong && x.MaNgonNgu == vi.MaNgonNgu && !string.IsNullOrWhiteSpace(x.NoiDungVanBan))
            .OrderBy(x => x.MaDiem)
            .ThenBy(x => x.MaNoiDung)
            .Take(5)
            .Join(
                db.DiemThamQuans.AsNoTracking(),
                content => content.MaDiem,
                poi => poi.MaDiem,
                (content, poi) => new
                {
                    Content = content,
                    PoiName = poi.TenDiem,
                    PoiIdentifier = poi.MaDinhDanh
                })
            .ToListAsync();

        if (baseContents.Count == 0)
        {
            return;
        }

        var createdItems = new List<(NoiDungThuyetMinh Item, string IsoCode)>();
        foreach (var baseContent in baseContents)
        {
            var seeds = BuildOnlineSampleSeeds(baseContent.PoiName, baseContent.PoiIdentifier, baseContent.Content, en, zhCn);
            foreach (var seed in seeds)
            {
                var exists = await db.NoiDungThuyetMinhs.AnyAsync(x => x.MaDiem == baseContent.Content.MaDiem && x.MaNgonNgu == seed.MaNgonNgu);
                if (exists)
                {
                    continue;
                }

                var entity = new NoiDungThuyetMinh
                {
                    MaDiem = baseContent.Content.MaDiem,
                    MaNgonNgu = seed.MaNgonNgu,
                    TieuDe = seed.TieuDe,
                    NoiDungVanBan = seed.NoiDungVanBan,
                    DuongDanAmThanh = null,
                    ChoPhepTTS = true,
                    ThoiLuongGiay = null,
                    MaTaiKhoanTao = adminId,
                    MaTaiKhoanCapNhat = adminId,
                    TrangThaiHoatDong = true,
                    NgayTao = now
                };

                db.NoiDungThuyetMinhs.Add(entity);
                createdItems.Add((entity, seed.IsoCode));
            }
        }

        if (createdItems.Count == 0)
        {
            return;
        }

        await db.SaveChangesAsync();

        if (!edgeTtsService.IsConfigured)
        {
            return;
        }

        var generatedAnyAudio = false;
        foreach (var (item, isoCode) in createdItems)
        {
            try
            {
                var (newPath, duration) = await edgeTtsService.GenerateAudioAsync(item, isoCode);
                item.DuongDanAmThanh = newPath;
                item.ThoiLuongGiay = duration;
                item.NgayCapNhat = DateTime.UtcNow;
                generatedAnyAudio = true;
            }
            catch
            {
                item.DuongDanAmThanh = null;
                item.ThoiLuongGiay = null;
                item.NgayCapNhat = DateTime.UtcNow;
            }
        }

        if (generatedAnyAudio)
        {
            await db.SaveChangesAsync();
        }
    }

    private static OnlineSampleContentSeed[] BuildOnlineSampleSeeds(
        string poiName,
        string poiIdentifier,
        NoiDungThuyetMinh baseContent,
        NgonNgu en,
        NgonNgu zhCn)
    {
        var englishTitle = $"{poiName} Audio Guide";
        var englishBody = $"This is a sample English audio guide for {poiName} ({poiIdentifier}). Original Vietnamese content: {baseContent.NoiDungVanBan}";

        var chineseTitle = $"{poiName} 中文语音导览";
        var chineseBody = $"这是关于 {poiName} ({poiIdentifier}) 的中文示例讲解内容，内容基于越南语原稿整理生成。";

        return
        [
            new OnlineSampleContentSeed("en", en.MaNgonNgu, englishTitle, englishBody),
            new OnlineSampleContentSeed("zh-CN", zhCn.MaNgonNgu, chineseTitle, chineseBody)
        ];
    }
}
