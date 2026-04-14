SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @AdminId INT = (
    SELECT TOP 1 MaTaiKhoan
    FROM TaiKhoan
    WHERE VaiTro = N'Admin'
    ORDER BY MaTaiKhoan
);

DECLARE @ImageSeed TABLE
(
    MaDinhDanh NVARCHAR(50) NOT NULL,
    TenTepTin NVARCHAR(255) NOT NULL,
    DuongDanHinhAnh NVARCHAR(255) NOT NULL,
    LaAnhDaiDien BIT NOT NULL,
    ThuTuHienThi INT NULL
);

INSERT INTO @ImageSeed (MaDinhDanh, TenTepTin, DuongDanHinhAnh, LaAnhDaiDien, ThuTuHienThi)
VALUES
    (N'POI001', N'cover.jpg', N'/images/poi/POI001/cover.jpg', 1, 0),
    (N'POI001', N'01.jpg', N'/images/poi/POI001/01.jpg', 0, 1),
    (N'POI002', N'cover.jpg', N'/images/poi/POI002/cover.jpg', 1, 0),
    (N'POI002', N'01.jpg', N'/images/poi/POI002/01.jpg', 0, 1),
    (N'VK100', N'cover.jpg', N'/images/poi/VK100/cover.jpg', 1, 0),
    (N'VK100', N'01.jpg', N'/images/poi/VK100/01.jpg', 0, 1),
    (N'VK101', N'cover.jpg', N'/images/poi/VK101/cover.jpg', 1, 0),
    (N'VK101', N'01.jpg', N'/images/poi/VK101/01.jpg', 0, 1),
    (N'VK102', N'cover.jpg', N'/images/poi/VK102/cover.jpg', 1, 0),
    (N'VK102', N'01.jpg', N'/images/poi/VK102/01.jpg', 0, 1),
    (N'VK103', N'cover.jpg', N'/images/poi/VK103/cover.jpg', 1, 0),
    (N'VK103', N'01.jpg', N'/images/poi/VK103/01.jpg', 0, 1),
    (N'VK104', N'cover.jpg', N'/images/poi/VK104/cover.jpg', 1, 0),
    (N'VK104', N'01.jpg', N'/images/poi/VK104/01.jpg', 0, 1),
    (N'VK105', N'cover.jpg', N'/images/poi/VK105/cover.jpg', 1, 0),
    (N'VK105', N'01.jpg', N'/images/poi/VK105/01.jpg', 0, 1),
    (N'VK106', N'cover.jpg', N'/images/poi/VK106/cover.jpg', 1, 0),
    (N'VK106', N'01.jpg', N'/images/poi/VK106/01.jpg', 0, 1),
    (N'VK107', N'cover.jpg', N'/images/poi/VK107/cover.jpg', 1, 0),
    (N'VK107', N'01.jpg', N'/images/poi/VK107/01.jpg', 0, 1),
    (N'VK108', N'cover.jpg', N'/images/poi/VK108/cover.jpg', 1, 0),
    (N'VK108', N'01.jpg', N'/images/poi/VK108/01.jpg', 0, 1),
    (N'VK109', N'cover.jpg', N'/images/poi/VK109/cover.jpg', 1, 0),
    (N'VK109', N'01.jpg', N'/images/poi/VK109/01.jpg', 0, 1),
    (N'VK110', N'cover.jpg', N'/images/poi/VK110/cover.jpg', 1, 0),
    (N'VK110', N'01.jpg', N'/images/poi/VK110/01.jpg', 0, 1),
    (N'VK111', N'cover.jpg', N'/images/poi/VK111/cover.jpg', 1, 0),
    (N'VK111', N'01.jpg', N'/images/poi/VK111/01.jpg', 0, 1),
    (N'VK112', N'cover.jpg', N'/images/poi/VK112/cover.jpg', 1, 0),
    (N'VK112', N'01.jpg', N'/images/poi/VK112/01.jpg', 0, 1),
    (N'VK113', N'cover.jpg', N'/images/poi/VK113/cover.jpg', 1, 0),
    (N'VK113', N'01.jpg', N'/images/poi/VK113/01.jpg', 0, 1),
    (N'VK114', N'cover.jpg', N'/images/poi/VK114/cover.jpg', 1, 0),
    (N'VK114', N'01.jpg', N'/images/poi/VK114/01.jpg', 0, 1),
    (N'VK115', N'cover.jpg', N'/images/poi/VK115/cover.jpg', 1, 0),
    (N'VK115', N'01.jpg', N'/images/poi/VK115/01.jpg', 0, 1),
    (N'VK116', N'cover.jpg', N'/images/poi/VK116/cover.jpg', 1, 0),
    (N'VK116', N'01.jpg', N'/images/poi/VK116/01.jpg', 0, 1),
    (N'VK117', N'cover.jpg', N'/images/poi/VK117/cover.jpg', 1, 0),
    (N'VK117', N'01.jpg', N'/images/poi/VK117/01.jpg', 0, 1),
    (N'VK118', N'cover.jpg', N'/images/poi/VK118/cover.jpg', 1, 0),
    (N'VK118', N'01.jpg', N'/images/poi/VK118/01.jpg', 0, 1),
    (N'VK119', N'cover.jpg', N'/images/poi/VK119/cover.jpg', 1, 0),
    (N'VK119', N'01.jpg', N'/images/poi/VK119/01.jpg', 0, 1),
    (N'VK120', N'cover.jpg', N'/images/poi/VK120/cover.jpg', 1, 0),
    (N'VK120', N'01.jpg', N'/images/poi/VK120/01.jpg', 0, 1),
    (N'VK140', N'cover.jpg', N'/images/poi/VK140/cover.jpg', 1, 0),
    (N'VK140', N'01.jpg', N'/images/poi/VK140/01.jpg', 0, 1),
    (N'VK999', N'cover.jpg', N'/images/poi/VK999/cover.jpg', 1, 0),
    (N'VK999', N'01.jpg', N'/images/poi/VK999/01.jpg', 0, 1);

DECLARE @MissingPoi TABLE (MaDinhDanh NVARCHAR(50) NOT NULL PRIMARY KEY);

INSERT INTO @MissingPoi (MaDinhDanh)
SELECT DISTINCT seed.MaDinhDanh
FROM @ImageSeed AS seed
LEFT JOIN DiemThamQuan AS poi ON poi.MaDinhDanh = seed.MaDinhDanh
WHERE poi.MaDiem IS NULL;

IF EXISTS (SELECT 1 FROM @MissingPoi)
BEGIN
    SELECT MaDinhDanh AS PoiChuaTonTaiTrongDb
    FROM @MissingPoi
    ORDER BY MaDinhDanh;

    RAISERROR (N'Khong the seed anh vi co POI chua ton tai trong bang DiemThamQuan.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

;WITH TargetPoi AS
(
    SELECT DISTINCT poi.MaDiem
    FROM DiemThamQuan AS poi
    INNER JOIN @ImageSeed AS seed ON seed.MaDinhDanh = poi.MaDinhDanh
)
DELETE imageRow
FROM HinhAnhDiemThamQuan AS imageRow
INNER JOIN TargetPoi AS target ON target.MaDiem = imageRow.MaDiem;

INSERT INTO HinhAnhDiemThamQuan
(
    MaDiem,
    TenTepTin,
    DuongDanHinhAnh,
    LaAnhDaiDien,
    ThuTuHienThi,
    NgayTaiLen,
    MaTaiKhoanTao
)
SELECT
    poi.MaDiem,
    seed.TenTepTin,
    seed.DuongDanHinhAnh,
    seed.LaAnhDaiDien,
    seed.ThuTuHienThi,
    SYSUTCDATETIME(),
    @AdminId
FROM @ImageSeed AS seed
INNER JOIN DiemThamQuan AS poi ON poi.MaDinhDanh = seed.MaDinhDanh;

SELECT
    poi.MaDinhDanh,
    COUNT(*) AS SoLuongAnh,
    SUM(CASE WHEN imageRow.LaAnhDaiDien = 1 THEN 1 ELSE 0 END) AS SoAnhDaiDien
FROM HinhAnhDiemThamQuan AS imageRow
INNER JOIN DiemThamQuan AS poi ON poi.MaDiem = imageRow.MaDiem
WHERE poi.MaDinhDanh IN (SELECT DISTINCT MaDinhDanh FROM @ImageSeed)
GROUP BY poi.MaDinhDanh
ORDER BY poi.MaDinhDanh;

COMMIT TRANSACTION;