using HeThongThuyetMinhDuLich.Mobile.Models;
using SQLite;

namespace HeThongThuyetMinhDuLich.Mobile.Services;

public class MobileCacheStore
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public MobileCacheStore()
    {
        var dbPath = Path.Combine(FileSystem.Current.AppDataDirectory, "audio-guide-cache.db3");
        _db = new SQLiteAsyncConnection(dbPath);
    }

    // ================== POI ==================

    public async Task SavePoisAsync(IEnumerable<DiemThamQuanItem> items)
    {
        await EnsureInitializedAsync();

        await _db.RunInTransactionAsync(conn =>
        {
            // 🔥 Clear toàn bộ (sync 1 chiều)
            conn.DeleteAll<CachedPoi>();

            foreach (var item in items)
            {
                conn.InsertOrReplace(new CachedPoi
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
                    TrangThaiHoatDong = item.TrangThaiHoatDong
                });
            }
        });
    }

    public async Task<IReadOnlyList<DiemThamQuanItem>> GetPoisAsync()
    {
        await EnsureInitializedAsync();

        var rows = await _db.Table<CachedPoi>().ToListAsync();

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
            TrangThaiHoatDong = x.TrangThaiHoatDong
        }).ToList();
    }

    // ================== NOI DUNG ==================

    public async Task SaveNoiDungAsync(int maDiem, IEnumerable<NoiDungItem> items)
    {
        await EnsureInitializedAsync();

        await _db.RunInTransactionAsync(conn =>
        {
            conn.Table<CachedNoiDung>().Delete(x => x.MaDiem == maDiem);

            foreach (var item in items)
            {
                conn.InsertOrReplace(new CachedNoiDung
                {
                    MaNoiDung = item.MaNoiDung,
                    MaDiem = item.MaDiem,
                    MaNgonNgu = item.MaNgonNgu,
                    TenNgonNgu = item.TenNgonNgu,
                    TieuDe = item.TieuDe,
                    NoiDungVanBan = item.NoiDungVanBan,
                    DuongDanAmThanh = item.DuongDanAmThanh,
                    ChoPhepTTS = item.ChoPhepTTS,
                    ThoiLuongGiay = item.ThoiLuongGiay
                });
            }
        });
    }

    public async Task<IReadOnlyList<NoiDungItem>> GetNoiDungAsync(int maDiem)
    {
        await EnsureInitializedAsync();

        var rows = await _db.Table<CachedNoiDung>()
                            .Where(x => x.MaDiem == maDiem)
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
            ChoPhepTTS = x.ChoPhepTTS,
            ThoiLuongGiay = x.ThoiLuongGiay
        }).ToList();
    }

    // ================== INIT ==================

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _db.CreateTableAsync<CachedPoi>();
        await _db.CreateTableAsync<CachedNoiDung>();

        _initialized = true;
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
    public bool ChoPhepTTS { get; set; }
    public int? ThoiLuongGiay { get; set; }
}
