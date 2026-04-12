using HeThongThuyetMinhDuLich.Api.Data;
using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Services;

public static class AdminSeedService
{
    private sealed record DemoPoiSeed(
        string Identifier,
        string Name,
        string CategoryName,
        string ShortDescription,
        string Address,
        decimal Latitude,
        decimal Longitude,
        int TriggerRadius,
        string ViTitle,
        string ViBody,
        string EnTitle,
        string EnBody,
        string ZhTitle,
        string ZhBody);

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
        var demoPoiSeeds = BuildDemoPoiSeeds();
        var adminId = await db.TaiKhoans
            .Where(x => x.TenDangNhap == "admin")
            .Select(x => (int?)x.MaTaiKhoan)
            .FirstOrDefaultAsync();
        if (!adminId.HasValue)
        {
            throw new InvalidOperationException("Khong tim thay tai khoan admin de seed du lieu mau.");
        }

        var vi = await EnsureLanguageAsync(db, "vi", "Tiếng Việt", isDefault: true);
        var en = await EnsureLanguageAsync(db, "en", "English");
        var zhCn = await EnsureLanguageAsync(db, "zh-CN", "中文(简体)");

        var categoryMap = await EnsureDemoCategoriesAsync(db, demoPoiSeeds, now);
        await EnsureDemoPoisAsync(db, demoPoiSeeds, categoryMap, adminId.Value, now);

        await EnsureAuditFieldsAsync(db, adminId.Value, now);
        await EnsureDemoNoiDungAsync(db, adminId.Value, now, vi, en, zhCn, demoPoiSeeds);
        await EnsureMinimumTriLanguageCoverageAsync(db, adminId.Value, now, vi, en, zhCn);
        await EnsureDemoQrAsync(db, adminId, now, demoPoiSeeds.Select(x => x.Identifier).ToHashSet(StringComparer.OrdinalIgnoreCase));

        if (string.Equals(dbProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureOnlineSampleTranslationsAsync(db, edgeTtsService, adminId.Value, now, vi, en, zhCn);
        }

        if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureMissingAudioAsync(db, edgeTtsService, now, vi, en, zhCn);
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

    private static DemoPoiSeed[] BuildDemoPoiSeeds() =>
    new[]
    {
        new DemoPoiSeed(
            "POI001",
            "Bến Nhà Rồng",
            "Di tích lịch sử",
            "Địa điểm lịch sử gắn với hành trình ra đi tìm đường cứu nước của Chủ tịch Hồ Chí Minh.",
            "1 Nguyễn Tất Thành, Quận 4, TP.HCM",
            10.769969m,
            106.704849m,
            150,
            "Giới thiệu Bến Nhà Rồng",
            "Bến Nhà Rồng là di tích lịch sử gắn với hành trình ra đi tìm đường cứu nước của Chủ tịch Hồ Chí Minh và là điểm dừng chân quan trọng trong hành trình khám phá trung tâm Thành phố Hồ Chí Minh.",
            "Ben Nha Rong Audio Guide",
            "Ben Nha Rong is a historic riverside landmark closely associated with President Ho Chi Minh's journey to seek a path for national liberation. It is a key stop for visitors exploring the story of modern Ho Chi Minh City.",
            "芽龙码头语音导览",
            "芽龙码头是与胡志明主席寻找民族解放道路历程紧密相连的重要历史遗迹，也是游客了解胡志明市近代历史的重要站点。"),
        new DemoPoiSeed(
            "POI002",
            "Bảo tàng Lịch sử TP.HCM",
            "Văn hóa - bảo tàng",
            "Bảo tàng lưu giữ nhiều hiện vật phản ánh tiến trình hình thành và phát triển của văn hóa Việt Nam.",
            "2 Nguyễn Bỉnh Khiêm, Quận 1, TP.HCM",
            10.787116m,
            106.705405m,
            180,
            "Giới thiệu Bảo tàng Lịch sử TP.HCM",
            "Bảo tàng Lịch sử TP.HCM lưu giữ nhiều bộ sưu tập hiện vật quý, giúp du khách hình dung tiến trình phát triển của văn hóa Việt Nam qua nhiều thời kỳ.",
            "History Museum Audio Guide",
            "The Ho Chi Minh City Museum of History preserves valuable collections that help visitors understand the development of Vietnamese culture across different periods.",
            "历史博物馆语音导览",
                "胡志明市历史博物馆珍藏了大量珍贵文物，系统展示了越南文化在不同时期的发展脉络。")
            };

    private static async Task<Dictionary<string, int>> EnsureDemoCategoriesAsync(
        DuLichDbContext db,
        IReadOnlyCollection<DemoPoiSeed> demoPoiSeeds,
        DateTime now)
    {
        var categoryNames = demoPoiSeeds.Select(x => x.CategoryName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var categories = await db.LoaiDiemThamQuans
            .Where(x => categoryNames.Contains(x.TenLoai))
            .ToListAsync();

        foreach (var categoryName in categoryNames.Where(name => categories.All(x => !string.Equals(x.TenLoai, name, StringComparison.OrdinalIgnoreCase))))
        {
            var category = new LoaiDiemThamQuan
            {
                TenLoai = categoryName,
                MoTa = $"Danh mục mẫu cho {categoryName.ToLowerInvariant()}.",
                TrangThaiHoatDong = true,
                NgayTao = now
            };
            db.LoaiDiemThamQuans.Add(category);
            categories.Add(category);
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }

        return categories.ToDictionary(x => x.TenLoai, x => x.MaLoai, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task EnsureDemoPoisAsync(
        DuLichDbContext db,
        IReadOnlyCollection<DemoPoiSeed> demoPoiSeeds,
        IReadOnlyDictionary<string, int> categoryMap,
        int adminId,
        DateTime now)
    {
        var identifiers = demoPoiSeeds.Select(x => x.Identifier).ToList();
        var existingPois = await db.DiemThamQuans
            .Where(x => identifiers.Contains(x.MaDinhDanh))
            .ToDictionaryAsync(x => x.MaDinhDanh, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in demoPoiSeeds)
        {
            if (!categoryMap.TryGetValue(seed.CategoryName, out var categoryId))
            {
                continue;
            }

            if (!existingPois.TryGetValue(seed.Identifier, out var poi))
            {
                poi = new DiemThamQuan
                {
                    MaDinhDanh = seed.Identifier,
                    MaTaiKhoanTao = adminId,
                    NgayTao = now
                };
                db.DiemThamQuans.Add(poi);
                existingPois[seed.Identifier] = poi;
            }

            poi.TenDiem = seed.Name;
            poi.MoTaNgan = seed.ShortDescription;
            poi.ViDo = seed.Latitude;
            poi.KinhDo = seed.Longitude;
            poi.BanKinhKichHoat = seed.TriggerRadius;
            poi.DiaChi = seed.Address;
            poi.MaLoai = categoryId;
            poi.MaTaiKhoanCapNhat = adminId;
            poi.TrangThaiHoatDong = true;
            poi.NgayCapNhat = now;
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task<NgonNgu> EnsureLanguageAsync(
        DuLichDbContext db,
        string isoCode,
        string displayName,
        bool isDefault = false)
    {
        var normalizedIsoCode = LanguageCatalog.NormalizeIsoCode(isoCode);
        var normalizedDisplayName = LanguageCatalog.NormalizeDisplayName(normalizedIsoCode, displayName);
        var language = await db.NgonNgus.FirstOrDefaultAsync(x => x.MaNgonNguQuocTe == normalizedIsoCode);
        if (language is null)
        {
            language = new NgonNgu
            {
                MaNgonNguQuocTe = normalizedIsoCode,
                TenNgonNgu = normalizedDisplayName,
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

        if (!string.Equals(language.MaNgonNguQuocTe, normalizedIsoCode, StringComparison.Ordinal))
        {
            language.MaNgonNguQuocTe = normalizedIsoCode;
            changed = true;
        }

        if (!string.Equals(language.TenNgonNgu, normalizedDisplayName, StringComparison.Ordinal))
        {
            language.TenNgonNgu = normalizedDisplayName;
            changed = true;
        }

        if (isDefault && !language.LaMacDinh)
        {
            var otherDefaults = await db.NgonNgus.Where(x => x.LaMacDinh && x.MaNgonNgu != language.MaNgonNgu).ToListAsync();
            foreach (var other in otherDefaults)
            {
                other.LaMacDinh = false;
                other.NgayCapNhat = DateTime.UtcNow;
            }

            language.LaMacDinh = true;
            changed = true;
        }

        if (changed)
        {
            language.NgayCapNhat = DateTime.UtcNow;
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
        NgonNgu zhCn,
        IReadOnlyCollection<DemoPoiSeed> demoPoiSeeds)
    {
        var poiSeeds = demoPoiSeeds.ToDictionary(
            x => x.Identifier,
            x => new[]
            {
                new DemoNoiDungSeed(vi.MaNgonNgu, x.ViTitle, x.ViBody),
                new DemoNoiDungSeed(en.MaNgonNgu, x.EnTitle, x.EnBody),
                new DemoNoiDungSeed(zhCn.MaNgonNgu, x.ZhTitle, x.ZhBody)
            },
            StringComparer.OrdinalIgnoreCase);

        var poiIdentifiers = poiSeeds.Keys.ToList();
        var pois = await db.DiemThamQuans
            .Where(x => poiIdentifiers.Contains(x.MaDinhDanh))
            .ToDictionaryAsync(x => x.MaDinhDanh);

        foreach (var (maDinhDanh, seeds) in poiSeeds)
        {
            if (!pois.TryGetValue(maDinhDanh, out var poi))
            {
                continue;
            }

            foreach (var seed in seeds)
            {
                var item = await db.NoiDungThuyetMinhs.FirstOrDefaultAsync(x => x.MaDiem == poi.MaDiem && x.MaNgonNgu == seed.MaNgonNgu);
                if (item is null)
                {
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
                        NgayTao = now,
                        NgayCapNhat = now
                    });
                    continue;
                }

                item.TieuDe = seed.TieuDe;
                item.NoiDungVanBan = seed.NoiDungVanBan;
                item.ChoPhepTTS = true;
                item.TrangThaiHoatDong = true;
                item.MaTaiKhoanCapNhat = adminId;
                item.NgayCapNhat = now;
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureDemoQrAsync(
        DuLichDbContext db,
        int? adminId,
        DateTime now,
        IReadOnlySet<string> demoPoiIdentifiers)
    {
        var pois = await db.DiemThamQuans
            .Where(x => demoPoiIdentifiers.Contains(x.MaDinhDanh))
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

    private static async Task EnsureMinimumTriLanguageCoverageAsync(
        DuLichDbContext db,
        int adminId,
        DateTime now,
        NgonNgu vi,
        NgonNgu en,
        NgonNgu zhCn)
    {
        var requiredLanguages = new[]
        {
            (vi, "vi"),
            (en, "en"),
            (zhCn, "zh-CN")
        };

        var pois = await db.DiemThamQuans
            .AsNoTracking()
            .Where(x => x.TrangThaiHoatDong)
            .ToListAsync();

        foreach (var poi in pois)
        {
            var contents = await db.NoiDungThuyetMinhs
                .Where(x => x.MaDiem == poi.MaDiem)
                .ToListAsync();

            var sourceContent = contents
                .FirstOrDefault(x => x.MaNgonNgu == vi.MaNgonNgu && !string.IsNullOrWhiteSpace(x.NoiDungVanBan))
                ?? contents.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.NoiDungVanBan));

            foreach (var (language, isoCode) in requiredLanguages)
            {
                var existing = contents.FirstOrDefault(x => x.MaNgonNgu == language.MaNgonNgu);
                if (existing is null)
                {
                    var generated = BuildFallbackContentSeed(poi, sourceContent, isoCode, language.MaNgonNgu);
                    existing = new NoiDungThuyetMinh
                    {
                        MaDiem = poi.MaDiem,
                        MaNgonNgu = language.MaNgonNgu,
                        TieuDe = generated.TieuDe,
                        NoiDungVanBan = generated.NoiDungVanBan,
                        DuongDanAmThanh = null,
                        ChoPhepTTS = true,
                        ThoiLuongGiay = 45,
                        MaTaiKhoanTao = adminId,
                        MaTaiKhoanCapNhat = adminId,
                        TrangThaiHoatDong = true,
                        NgayTao = now,
                        NgayCapNhat = now
                    };
                    db.NoiDungThuyetMinhs.Add(existing);
                    contents.Add(existing);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(existing.TieuDe) || string.IsNullOrWhiteSpace(existing.NoiDungVanBan))
                {
                    var generated = BuildFallbackContentSeed(poi, sourceContent, isoCode, language.MaNgonNgu);
                    existing.TieuDe = string.IsNullOrWhiteSpace(existing.TieuDe) ? generated.TieuDe : existing.TieuDe;
                    existing.NoiDungVanBan = string.IsNullOrWhiteSpace(existing.NoiDungVanBan) ? generated.NoiDungVanBan : existing.NoiDungVanBan;
                }

                existing.ChoPhepTTS = true;
                existing.TrangThaiHoatDong = true;
                existing.MaTaiKhoanCapNhat = adminId;
                existing.NgayCapNhat = now;
            }
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync();
        }
    }

    private static DemoNoiDungSeed BuildFallbackContentSeed(
        DiemThamQuan poi,
        NoiDungThuyetMinh? sourceContent,
        string isoCode,
        int languageId)
    {
        var baseVietnamese = sourceContent?.NoiDungVanBan?.Trim();
        return isoCode switch
        {
            "en" => new DemoNoiDungSeed(
                languageId,
                $"{poi.TenDiem} Audio Guide",
                $"{poi.TenDiem} is one of the featured stops in this demo route. Visitors can use this point to hear a short introduction, review its location at {poi.DiaChi}, and continue exploring nearby attractions.{(string.IsNullOrWhiteSpace(baseVietnamese) ? string.Empty : $" Source note: {baseVietnamese}")}"),
            "zh-CN" => new DemoNoiDungSeed(
                languageId,
                $"{poi.TenDiem} 语音导览",
                $"{poi.TenDiem} 是本次演示路线中的示范景点之一，游客可以在这里收听简介、查看地址 {poi.DiaChi}，并继续探索周边景点。{(string.IsNullOrWhiteSpace(baseVietnamese) ? string.Empty : " 内容基于现有越南语介绍整理生成。")}"),
            _ => new DemoNoiDungSeed(
                languageId,
                $"Giới thiệu {poi.TenDiem}",
                string.IsNullOrWhiteSpace(baseVietnamese)
                    ? $"{poi.TenDiem} là điểm tham quan đang hoạt động trong bộ dữ liệu demo. Du khách có thể sử dụng điểm này để kiểm tra quét QR, nghe thuyết minh và trải nghiệm chuyển đổi ngôn ngữ trực tiếp trong ứng dụng."
                    : baseVietnamese)
        };
    }

    private static async Task EnsureMissingAudioAsync(
        DuLichDbContext db,
        EdgeTtsService edgeTtsService,
        DateTime now,
        params NgonNgu[] supportedLanguages)
    {
        if (!edgeTtsService.IsConfigured)
        {
            return;
        }

        var supportedLanguageMap = supportedLanguages.ToDictionary(x => x.MaNgonNgu, x => x.MaNgonNguQuocTe);
        var items = await db.NoiDungThuyetMinhs
            .Where(x => x.TrangThaiHoatDong
                && x.ChoPhepTTS
                && string.IsNullOrWhiteSpace(x.DuongDanAmThanh)
                && supportedLanguageMap.Keys.Contains(x.MaNgonNgu)
                && !string.IsNullOrWhiteSpace(x.NoiDungVanBan))
            .ToListAsync();

        var generatedAnyAudio = false;
        foreach (var item in items)
        {
            if (!supportedLanguageMap.TryGetValue(item.MaNgonNgu, out var isoCode) || string.IsNullOrWhiteSpace(isoCode))
            {
                continue;
            }

            try
            {
                var (newPath, duration) = await edgeTtsService.GenerateAudioAsync(item, isoCode);
                item.DuongDanAmThanh = newPath;
                item.ThoiLuongGiay = duration;
                item.NgayCapNhat = now;
                generatedAnyAudio = true;
            }
            catch
            {
                item.NgayCapNhat = now;
            }
        }

        if (generatedAnyAudio)
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

        // Do not block API startup on best-effort audio generation.
        foreach (var (item, _) in createdItems)
        {
            item.DuongDanAmThanh = null;
            item.ThoiLuongGiay = null;
            item.NgayCapNhat = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
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
