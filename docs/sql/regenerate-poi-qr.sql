SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (
    SELECT MaDinhDanh
    FROM DiemThamQuan
    WHERE MaDinhDanh IS NULL OR LTRIM(RTRIM(MaDinhDanh)) = ''
)
BEGIN
    THROW 50001, 'Khong the tao QR vi ton tai POI chua co MaDinhDanh.', 1;
END;

IF EXISTS (
    SELECT UPPER(LTRIM(RTRIM(MaDinhDanh)))
    FROM DiemThamQuan
    GROUP BY UPPER(LTRIM(RTRIM(MaDinhDanh)))
    HAVING COUNT(*) > 1
)
BEGIN
    THROW 50002, 'Khong the tao QR vi ton tai MaDinhDanh bi trung.', 1;
END;

;WITH QrSource AS (
    SELECT
        d.MaDiem,
        GiaTriQrMoi = CONCAT('QR_', UPPER(LTRIM(RTRIM(d.MaDinhDanh))))
    FROM DiemThamQuan d
)
UPDATE q
SET
    q.GiaTriQR = s.GiaTriQrMoi,
    q.TrangThaiHoatDong = 1,
    q.NgayCapNhat = SYSUTCDATETIME()
FROM MaQR q
INNER JOIN QrSource s ON s.MaDiem = q.MaDiem
WHERE q.GiaTriQR <> s.GiaTriQrMoi
   OR q.TrangThaiHoatDong = 0;

;WITH QrSource AS (
    SELECT
        d.MaDiem,
        GiaTriQrMoi = CONCAT('QR_', UPPER(LTRIM(RTRIM(d.MaDinhDanh))))
    FROM DiemThamQuan d
)
INSERT INTO MaQR (
    MaDiem,
    GiaTriQR,
    TrangThaiHoatDong,
    NgayTao,
    NgayCapNhat,
    MaTaiKhoanTao
)
SELECT
    s.MaDiem,
    s.GiaTriQrMoi,
    1,
    SYSUTCDATETIME(),
    SYSUTCDATETIME(),
    NULL
FROM QrSource s
LEFT JOIN MaQR q ON q.MaDiem = s.MaDiem
WHERE q.MaQR IS NULL;

COMMIT TRANSACTION;

SELECT
    d.MaDiem,
    d.MaDinhDanh,
    d.TenDiem,
    q.MaQR,
    q.GiaTriQR,
    q.TrangThaiHoatDong
FROM DiemThamQuan d
INNER JOIN MaQR q ON q.MaDiem = d.MaDiem
ORDER BY d.MaDiem;