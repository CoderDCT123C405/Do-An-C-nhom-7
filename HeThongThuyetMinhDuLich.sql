CREATE DATABASE HeThongThuyetMinhDuLich;
GO

USE HeThongThuyetMinhDuLich;
GO

CREATE TABLE TaiKhoan (
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhauMaHoa VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NULL,
    VaiTro NVARCHAR(50) NULL,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL
);
GO

CREATE TABLE NgonNgu (
    MaNgonNgu INT IDENTITY(1,1) PRIMARY KEY,
    MaNgonNguQuocTe VARCHAR(10) NOT NULL UNIQUE,
    TenNgonNgu NVARCHAR(100) NOT NULL,
    LaMacDinh BIT NOT NULL DEFAULT 0,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1
);
GO

CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NULL UNIQUE,
    MatKhauMaHoa VARCHAR(255) NULL,
    HoTen NVARCHAR(100) NULL,
    Email VARCHAR(100) NULL UNIQUE,
    SoDienThoai VARCHAR(20) NULL,
    MaNgonNguMacDinh INT NULL,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL,
    CONSTRAINT FK_NguoiDung_NgonNgu
        FOREIGN KEY (MaNgonNguMacDinh) REFERENCES NgonNgu(MaNgonNgu)
);
GO

CREATE TABLE LoaiDiemThamQuan (
    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(255) NULL,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL
);
GO

CREATE TABLE DiemThamQuan (
    MaDiem INT IDENTITY(1,1) PRIMARY KEY,
    MaDinhDanh VARCHAR(50) NOT NULL UNIQUE,
    TenDiem NVARCHAR(200) NOT NULL,
    MoTaNgan NVARCHAR(500) NULL,
    ViDo DECIMAL(10,7) NOT NULL,
    KinhDo DECIMAL(10,7) NOT NULL,
    BanKinhKichHoat DECIMAL(8,2) NOT NULL,
    DiaChi NVARCHAR(255) NULL,
    MaLoai INT NOT NULL,
    MaTaiKhoanTao INT NULL,
    MaTaiKhoanCapNhat INT NULL,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL,
    CONSTRAINT FK_DiemThamQuan_LoaiDiemThamQuan
        FOREIGN KEY (MaLoai) REFERENCES LoaiDiemThamQuan(MaLoai),
    CONSTRAINT FK_DiemThamQuan_TaiKhoanTao
        FOREIGN KEY (MaTaiKhoanTao) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT FK_DiemThamQuan_TaiKhoanCapNhat
        FOREIGN KEY (MaTaiKhoanCapNhat) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

CREATE TABLE NoiDungThuyetMinh (
    MaNoiDung INT IDENTITY(1,1) PRIMARY KEY,
    MaDiem INT NOT NULL,
    MaNgonNgu INT NOT NULL,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDungVanBan NVARCHAR(MAX) NULL,
    DuongDanAmThanh VARCHAR(255) NULL,
    ChoPhepTTS BIT NOT NULL DEFAULT 1,
    ThoiLuongGiay INT NULL,
    MaTaiKhoanTao INT NULL,
    MaTaiKhoanCapNhat INT NULL,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCapNhat DATETIME NULL,
    CONSTRAINT FK_NoiDungThuyetMinh_DiemThamQuan
        FOREIGN KEY (MaDiem) REFERENCES DiemThamQuan(MaDiem),
    CONSTRAINT FK_NoiDungThuyetMinh_NgonNgu
        FOREIGN KEY (MaNgonNgu) REFERENCES NgonNgu(MaNgonNgu),
    CONSTRAINT FK_NoiDungThuyetMinh_TaiKhoanTao
        FOREIGN KEY (MaTaiKhoanTao) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT FK_NoiDungThuyetMinh_TaiKhoanCapNhat
        FOREIGN KEY (MaTaiKhoanCapNhat) REFERENCES TaiKhoan(MaTaiKhoan),
    CONSTRAINT UQ_NoiDungThuyetMinh_MaDiem_MaNgonNgu
        UNIQUE (MaDiem, MaNgonNgu)
);
GO

CREATE TABLE HinhAnhDiemThamQuan (
    MaHinhAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaDiem INT NOT NULL,
    TenTepTin NVARCHAR(255) NOT NULL,
    DuongDanHinhAnh VARCHAR(255) NOT NULL,
    LaAnhDaiDien BIT NOT NULL DEFAULT 0,
    ThuTuHienThi INT NULL,
    NgayTaiLen DATETIME NOT NULL DEFAULT GETDATE(),
    MaTaiKhoanTao INT NULL,
    CONSTRAINT FK_HinhAnhDiemThamQuan_DiemThamQuan
        FOREIGN KEY (MaDiem) REFERENCES DiemThamQuan(MaDiem),
    CONSTRAINT FK_HinhAnhDiemThamQuan_TaiKhoanTao
        FOREIGN KEY (MaTaiKhoanTao) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

CREATE TABLE MaQR (
    MaQR INT IDENTITY(1,1) PRIMARY KEY,
    MaDiem INT NOT NULL,
    GiaTriQR VARCHAR(255) NOT NULL UNIQUE,
    TrangThaiHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    MaTaiKhoanTao INT NULL,
    CONSTRAINT FK_MaQR_DiemThamQuan
        FOREIGN KEY (MaDiem) REFERENCES DiemThamQuan(MaDiem),
    CONSTRAINT FK_MaQR_TaiKhoanTao
        FOREIGN KEY (MaTaiKhoanTao) REFERENCES TaiKhoan(MaTaiKhoan)
);
GO

CREATE TABLE LichSuPhat (
    MaLichSuPhat BIGINT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NULL,
    MaDiem INT NOT NULL,
    MaNoiDung INT NOT NULL,
    CachKichHoat VARCHAR(20) NOT NULL,
    ThoiGianBatDau DATETIME NOT NULL DEFAULT GETDATE(),
    ThoiLuongDaNghe INT NULL,
    CONSTRAINT FK_LichSuPhat_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_LichSuPhat_DiemThamQuan
        FOREIGN KEY (MaDiem) REFERENCES DiemThamQuan(MaDiem),
    CONSTRAINT FK_LichSuPhat_NoiDungThuyetMinh
        FOREIGN KEY (MaNoiDung) REFERENCES NoiDungThuyetMinh(MaNoiDung),
    CONSTRAINT CK_LichSuPhat_CachKichHoat
        CHECK (CachKichHoat IN ('gps', 'qr', 'manual')),
    CONSTRAINT CK_LichSuPhat_ThoiLuongDaNghe
        CHECK (ThoiLuongDaNghe IS NULL OR ThoiLuongDaNghe >= 0)
);
GO

CREATE INDEX IX_NguoiDung_MaNgonNguMacDinh
ON NguoiDung(MaNgonNguMacDinh);
GO

CREATE INDEX IX_DiemThamQuan_MaLoai
ON DiemThamQuan(MaLoai);
GO

CREATE INDEX IX_DiemThamQuan_ToaDo
ON DiemThamQuan(ViDo, KinhDo);
GO

CREATE INDEX IX_NoiDungThuyetMinh_MaDiem
ON NoiDungThuyetMinh(MaDiem);
GO

CREATE INDEX IX_NoiDungThuyetMinh_MaNgonNgu
ON NoiDungThuyetMinh(MaNgonNgu);
GO

CREATE INDEX IX_HinhAnhDiemThamQuan_MaDiem
ON HinhAnhDiemThamQuan(MaDiem);
GO

CREATE INDEX IX_MaQR_MaDiem
ON MaQR(MaDiem);
GO

CREATE INDEX IX_LichSuPhat_MaNguoiDung
ON LichSuPhat(MaNguoiDung);
GO

CREATE INDEX IX_LichSuPhat_MaDiem
ON LichSuPhat(MaDiem);
GO

CREATE INDEX IX_LichSuPhat_MaNoiDung
ON LichSuPhat(MaNoiDung);
GO

CREATE INDEX IX_LichSuPhat_ThoiGianBatDau
ON LichSuPhat(ThoiGianBatDau);
GO

INSERT INTO TaiKhoan (
    TenDangNhap,
    MatKhauMaHoa,
    HoTen,
    Email,
    VaiTro,
    TrangThaiHoatDong
)
VALUES
    ('admin', 'hashed_admin_123', N'Quan tri vien', 'admin@thuyetminhdulich.vn', N'Admin', 1),
    ('bien_tap_01', 'hashed_editor_123', N'Nhan vien bien tap', 'bientap01@thuyetminhdulich.vn', N'BienTap', 1);
GO

INSERT INTO NgonNgu (
    MaNgonNguQuocTe,
    TenNgonNgu,
    LaMacDinh,
    TrangThaiHoatDong
)
VALUES
    ('vi', N'Tieng Viet', 1, 1),
    ('en', N'Tieng Anh', 0, 1),
    ('ko', N'Tieng Han', 0, 1);
GO

INSERT INTO NguoiDung (
    TenDangNhap,
    MatKhauMaHoa,
    HoTen,
    Email,
    SoDienThoai,
    MaNgonNguMacDinh,
    TrangThaiHoatDong
)
VALUES
    ('khach01', 'hashed_user_01', N'Nguyen Van A', 'khach01@email.com', '0901000001', 1, 1),
    ('khach02', 'hashed_user_02', N'Tran Thi B', 'khach02@email.com', '0901000002', 2, 1);
GO

INSERT INTO LoaiDiemThamQuan (
    TenLoai,
    MoTa,
    TrangThaiHoatDong
)
VALUES
    (N'Khu di tich', N'Cac dia diem mang gia tri lich su va van hoa', 1),
    (N'Pho am thuc', N'Cac khu vuc tap trung hoat dong am thuc dia phuong', 1),
    (N'Tuyen pho di bo', N'Khong gian tham quan va di bo phuc vu du lich', 1);
GO

INSERT INTO DiemThamQuan (
    MaDinhDanh,
    TenDiem,
    MoTaNgan,
    ViDo,
    KinhDo,
    BanKinhKichHoat,
    DiaChi,
    MaLoai,
    MaTaiKhoanTao,
    MaTaiKhoanCapNhat,
    TrangThaiHoatDong
)
VALUES
    ('POI001', N'Cho Ben Thanh', N'Diem tham quan noi tieng tai trung tam Thanh pho Ho Chi Minh', 10.7721400, 106.6982200, 80.00, N'Le Loi, Quan 1, TP Ho Chi Minh', 1, 1, 1, 1),
    ('POI002', N'Pho di bo Nguyen Hue', N'Tuyen pho di bo thu hut nhieu du khach va su kien van hoa', 10.7735600, 106.7039900, 120.00, N'Nguyen Hue, Quan 1, TP Ho Chi Minh', 3, 1, 2, 1),
    ('POI003', N'Pho am thuc Ho Thi Ky', N'Khu pho am thuc da dang mon an va khong gian nhon nhip', 10.7648500, 106.6762700, 100.00, N'Ho Thi Ky, Quan 10, TP Ho Chi Minh', 2, 2, 2, 1);
GO

INSERT INTO NoiDungThuyetMinh (
    MaDiem,
    MaNgonNgu,
    TieuDe,
    NoiDungVanBan,
    DuongDanAmThanh,
    ChoPhepTTS,
    ThoiLuongGiay,
    MaTaiKhoanTao,
    MaTaiKhoanCapNhat,
    TrangThaiHoatDong
)
VALUES
    (1, 1, N'Thuyet minh Cho Ben Thanh', N'Cho Ben Thanh la mot trong nhung bieu tuong du lich noi bat cua Thanh pho Ho Chi Minh.', '/audio/cho-ben-thanh-vi.mp3', 1, 95, 1, 1, 1),
    (1, 2, N'Ben Thanh Market Audio Guide', N'Ben Thanh Market is one of the most iconic tourist destinations in Ho Chi Minh City.', '/audio/cho-ben-thanh-en.mp3', 1, 92, 1, 1, 1),
    (2, 1, N'Thuyet minh Pho di bo Nguyen Hue', N'Pho di bo Nguyen Hue la khong gian cong cong hien dai, noi dien ra nhieu su kien van hoa va giai tri.', '/audio/nguyen-hue-vi.mp3', 1, 110, 1, 2, 1),
    (2, 2, N'Nguyen Hue Walking Street Audio Guide', N'Nguyen Hue Walking Street is a vibrant public space in the city center.', '/audio/nguyen-hue-en.mp3', 1, 105, 1, 2, 1),
    (3, 1, N'Thuyet minh Pho am thuc Ho Thi Ky', N'Pho am thuc Ho Thi Ky noi bat voi nhieu mon an duong pho phong phu va gia ca hop ly.', '/audio/ho-thi-ky-vi.mp3', 1, 88, 2, 2, 1);
GO

INSERT INTO HinhAnhDiemThamQuan (
    MaDiem,
    TenTepTin,
    DuongDanHinhAnh,
    LaAnhDaiDien,
    ThuTuHienThi,
    MaTaiKhoanTao
)
VALUES
    (1, N'cho-ben-thanh-01.jpg', '/images/cho-ben-thanh-01.jpg', 1, 1, 1),
    (1, N'cho-ben-thanh-02.jpg', '/images/cho-ben-thanh-02.jpg', 0, 2, 1),
    (2, N'nguyen-hue-01.jpg', '/images/nguyen-hue-01.jpg', 1, 1, 1),
    (3, N'ho-thi-ky-01.jpg', '/images/ho-thi-ky-01.jpg', 1, 1, 2);
GO

INSERT INTO MaQR (
    MaDiem,
    GiaTriQR,
    TrangThaiHoatDong,
    MaTaiKhoanTao
)
VALUES
    (1, 'QR_POI001', 1, 1),
    (2, 'QR_POI002', 1, 1),
    (3, 'QR_POI003', 1, 2);
GO

INSERT INTO LichSuPhat (
    MaNguoiDung,
    MaDiem,
    MaNoiDung,
    CachKichHoat,
    ThoiGianBatDau,
    ThoiLuongDaNghe
)
VALUES
    (1, 1, 1, 'gps', '2026-03-13T09:00:00', 80),
    (1, 2, 3, 'qr', '2026-03-13T09:20:00', 65),
    (2, 1, 2, 'manual', '2026-03-13T10:00:00', 50),
    (2, 3, 5, 'gps', '2026-03-13T10:30:00', 88);
GO

-- Truy van 1: Danh sach diem tham quan kem loai
SELECT
    d.MaDiem,
    d.MaDinhDanh,
    d.TenDiem,
    l.TenLoai,
    d.DiaChi,
    d.ViDo,
    d.KinhDo,
    d.BanKinhKichHoat
FROM DiemThamQuan d
INNER JOIN LoaiDiemThamQuan l ON d.MaLoai = l.MaLoai
WHERE d.TrangThaiHoatDong = 1;
GO

-- Truy van 2: Noi dung thuyet minh cua tung diem theo ngon ngu
SELECT
    d.TenDiem,
    n.TenNgonNgu,
    nd.TieuDe,
    nd.DuongDanAmThanh,
    nd.ThoiLuongGiay
FROM NoiDungThuyetMinh nd
INNER JOIN DiemThamQuan d ON nd.MaDiem = d.MaDiem
INNER JOIN NgonNgu n ON nd.MaNgonNgu = n.MaNgonNgu
WHERE nd.TrangThaiHoatDong = 1
ORDER BY d.MaDiem, n.MaNgonNgu;
GO

-- Truy van 3: Tra cuu diem tham quan theo ma QR
SELECT
    q.GiaTriQR,
    d.TenDiem,
    d.DiaChi,
    d.BanKinhKichHoat
FROM MaQR q
INNER JOIN DiemThamQuan d ON q.MaDiem = d.MaDiem
WHERE q.GiaTriQR = 'QR_POI001';
GO

-- Truy van 4: Lich su phat thuyet minh kem thong tin nguoi dung
SELECT
    ls.MaLichSuPhat,
    ndg.HoTen,
    d.TenDiem,
    nd.TieuDe,
    ls.CachKichHoat,
    ls.ThoiGianBatDau,
    ls.ThoiLuongDaNghe
FROM LichSuPhat ls
LEFT JOIN NguoiDung ndg ON ls.MaNguoiDung = ndg.MaNguoiDung
INNER JOIN DiemThamQuan d ON ls.MaDiem = d.MaDiem
INNER JOIN NoiDungThuyetMinh nd ON ls.MaNoiDung = nd.MaNoiDung
ORDER BY ls.ThoiGianBatDau DESC;
GO

-- Truy van 5: Thong ke so luot nghe theo diem tham quan
SELECT
    d.MaDiem,
    d.TenDiem,
    COUNT(ls.MaLichSuPhat) AS SoLuotNghe,
    SUM(ISNULL(ls.ThoiLuongDaNghe, 0)) AS TongThoiLuongDaNghe
FROM DiemThamQuan d
LEFT JOIN LichSuPhat ls ON d.MaDiem = ls.MaDiem
GROUP BY d.MaDiem, d.TenDiem
ORDER BY SoLuotNghe DESC, d.TenDiem;
GO

-- Truy van 6: Thong ke so luot nghe theo cach kich hoat
SELECT
    CachKichHoat,
    COUNT(MaLichSuPhat) AS SoLuotPhat,
    SUM(ISNULL(ThoiLuongDaNghe, 0)) AS TongSoGiayDaNghe
FROM LichSuPhat
GROUP BY CachKichHoat
ORDER BY SoLuotPhat DESC;
GO

-- Truy van 7: Tim diem tham quan co noi dung theo ngon ngu cu the
SELECT
    d.TenDiem,
    n.TenNgonNgu,
    nd.TieuDe
FROM NoiDungThuyetMinh nd
INNER JOIN DiemThamQuan d ON nd.MaDiem = d.MaDiem
INNER JOIN NgonNgu n ON nd.MaNgonNgu = n.MaNgonNgu
WHERE n.MaNgonNguQuocTe = 'en';
GO
