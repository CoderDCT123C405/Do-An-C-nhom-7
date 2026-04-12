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

            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    """
                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[dbo].[NoiDungThuyetMinh]')
                          AND name = N'MaTaiKhoanCapNhat'
                          AND is_nullable = 1
                    )
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM sys.indexes
                            WHERE name = N'IX_NoiDungThuyetMinh_MaTaiKhoanCapNhat'
                              AND object_id = OBJECT_ID(N'[dbo].[NoiDungThuyetMinh]')
                        )
                        BEGIN
                            DROP INDEX [IX_NoiDungThuyetMinh_MaTaiKhoanCapNhat] ON [dbo].[NoiDungThuyetMinh];
                        END;

                        ALTER TABLE [dbo].[NoiDungThuyetMinh] ALTER COLUMN [MaTaiKhoanCapNhat] int NOT NULL;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM sys.indexes
                            WHERE name = N'IX_NoiDungThuyetMinh_MaTaiKhoanCapNhat'
                              AND object_id = OBJECT_ID(N'[dbo].[NoiDungThuyetMinh]')
                        )
                        BEGIN
                            CREATE INDEX [IX_NoiDungThuyetMinh_MaTaiKhoanCapNhat] ON [dbo].[NoiDungThuyetMinh] ([MaTaiKhoanCapNhat]);
                        END;
                    END;
                    """);

                migrationBuilder.Sql(
                    """
                    IF EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[dbo].[DiemThamQuan]')
                          AND name = N'MaTaiKhoanCapNhat'
                          AND is_nullable = 1
                    )
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM sys.indexes
                            WHERE name = N'IX_DiemThamQuan_MaTaiKhoanCapNhat'
                              AND object_id = OBJECT_ID(N'[dbo].[DiemThamQuan]')
                        )
                        BEGIN
                            DROP INDEX [IX_DiemThamQuan_MaTaiKhoanCapNhat] ON [dbo].[DiemThamQuan];
                        END;

                        ALTER TABLE [dbo].[DiemThamQuan] ALTER COLUMN [MaTaiKhoanCapNhat] int NOT NULL;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM sys.indexes
                            WHERE name = N'IX_DiemThamQuan_MaTaiKhoanCapNhat'
                              AND object_id = OBJECT_ID(N'[dbo].[DiemThamQuan]')
                        )
                        BEGIN
                            CREATE INDEX [IX_DiemThamQuan_MaTaiKhoanCapNhat] ON [dbo].[DiemThamQuan] ([MaTaiKhoanCapNhat]);
                        END;
                    END;
                    """);
            }
            else
            {
                migrationBuilder.AlterColumn<int>(
                    name: "MaTaiKhoanCapNhat",
                    table: "NoiDungThuyetMinh",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER",
                    oldNullable: true);

                migrationBuilder.AlterColumn<int>(
                    name: "MaTaiKhoanCapNhat",
                    table: "DiemThamQuan",
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "INTEGER",
                    oldNullable: true);
            }
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
