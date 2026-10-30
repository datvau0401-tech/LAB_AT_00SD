/* =====================================================================
   BÀI 4 - QUẢN LÝ CHO THUÊ VĂN PHÒNG Ở CAO ỐC
   Chạy toàn bộ file này trong SSMS (chạy lại nhiều lần được, sẽ tạo lại từ đầu)
   ===================================================================== */
IF DB_ID(N'QuanLyVanPhong') IS NULL CREATE DATABASE QuanLyVanPhong;
GO
USE QuanLyVanPhong;
GO

-- Xóa bảng cũ (theo thứ tự ngược của khóa ngoại)
DROP TABLE IF EXISTS TheGiuXe;
DROP TABLE IF EXISTS NhanVien;
DROP TABLE IF EXISTS HoaDonDichVu;
DROP TABLE IF EXISTS ThanhToanThue;
DROP TABLE IF EXISTS PhieuGiaHan;
DROP TABLE IF EXISTS ChiTietHopDong;
DROP TABLE IF EXISTS HopDong;
DROP TABLE IF EXISTS DichVu;
DROP TABLE IF EXISTS KhachHang;
DROP TABLE IF EXISTS Phong;
DROP TABLE IF EXISTS Tang;
GO

CREATE TABLE Tang (
    MaTang  INT           NOT NULL PRIMARY KEY,
    TenTang NVARCHAR(50)  NOT NULL
);

CREATE TABLE Phong (
    MaPhong   VARCHAR(10)   NOT NULL PRIMARY KEY,
    MaTang    INT           NOT NULL REFERENCES Tang(MaTang),
    DienTich  DECIMAL(8,2)  NOT NULL CHECK (DienTich > 0),
    Huong     NVARCHAR(20)  NOT NULL,
    GiaThue   DECIMAL(18,0) NOT NULL CHECK (GiaThue > 0),          -- giá / tháng
    TrangThai NVARCHAR(10)  NOT NULL DEFAULT N'Trong'
              CHECK (TrangThai IN (N'Trong', N'DangThue'))
);

CREATE TABLE KhachHang (
    MaKH      INT IDENTITY(1,1) PRIMARY KEY,
    TenKH     NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(20)   NOT NULL UNIQUE,                        -- UNIQUE
    DiaChi    NVARCHAR(200) NULL
);

CREATE TABLE HopDong (
    MaHD       INT IDENTITY(1,1) PRIMARY KEY,
    MaKH       INT  NOT NULL REFERENCES KhachHang(MaKH),
    NgayLap    DATE NOT NULL,
    NgayBatDau DATE NOT NULL,
    SoThang    INT  NOT NULL CHECK (SoThang >= 6)                   -- thuê tối thiểu 6 tháng
);

-- 1 hợp đồng thuê nhiều phòng
CREATE TABLE ChiTietHopDong (
    MaHD          INT           NOT NULL REFERENCES HopDong(MaHD),
    MaPhong       VARCHAR(10)   NOT NULL REFERENCES Phong(MaPhong),
    GiaThue       DECIMAL(18,0) NOT NULL,                           -- giá chốt lúc ký
    SoThangGiaHan INT           NOT NULL DEFAULT 0,                 -- tổng tháng đã gia hạn
    PRIMARY KEY (MaHD, MaPhong)
);

CREATE TABLE PhieuGiaHan (
    MaPG           INT IDENTITY(1,1) PRIMARY KEY,
    MaHD           INT         NOT NULL,
    MaPhong        VARCHAR(10) NOT NULL,
    NgayLap        DATE        NOT NULL,
    SoThangGiaHan  INT         NOT NULL CHECK (SoThangGiaHan > 0),
    FOREIGN KEY (MaHD, MaPhong) REFERENCES ChiTietHopDong(MaHD, MaPhong)
);

-- Lịch thu tiền thuê theo từng tháng của từng phòng
CREATE TABLE ThanhToanThue (
    MaTT     INT IDENTITY(1,1) PRIMARY KEY,
    MaHD     INT           NOT NULL,
    MaPhong  VARCHAR(10)   NOT NULL,
    ThangThu INT           NOT NULL CHECK (ThangThu >= 1),
    SoTien   DECIMAL(18,0) NOT NULL,
    HanThu   DATE          NOT NULL,       -- tháng 1-6: ngày ký; tháng 7+: đầu tháng thuê
    DaThu    BIT           NOT NULL DEFAULT 0,
    NgayThu  DATE          NULL,
    FOREIGN KEY (MaHD, MaPhong) REFERENCES ChiTietHopDong(MaHD, MaPhong),
    UNIQUE (MaHD, MaPhong, ThangThu)                                -- UNIQUE: mỗi tháng 1 dòng
);

CREATE TABLE DichVu (
    MaDV  INT IDENTITY(1,1) PRIMARY KEY,
    TenDV NVARCHAR(50) NOT NULL UNIQUE
);

-- Hóa đơn dịch vụ (điện, nước, internet...) thanh toán cuối tháng
CREATE TABLE HoaDonDichVu (
    MaHDDV  INT IDENTITY(1,1) PRIMARY KEY,
    MaHD    INT           NOT NULL,
    MaPhong VARCHAR(10)   NOT NULL,
    MaDV    INT           NOT NULL REFERENCES DichVu(MaDV),
    Thang   DATE          NOT NULL,        -- ngày 1 của tháng sử dụng
    SoTien  DECIMAL(18,0) NOT NULL CHECK (SoTien > 0),
    HanThu  DATE          NOT NULL,        -- cuối tháng
    DaThu   BIT           NOT NULL DEFAULT 0,
    NgayThu DATE          NULL,
    FOREIGN KEY (MaHD, MaPhong) REFERENCES ChiTietHopDong(MaHD, MaPhong),
    UNIQUE (MaHD, MaPhong, MaDV, Thang)
);

-- Nhân viên làm việc trong cao ốc + thẻ giữ xe (phần mở rộng theo đề)
CREATE TABLE NhanVien (
    MaNV    INT IDENTITY(1,1) PRIMARY KEY,
    HoTen   NVARCHAR(100) NOT NULL,
    ChucVu  NVARCHAR(50)  NULL,
    HinhAnh NVARCHAR(260) NULL,
    MaKH    INT           NOT NULL REFERENCES KhachHang(MaKH),     -- công ty thuê
    MaPhong VARCHAR(10)   NOT NULL REFERENCES Phong(MaPhong)
);

CREATE TABLE TheGiuXe (
    MaThe   VARCHAR(10)  NOT NULL PRIMARY KEY,
    MaNV    INT          NOT NULL REFERENCES NhanVien(MaNV),
    LoaiXe  NVARCHAR(30) NULL,
    BienSo  VARCHAR(15)  NOT NULL UNIQUE
);
GO

-- ===================== DỮ LIỆU MẪU =====================
INSERT INTO Tang VALUES (1, N'Tầng 1'), (2, N'Tầng 2'), (3, N'Tầng 3');

INSERT INTO Phong (MaPhong, MaTang, DienTich, Huong, GiaThue) VALUES
('P101', 1, 40, N'Đông', 12000000),
('P102', 1, 55, N'Tây',  16000000),
('P201', 2, 45, N'Nam',  15000000),
('P202', 2, 60, N'Bắc',  19000000),
('P301', 3, 50, N'Đông', 18000000),
('P302', 3, 80, N'Nam',  26000000);

INSERT INTO DichVu (TenDV) VALUES (N'Điện'), (N'Nước'), (N'Internet');

INSERT INTO KhachHang (TenKH, DienThoai, DiaChi) VALUES
(N'Công ty ABC', '0901000001', N'Quận 1, TP.HCM'),
(N'Công ty XYZ', '0901000002', N'Quận 3, TP.HCM');
GO
