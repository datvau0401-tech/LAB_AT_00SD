/* =====================================================================
   BÀI 5 - QUẢN LÝ HỒ SƠ BỆNH ÁN CỦA BỆNH NHÂN
   01_schema.sql : tạo CSDL, bảng, khóa chính/ngoại, UNIQUE, CHECK, trigger
   Chạy: sqlcmd -S . -E -I -f 65001 -i 01_schema.sql   (hoặc mở bằng SSMS)
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_ID(N'QuanLyBenhAn') IS NULL
    CREATE DATABASE QuanLyBenhAn COLLATE Vietnamese_CI_AS;
GO
USE QuanLyBenhAn;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------- Tham số hệ thống ---------- */
CREATE TABLE dbo.ThamSo (
    TenThamSo varchar(50)    NOT NULL CONSTRAINT PK_ThamSo PRIMARY KEY,
    GiaTri    decimal(18,0)  NOT NULL CONSTRAINT CK_ThamSo_GiaTri CHECK (GiaTri >= 0),
    MoTa      nvarchar(200)  NULL
);
GO

/* ---------- Bệnh nhân (thông tin phiếu đăng ký) ---------- */
CREATE TABLE dbo.BenhNhan (
    MaBN        int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BenhNhan PRIMARY KEY,
    HoTen       nvarchar(100)  NOT NULL,
    DiaChi      nvarchar(200)  NOT NULL,
    NgaySinh    date           NOT NULL,
    DienThoai   varchar(15)    NOT NULL,
    Anh         varbinary(max) NULL,              -- ảnh 3x4
    NgayDangKy  datetime2(0)   NOT NULL CONSTRAINT DF_BenhNhan_NgayDK DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_BenhNhan_DienThoai UNIQUE (DienThoai),
    CONSTRAINT CK_BenhNhan_HoTen     CHECK (LEN(LTRIM(RTRIM(HoTen))) > 0),
    CONSTRAINT CK_BenhNhan_NgaySinh  CHECK (NgaySinh <= CAST(GETDATE() AS date)),
    CONSTRAINT CK_BenhNhan_DienThoai CHECK (LEN(DienThoai) >= 9 AND DienThoai NOT LIKE '%[^0-9]%')
);
GO

/* ---------- Hồ sơ bệnh án (mỗi bệnh nhân đúng 1 hồ sơ) ---------- */
CREATE TABLE dbo.HoSoBenhAn (
    MaHS    int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoSoBenhAn PRIMARY KEY,
    MaBN    int  NOT NULL,
    MaHoSo  AS (CONVERT(varchar(10), 'HS' + RIGHT('000000' + CONVERT(varchar(10), MaHS), 6))) PERSISTED NOT NULL,
    NgayCap date NOT NULL CONSTRAINT DF_HoSo_NgayCap DEFAULT CONVERT(date, SYSDATETIME()),
    CONSTRAINT UQ_HoSo_MaBN   UNIQUE (MaBN),
    CONSTRAINT FK_HoSo_BenhNhan FOREIGN KEY (MaBN) REFERENCES dbo.BenhNhan(MaBN)
);
GO
CREATE UNIQUE INDEX UX_HoSo_MaHoSo ON dbo.HoSoBenhAn(MaHoSo);
GO

/* ---------- Lệ phí hằng năm (1 hồ sơ - 1 năm - 1 lần đóng) ---------- */
CREATE TABLE dbo.LePhiNam (
    MaLP    int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LePhiNam PRIMARY KEY,
    MaHS    int           NOT NULL,
    Nam     smallint      NOT NULL,
    SoTien  decimal(18,0) NOT NULL,
    NgayDong datetime2(0) NOT NULL CONSTRAINT DF_LePhi_NgayDong DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_LePhi_HoSo_Nam UNIQUE (MaHS, Nam),
    CONSTRAINT FK_LePhi_HoSo FOREIGN KEY (MaHS) REFERENCES dbo.HoSoBenhAn(MaHS),
    CONSTRAINT CK_LePhi_Nam    CHECK (Nam BETWEEN 2000 AND 2100),
    CONSTRAINT CK_LePhi_SoTien CHECK (SoTien > 0)
);
GO

/* ---------- Khoa, bác sĩ ---------- */
CREATE TABLE dbo.Khoa (
    MaKhoa  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Khoa PRIMARY KEY,
    TenKhoa nvarchar(100) NOT NULL,
    CONSTRAINT UQ_Khoa_Ten UNIQUE (TenKhoa)
);
CREATE TABLE dbo.BacSi (
    MaBS    int IDENTITY(1,1) NOT NULL CONSTRAINT PK_BacSi PRIMARY KEY,
    HoTen   nvarchar(100) NOT NULL,
    MaKhoa  int NOT NULL,
    ChuyenMon nvarchar(100) NULL,
    CONSTRAINT FK_BacSi_Khoa FOREIGN KEY (MaKhoa) REFERENCES dbo.Khoa(MaKhoa)
);
GO

/* ---------- Danh mục thuốc, loại xét nghiệm ---------- */
CREATE TABLE dbo.Thuoc (
    MaThuoc  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Thuoc PRIMARY KEY,
    TenThuoc nvarchar(100) NOT NULL,
    DonVi    nvarchar(20)  NOT NULL,
    DonGia   decimal(18,0) NOT NULL,
    CONSTRAINT UQ_Thuoc_Ten UNIQUE (TenThuoc),
    CONSTRAINT CK_Thuoc_DonGia CHECK (DonGia >= 0)
);
CREATE TABLE dbo.LoaiXetNghiem (
    MaLoaiXN int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoaiXN PRIMARY KEY,
    TenXN    nvarchar(100) NOT NULL,
    DonGia   decimal(18,0) NOT NULL,
    CONSTRAINT UQ_LoaiXN_Ten UNIQUE (TenXN),
    CONSTRAINT CK_LoaiXN_DonGia CHECK (DonGia >= 0)
);
GO

/* ---------- Lịch hẹn khám định kỳ theo quý ---------- */
CREATE TABLE dbo.LichHen (
    MaHen     int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LichHen PRIMARY KEY,
    MaHS      int          NOT NULL,
    Nam       smallint     NOT NULL,
    Quy       tinyint      NOT NULL,
    NgayHen   datetime2(0) NOT NULL,
    MaBS      int          NULL,
    TrangThai varchar(10)  NOT NULL CONSTRAINT DF_LichHen_TT DEFAULT 'DaHen',
    NgayTao   datetime2(0) NOT NULL CONSTRAINT DF_LichHen_NgayTao DEFAULT SYSDATETIME(),
    CONSTRAINT FK_LichHen_HoSo FOREIGN KEY (MaHS) REFERENCES dbo.HoSoBenhAn(MaHS),
    CONSTRAINT FK_LichHen_BacSi FOREIGN KEY (MaBS) REFERENCES dbo.BacSi(MaBS),
    CONSTRAINT CK_LichHen_TT   CHECK (TrangThai IN ('DaHen','DaKham','Huy')),
    CONSTRAINT CK_LichHen_Quy  CHECK (Quy BETWEEN 1 AND 4),
    CONSTRAINT CK_LichHen_QuyKhop CHECK (Nam = YEAR(NgayHen) AND Quy = (MONTH(NgayHen) + 2) / 3)
);
GO
-- Mỗi hồ sơ chỉ có 1 lịch (chưa hủy) cho mỗi quý
CREATE UNIQUE INDEX UX_LichHen_MotQuy ON dbo.LichHen(MaHS, Nam, Quy) WHERE TrangThai <> 'Huy';
-- Một bác sĩ không nhận 2 lịch cùng thời điểm
CREATE UNIQUE INDEX UX_LichHen_BacSiGio ON dbo.LichHen(MaBS, NgayHen) WHERE TrangThai <> 'Huy' AND MaBS IS NOT NULL;
GO

/* ---------- Phiếu khám ---------- */
CREATE TABLE dbo.PhieuKham (
    MaPK        int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuKham PRIMARY KEY,
    MaHS        int          NOT NULL,
    MaBS        int          NOT NULL,
    MaKhoa      int          NOT NULL,
    NgayGioKham datetime2(0) NOT NULL CONSTRAINT DF_PK_NgayGio DEFAULT SYSDATETIME(),
    LoaiKham    varchar(10)  NOT NULL,
    TrieuChung  nvarchar(500) NULL,
    NhietDo     decimal(4,1)  NULL,
    HuyetAp     varchar(10)   NULL,
    ChanDoan    nvarchar(300) NULL,
    MaHen       int          NULL,
    TrangThai   varchar(10)  NOT NULL CONSTRAINT DF_PK_TT DEFAULT 'DangKham',
    CONSTRAINT FK_PK_HoSo  FOREIGN KEY (MaHS)   REFERENCES dbo.HoSoBenhAn(MaHS),
    CONSTRAINT FK_PK_BacSi FOREIGN KEY (MaBS)   REFERENCES dbo.BacSi(MaBS),
    CONSTRAINT FK_PK_Khoa  FOREIGN KEY (MaKhoa) REFERENCES dbo.Khoa(MaKhoa),
    CONSTRAINT FK_PK_LichHen FOREIGN KEY (MaHen) REFERENCES dbo.LichHen(MaHen),
    CONSTRAINT CK_PK_Loai  CHECK (LoaiKham IN ('DieuTri','DinhKy')),
    CONSTRAINT CK_PK_TT    CHECK (TrangThai IN ('DangKham','HoanTat')),
    CONSTRAINT CK_PK_NhietDo CHECK (NhietDo IS NULL OR NhietDo BETWEEN 30 AND 45),
    CONSTRAINT CK_PK_DinhKyCoHen CHECK ((LoaiKham = 'DinhKy' AND MaHen IS NOT NULL) OR (LoaiKham = 'DieuTri' AND MaHen IS NULL))
);
GO
-- 1 lịch hẹn chỉ gắn với 1 phiếu khám
CREATE UNIQUE INDEX UX_PK_MaHen ON dbo.PhieuKham(MaHen) WHERE MaHen IS NOT NULL;
-- mỗi hồ sơ chỉ có tối đa 1 phiếu đang khám (chưa thanh toán)
CREATE UNIQUE INDEX UX_PK_MotPhieuMo ON dbo.PhieuKham(MaHS) WHERE TrangThai = 'DangKham';
GO

/* ---------- Xét nghiệm / toa thuốc gắn với phiếu khám ---------- */
CREATE TABLE dbo.PhieuXetNghiem (
    MaXN     int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PhieuXN PRIMARY KEY,
    MaPK     int NOT NULL,
    MaLoaiXN int NOT NULL,
    DonGia   decimal(18,0) NOT NULL,
    NgayLam  datetime2(0) NOT NULL CONSTRAINT DF_XN_NgayLam DEFAULT SYSDATETIME(),
    KetQua   nvarchar(500) NULL,
    CONSTRAINT FK_XN_PhieuKham FOREIGN KEY (MaPK)     REFERENCES dbo.PhieuKham(MaPK),
    CONSTRAINT FK_XN_Loai      FOREIGN KEY (MaLoaiXN) REFERENCES dbo.LoaiXetNghiem(MaLoaiXN),
    CONSTRAINT UQ_XN_Phieu_Loai UNIQUE (MaPK, MaLoaiXN)
);
CREATE TABLE dbo.ToaThuoc (
    MaToa   int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ToaThuoc PRIMARY KEY,
    MaPK    int NOT NULL,
    NgayLap datetime2(0) NOT NULL CONSTRAINT DF_Toa_NgayLap DEFAULT SYSDATETIME(),
    GhiChu  nvarchar(300) NULL,
    CONSTRAINT FK_Toa_PhieuKham FOREIGN KEY (MaPK) REFERENCES dbo.PhieuKham(MaPK),
    CONSTRAINT UQ_Toa_MaPK UNIQUE (MaPK)           -- 1 phiếu khám - 1 toa
);
CREATE TABLE dbo.ChiTietToa (
    MaToa   int NOT NULL,
    MaThuoc int NOT NULL,
    SoLuong int NOT NULL,
    DonGia  decimal(18,0) NOT NULL,
    LieuDung nvarchar(200) NULL,
    CONSTRAINT PK_ChiTietToa PRIMARY KEY (MaToa, MaThuoc),
    CONSTRAINT FK_CTT_Toa   FOREIGN KEY (MaToa)   REFERENCES dbo.ToaThuoc(MaToa),
    CONSTRAINT FK_CTT_Thuoc FOREIGN KEY (MaThuoc) REFERENCES dbo.Thuoc(MaThuoc),
    CONSTRAINT CK_CTT_SoLuong CHECK (SoLuong > 0)
);
GO

/* ---------- Hóa đơn viện phí (1 phiếu khám - 1 hóa đơn) ---------- */
CREATE TABLE dbo.HoaDonVienPhi (
    MaHD          int IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
    MaPK          int NOT NULL,
    TienKham      decimal(18,0) NOT NULL,
    TienXetNghiem decimal(18,0) NOT NULL,
    TienThuoc     decimal(18,0) NOT NULL,
    TongTien      AS (TienKham + TienXetNghiem + TienThuoc) PERSISTED,
    PhuongThuc    varchar(12) NOT NULL,
    NgayThanhToan datetime2(0) NOT NULL CONSTRAINT DF_HD_Ngay DEFAULT SYSDATETIME(),
    CONSTRAINT FK_HD_PhieuKham FOREIGN KEY (MaPK) REFERENCES dbo.PhieuKham(MaPK),
    CONSTRAINT UQ_HD_MaPK UNIQUE (MaPK),
    CONSTRAINT CK_HD_PhuongThuc CHECK (PhuongThuc IN ('TienMat','The','ChuyenKhoan')),
    CONSTRAINT CK_HD_Tien CHECK (TienKham >= 0 AND TienXetNghiem >= 0 AND TienThuoc >= 0)
);
GO

/* ---------- Hàm: hồ sơ còn hạn (đã đóng lệ phí năm) ---------- */
CREATE FUNCTION dbo.fn_HoSoConHan (@MaHS int, @Nam int)
RETURNS bit
AS
BEGIN
    RETURN CASE WHEN EXISTS (SELECT 1 FROM dbo.LePhiNam WHERE MaHS = @MaHS AND Nam = @Nam) THEN 1 ELSE 0 END;
END;
GO

/* ---------- Trigger: khóa dữ liệu sau khi hoàn tất thanh toán ---------- */
-- Phiếu khám: không sửa phiếu đã hoàn tất; chỉ được chuyển sang HoanTat khi đã có hóa đơn
CREATE TRIGGER dbo.trg_PhieuKham_Khoa ON dbo.PhieuKham
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM deleted WHERE TrangThai = 'HoanTat')
    BEGIN
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- hủy toàn bộ câu lệnh/giao dịch gây ra vi phạm
        THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1;
    END;
    IF EXISTS (SELECT 1 FROM inserted i
               WHERE i.TrangThai = 'HoanTat'
                 AND NOT EXISTS (SELECT 1 FROM dbo.HoaDonVienPhi h WHERE h.MaPK = i.MaPK))
    BEGIN
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW 50011, N'Chỉ được hoàn tất phiếu khám sau khi đã thanh toán viện phí.', 1;
    END;
END;
GO
CREATE TRIGGER dbo.trg_ToaThuoc_Khoa ON dbo.ToaThuoc
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1
               FROM (SELECT MaPK FROM inserted UNION SELECT MaPK FROM deleted) x
               JOIN dbo.PhieuKham p ON p.MaPK = x.MaPK
               WHERE p.TrangThai = 'HoanTat')
    BEGIN
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- hủy toàn bộ câu lệnh/giao dịch gây ra vi phạm
        THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1;
    END;
END;
GO
CREATE TRIGGER dbo.trg_ChiTietToa_Khoa ON dbo.ChiTietToa
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1
               FROM (SELECT MaToa FROM inserted UNION SELECT MaToa FROM deleted) x
               JOIN dbo.ToaThuoc t ON t.MaToa = x.MaToa
               JOIN dbo.PhieuKham p ON p.MaPK = t.MaPK
               WHERE p.TrangThai = 'HoanTat')
    BEGIN
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- hủy toàn bộ câu lệnh/giao dịch gây ra vi phạm
        THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1;
    END;
END;
GO
CREATE TRIGGER dbo.trg_PhieuXN_Khoa ON dbo.PhieuXetNghiem
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1
               FROM (SELECT MaPK FROM inserted UNION SELECT MaPK FROM deleted) x
               JOIN dbo.PhieuKham p ON p.MaPK = x.MaPK
               WHERE p.TrangThai = 'HoanTat')
    BEGIN
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;   -- hủy toàn bộ câu lệnh/giao dịch gây ra vi phạm
        THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1;
    END;
END;
GO
PRINT N'01_schema.sql: hoàn tất.';
GO
