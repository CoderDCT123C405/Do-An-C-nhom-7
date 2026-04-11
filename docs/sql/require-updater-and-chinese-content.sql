SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @FallbackTaiKhoanId int;
SELECT TOP (1) @FallbackTaiKhoanId = MaTaiKhoan
FROM dbo.TaiKhoan
WHERE TrangThaiHoatDong = 1
ORDER BY CASE WHEN MaTaiKhoan = 1 THEN 0 ELSE 1 END, MaTaiKhoan;

IF @FallbackTaiKhoanId IS NULL
BEGIN
    RAISERROR(N'Khong tim thay tai khoan quan tri de backfill MaTaiKhoanCapNhat.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

UPDATE dbo.DiemThamQuan
SET MaTaiKhoanCapNhat = COALESCE(MaTaiKhoanCapNhat, MaTaiKhoanTao, @FallbackTaiKhoanId),
    NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
WHERE MaTaiKhoanCapNhat IS NULL;

UPDATE dbo.NoiDungThuyetMinh
SET MaTaiKhoanCapNhat = COALESCE(MaTaiKhoanCapNhat, MaTaiKhoanTao, @FallbackTaiKhoanId),
    NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
WHERE MaTaiKhoanCapNhat IS NULL;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.DiemThamQuan')
      AND name = N'MaTaiKhoanCapNhat'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.DiemThamQuan ALTER COLUMN MaTaiKhoanCapNhat int NOT NULL;
END;

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.NoiDungThuyetMinh')
      AND name = N'MaTaiKhoanCapNhat'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.NoiDungThuyetMinh ALTER COLUMN MaTaiKhoanCapNhat int NOT NULL;
END;

UPDATE dbo.NgonNgu
SET TenNgonNgu = N'Tiếng Việt',
    TrangThaiHoatDong = 1,
    NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
WHERE MaNgonNguQuocTe = N'vi';

UPDATE dbo.NgonNgu
SET TenNgonNgu = N'Tiếng Anh',
    TrangThaiHoatDong = 1,
    NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
WHERE MaNgonNguQuocTe = N'en';

IF EXISTS (SELECT 1 FROM dbo.NgonNgu WHERE MaNgonNguQuocTe = N'ko')
BEGIN
    UPDATE dbo.NgonNgu
    SET MaNgonNguQuocTe = N'zh-CN',
        TenNgonNgu = N'Tiếng Trung',
        TrangThaiHoatDong = 1,
        NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
    WHERE MaNgonNguQuocTe = N'ko';
END;
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.NgonNgu WHERE MaNgonNguQuocTe = N'zh-CN')
BEGIN
    INSERT INTO dbo.NgonNgu (MaNgonNguQuocTe, TenNgonNgu, LaMacDinh, TrangThaiHoatDong, NgayTao, NgayCapNhat)
    VALUES (N'zh-CN', N'Tiếng Trung', 0, 1, GETUTCDATE(), GETUTCDATE());
END;
ELSE
BEGIN
    UPDATE dbo.NgonNgu
    SET TenNgonNgu = N'Tiếng Trung',
        TrangThaiHoatDong = 1,
        NgayCapNhat = COALESCE(NgayCapNhat, GETUTCDATE())
    WHERE MaNgonNguQuocTe = N'zh-CN';
END;

DECLARE @ViId int = (SELECT TOP (1) MaNgonNgu FROM dbo.NgonNgu WHERE MaNgonNguQuocTe = N'vi');
DECLARE @EnId int = (SELECT TOP (1) MaNgonNgu FROM dbo.NgonNgu WHERE MaNgonNguQuocTe = N'en');
DECLARE @ZhId int = (SELECT TOP (1) MaNgonNgu FROM dbo.NgonNgu WHERE MaNgonNguQuocTe = N'zh-CN');
DECLARE @TargetMaDiem int = (SELECT TOP (1) MaDiem FROM dbo.DiemThamQuan ORDER BY MaDiem);
DECLARE @TargetTenDiem nvarchar(200) = (SELECT TOP (1) TenDiem FROM dbo.DiemThamQuan WHERE MaDiem = @TargetMaDiem);

IF @TargetMaDiem IS NOT NULL AND @ViId IS NOT NULL AND @EnId IS NOT NULL AND @ZhId IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.NoiDungThuyetMinh
        WHERE MaDiem = @TargetMaDiem AND MaNgonNgu = @ViId
    )
    BEGIN
        INSERT INTO dbo.NoiDungThuyetMinh
        (
            MaDiem,
            MaNgonNgu,
            TieuDe,
            NoiDungVanBan,
            DuongDanAmThanh,
            ChoPhepTTS,
            ThoiLuongGiay,
            MaTaiKhoanTao,
            MaTaiKhoanCapNhat,
            TrangThaiHoatDong,
            NgayTao,
            NgayCapNhat
        )
        VALUES
        (
            @TargetMaDiem,
            @ViId,
            N'Thuyết minh - ' + COALESCE(@TargetTenDiem, N'Điểm tham quan'),
            N'Giới thiệu ' + COALESCE(@TargetTenDiem, N'điểm tham quan') + N'. Nội dung tiếng Việt có dấu được bổ sung để đủ ba ngôn ngữ mặc định.',
            NULL,
            1,
            NULL,
            @FallbackTaiKhoanId,
            @FallbackTaiKhoanId,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.NoiDungThuyetMinh
        WHERE MaDiem = @TargetMaDiem AND MaNgonNgu = @EnId
    )
    BEGIN
        INSERT INTO dbo.NoiDungThuyetMinh
        (
            MaDiem,
            MaNgonNgu,
            TieuDe,
            NoiDungVanBan,
            DuongDanAmThanh,
            ChoPhepTTS,
            ThoiLuongGiay,
            MaTaiKhoanTao,
            MaTaiKhoanCapNhat,
            TrangThaiHoatDong,
            NgayTao,
            NgayCapNhat
        )
        VALUES
        (
            @TargetMaDiem,
            @EnId,
            N'Audio guide - ' + COALESCE(@TargetTenDiem, N'Point of interest'),
            N'Introduction to ' + COALESCE(@TargetTenDiem, N'this point of interest') + N'. This English narration is inserted to complete the default 3-language set.',
            NULL,
            1,
            NULL,
            @FallbackTaiKhoanId,
            @FallbackTaiKhoanId,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.NoiDungThuyetMinh
        WHERE MaDiem = @TargetMaDiem AND MaNgonNgu = @ZhId
    )
    BEGIN
        INSERT INTO dbo.NoiDungThuyetMinh
        (
            MaDiem,
            MaNgonNgu,
            TieuDe,
            NoiDungVanBan,
            DuongDanAmThanh,
            ChoPhepTTS,
            ThoiLuongGiay,
            MaTaiKhoanTao,
            MaTaiKhoanCapNhat,
            TrangThaiHoatDong,
            NgayTao,
            NgayCapNhat
        )
        VALUES
        (
            @TargetMaDiem,
            @ZhId,
            N'语音导览 - ' + COALESCE(@TargetTenDiem, N'景点'),
            N'这里是 ' + COALESCE(@TargetTenDiem, N'该景点') + N' 的中文介绍。此内容用于补齐默认的三种讲解语言。',
            NULL,
            1,
            NULL,
            @FallbackTaiKhoanId,
            @FallbackTaiKhoanId,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );
    END;
END;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM dbo.__EFMigrationsHistory
        WHERE MigrationId = N'20260411124028_RequireUpdaterAuditFieldsAndChineseContent'
    )
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260411124028_RequireUpdaterAuditFieldsAndChineseContent', N'8.0.14');
END;

COMMIT TRANSACTION;
