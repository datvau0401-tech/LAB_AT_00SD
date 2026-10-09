/* =====================================================================
   e-SHOPPING - Cửa hàng ABC
   CSDL của hệ thống e-Shopping (dbo) + mô phỏng Hệ thống quản lý sản phẩm
   bên ngoài (schema ext). e-Shopping chỉ ĐỌC schema ext qua Adapter.
   ===================================================================== */
IF DB_ID(N'eShoppingDB') IS NULL
    CREATE DATABASE eShoppingDB;
GO
USE eShoppingDB;
GO

/* ---------- Xóa bảng cũ (theo thứ tự phụ thuộc) ---------- */
IF OBJECT_ID('dbo.NhatKyEmail','U')      IS NOT NULL DROP TABLE dbo.NhatKyEmail;
IF OBJECT_ID('dbo.TheThanhToan','U')     IS NOT NULL DROP TABLE dbo.TheThanhToan;
IF OBJECT_ID('dbo.ChiTietDonHang','U')   IS NOT NULL DROP TABLE dbo.ChiTietDonHang;
IF OBJECT_ID('dbo.DonHang','U')          IS NOT NULL DROP TABLE dbo.DonHang;
IF OBJECT_ID('dbo.NguoiNhan','U')        IS NOT NULL DROP TABLE dbo.NguoiNhan;
IF OBJECT_ID('dbo.KhachHang','U')        IS NOT NULL DROP TABLE dbo.KhachHang;
IF OBJECT_ID('dbo.BangGiaGiaoHang','U')  IS NOT NULL DROP TABLE dbo.BangGiaGiaoHang;
IF OBJECT_ID('dbo.LoaiGiaoHang','U')     IS NOT NULL DROP TABLE dbo.LoaiGiaoHang;
IF OBJECT_ID('dbo.KhuVucGiaoHang','U')   IS NOT NULL DROP TABLE dbo.KhuVucGiaoHang;
IF OBJECT_ID('dbo.LoaiThe','U')          IS NOT NULL DROP TABLE dbo.LoaiThe;
IF OBJECT_ID('ext.HinhAnhSanPham','U')   IS NOT NULL DROP TABLE ext.HinhAnhSanPham;
IF OBJECT_ID('ext.SanPham','U')          IS NOT NULL DROP TABLE ext.SanPham;
IF OBJECT_ID('ext.NhomSanPham','U')      IS NOT NULL DROP TABLE ext.NhomSanPham;
GO
IF SCHEMA_ID('ext') IS NULL EXEC('CREATE SCHEMA ext');
GO

/* =====================================================================
   A. MÔ PHỎNG HỆ THỐNG QUẢN LÝ SẢN PHẨM (đã có sẵn trong cửa hàng)
   e-Shopping KHÔNG sở hữu và KHÔNG ghi các bảng này.
   ===================================================================== */
CREATE TABLE ext.NhomSanPham (
    MaNhom  NVARCHAR(20)  NOT NULL PRIMARY KEY,
    TenNhom NVARCHAR(100) NOT NULL UNIQUE
);
CREATE TABLE ext.SanPham (
    MaSanPham       NVARCHAR(20)   NOT NULL PRIMARY KEY,
    TenSanPham      NVARCHAR(200)  NOT NULL,
    TenNhaSanXuat   NVARCHAR(100)  NOT NULL,
    MoTa            NVARCHAR(1000) NULL,
    ThongSoKyThuat  NVARCHAR(1000) NULL,
    GiaBan          DECIMAL(18,0)  NOT NULL CONSTRAINT CK_ext_SanPham_Gia CHECK (GiaBan >= 0),
    ConHang         BIT            NOT NULL,
    MaNhom          NVARCHAR(20)   NOT NULL,
    CONSTRAINT FK_ext_SanPham_Nhom FOREIGN KEY (MaNhom) REFERENCES ext.NhomSanPham(MaNhom)
);
CREATE TABLE ext.HinhAnhSanPham (
    MaHinh     INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham  NVARCHAR(20)  NOT NULL,
    DuongDan   NVARCHAR(260) NOT NULL,
    CONSTRAINT FK_ext_Hinh_SanPham FOREIGN KEY (MaSanPham) REFERENCES ext.SanPham(MaSanPham)
);
GO

/* =====================================================================
   B. CSDL CỦA HỆ THỐNG e-SHOPPING
   ===================================================================== */
CREATE TABLE dbo.KhachHang (
    MaKhachHang  NVARCHAR(20)  NOT NULL PRIMARY KEY,
    HoTen        NVARCHAR(100) NOT NULL,
    NgaySinh     DATE          NOT NULL,
    SoGiayTo     NVARCHAR(30)  NOT NULL,      -- số CMND/Passport
    DiaChi       NVARCHAR(250) NOT NULL,
    DienThoai    NVARCHAR(20)  NOT NULL,
    TenDangNhap  NVARCHAR(50)  NOT NULL,
    MatKhauHash  NVARCHAR(200) NOT NULL,      -- PBKDF2, không lưu mật khẩu gốc
    Email        NVARCHAR(150) NULL,          -- tùy chọn: có email mới gửi xác nhận
    CONSTRAINT UQ_KhachHang_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT UQ_KhachHang_SoGiayTo    UNIQUE (SoGiayTo)
);

CREATE TABLE dbo.KhuVucGiaoHang (
    MaKhuVuc  NVARCHAR(20)  NOT NULL PRIMARY KEY,
    TenKhuVuc NVARCHAR(100) NOT NULL UNIQUE
);

-- 3 loại phiếu đặt hàng; NguongMienPhi = tổng tiền hàng tối thiểu để miễn phí giao
CREATE TABLE dbo.LoaiGiaoHang (
    MaLoaiGiaoHang    NVARCHAR(20)  NOT NULL PRIMARY KEY,
    TenLoai           NVARCHAR(100) NOT NULL,
    MoTaThoiGianXuLy  NVARCHAR(100) NOT NULL,
    NguongMienPhi     DECIMAL(18,0) NULL CONSTRAINT CK_LoaiGiaoHang_Nguong CHECK (NguongMienPhi IS NULL OR NguongMienPhi > 0),
    ThuTu             INT           NOT NULL
);

-- Phí giao theo (khu vực, loại giao hàng)
CREATE TABLE dbo.BangGiaGiaoHang (
    MaKhuVuc        NVARCHAR(20)  NOT NULL,
    MaLoaiGiaoHang  NVARCHAR(20)  NOT NULL,
    PhiGiaoHang     DECIMAL(18,0) NOT NULL CONSTRAINT CK_BangGia_Phi CHECK (PhiGiaoHang >= 0),
    CONSTRAINT PK_BangGiaGiaoHang PRIMARY KEY (MaKhuVuc, MaLoaiGiaoHang),
    CONSTRAINT FK_BangGia_KhuVuc FOREIGN KEY (MaKhuVuc)       REFERENCES dbo.KhuVucGiaoHang(MaKhuVuc),
    CONSTRAINT FK_BangGia_Loai   FOREIGN KEY (MaLoaiGiaoHang) REFERENCES dbo.LoaiGiaoHang(MaLoaiGiaoHang)
);

CREATE TABLE dbo.LoaiThe (
    MaLoaiThe       NVARCHAR(20)  NOT NULL PRIMARY KEY,
    TenLoaiThe      NVARCHAR(50)  NOT NULL UNIQUE,
    SoChuSoThe      TINYINT       NOT NULL CONSTRAINT CK_LoaiThe_SoThe CHECK (SoChuSoThe IN (15,16)),
    SoChuSoCSV      TINYINT       NOT NULL CONSTRAINT CK_LoaiThe_CSV   CHECK (SoChuSoCSV IN (3,4)),
    LePhiGiaoDich   DECIMAL(18,0) NOT NULL CONSTRAINT CK_LoaiThe_LePhi CHECK (LePhiGiaoDich >= 0)
);

-- Người nhận hàng có thể khác người mua
CREATE TABLE dbo.NguoiNhan (
    MaNguoiNhan NVARCHAR(30)  NOT NULL PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    DiaChi      NVARCHAR(250) NOT NULL,
    DienThoai   NVARCHAR(20)  NOT NULL,
    MaKhuVuc    NVARCHAR(20)  NOT NULL,
    CONSTRAINT FK_NguoiNhan_KhuVuc FOREIGN KEY (MaKhuVuc) REFERENCES dbo.KhuVucGiaoHang(MaKhuVuc)
);

CREATE TABLE dbo.DonHang (
    MaDonHang       NVARCHAR(30)  NOT NULL PRIMARY KEY,
    MaKhachHang     NVARCHAR(20)  NOT NULL,
    MaNguoiNhan     NVARCHAR(30)  NOT NULL,
    MaLoaiGiaoHang  NVARCHAR(20)  NOT NULL,
    ThoiDiemDat     DATETIME2(0)  NOT NULL,
    TongTienHang    DECIMAL(18,0) NOT NULL CONSTRAINT CK_DonHang_TienHang CHECK (TongTienHang >= 0),
    PhiGiaoHang     DECIMAL(18,0) NOT NULL CONSTRAINT CK_DonHang_PhiGiao  CHECK (PhiGiaoHang >= 0),
    LePhiThe        DECIMAL(18,0) NOT NULL CONSTRAINT CK_DonHang_LePhi    CHECK (LePhiThe >= 0),
    TongTriGia      DECIMAL(18,0) NOT NULL,
    TrangThai       NVARCHAR(30)  NOT NULL CONSTRAINT DF_DonHang_TrangThai DEFAULT (N'Đã đặt'),
    EmailDaGui      BIT           NOT NULL CONSTRAINT DF_DonHang_Email DEFAULT (0),
    CONSTRAINT CK_DonHang_Tong CHECK (TongTriGia = TongTienHang + PhiGiaoHang + LePhiThe),
    CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY (MaKhachHang)    REFERENCES dbo.KhachHang(MaKhachHang),
    CONSTRAINT FK_DonHang_NguoiNhan FOREIGN KEY (MaNguoiNhan)    REFERENCES dbo.NguoiNhan(MaNguoiNhan),
    CONSTRAINT FK_DonHang_Loai      FOREIGN KEY (MaLoaiGiaoHang) REFERENCES dbo.LoaiGiaoHang(MaLoaiGiaoHang)
);

-- MaSanPham tham chiếu hệ thống NGOÀI nên KHÔNG có FK;
-- TenSanPham và DonGia là bản chụp (snapshot) tại thời điểm đặt hàng.
CREATE TABLE dbo.ChiTietDonHang (
    MaChiTiet   NVARCHAR(40)  NOT NULL PRIMARY KEY,
    MaDonHang   NVARCHAR(30)  NOT NULL,
    MaSanPham   NVARCHAR(20)  NOT NULL,
    TenSanPham  NVARCHAR(200) NOT NULL,
    SoLuong     INT           NOT NULL CONSTRAINT CK_CTDH_SoLuong CHECK (SoLuong > 0),
    DonGia      DECIMAL(18,0) NOT NULL CONSTRAINT CK_CTDH_DonGia  CHECK (DonGia >= 0),
    CONSTRAINT UQ_CTDH_DonHang_SanPham UNIQUE (MaDonHang, MaSanPham),
    CONSTRAINT FK_CTDH_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang)
);

-- Thẻ tín dụng dùng thanh toán: CHỈ lưu 4 số cuối. Không lưu số thẻ đầy đủ và CSV.
CREATE TABLE dbo.TheThanhToan (
    MaThanhToan      NVARCHAR(30)  NOT NULL PRIMARY KEY,
    MaDonHang        NVARCHAR(30)  NOT NULL,
    MaLoaiThe        NVARCHAR(20)  NOT NULL,
    SoTheCuoi        CHAR(4)       NOT NULL,
    TenChuThe        NVARCHAR(100) NOT NULL,
    NgayHetHan       DATE          NOT NULL,
    MaXacNhanNgoai   NVARCHAR(50)  NULL,       -- mã giao dịch do dịch vụ thanh toán trả về
    SoTien           DECIMAL(18,0) NOT NULL CONSTRAINT CK_TTT_SoTien CHECK (SoTien >= 0),
    CONSTRAINT UQ_TTT_DonHang UNIQUE (MaDonHang),
    CONSTRAINT FK_TTT_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang),
    CONSTRAINT FK_TTT_LoaiThe FOREIGN KEY (MaLoaiThe) REFERENCES dbo.LoaiThe(MaLoaiThe)
);

-- Nhật ký email do dịch vụ email (mô phỏng) ghi lại
CREATE TABLE dbo.NhatKyEmail (
    MaEmail      INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang    NVARCHAR(30)  NOT NULL,
    EmailNhan    NVARCHAR(150) NOT NULL,
    TieuDe       NVARCHAR(200) NOT NULL,
    NoiDung      NVARCHAR(MAX) NOT NULL,
    ThoiDiemGui  DATETIME2(0)  NOT NULL CONSTRAINT DF_NhatKyEmail_Gui DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_NhatKyEmail_DonHang FOREIGN KEY (MaDonHang) REFERENCES dbo.DonHang(MaDonHang)
);
GO
CREATE INDEX IX_DonHang_KhachHang ON dbo.DonHang(MaKhachHang);
CREATE INDEX IX_ChiTietDonHang_SanPham ON dbo.ChiTietDonHang(MaSanPham);
CREATE INDEX IX_ext_SanPham_Nhom ON ext.SanPham(MaNhom);
GO

/* =====================================================================
   C. DỮ LIỆU MẪU
   Mức phí giao, lệ phí thẻ là SỐ MINH HỌA (đề không quy định) -> ghi rõ
   là giả định trong báo cáo.
   ===================================================================== */
INSERT INTO ext.NhomSanPham(MaNhom,TenNhom) VALUES
(N'NH01',N'Máy chụp hình kỹ thuật số'),
(N'NH02',N'Đồ chơi'),
(N'NH03',N'Thiết bị điện gia dụng'),
(N'NH04',N'Thiết bị máy tính');

INSERT INTO ext.SanPham(MaSanPham,TenSanPham,TenNhaSanXuat,MoTa,ThongSoKyThuat,GiaBan,ConHang,MaNhom) VALUES
(N'SP001',N'Canon IXUS 285 HS',N'Canon',N'Máy ảnh compact gọn nhẹ, kết nối Wi-Fi.',N'20.2MP; zoom quang 12x; quay Full HD',6500000,1,N'NH01'),
(N'SP002',N'Sony Cyber-shot W830',N'Sony',N'Máy ảnh bỏ túi cho du lịch.',N'20.1MP; zoom quang 8x; màn hình 2.7 inch',3200000,1,N'NH01'),
(N'SP003',N'Bộ xếp hình thành phố 850 mảnh',N'LEGO',N'Bộ xếp hình sáng tạo cho trẻ từ 6 tuổi.',N'850 mảnh; nhựa ABS an toàn',850000,1,N'NH02'),
(N'SP004',N'Gấu bông Teddy 120cm',N'Teddy Việt',N'Gấu bông size lớn, làm quà tặng.',N'Cao 120cm; bông PP; vải nhung',450000,0,N'NH02'),
(N'SP005',N'Nồi chiên không dầu 5L',N'Philips',N'Nồi chiên không dầu dung tích lớn.',N'5 lít; 1700W; hẹn giờ 60 phút',1800000,1,N'NH03'),
(N'SP006',N'Máy xay sinh tố 1.5L',N'Panasonic',N'Máy xay đa năng cho gia đình.',N'1.5 lít; 600W; 2 tốc độ',650000,1,N'NH03'),
(N'SP007',N'Chuột không dây',N'Logitech',N'Chuột không dây nhỏ gọn.',N'Kết nối 2.4GHz; pin AA; 1000 DPI',350000,1,N'NH04'),
(N'SP008',N'Bàn phím cơ',N'Keychron',N'Bàn phím cơ không dây layout 75%.',N'Bluetooth 5.1; switch Brown; đèn nền',1200000,1,N'NH04');

INSERT INTO ext.HinhAnhSanPham(MaSanPham,DuongDan) VALUES
(N'SP001',N'images/sp001_1.jpg'),(N'SP001',N'images/sp001_2.jpg'),
(N'SP002',N'images/sp002_1.jpg'),(N'SP003',N'images/sp003_1.jpg'),
(N'SP005',N'images/sp005_1.jpg'),(N'SP008',N'images/sp008_1.jpg');

INSERT INTO dbo.KhuVucGiaoHang(MaKhuVuc,TenKhuVuc) VALUES
(N'KV01',N'Nội thành TP.HCM'),
(N'KV02',N'Ngoại thành TP.HCM'),
(N'KV03',N'Tỉnh/thành phố khác');

INSERT INTO dbo.LoaiGiaoHang(MaLoaiGiaoHang,TenLoai,MoTaThoiGianXuLy,NguongMienPhi,ThuTu) VALUES
(N'THUONG',   N'Phiếu đặt hàng thường',                    N'3-5 ngày',  NULL,    1),
(N'NHANH',    N'Phiếu đặt hàng chuyển phát nhanh',         N'1-2 ngày',  1000000, 2),
(N'NHANH_NGAY',N'Phiếu đặt hàng chuyển phát nhanh trong ngày',N'Trong ngày',5000000,3);

INSERT INTO dbo.BangGiaGiaoHang(MaKhuVuc,MaLoaiGiaoHang,PhiGiaoHang) VALUES
(N'KV01',N'THUONG',20000),(N'KV01',N'NHANH',35000),(N'KV01',N'NHANH_NGAY',60000),
(N'KV02',N'THUONG',30000),(N'KV02',N'NHANH',50000),(N'KV02',N'NHANH_NGAY',90000),
(N'KV03',N'THUONG',40000),(N'KV03',N'NHANH',70000),(N'KV03',N'NHANH_NGAY',150000);

INSERT INTO dbo.LoaiThe(MaLoaiThe,TenLoaiThe,SoChuSoThe,SoChuSoCSV,LePhiGiaoDich) VALUES
(N'VISA',    N'VISA',             16,3,5000),
(N'MASTER',  N'Mastercard',       16,3,5000),
(N'DISCOVER',N'Discover',         16,3,6000),
(N'AMEX',    N'American Express', 15,4,10000);
GO

/* ---------- Truy vấn kiểm tra sau khi chạy thử (chỉ ĐỌC) ----------
SELECT * FROM dbo.DonHang ORDER BY ThoiDiemDat DESC;
SELECT * FROM dbo.ChiTietDonHang;
SELECT * FROM dbo.TheThanhToan;      -- chỉ có 4 số cuối
SELECT * FROM dbo.NhatKyEmail;       -- không có thông tin thẻ
------------------------------------------------------------------- */