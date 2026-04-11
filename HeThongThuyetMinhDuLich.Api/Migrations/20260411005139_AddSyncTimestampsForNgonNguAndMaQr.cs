using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongThuyetMinhDuLich.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncTimestampsForNgonNguAndMaQr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "NgonNgu",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "NgonNgu",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "MaQR",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("UPDATE [NgonNgu] SET [NgayCapNhat] = COALESCE([NgayCapNhat], [NgayTao])");
            migrationBuilder.Sql("UPDATE [MaQR] SET [NgayCapNhat] = COALESCE([NgayCapNhat], [NgayTao])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "NgonNgu");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "NgonNgu");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "MaQR");
        }
    }
}
