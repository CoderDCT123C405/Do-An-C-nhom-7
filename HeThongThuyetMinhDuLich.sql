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
    MaTaiKhoanCapNhat INT NOT NULL,
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
    MaTaiKhoanCapNhat INT NOT NULL,
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
    ('admin', 'hashed_admin_123', N'Quản trị viên hệ thống', 'admin@thuyetminhdulich.vn', N'Admin', 1),
    ('bien_tap_01', 'hashed_editor_123', N'Biên tập viên nội dung', 'bientap01@thuyetminhdulich.vn', N'BienTap', 1);
GO

INSERT INTO NgonNgu (
    MaNgonNguQuocTe,
    TenNgonNgu,
    LaMacDinh,
    TrangThaiHoatDong
)
VALUES
    ('vi', N'Tiếng Việt', 1, 1),
    ('en', N'Tiếng Anh', 0, 1),
    ('zh-CN', N'Tiếng Trung', 0, 1);
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
    ('khach01', 'hashed_user_01', N'Nguyễn Văn An', 'khach01@email.com', '0901000001', 1, 1),
    ('khach02', 'hashed_user_02', N'Trần Thị Bình', 'khach02@email.com', '0901000002', 2, 1);
GO

INSERT INTO LoaiDiemThamQuan (
    TenLoai,
    MoTa,
    TrangThaiHoatDong
)
VALUES
    (N'Khu di tích', N'Các địa điểm mang giá trị lịch sử và văn hóa tiêu biểu', 1),
    (N'Phố ẩm thực', N'Các khu vực tập trung hoạt động ẩm thực địa phương', 1),
    (N'Tuyến phố đi bộ', N'Không gian tham quan và đi bộ phục vụ du lịch', 1);
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
    ('POI001', N'Chợ Bến Thành', N'Điểm tham quan nổi tiếng tại trung tâm Thành phố Hồ Chí Minh', 10.7721400, 106.6982200, 80.00, N'Lê Lợi, Quận 1, TP Hồ Chí Minh', 1, 1, 1, 1),
    ('POI002', N'Phố đi bộ Nguyễn Huệ', N'Tuyến phố đi bộ thu hút nhiều du khách và sự kiện văn hóa', 10.7735600, 106.7039900, 120.00, N'Nguyễn Huệ, Quận 1, TP Hồ Chí Minh', 3, 1, 2, 1),
    ('POI003', N'Phố ẩm thực Hồ Thị Kỷ', N'Khu phố ẩm thực đa dạng món ăn và không gian nhộn nhịp', 10.7648500, 106.6762700, 100.00, N'Hồ Thị Kỷ, Quận 10, TP Hồ Chí Minh', 2, 2, 2, 1);
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
    (1, 1, N'Thuyết minh Chợ Bến Thành', N'Chợ Bến Thành là một trong những biểu tượng du lịch nổi bật của Thành phố Hồ Chí Minh, nơi du khách có thể cảm nhận nhịp sống thương mại truyền thống và khám phá nhiều đặc sản địa phương.', '/audio/cho-ben-thanh-vi.mp3', 1, 95, 1, 1, 1),
    (1, 2, N'Ben Thanh Market Audio Guide', N'Ben Thanh Market is one of the most iconic attractions in Ho Chi Minh City, offering visitors a lively local trading atmosphere and a rich selection of regional specialties.', '/audio/cho-ben-thanh-en.mp3', 1, 92, 1, 1, 1),
    (1, 3, N'边城市场语音导览', N'边城市场是胡志明市最具代表性的旅游地标之一，游客可以在这里感受传统商业氛围，并品尝多种越南地方特色美食。', '/audio/cho-ben-thanh-zh.mp3', 1, 94, 1, 1, 1),
    (2, 1, N'Thuyết minh Phố đi bộ Nguyễn Huệ', N'Phố đi bộ Nguyễn Huệ là không gian công cộng hiện đại, nơi diễn ra nhiều sự kiện văn hóa, nghệ thuật và lễ hội của thành phố.', '/audio/nguyen-hue-vi.mp3', 1, 110, 1, 2, 1),
    (2, 2, N'Nguyen Hue Walking Street Audio Guide', N'Nguyen Hue Walking Street is a vibrant public space in the city center where many cultural activities and festivals take place.', '/audio/nguyen-hue-en.mp3', 1, 105, 1, 2, 1),
    (3, 1, N'Thuyết minh Phố ẩm thực Hồ Thị Kỷ', N'Phố ẩm thực Hồ Thị Kỷ nổi bật với nhiều món ăn đường phố phong phú, không gian nhộn nhịp và mức giá phù hợp với nhiều nhóm du khách.', '/audio/ho-thi-ky-vi.mp3', 1, 88, 2, 2, 1);
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
