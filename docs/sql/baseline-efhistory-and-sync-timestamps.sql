SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory
    (
        MigrationId nvarchar(150) NOT NULL,
        ProductVersion nvarchar(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (MigrationId)
    );
END;

IF COL_LENGTH('dbo.NgonNgu', 'NgayTao') IS NULL
BEGIN
    ALTER TABLE dbo.NgonNgu
    ADD NgayTao datetime2 NOT NULL
        CONSTRAINT DF_NgonNgu_NgayTao DEFAULT GETUTCDATE();
END;

IF COL_LENGTH('dbo.NgonNgu', 'NgayCapNhat') IS NULL
BEGIN
    ALTER TABLE dbo.NgonNgu
    ADD NgayCapNhat datetime2 NULL;
END;

IF COL_LENGTH('dbo.MaQR', 'NgayCapNhat') IS NULL
BEGIN
    ALTER TABLE dbo.MaQR
    ADD NgayCapNhat datetime2 NULL;
END;

EXEC sys.sp_executesql N'
UPDATE dbo.NgonNgu
SET NgayTao = COALESCE(NgayTao, GETUTCDATE()),
    NgayCapNhat = COALESCE(NgayCapNhat, NgayTao, GETUTCDATE());';

EXEC sys.sp_executesql N'
UPDATE dbo.MaQR
SET NgayCapNhat = COALESCE(NgayCapNhat, NgayTao, GETUTCDATE());';

IF NOT EXISTS (
    SELECT 1
    FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260316022940_InitialCreate'
)
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260316022940_InitialCreate', N'8.0.14');
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260411005139_AddSyncTimestampsForNgonNguAndMaQr'
)
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260411005139_AddSyncTimestampsForNgonNguAndMaQr', N'8.0.14');
END;

COMMIT TRANSACTION;