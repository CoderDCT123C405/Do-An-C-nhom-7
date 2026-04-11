using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongThuyetMinhDuLich.Api.Migrations
{
    /// <inheritdoc />
    public partial class RequireUpdaterAuditFieldsAndChineseContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [NoiDungThuyetMinh] SET [MaTaiKhoanCapNhat] = COALESCE([MaTaiKhoanTao], 1) WHERE [MaTaiKhoanCapNhat] IS NULL");
            migrationBuilder.Sql("UPDATE [DiemThamQuan] SET [MaTaiKhoanCapNhat] = COALESCE([MaTaiKhoanTao], 1) WHERE [MaTaiKhoanCapNhat] IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "MaTaiKhoanCapNhat",
                table: "NoiDungThuyetMinh",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaTaiKhoanCapNhat",
                table: "DiemThamQuan",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaTaiKhoanCapNhat",
                table: "NoiDungThuyetMinh",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaTaiKhoanCapNhat",
                table: "DiemThamQuan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
