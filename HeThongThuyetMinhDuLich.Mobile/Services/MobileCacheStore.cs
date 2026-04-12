using HeThongThuyetMinhDuLich.Mobile.Models;
using SQLite;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class MobileCacheStore
{
    public const string SyncKeyPois = "pois";
    public const string SyncKeyNoiDung = "noidung";
    public const string SyncKeyNgonNgu = "ngonngu";
    public const string SyncKeyQr = "qr";

    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public MobileCacheStore()
    {
        var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "audio-guide-cache.db3");
        _db = new SQLiteAsyncConnection(dbPath);
    }

    // ================== POI ==================

    public async Task SavePoisAsync(IEnumerable<DiemThamQuanItem> items, bool replaceMissing = true)
    {
        await EnsureInitializedAsync();

        var incomingItems = items.ToList();

        await _db.RunInTransactionAsync(conn =>
        {
            var existingById = conn.Table<CachedPoi>().ToList().ToDictionary(x => x.MaDiem);
            var incomingIds = incomingItems.Select(x => x.MaDiem).ToHashSet();

            foreach (var item in incomingItems)
            {
                var row = new CachedPoi
                {
                    MaDiem = item.MaDiem,
                    MaDinhDanh = item.MaDinhDanh,
                    TenDiem = item.TenDiem,
                    MoTaNgan = item.MoTaNgan,
                    ViDo = item.ViDo,
                    KinhDo = item.KinhDo,
                    BanKinhKichHoat = item.BanKinhKichHoat,
                    DiaChi = item.DiaChi,
                    MaLoai = item.MaLoai,
                    TrangThaiHoatDong = item.TrangThaiHoatDong,
                    NgayCapNhat = item.NgayCapNhat
                };

                if (!existingById.TryGetValue(item.MaDiem, out var existing) || !AreEquivalent(existing, row))
                {
                    conn.InsertOrReplace(row);
                }
            }

            if (!replaceMissing)
            {
                return;
            }

            foreach (var existingId in existingById.Keys)
            {
                if (!incomingIds.Contains(existingId))
                {
                    conn.Delete<CachedPoi>(existingId);
                }
            }
        });
    }

    public async Task SavePoiAsync(DiemThamQuanItem item)
    {
        await EnsureInitializedAsync();

        var normalizedUpdatedAt = item.NgayCapNhat == default ? DateTime.UtcNow : item.NgayCapNhat;

        await _db.InsertOrReplaceAsync(new CachedPoi
        {
            MaDiem = item.MaDiem,
            MaDinhDanh = item.MaDinhDanh,
            TenDiem = item.TenDiem,
            MoTaNgan = item.MoTaNgan,
            ViDo = item.ViDo,
            KinhDo = item.KinhDo,
            BanKinhKichHoat = item.BanKinhKichHoat,
            DiaChi = item.DiaChi,
            MaLoai = item.MaLoai,
            TrangThaiHoatDong = item.TrangThaiHoatDong,
            NgayCapNhat = normalizedUpdatedAt
        });
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetPoisAsync()
    {
        await EnsureInitializedAsync();

        var rows = await _db.Table<CachedPoi>().Where(x => x.TrangThaiHoatDong).ToListAsync();

        return rows.Select(x => new DiemThamQuanItem
        {
            MaDiem = x.MaDiem,
            MaDinhDanh = x.MaDinhDanh,
            TenDiem = x.TenDiem,
            MoTaNgan = x.MoTaNgan,
            ViDo = x.ViDo,
            KinhDo = x.KinhDo,
            BanKinhKichHoat = x.BanKinhKichHoat,
            DiaChi = x.DiaChi,
            MaLoai = x.MaLoai,
            TrangThaiHoatDong = x.TrangThaiHoatDong,
            NgayCapNhat = x.NgayCapNhat
        }).ToList();
    }

    public async Task<DiemThamQuanItem?> GetPoiAsync(int maDiem)
    {
        await EnsureInitializedAsync();

        var row = await _db.FindAsync<CachedPoi>(maDiem);
        if (row is null)
        {
            return null;
        }

        return new DiemThamQuanItem
        {
            MaDiem = row.MaDiem,
            MaDinhDanh = row.MaDinhDanh,
            TenDiem = row.TenDiem,
            MoTaNgan = row.MoTaNgan,
            ViDo = row.ViDo,
            KinhDo = row.KinhDo,
            BanKinhKichHoat = row.BanKinhKichHoat,
            DiaChi = row.DiaChi,
            MaLoai = row.MaLoai,
            TrangThaiHoatDong = row.TrangThaiHoatDong,
            NgayCapNhat = row.NgayCapNhat
        };
    }

    public async Task SaveNgonNguAsync(IEnumerable<NgonNguItem> items, bool replaceMissing = true)
    {
        await EnsureInitializedAsync();

        var incomingItems = items.ToList();

        await _db.RunInTransactionAsync(conn =>
        {
            var existingById = conn.Table<CachedNgonNgu>().ToList().ToDictionary(x => x.MaNgonNgu);
            var incomingIds = incomingItems.Select(x => x.MaNgonNgu).ToHashSet();

            foreach (var item in incomingItems)
            {
                var row = new CachedNgonNgu
                {
                    MaNgonNgu = item.MaNgonNgu,
                    MaNgonNguQuocTe = item.MaNgonNguQuocTe,
                    TenNgonNgu = item.TenNgonNgu,
                    LaMacDinh = item.LaMacDinh,
                    TrangThaiHoatDong = item.TrangThaiHoatDong,
                    NgayCapNhat = item.NgayCapNhat == default ? DateTime.UtcNow : item.NgayCapNhat
                };

                if (!existingById.TryGetValue(item.MaNgonNgu, out var existing) || !AreEquivalent(existing, row))
                {
                    conn.InsertOrReplace(row);
                }
            }

            if (!replaceMissing)
            {
                return;
            }

            foreach (var existingId in existingById.Keys)
            {
                if (!incomingIds.Contains(existingId))
                {
                    conn.Delete<CachedNgonNgu>(existingId);
                }
            }
        });
    }

    public async Task<IReadOnlyList<NgonNguItem>> GetNgonNguAsync()
    {
        await EnsureInitializedAsync();

        var rows = await _db.Table<CachedNgonNgu>().Where(x => x.TrangThaiHoatDong).ToListAsync();

        return rows.Select(x => new NgonNguItem
        {
            MaNgonNgu = x.MaNgonNgu,
            MaNgonNguQuocTe = x.MaNgonNguQuocTe,
            TenNgonNgu = x.TenNgonNgu,
            LaMacDinh = x.LaMacDinh,
            TrangThaiHoatDong = x.TrangThaiHoatDong,
            NgayCapNhat = x.NgayCapNhat
        }).ToList();
    }

    // ================== NOI DUNG ==================

    public async Task SaveNoiDungAsync(int maDiem, IEnumerable<NoiDungItem> items, bool replaceMissing = true)
    {
        await EnsureInitializedAsync();

        var incomingItems = items.ToList();

        await _db.RunInTransactionAsync(conn =>
        {
            var existingById = conn.Table<CachedNoiDung>()
                .Where(x => x.MaDiem == maDiem)
                .ToList()
                .ToDictionary(x => x.MaNoiDung);
            var incomingIds = incomingItems.Select(x => x.MaNoiDung).ToHashSet();

            foreach (var item in incomingItems)
            {
                var row = new CachedNoiDung
                {
                    MaNoiDung = item.MaNoiDung,
                    MaDiem = item.MaDiem,
                    MaNgonNgu = item.MaNgonNgu,
                    TenNgonNgu = item.TenNgonNgu,
                    TieuDe = item.TieuDe,
                    NoiDungVanBan = item.NoiDungVanBan,
                    DuongDanAmThanh = item.DuongDanAmThanh,
                    TepAmThanhNoiBo = item.TepAmThanhNoiBo,
                    ChoPhepTTS = item.ChoPhepTTS,
                    ThoiLuongGiay = item.ThoiLuongGiay,
                    TrangThaiHoatDong = item.TrangThaiHoatDong,
                    NgayCapNhat = item.NgayCapNhat == default ? DateTime.UtcNow : item.NgayCapNhat
                };

                if (!existingById.TryGetValue(item.MaNoiDung, out var existing) || !AreEquivalent(existing, row))
                {
                    conn.InsertOrReplace(row);
                }
            }

            if (!replaceMissing)
            {
                return;
            }

            foreach (var existingId in existingById.Keys)
            {
                if (!incomingIds.Contains(existingId))
                {
                    conn.Delete<CachedNoiDung>(existingId);
                }
            }
        });
    }

    public async Task<IReadOnlyList<NoiDungItem>> GetNoiDungAsync(int maDiem)
    {
        await EnsureInitializedAsync();

        var rows = await _db.Table<CachedNoiDung>()
                            .Where(x => x.MaDiem == maDiem && x.TrangThaiHoatDong)
                            .ToListAsync();

        return rows.Select(x => new NoiDungItem
        {
            MaNoiDung = x.MaNoiDung,
            MaDiem = x.MaDiem,
            MaNgonNgu = x.MaNgonNgu,
            TenNgonNgu = x.TenNgonNgu,
            TieuDe = x.TieuDe,
            NoiDungVanBan = x.NoiDungVanBan,
            DuongDanAmThanh = x.DuongDanAmThanh,
            TepAmThanhNoiBo = x.TepAmThanhNoiBo,
            ChoPhepTTS = x.ChoPhepTTS,
            ThoiLuongGiay = x.ThoiLuongGiay,
            TrangThaiHoatDong = x.TrangThaiHoatDong,
            NgayCapNhat = x.NgayCapNhat
        }).ToList();
    }

    public async Task SaveQrMappingsAsync(IEnumerable<QrSummaryItem> items, bool replaceMissing = true)
    {
        await EnsureInitializedAsync();

        var incomingItems = items.ToList();

        await _db.RunInTransactionAsync(conn =>
        {
            var existingById = conn.Table<CachedQr>().ToList().ToDictionary(x => x.MaQR);
            var incomingIds = incomingItems.Select(x => x.MaQR).ToHashSet();

            foreach (var item in incomingItems)
            {
                var row = new CachedQr
                {
                    MaQR = item.MaQR,
                    MaDiem = item.MaDiem,
                    GiaTriQR = item.GiaTriQR,
                    TrangThaiHoatDong = item.TrangThaiHoatDong,
                    NgayCapNhat = item.NgayCapNhat == default ? DateTime.UtcNow : item.NgayCapNhat
                };

                if (!existingById.TryGetValue(item.MaQR, out var existing) || !AreEquivalent(existing, row))
                {
                    conn.InsertOrReplace(row);
                }
            }

            if (!replaceMissing)
            {
                return;
            }

            foreach (var existingId in existingById.Keys)
            {
                if (!incomingIds.Contains(existingId))
                {
                    conn.Delete<CachedQr>(existingId);
                }
            }
        });
    }

    public async Task SaveQrMappingAsync(QrSummaryItem item)
    {
        await EnsureInitializedAsync();

        await _db.InsertOrReplaceAsync(new CachedQr
        {
            MaQR = item.MaQR,
            MaDiem = item.MaDiem,
            GiaTriQR = item.GiaTriQR,
            TrangThaiHoatDong = item.TrangThaiHoatDong,
            NgayCapNhat = item.NgayCapNhat == default ? DateTime.UtcNow : item.NgayCapNhat
        });
    }

    public async Task<QrSummaryItem?> GetQrMappingAsync(string qrValue)
    {
        await EnsureInitializedAsync();

        var row = await _db.Table<CachedQr>()
            .Where(x => x.GiaTriQR == qrValue && x.TrangThaiHoatDong)
            .FirstOrDefaultAsync();

        if (row is null)
        {
            return null;
        }

        return new QrSummaryItem
        {
            MaQR = row.MaQR,
            MaDiem = row.MaDiem,
            GiaTriQR = row.GiaTriQR,
            TrangThaiHoatDong = row.TrangThaiHoatDong,
            NgayCapNhat = row.NgayCapNhat
        };
    }

    public async Task<DateTime?> GetSyncCheckpointAsync(string key)
    {
        await EnsureInitializedAsync();

        var row = await _db.FindAsync<CachedSyncCheckpoint>(key);
        return row?.LastSyncedUtc;
    }

    public async Task SetSyncCheckpointAsync(string key, DateTime lastSyncedUtc)
    {
        await EnsureInitializedAsync();

        await _db.InsertOrReplaceAsync(new CachedSyncCheckpoint
        {
            Key = key,
            LastSyncedUtc = lastSyncedUtc.ToUniversalTime()
        });
    }

    public async Task ResetCacheAsync()
    {
        await EnsureInitializedAsync();

        await _db.RunInTransactionAsync(conn =>
        {
            conn.DeleteAll<CachedPoi>();
            conn.DeleteAll<CachedNgonNgu>();
            conn.DeleteAll<CachedNoiDung>();
            conn.DeleteAll<CachedQr>();
            conn.DeleteAll<CachedSyncCheckpoint>();
        });
    }

    public async Task EnqueuePlaybackHistoryAsync(LichSuPhatCreateRequest request)
    {
        await EnsureInitializedAsync();

        await _db.InsertAsync(new PendingPlaybackHistory
        {
            MaNguoiDung = request.MaNguoiDung,
            MaDiem = request.MaDiem,
            MaNoiDung = request.MaNoiDung,
            CachKichHoat = request.CachKichHoat,
            ThoiGianBatDau = request.ThoiGianBatDau,
            ThoiLuongDaNghe = request.ThoiLuongDaNghe
        });
    }

    public async Task<IReadOnlyList<PendingPlaybackHistory>> GetPendingPlaybackHistoryAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<PendingPlaybackHistory>().OrderBy(x => x.Id).ToListAsync();
    }

    public async Task RemovePendingPlaybackHistoryAsync(IEnumerable<long> ids)
    {
        await EnsureInitializedAsync();

        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return;
        }

        await _db.RunInTransactionAsync(conn =>
        {
            foreach (var id in idList)
            {
                conn.Delete<PendingPlaybackHistory>(id);
            }
        });
    }

    // ================== INIT ==================

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _db.CreateTableAsync<CachedPoi>();
        await _db.CreateTableAsync<CachedNgonNgu>();
        await _db.CreateTableAsync<CachedNoiDung>();
        await _db.CreateTableAsync<CachedQr>();
        await _db.CreateTableAsync<PendingPlaybackHistory>();
        await _db.CreateTableAsync<CachedSyncCheckpoint>();

        _initialized = true;
    }

    private static bool AreEquivalent(CachedPoi left, CachedPoi right)
    {
        return left.MaDinhDanh == right.MaDinhDanh
            && left.TenDiem == right.TenDiem
            && left.MoTaNgan == right.MoTaNgan
            && left.ViDo == right.ViDo
            && left.KinhDo == right.KinhDo
            && left.BanKinhKichHoat == right.BanKinhKichHoat
            && left.DiaChi == right.DiaChi
            && left.MaLoai == right.MaLoai
            && left.TrangThaiHoatDong == right.TrangThaiHoatDong
            && left.NgayCapNhat == right.NgayCapNhat;
    }

    private static bool AreEquivalent(CachedNgonNgu left, CachedNgonNgu right)
    {
        return left.MaNgonNguQuocTe == right.MaNgonNguQuocTe
            && left.TenNgonNgu == right.TenNgonNgu
            && left.LaMacDinh == right.LaMacDinh
            && left.TrangThaiHoatDong == right.TrangThaiHoatDong
            && left.NgayCapNhat == right.NgayCapNhat;
    }

    private static bool AreEquivalent(CachedNoiDung left, CachedNoiDung right)
    {
        return left.MaDiem == right.MaDiem
            && left.MaNgonNgu == right.MaNgonNgu
            && left.TenNgonNgu == right.TenNgonNgu
            && left.TieuDe == right.TieuDe
            && left.NoiDungVanBan == right.NoiDungVanBan
            && left.DuongDanAmThanh == right.DuongDanAmThanh
            && left.TepAmThanhNoiBo == right.TepAmThanhNoiBo
            && left.ChoPhepTTS == right.ChoPhepTTS
            && left.ThoiLuongGiay == right.ThoiLuongGiay
            && left.TrangThaiHoatDong == right.TrangThaiHoatDong
            && left.NgayCapNhat == right.NgayCapNhat;
    }

    private static bool AreEquivalent(CachedQr left, CachedQr right)
    {
        return left.MaDiem == right.MaDiem
            && left.GiaTriQR == right.GiaTriQR
            && left.TrangThaiHoatDong == right.TrangThaiHoatDong
            && left.NgayCapNhat == right.NgayCapNhat;
    }
}
public class CachedPoi
{
    [PrimaryKey]
    public int MaDiem { get; set; }
    public string MaDinhDanh { get; set; } = string.Empty;
    public string TenDiem { get; set; } = string.Empty;
    public string? MoTaNgan { get; set; }
    public decimal ViDo { get; set; }
    public decimal KinhDo { get; set; }
    public decimal BanKinhKichHoat { get; set; }
    public string? DiaChi { get; set; }
    public int MaLoai { get; set; }
    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class CachedNgonNgu
{
    [PrimaryKey]
    public int MaNgonNgu { get; set; }
    public string MaNgonNguQuocTe { get; set; } = string.Empty;
    public string TenNgonNgu { get; set; } = string.Empty;
    public bool LaMacDinh { get; set; }
    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class CachedNoiDung
{
    [PrimaryKey]
    public int MaNoiDung { get; set; }

    [Indexed]
    public int MaDiem { get; set; }

    public int MaNgonNgu { get; set; }
    public string? TenNgonNgu { get; set; }
    public string? TieuDe { get; set; }
    public string? NoiDungVanBan { get; set; }
    public string? DuongDanAmThanh { get; set; }
    public string? TepAmThanhNoiBo { get; set; }
    public bool ChoPhepTTS { get; set; }
    public int? ThoiLuongGiay { get; set; }
    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class CachedQr
{
    [PrimaryKey]
    public int MaQR { get; set; }
    public int MaDiem { get; set; }

    [Indexed(Unique = true)]
    public string GiaTriQR { get; set; } = string.Empty;

    public bool TrangThaiHoatDong { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class CachedSyncCheckpoint
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;
    public DateTime LastSyncedUtc { get; set; }
}

public class PendingPlaybackHistory
{
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    public int? MaNguoiDung { get; set; }
    public int MaDiem { get; set; }
    public int MaNoiDung { get; set; }
    public string CachKichHoat { get; set; } = string.Empty;
    public DateTime? ThoiGianBatDau { get; set; }
    public int? ThoiLuongDaNghe { get; set; }
}
