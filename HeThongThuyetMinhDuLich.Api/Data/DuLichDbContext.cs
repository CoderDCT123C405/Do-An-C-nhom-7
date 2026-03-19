using HeThongThuyetMinhDuLich.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HeThongThuyetMinhDuLich.Api.Data;

public class DuLichDbContext(DbContextOptions<DuLichDbContext> options) : DbContext(options)
{
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<NgonNgu> NgonNgus => Set<NgonNgu>();
    public DbSet<LoaiDiemThamQuan> LoaiDiemThamQuans => Set<LoaiDiemThamQuan>();
    public DbSet<DiemThamQuan> DiemThamQuans => Set<DiemThamQuan>();
    public DbSet<NoiDungThuyetMinh> NoiDungThuyetMinhs => Set<NoiDungThuyetMinh>();
    public DbSet<HinhAnhDiemThamQuan> HinhAnhDiemThamQuans => Set<HinhAnhDiemThamQuan>();
    public DbSet<MaQr> MaQrs => Set<MaQr>();
    public DbSet<LichSuPhat> LichSuPhats => Set<LichSuPhat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan");
            entity.HasKey(x => x.MaTaiKhoan);
            entity.HasIndex(x => x.TenDangNhap).IsUnique();
            entity.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TenDangNhap).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MatKhauMaHoa).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<NgonNgu>(entity =>
        {
            entity.ToTable("NgonNgu");
            entity.HasKey(x => x.MaNgonNgu);
            entity.HasIndex(x => x.MaNgonNguQuocTe).IsUnique();
            entity.Property(x => x.MaNgonNguQuocTe).HasMaxLength(10).IsRequired();
            entity.Property(x => x.TenNgonNgu).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("NguoiDung");
            entity.HasKey(x => x.MaNguoiDung);
            entity.HasIndex(x => x.TenDangNhap).IsUnique().HasFilter("[TenDangNhap] IS NOT NULL");
            entity.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            entity.Property(x => x.TenDangNhap).HasMaxLength(50);
            entity.Property(x => x.MatKhauMaHoa).HasMaxLength(255);
            entity.Property(x => x.HoTen).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.SoDienThoai).HasMaxLength(20);
            entity.HasOne(x => x.NgonNguMacDinh)
                .WithMany(x => x.NguoiDungs)
                .HasForeignKey(x => x.MaNgonNguMacDinh);
        });

        modelBuilder.Entity<LoaiDiemThamQuan>(entity =>
        {
            entity.ToTable("LoaiDiemThamQuan");
            entity.HasKey(x => x.MaLoai);
            entity.HasIndex(x => x.TenLoai).IsUnique();
            entity.Property(x => x.TenLoai).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MoTa).HasMaxLength(255);
        });

        modelBuilder.Entity<DiemThamQuan>(entity =>
        {
            entity.ToTable("DiemThamQuan");
            entity.HasKey(x => x.MaDiem);
            entity.HasIndex(x => x.MaDinhDanh).IsUnique();
            entity.HasIndex(x => x.MaLoai);
            entity.HasIndex(x => new { x.ViDo, x.KinhDo });
            entity.Property(x => x.MaDinhDanh).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TenDiem).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MoTaNgan).HasMaxLength(500);
            entity.Property(x => x.BanKinhKichHoat).HasPrecision(8, 2);
            entity.Property(x => x.ViDo).HasPrecision(10, 7);
            entity.Property(x => x.KinhDo).HasPrecision(10, 7);
            entity.Property(x => x.DiaChi).HasMaxLength(255);
            entity.HasOne(x => x.LoaiDiemThamQuan)
                .WithMany(x => x.DiemThamQuans)
                .HasForeignKey(x => x.MaLoai)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TaiKhoanTao)
                .WithMany(x => x.DiemThamQuanDaTaos)
                .HasForeignKey(x => x.MaTaiKhoanTao)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.TaiKhoanCapNhat)
                .WithMany(x => x.DiemThamQuanDaCapNhats)
                .HasForeignKey(x => x.MaTaiKhoanCapNhat)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<NoiDungThuyetMinh>(entity =>
        {
            entity.ToTable("NoiDungThuyetMinh");
            entity.HasKey(x => x.MaNoiDung);
            entity.HasIndex(x => new { x.MaDiem, x.MaNgonNgu }).IsUnique();
            entity.HasIndex(x => x.MaDiem);
            entity.HasIndex(x => x.MaNgonNgu);
            entity.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DuongDanAmThanh).HasMaxLength(255);
            entity.HasOne(x => x.DiemThamQuan)
                .WithMany(x => x.NoiDungThuyetMinhs)
                .HasForeignKey(x => x.MaDiem)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.NgonNgu)
                .WithMany(x => x.NoiDungThuyetMinhs)
                .HasForeignKey(x => x.MaNgonNgu)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TaiKhoanTao)
                .WithMany(x => x.NoiDungDaTaos)
                .HasForeignKey(x => x.MaTaiKhoanTao)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.TaiKhoanCapNhat)
                .WithMany(x => x.NoiDungDaCapNhats)
                .HasForeignKey(x => x.MaTaiKhoanCapNhat)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<HinhAnhDiemThamQuan>(entity =>
        {
            entity.ToTable("HinhAnhDiemThamQuan");
            entity.HasKey(x => x.MaHinhAnh);
            entity.HasIndex(x => x.MaDiem);
            entity.Property(x => x.TenTepTin).HasMaxLength(255).IsRequired();
            entity.Property(x => x.DuongDanHinhAnh).HasMaxLength(255).IsRequired();
            entity.HasOne(x => x.DiemThamQuan)
                .WithMany(x => x.HinhAnhDiemThamQuans)
                .HasForeignKey(x => x.MaDiem)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TaiKhoanTao)
                .WithMany(x => x.HinhAnhDaTaos)
                .HasForeignKey(x => x.MaTaiKhoanTao)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MaQr>(entity =>
        {
            entity.ToTable("MaQR");
            entity.HasKey(x => x.MaQR);
            entity.HasIndex(x => x.GiaTriQR).IsUnique();
            entity.HasIndex(x => x.MaDiem);
            entity.Property(x => x.GiaTriQR).HasMaxLength(255).IsRequired();
            entity.HasOne(x => x.DiemThamQuan)
                .WithMany(x => x.MaQrs)
                .HasForeignKey(x => x.MaDiem)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TaiKhoanTao)
                .WithMany(x => x.MaQrDaTaos)
                .HasForeignKey(x => x.MaTaiKhoanTao)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<LichSuPhat>(entity =>
        {
            entity.ToTable("LichSuPhat");
            entity.HasKey(x => x.MaLichSuPhat);
            entity.HasIndex(x => x.MaNguoiDung);
            entity.HasIndex(x => x.MaDiem);
            entity.HasIndex(x => x.MaNoiDung);
            entity.HasIndex(x => x.ThoiGianBatDau);
            entity.Property(x => x.CachKichHoat).HasMaxLength(20).IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_LichSuPhat_CachKichHoat", "[CachKichHoat] IN ('gps', 'qr', 'manual')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_LichSuPhat_ThoiLuongDaNghe", "[ThoiLuongDaNghe] IS NULL OR [ThoiLuongDaNghe] >= 0"));
            entity.HasOne(x => x.NguoiDung)
                .WithMany(x => x.LichSuPhats)
                .HasForeignKey(x => x.MaNguoiDung)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.DiemThamQuan)
                .WithMany(x => x.LichSuPhats)
                .HasForeignKey(x => x.MaDiem)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.NoiDungThuyetMinh)
                .WithMany(x => x.LichSuPhats)
                .HasForeignKey(x => x.MaNoiDung)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
