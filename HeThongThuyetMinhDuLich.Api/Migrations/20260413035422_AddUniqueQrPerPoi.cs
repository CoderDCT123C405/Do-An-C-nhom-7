using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongThuyetMinhDuLich.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueQrPerPoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaQR_MaDiem",
                table: "MaQR");

            migrationBuilder.CreateIndex(
                name: "IX_MaQR_MaDiem",
                table: "MaQR",
                column: "MaDiem",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaQR_MaDiem",
                table: "MaQR");

            migrationBuilder.CreateIndex(
                name: "IX_MaQR_MaDiem",
                table: "MaQR",
                column: "MaDiem");
        }
    }
}
