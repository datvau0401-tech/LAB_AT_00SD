/* =====================================================================
   QuanLyNhanSuDB - tao CSDL + du lieu mau cho do an Quan ly nhan su
   Chay:  sqlcmd -S "(LocalDB)\MSSQLLocalDB" -i Database\QuanLyNhanSuDB.sql
   hoac mo bang SSMS / Visual Studio (SQL Server Object Explorer) roi Execute.
   Chay lai nhieu lan duoc (xoa DB cu va tao moi).
   ===================================================================== */
USE master;
GO
IF DB_ID(N'QuanLyNhanSuDB') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyNhanSuDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyNhanSuDB;
END
GO
CREATE DATABASE QuanLyNhanSuDB;
GO
USE QuanLyNhanSuDB;
GO

CREATE TABLE PhongBan(
    MaPB   VARCHAR(20)    NOT NULL PRIMARY KEY,
    TenPB  NVARCHAR(100)  NOT NULL
);
CREATE TABLE NhanVien(
    MaNV      VARCHAR(20)    NOT NULL PRIMARY KEY,
    HoTen     NVARCHAR(100)  NOT NULL,
    NgaySinh  DATE           NULL,
    GioiTinh  NVARCHAR(10)   NULL,
    DiaChi    NVARCHAR(200)  NULL,
    SoDienThoai VARCHAR(20)  NULL
);
CREATE TABLE ThamSo(
    TenThamSo VARCHAR(50)    NOT NULL PRIMARY KEY,
    GiaTri    DECIMAL(18,4)  NOT NULL,
    MoTa      NVARCHAR(200)  NULL
);

/* ---- Quyet dinh (bang cha) + cac bang con theo loai ---- */
CREATE TABLE QuyetDinh(
    MaQD         VARCHAR(30)   NOT NULL PRIMARY KEY,
    LoaiQD       VARCHAR(20)   NOT NULL
                 CHECK (LoaiQD IN ('TuyenDung','ChucVu','Luong','KhenThuong','KyLuat')),
    SoQD         NVARCHAR(50)  NOT NULL UNIQUE,
    NgayQD       DATE          NOT NULL,
    NgayHieuLuc  DATE          NULL,
    TrangThaiKy  BIT           NOT NULL DEFAULT 0,
    MaNV         VARCHAR(20)   NULL REFERENCES NhanVien(MaNV)   -- NULL khi la quyet dinh cho tap the
);
CREATE TABLE QuyetDinhTuyenDung(
    MaQD                 VARCHAR(30)   NOT NULL PRIMARY KEY REFERENCES QuyetDinh(MaQD),
    ThoiGianThuViecThang INT           NOT NULL CHECK (ThoiGianThuViecThang > 0),
    MucLuongThuViec      DECIMAL(18,0) NOT NULL CHECK (MucLuongThuViec >= 0),
    DieuKhoan            NVARCHAR(MAX) NOT NULL,
    MaPB                 VARCHAR(20)   NOT NULL REFERENCES PhongBan(MaPB)
);
CREATE TABLE QuyetDinhChucVu(
    MaQD         VARCHAR(30)   NOT NULL PRIMARY KEY REFERENCES QuyetDinh(MaQD),
    ChucVu       NVARCHAR(100) NOT NULL,
    PhuCapChucVu DECIMAL(18,0) NOT NULL DEFAULT 0
);
CREATE TABLE QuyetDinhLuong(
    MaQD         VARCHAR(30)   NOT NULL PRIMARY KEY REFERENCES QuyetDinh(MaQD),
    LuongCoBan   DECIMAL(18,0) NOT NULL CHECK (LuongCoBan >= 0),
    PhuCapLuong  DECIMAL(18,0) NOT NULL DEFAULT 0,
    PhuCapDocHai DECIMAL(18,0) NOT NULL DEFAULT 0
);
CREATE TABLE QuyetDinhKhenThuong(
    MaQD             VARCHAR(30)   NOT NULL PRIMARY KEY REFERENCES QuyetDinh(MaQD),
    LoaiDoiTuong     VARCHAR(10)   NOT NULL CHECK (LoaiDoiTuong IN ('CaNhan','TapThe')),
    HinhThuc         NVARCHAR(100) NULL,
    LyDo             NVARCHAR(300) NOT NULL,
    SoTienKhenThuong DECIMAL(18,0) NOT NULL CHECK (SoTienKhenThuong > 0),
    MaPB             VARCHAR(20)   NULL REFERENCES PhongBan(MaPB)
);
CREATE TABLE QuyetDinhKyLuat(
    MaQD       VARCHAR(30)   NOT NULL PRIMARY KEY REFERENCES QuyetDinh(MaQD),
    HinhThuc   NVARCHAR(100) NULL,
    LyDo       NVARCHAR(300) NOT NULL,
    SoTienPhat DECIMAL(18,0) NOT NULL DEFAULT 0,
    MaPB       VARCHAR(20)   NULL REFERENCES PhongBan(MaPB)
);

/* ---- Hop dong + bao hiem ---- */
CREATE TABLE HopDong(
    MaHD             VARCHAR(30)  NOT NULL PRIMARY KEY,
    MaNV             VARCHAR(20)  NOT NULL REFERENCES NhanVien(MaNV),
    MaQD             VARCHAR(30)  NOT NULL REFERENCES QuyetDinh(MaQD),
    LoaiHopDong      VARCHAR(10)  NOT NULL CHECK (LoaiHopDong IN ('DaiHan','NganHan')),
    NgayKy           DATE         NOT NULL,
    NgayBatDau       DATE         NOT NULL,
    NgayKetThuc      DATE         NULL,
    TrangThaiHieuLuc BIT          NOT NULL DEFAULT 1
);
-- Mot quyet dinh tuyen dung chi dung cho 1 hop dong
CREATE UNIQUE INDEX UX_HopDong_MaQD ON HopDong(MaQD);
-- Mot nhan vien chi co 1 hop dong con hieu luc
CREATE UNIQUE INDEX UX_HopDong_NV_HieuLuc ON HopDong(MaNV) WHERE TrangThaiHieuLuc = 1;

CREATE TABLE HoSoBaoHiem(
    MaHS      VARCHAR(30)   NOT NULL PRIMARY KEY,
    MaHD      VARCHAR(30)   NOT NULL REFERENCES HopDong(MaHD),
    SoBHYT    VARCHAR(30)   NULL,
    SoBHXH    VARCHAR(30)   NULL,
    MucDongBH DECIMAL(18,0) NOT NULL,
    NgayLapHS DATE          NOT NULL
);

/* ---- Cham cong, phu troi ---- */
CREATE TABLE BangChamCong(
    MaBCC VARCHAR(30) NOT NULL PRIMARY KEY,
    Thang INT NOT NULL CHECK (Thang BETWEEN 1 AND 12),
    Nam   INT NOT NULL,
    CONSTRAINT UQ_BangChamCong UNIQUE (Thang, Nam)
);
CREATE TABLE ChiTietChamCong(      -- moi dong = 1 ngay cong cua 1 nhan vien
    MaBCC    VARCHAR(30) NOT NULL REFERENCES BangChamCong(MaBCC),
    MaNV     VARCHAR(20) NOT NULL REFERENCES NhanVien(MaNV),
    NgayCong DATE        NOT NULL,
    PRIMARY KEY (MaBCC, MaNV, NgayCong)
);
CREATE TABLE PhieuGhiNhanPhuTrong(
    SoPhieu      VARCHAR(30)  NOT NULL PRIMARY KEY,
    NgayPhuTrong DATE         NOT NULL,
    SoGio        DECIMAL(6,2) NOT NULL,
    HinhThuc     NVARCHAR(30) NOT NULL CHECK (HinhThuc IN (N'Tăng ca', N'Ngày lễ', N'Ngày nghỉ')),
    HeSo         DECIMAL(4,2) NOT NULL DEFAULT 1.5
);
CREATE TABLE ChiTietPhuTrong(
    SoPhieu VARCHAR(30)  NOT NULL REFERENCES PhieuGhiNhanPhuTrong(SoPhieu),
    MaNV    VARCHAR(20)  NOT NULL REFERENCES NhanVien(MaNV),
    SoGioNV DECIMAL(6,2) NOT NULL,
    PRIMARY KEY (SoPhieu, MaNV)
);

/* ---- Phieu chi / phieu thu phat ---- */
CREATE TABLE PhieuChi(
    MaPhieuChi VARCHAR(30)   NOT NULL PRIMARY KEY,
    MaQD       VARCHAR(30)   NOT NULL REFERENCES QuyetDinh(MaQD),
    MaNV       VARCHAR(20)   NULL REFERENCES NhanVien(MaNV),
    NgayChi    DATE          NOT NULL,
    SoTien     DECIMAL(18,0) NOT NULL,
    LoaiChi    NVARCHAR(50)  NOT NULL
);
CREATE TABLE PhieuThuPhat(
    MaPhieuThu VARCHAR(30)   NOT NULL PRIMARY KEY,
    MaQD       VARCHAR(30)   NOT NULL REFERENCES QuyetDinh(MaQD),
    MaNV       VARCHAR(20)   NULL REFERENCES NhanVien(MaNV),
    NgayThu    DATE          NOT NULL,
    SoTienNop  DECIMAL(18,0) NOT NULL,
    HinhThucThu NVARCHAR(30) NOT NULL CHECK (HinhThucThu IN (N'Trừ lương', N'Nộp tiền mặt'))
);

/* ---- Bang luong ---- */
CREATE TABLE BangLuong(
    MaBL             VARCHAR(40)   NOT NULL PRIMARY KEY,
    MaNV             VARCHAR(20)   NOT NULL REFERENCES NhanVien(MaNV),
    MaBCC            VARCHAR(30)   NOT NULL REFERENCES BangChamCong(MaBCC),
    MaHS             VARCHAR(30)   NULL REFERENCES HoSoBaoHiem(MaHS),
    Thang            INT           NOT NULL,
    Nam              INT           NOT NULL,
    LuongChinh       DECIMAL(18,0) NOT NULL,
    TongPhuCap       DECIMAL(18,0) NOT NULL,
    TienPhuTrong     DECIMAL(18,0) NOT NULL,
    TongKhauTru      DECIMAL(18,0) NOT NULL,
    TienPhatTruLuong DECIMAL(18,0) NOT NULL,
    LuongThucNhan    DECIMAL(18,0) NOT NULL,
    CONSTRAINT UQ_BangLuong UNIQUE (MaNV, Thang, Nam)
);
CREATE TABLE BangLuongQuyetDinh(
    MaBL VARCHAR(40) NOT NULL REFERENCES BangLuong(MaBL),
    MaQD VARCHAR(30) NOT NULL REFERENCES QuyetDinh(MaQD),
    PRIMARY KEY (MaBL, MaQD)
);
GO

/* =====================  DU LIEU MAU  ===================== */
INSERT INTO PhongBan VALUES
 ('PB01', N'Phòng Tài chính - Kế toán'),
 ('PB02', N'Phòng Kỹ thuật'),
 ('PB03', N'Phòng Sản xuất'),
 ('PB04', N'Phòng Kinh doanh');

INSERT INTO NhanVien(MaNV,HoTen,NgaySinh,GioiTinh) VALUES
 ('NV001', N'Nguyễn Văn An',  '1995-03-12', N'Nam'),
 ('NV002', N'Trần Thị Bình',  '1998-07-25', N'Nữ'),
 ('NV003', N'Lê Hoàng Cường', '2000-01-09', N'Nam'),   -- da co QD tuyen dung da ky, chua co hop dong
 ('NV004', N'Phạm Thu Dung',  '1999-11-30', N'Nữ'),    -- co QD tuyen dung dang cho ky
 ('NV005', N'Võ Minh Em',     '2001-05-18', N'Nam');   -- chua co gi, de thu lap QD tuyen dung

INSERT INTO ThamSo(TenThamSo,GiaTri,MoTa) VALUES
 ('SoNgayCongChuan', 26,    N'Số ngày công chuẩn trong tháng'),
 ('SoGioMotNgay',    8,     N'Số giờ làm việc một ngày'),
 ('TyLeBHXH',        0.08,  N'Tỷ lệ NV đóng BHXH'),
 ('TyLeBHYT',        0.015, N'Tỷ lệ NV đóng BHYT');

-- NV001: QD tuyen dung (da ky) -> hop dong dai han + BH + QD luong + QD chuc vu
INSERT INTO QuyetDinh VALUES
 ('QD_SEED_01','TuyenDung',N'TD-001/2026','2026-01-02','2026-01-05',1,'NV001'),
 ('QD_SEED_02','Luong',    N'LUONG-001/2026','2026-03-06','2026-03-06',1,'NV001'),
 ('QD_SEED_03','ChucVu',   N'CV-001/2026','2026-03-06','2026-03-06',1,'NV001'),
 ('QD_SEED_04','TuyenDung',N'TD-002/2026','2026-01-02','2026-01-05',1,'NV002'),
 ('QD_SEED_05','Luong',    N'LUONG-002/2026','2026-03-06','2026-03-06',1,'NV002'),
 ('QD_SEED_06','TuyenDung',N'TD-003/2026','2026-09-01','2026-09-01',1,'NV003'),
 ('QD_SEED_07','TuyenDung',N'TD-004/2026','2026-10-20',NULL,0,'NV004'),
 ('QD_SEED_08','KyLuat',   N'KL-001/2026','2026-09-20','2026-09-20',1,'NV002');

INSERT INTO QuyetDinhTuyenDung VALUES
 ('QD_SEED_01',2,6000000, N'Hai bên thực hiện đúng nội quy công ty trong thời gian thử việc.','PB01'),
 ('QD_SEED_04',2,5500000, N'Hai bên thực hiện đúng nội quy công ty trong thời gian thử việc.','PB03'),
 ('QD_SEED_06',2,5000000, N'Hai bên thực hiện đúng nội quy công ty trong thời gian thử việc.','PB02'),
 ('QD_SEED_07',2,5000000, N'Hai bên thực hiện đúng nội quy công ty trong thời gian thử việc.','PB04');
INSERT INTO QuyetDinhLuong VALUES ('QD_SEED_02',13000000,500000,0),('QD_SEED_05',9000000,0,300000);
INSERT INTO QuyetDinhChucVu VALUES ('QD_SEED_03',N'Kế toán trưởng',1000000);
INSERT INTO QuyetDinhKyLuat VALUES ('QD_SEED_08',N'Phạt tiền',N'Đi làm trễ nhiều lần',200000,NULL);

INSERT INTO HopDong VALUES
 ('HD_SEED_01','NV001','QD_SEED_01','DaiHan', '2026-03-06','2026-03-06',NULL,1),
 ('HD_SEED_02','NV002','QD_SEED_04','NganHan','2026-03-06','2026-03-06','2027-03-05',1);
INSERT INTO HoSoBaoHiem VALUES ('HS_SEED_01','HD_SEED_01','BHYT0001','BHXH0001',13000000,'2026-03-06');

-- Cham cong thang 9/2026: NV001 va NV002 moi nguoi 24 ngay (bo Chu nhat)
INSERT INTO BangChamCong VALUES ('BCC202609',9,2026);
DECLARE @d DATE='2026-09-01', @n INT=0;
WHILE @d <= '2026-09-30'
BEGIN
    IF DATEDIFF(DAY,'19000107',@d) % 7 <> 0   -- bo Chu nhat (07/01/1900 la Chu nhat)
    BEGIN
        INSERT INTO ChiTietChamCong VALUES ('BCC202609','NV001',@d);
        INSERT INTO ChiTietChamCong VALUES ('BCC202609','NV002',@d);
    END
    SET @d = DATEADD(DAY,1,@d);
END

INSERT INTO PhieuGhiNhanPhuTrong VALUES ('PT001','2026-09-12',4,N'Tăng ca',1.5);
INSERT INTO ChiTietPhuTrong VALUES ('PT001','NV001',4),('PT001','NV002',4);
INSERT INTO PhieuThuPhat VALUES ('PT_SEED_01','QD_SEED_08','NV002','2026-09-20',200000,N'Trừ lương');
GO
PRINT N'Da tao xong QuanLyNhanSuDB.';

/* ---- Bang bo sung cho form Lich phan cong ca va Danh gia nhan su
        (app cung tu tao neu chua co) ---- */
USE QuanLyNhanSuDB;
GO
CREATE TABLE LichPhanCong(
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    MaNV    VARCHAR(20)   NOT NULL REFERENCES NhanVien(MaNV),
    NgayLam DATE          NOT NULL,
    Ca      NVARCHAR(30)  NOT NULL,
    GhiChu  NVARCHAR(200) NULL
);
CREATE TABLE DanhGiaNhanSu(
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    MaNV    VARCHAR(20)   NOT NULL REFERENCES NhanVien(MaNV),
    Thang   INT           NOT NULL,
    Nam     INT           NOT NULL,
    Diem    INT           NOT NULL CHECK (Diem BETWEEN 1 AND 10),
    XepLoai NVARCHAR(30)  NOT NULL,
    NhanXet NVARCHAR(300) NULL
);
GO
