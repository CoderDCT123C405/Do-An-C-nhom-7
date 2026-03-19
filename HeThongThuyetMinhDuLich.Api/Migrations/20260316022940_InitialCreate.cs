using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongThuyetMinhDuLich.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiDiemThamQuan",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiDiemThamQuan", x => x.MaLoai);
                });

            migrationBuilder.CreateTable(
                name: "NgonNgu",
                columns: table => new
                {
                    MaNgonNgu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNgonNguQuocTe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenNgonNgu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LaMacDinh = table.Column<bool>(type: "bit", nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NgonNgu", x => x.MaNgonNgu);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhauMaHoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MatKhauMaHoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaNgonNguMacDinh = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDung_NgonNgu_MaNgonNguMacDinh",
                        column: x => x.MaNgonNguMacDinh,
                        principalTable: "NgonNgu",
                        principalColumn: "MaNgonNgu");
                });

            migrationBuilder.CreateTable(
                name: "DiemThamQuan",
                columns: table => new
                {
                    MaDiem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDinhDanh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenDiem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTaNgan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ViDo = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    KinhDo = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    BanKinhKichHoat = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaLoai = table.Column<int>(type: "int", nullable: false),
                    MaTaiKhoanTao = table.Column<int>(type: "int", nullable: true),
                    MaTaiKhoanCapNhat = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemThamQuan", x => x.MaDiem);
                    table.ForeignKey(
                        name: "FK_DiemThamQuan_LoaiDiemThamQuan_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LoaiDiemThamQuan",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemThamQuan_TaiKhoan_MaTaiKhoanCapNhat",
                        column: x => x.MaTaiKhoanCapNhat,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                    table.ForeignKey(
                        name: "FK_DiemThamQuan_TaiKhoan_MaTaiKhoanTao",
                        column: x => x.MaTaiKhoanTao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhDiemThamQuan",
                columns: table => new
                {
                    MaHinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDiem = table.Column<int>(type: "int", nullable: false),
                    TenTepTin = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuongDanHinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LaAnhDaiDien = table.Column<bool>(type: "bit", nullable: false),
                    ThuTuHienThi = table.Column<int>(type: "int", nullable: true),
                    NgayTaiLen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaTaiKhoanTao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhDiemThamQuan", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhDiemThamQuan_DiemThamQuan_MaDiem",
                        column: x => x.MaDiem,
                        principalTable: "DiemThamQuan",
                        principalColumn: "MaDiem",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HinhAnhDiemThamQuan_TaiKhoan_MaTaiKhoanTao",
                        column: x => x.MaTaiKhoanTao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "MaQR",
                columns: table => new
                {
                    MaQR = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDiem = table.Column<int>(type: "int", nullable: false),
                    GiaTriQR = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaTaiKhoanTao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaQR", x => x.MaQR);
                    table.ForeignKey(
                        name: "FK_MaQR_DiemThamQuan_MaDiem",
                        column: x => x.MaDiem,
                        principalTable: "DiemThamQuan",
                        principalColumn: "MaDiem",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaQR_TaiKhoan_MaTaiKhoanTao",
                        column: x => x.MaTaiKhoanTao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "NoiDungThuyetMinh",
                columns: table => new
                {
                    MaNoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDiem = table.Column<int>(type: "int", nullable: false),
                    MaNgonNgu = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDungVanBan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuongDanAmThanh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChoPhepTTS = table.Column<bool>(type: "bit", nullable: false),
                    ThoiLuongGiay = table.Column<int>(type: "int", nullable: true),
                    MaTaiKhoanTao = table.Column<int>(type: "int", nullable: true),
                    MaTaiKhoanCapNhat = table.Column<int>(type: "int", nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoiDungThuyetMinh", x => x.MaNoiDung);
                    table.ForeignKey(
                        name: "FK_NoiDungThuyetMinh_DiemThamQuan_MaDiem",
                        column: x => x.MaDiem,
                        principalTable: "DiemThamQuan",
                        principalColumn: "MaDiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoiDungThuyetMinh_NgonNgu_MaNgonNgu",
                        column: x => x.MaNgonNgu,
                        principalTable: "NgonNgu",
                        principalColumn: "MaNgonNgu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NoiDungThuyetMinh_TaiKhoan_MaTaiKhoanCapNhat",
                        column: x => x.MaTaiKhoanCapNhat,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                    table.ForeignKey(
                        name: "FK_NoiDungThuyetMinh_TaiKhoan_MaTaiKhoanTao",
                        column: x => x.MaTaiKhoanTao,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "LichSuPhat",
                columns: table => new
                {
                    MaLichSuPhat = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    MaDiem = table.Column<int>(type: "int", nullable: false),
                    MaNoiDung = table.Column<int>(type: "int", nullable: false),
                    CachKichHoat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiLuongDaNghe = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuPhat", x => x.MaLichSuPhat);
                    table.CheckConstraint("CK_LichSuPhat_CachKichHoat", "[CachKichHoat] IN ('gps', 'qr', 'manual')");
                    table.CheckConstraint("CK_LichSuPhat_ThoiLuongDaNghe", "[ThoiLuongDaNghe] IS NULL OR [ThoiLuongDaNghe] >= 0");
                    table.ForeignKey(
                        name: "FK_LichSuPhat_DiemThamQuan_MaDiem",
                        column: x => x.MaDiem,
                        principalTable: "DiemThamQuan",
                        principalColumn: "MaDiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuPhat_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LichSuPhat_NoiDungThuyetMinh_MaNoiDung",
                        column: x => x.MaNoiDung,
                        principalTable: "NoiDungThuyetMinh",
                        principalColumn: "MaNoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiemThamQuan_MaDinhDanh",
                table: "DiemThamQuan",
                column: "MaDinhDanh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemThamQuan_MaLoai",
                table: "DiemThamQuan",
                column: "MaLoai");

            migrationBuilder.CreateIndex(
                name: "IX_DiemThamQuan_MaTaiKhoanCapNhat",
                table: "DiemThamQuan",
                column: "MaTaiKhoanCapNhat");

            migrationBuilder.CreateIndex(
                name: "IX_DiemThamQuan_MaTaiKhoanTao",
                table: "DiemThamQuan",
                column: "MaTaiKhoanTao");

            migrationBuilder.CreateIndex(
                name: "IX_DiemThamQuan_ViDo_KinhDo",
                table: "DiemThamQuan",
                columns: new[] { "ViDo", "KinhDo" });

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhDiemThamQuan_MaDiem",
                table: "HinhAnhDiemThamQuan",
                column: "MaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhDiemThamQuan_MaTaiKhoanTao",
                table: "HinhAnhDiemThamQuan",
                column: "MaTaiKhoanTao");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuPhat_MaDiem",
                table: "LichSuPhat",
                column: "MaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuPhat_MaNguoiDung",
                table: "LichSuPhat",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuPhat_MaNoiDung",
                table: "LichSuPhat",
                column: "MaNoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuPhat_ThoiGianBatDau",
                table: "LichSuPhat",
                column: "ThoiGianBatDau");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiDiemThamQuan_TenLoai",
                table: "LoaiDiemThamQuan",
                column: "TenLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaQR_GiaTriQR",
                table: "MaQR",
                column: "GiaTriQR",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaQR_MaDiem",
                table: "MaQR",
                column: "MaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_MaQR_MaTaiKhoanTao",
                table: "MaQR",
                column: "MaTaiKhoanTao");

            migrationBuilder.CreateIndex(
                name: "IX_NgonNgu_MaNgonNguQuocTe",
                table: "NgonNgu",
                column: "MaNgonNguQuocTe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaNgonNguMacDinh",
                table: "NguoiDung",
                column: "MaNgonNguMacDinh");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true,
                filter: "[TenDangNhap] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThuyetMinh_MaDiem",
                table: "NoiDungThuyetMinh",
                column: "MaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThuyetMinh_MaDiem_MaNgonNgu",
                table: "NoiDungThuyetMinh",
                columns: new[] { "MaDiem", "MaNgonNgu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThuyetMinh_MaNgonNgu",
                table: "NoiDungThuyetMinh",
                column: "MaNgonNgu");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThuyetMinh_MaTaiKhoanCapNhat",
                table: "NoiDungThuyetMinh",
                column: "MaTaiKhoanCapNhat");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThuyetMinh_MaTaiKhoanTao",
                table: "NoiDungThuyetMinh",
                column: "MaTaiKhoanTao");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HinhAnhDiemThamQuan");

            migrationBuilder.DropTable(
                name: "LichSuPhat");

            migrationBuilder.DropTable(
                name: "MaQR");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "NoiDungThuyetMinh");

            migrationBuilder.DropTable(
                name: "DiemThamQuan");

            migrationBuilder.DropTable(
                name: "NgonNgu");

            migrationBuilder.DropTable(
                name: "LoaiDiemThamQuan");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
