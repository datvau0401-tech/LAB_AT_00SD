/* =====================================================================
   02_procs.sql : type, hàm, stored procedure (có TRANSACTION) cho các nghiệp vụ
   Mã lỗi nghiệp vụ (THROW):
     50001 họ tên trống        50002 SĐT đã có hồ sơ      50003 lệ phí < quy định
     50004 ngày sinh tương lai 50005 hồ sơ không tồn tại  50006 đã đóng lệ phí năm này
     50010 phiếu đã hoàn tất   50011 hoàn tất khi chưa thanh toán  50012 phiếu không tồn tại
     50020 bác sĩ không có     50021 còn phiếu đang khám  50022 chưa đóng lệ phí năm
     50023 định kỳ cần lịch hẹn 50024 lịch hẹn không hợp lệ 50025 điều trị không gắn lịch hẹn
     50030 hẹn vào quá khứ     50031 chưa đóng lệ phí năm của ngày hẹn
     50032 quý đã có lịch     50033 bác sĩ trùng giờ      50034 không thể hủy lịch
     50040 loại XN không có    50041 XN đã có trong phiếu  50042 không tìm thấy XN
     50043 toa đã tồn tại      50044 toa rỗng              50045 thuốc không tồn tại
     50051 phương thức thanh toán không hợp lệ
   ===================================================================== */
USE QuanLyBenhAn;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF TYPE_ID(N'dbo.ChiTietToaType') IS NULL
    CREATE TYPE dbo.ChiTietToaType AS TABLE (
        MaThuoc  int NOT NULL PRIMARY KEY,
        SoLuong  int NOT NULL CHECK (SoLuong > 0),
        LieuDung nvarchar(200) NULL
    );
GO

/* ---------- Tính viện phí tạm tính của 1 phiếu khám ----------
   Khám định kỳ theo quý: miễn phí (kể cả xét nghiệm/thuốc phát sinh trong đợt khám). */
CREATE OR ALTER FUNCTION dbo.fn_TinhVienPhi (@MaPK int)
RETURNS TABLE
AS
RETURN
SELECT pk.MaPK,
       TienKham = CASE WHEN pk.LoaiKham = 'DinhKy' THEN CAST(0 AS decimal(18,0))
                       ELSE CAST(ISNULL((SELECT GiaTri FROM dbo.ThamSo WHERE TenThamSo = 'PhiKham'), 0) AS decimal(18,0)) END,
       TienXetNghiem = CASE WHEN pk.LoaiKham = 'DinhKy' THEN CAST(0 AS decimal(18,0))
                            ELSE CAST(ISNULL((SELECT SUM(x.DonGia) FROM dbo.PhieuXetNghiem x WHERE x.MaPK = pk.MaPK), 0) AS decimal(18,0)) END,
       TienThuoc = CASE WHEN pk.LoaiKham = 'DinhKy' THEN CAST(0 AS decimal(18,0))
                        ELSE CAST(ISNULL((SELECT SUM(c.SoLuong * c.DonGia)
                                          FROM dbo.ToaThuoc t JOIN dbo.ChiTietToa c ON c.MaToa = t.MaToa
                                          WHERE t.MaPK = pk.MaPK), 0) AS decimal(18,0)) END
FROM dbo.PhieuKham pk
WHERE pk.MaPK = @MaPK;
GO

/* =====================================================================
   MODULE 1 - ĐĂNG KÝ LẦN ĐẦU + LỆ PHÍ NĂM + CẤP HỒ SƠ (1 transaction)
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.sp_DangKyLanDau
    @HoTen nvarchar(100), @DiaChi nvarchar(200), @NgaySinh date, @DienThoai varchar(15),
    @Anh varbinary(max) = NULL, @SoTienLePhi decimal(18,0),
    @MaBN int OUTPUT, @MaHS int OUTPUT, @MaHoSo varchar(10) OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @LePhiToiThieu decimal(18,0) = (SELECT GiaTri FROM dbo.ThamSo WHERE TenThamSo = 'LePhiNam');

    IF LEN(LTRIM(RTRIM(ISNULL(@HoTen, N'')))) = 0
    BEGIN THROW 50001, N'Họ tên không được để trống.', 1; END;
    IF @NgaySinh > CAST(GETDATE() AS date)
    BEGIN THROW 50004, N'Ngày sinh không được ở tương lai.', 1; END;
    IF EXISTS (SELECT 1 FROM dbo.BenhNhan WHERE DienThoai = @DienThoai)
    BEGIN THROW 50002, N'Số điện thoại này đã đăng ký hồ sơ. Bệnh nhân khám lần sau chỉ cần tra cứu hồ sơ và gia hạn lệ phí.', 1; END;
    IF @SoTienLePhi < ISNULL(@LePhiToiThieu, 0)
    BEGIN THROW 50003, N'Lệ phí năm đóng chưa đủ so với mức quy định.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        INSERT dbo.BenhNhan (HoTen, DiaChi, NgaySinh, DienThoai, Anh)
        VALUES (LTRIM(RTRIM(@HoTen)), @DiaChi, @NgaySinh, @DienThoai, @Anh);
        SET @MaBN = SCOPE_IDENTITY();

        INSERT dbo.HoSoBenhAn (MaBN) VALUES (@MaBN);
        SET @MaHS = SCOPE_IDENTITY();
        SELECT @MaHoSo = MaHoSo FROM dbo.HoSoBenhAn WHERE MaHS = @MaHS;

        INSERT dbo.LePhiNam (MaHS, Nam, SoTien) VALUES (@MaHS, YEAR(SYSDATETIME()), @SoTienLePhi);
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GiaHanLePhi
    @MaHS int, @SoTien decimal(18,0), @Nam smallint = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    SET @Nam = ISNULL(@Nam, YEAR(SYSDATETIME()));
    DECLARE @LePhiToiThieu decimal(18,0) = (SELECT GiaTri FROM dbo.ThamSo WHERE TenThamSo = 'LePhiNam');

    IF NOT EXISTS (SELECT 1 FROM dbo.HoSoBenhAn WHERE MaHS = @MaHS)
    BEGIN THROW 50005, N'Hồ sơ bệnh án không tồn tại.', 1; END;
    IF @SoTien < ISNULL(@LePhiToiThieu, 0)
    BEGIN THROW 50003, N'Lệ phí năm đóng chưa đủ so với mức quy định.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        IF EXISTS (SELECT 1 FROM dbo.LePhiNam WITH (UPDLOCK, HOLDLOCK) WHERE MaHS = @MaHS AND Nam = @Nam)
        BEGIN THROW 50006, N'Hồ sơ đã đóng lệ phí cho năm này.', 1; END;
        INSERT dbo.LePhiNam (MaHS, Nam, SoTien) VALUES (@MaHS, @Nam, @SoTien);
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* =====================================================================
   MODULE 2 - TRA CỨU HỒ SƠ / LỊCH SỬ KHÁM
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.sp_TraCuuHoSo @TuKhoa nvarchar(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @TuKhoa = LTRIM(RTRIM(ISNULL(@TuKhoa, N'')));
    SELECT h.MaHS, h.MaHoSo, b.HoTen, b.NgaySinh, b.DienThoai, b.DiaChi, h.NgayCap,
           ConHan = dbo.fn_HoSoConHan(h.MaHS, YEAR(SYSDATETIME()))
    FROM dbo.HoSoBenhAn h
    JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN
    WHERE @TuKhoa = N''
       OR h.MaHoSo = @TuKhoa
       OR b.DienThoai = @TuKhoa
       OR CHARINDEX(@TuKhoa, b.HoTen) > 0
    ORDER BY b.HoTen, h.MaHS;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_LichSuKham @MaHS int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pk.MaPK, pk.NgayGioKham, pk.LoaiKham, bs.HoTen AS BacSi, k.TenKhoa AS Khoa,
           pk.TrieuChung, pk.NhietDo, pk.HuyetAp, pk.ChanDoan, pk.TrangThai, hd.TongTien
    FROM dbo.PhieuKham pk
    JOIN dbo.BacSi bs ON bs.MaBS = pk.MaBS
    JOIN dbo.Khoa k   ON k.MaKhoa = pk.MaKhoa
    LEFT JOIN dbo.HoaDonVienPhi hd ON hd.MaPK = pk.MaPK
    WHERE pk.MaHS = @MaHS
    ORDER BY pk.NgayGioKham DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_LePhiTheoHoSo @MaHS int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Nam, SoTien, NgayDong FROM dbo.LePhiNam WHERE MaHS = @MaHS ORDER BY Nam DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_XetNghiemTheoPhieu @MaPK int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT x.MaXN, l.TenXN, x.DonGia, x.NgayLam, x.KetQua
    FROM dbo.PhieuXetNghiem x JOIN dbo.LoaiXetNghiem l ON l.MaLoaiXN = x.MaLoaiXN
    WHERE x.MaPK = @MaPK ORDER BY x.MaXN;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ToaTheoPhieu @MaPK int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.MaToa, th.TenThuoc, th.DonVi, c.SoLuong, c.DonGia, ThanhTien = c.SoLuong * c.DonGia, c.LieuDung, t.NgayLap
    FROM dbo.ToaThuoc t
    JOIN dbo.ChiTietToa c ON c.MaToa = t.MaToa
    JOIN dbo.Thuoc th ON th.MaThuoc = c.MaThuoc
    WHERE t.MaPK = @MaPK ORDER BY th.TenThuoc;
END;
GO

/* =====================================================================
   MODULE 3 - LỊCH KHÁM ĐỊNH KỲ THEO QUÝ (miễn phí, 1 lịch / quý / hồ sơ)
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.sp_TaoLichHen
    @MaHS int, @NgayHen datetime2(0), @MaBS int = NULL, @MaHen int OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @Nam smallint = YEAR(@NgayHen), @Quy tinyint = (MONTH(@NgayHen) + 2) / 3;

    IF NOT EXISTS (SELECT 1 FROM dbo.HoSoBenhAn WHERE MaHS = @MaHS)
    BEGIN THROW 50005, N'Hồ sơ bệnh án không tồn tại.', 1; END;
    IF @NgayHen < SYSDATETIME()
    BEGIN THROW 50030, N'Không thể đặt lịch hẹn trong quá khứ.', 1; END;
    IF @MaBS IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.BacSi WHERE MaBS = @MaBS)
    BEGIN THROW 50020, N'Bác sĩ không tồn tại.', 1; END;
    IF dbo.fn_HoSoConHan(@MaHS, @Nam) = 0
    BEGIN THROW 50031, N'Hồ sơ chưa đóng lệ phí cho năm của ngày hẹn nên chưa được hưởng quyền khám định kỳ.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        IF EXISTS (SELECT 1 FROM dbo.LichHen WITH (UPDLOCK, HOLDLOCK)
                   WHERE MaHS = @MaHS AND Nam = @Nam AND Quy = @Quy AND TrangThai <> 'Huy')
        BEGIN THROW 50032, N'Hồ sơ đã có lịch khám định kỳ trong quý này (mỗi quý chỉ được 1 lần miễn phí).', 1; END;
        IF @MaBS IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.LichHen WITH (UPDLOCK, HOLDLOCK)
                                         WHERE MaBS = @MaBS AND NgayHen = @NgayHen AND TrangThai <> 'Huy')
        BEGIN THROW 50033, N'Bác sĩ đã có lịch hẹn vào thời điểm này.', 1; END;

        INSERT dbo.LichHen (MaHS, Nam, Quy, NgayHen, MaBS) VALUES (@MaHS, @Nam, @Quy, @NgayHen, @MaBS);
        SET @MaHen = SCOPE_IDENTITY();
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_HuyLichHen @MaHen int
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.LichHen SET TrangThai = 'Huy' WHERE MaHen = @MaHen AND TrangThai = 'DaHen';
    IF @@ROWCOUNT = 0
    BEGIN THROW 50034, N'Chỉ được hủy lịch hẹn đang ở trạng thái "Đã hẹn".', 1; END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_LichHenTheoHoSo @MaHS int, @ChiConHieuLuc bit = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.MaHen, l.Nam, l.Quy, l.NgayHen, bs.HoTen AS BacSi, l.TrangThai
    FROM dbo.LichHen l LEFT JOIN dbo.BacSi bs ON bs.MaBS = l.MaBS
    WHERE l.MaHS = @MaHS AND (@ChiConHieuLuc = 0 OR l.TrangThai = 'DaHen')
    ORDER BY l.NgayHen DESC;
END;
GO

/* =====================================================================
   MODULE 4 - KHÁM BỆNH: phiếu khám, xét nghiệm, toa thuốc
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.sp_TaoPhieuKham
    @MaHS int, @MaBS int, @LoaiKham varchar(10),
    @TrieuChung nvarchar(500) = NULL, @NhietDo decimal(4,1) = NULL, @HuyetAp varchar(10) = NULL,
    @MaHen int = NULL, @MaPK int OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @MaKhoa int, @Nam int = YEAR(SYSDATETIME()), @Quy int = (MONTH(SYSDATETIME()) + 2) / 3;

    IF NOT EXISTS (SELECT 1 FROM dbo.HoSoBenhAn WHERE MaHS = @MaHS)
    BEGIN THROW 50005, N'Hồ sơ bệnh án không tồn tại.', 1; END;
    SELECT @MaKhoa = MaKhoa FROM dbo.BacSi WHERE MaBS = @MaBS;
    IF @MaKhoa IS NULL
    BEGIN THROW 50020, N'Bác sĩ không tồn tại.', 1; END;
    IF dbo.fn_HoSoConHan(@MaHS, @Nam) = 0
    BEGIN THROW 50022, N'Hồ sơ chưa đóng lệ phí năm nay, cần gia hạn lệ phí trước khi khám.', 1; END;
    IF @LoaiKham = 'DinhKy' AND @MaHen IS NULL
    BEGIN THROW 50023, N'Khám sức khỏe định kỳ phải gắn với một lịch hẹn của quý.', 1; END;
    IF @LoaiKham = 'DieuTri' AND @MaHen IS NOT NULL
    BEGIN THROW 50025, N'Khám điều trị không được gắn với lịch hẹn định kỳ.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        IF EXISTS (SELECT 1 FROM dbo.PhieuKham WITH (UPDLOCK, HOLDLOCK) WHERE MaHS = @MaHS AND TrangThai = 'DangKham')
        BEGIN THROW 50021, N'Hồ sơ còn một phiếu khám chưa thanh toán. Hãy hoàn tất phiếu đó trước.', 1; END;

        IF @LoaiKham = 'DinhKy'
        BEGIN
            -- lịch hẹn phải thuộc hồ sơ, còn hiệu lực và đúng quý hiện tại
            IF NOT EXISTS (SELECT 1 FROM dbo.LichHen WITH (UPDLOCK)
                           WHERE MaHen = @MaHen AND MaHS = @MaHS AND TrangThai = 'DaHen' AND Nam = @Nam AND Quy = @Quy)
            BEGIN THROW 50024, N'Lịch hẹn không hợp lệ (sai hồ sơ, đã khám/hủy hoặc không thuộc quý hiện tại).', 1; END;
            UPDATE dbo.LichHen SET TrangThai = 'DaKham' WHERE MaHen = @MaHen;
        END;

        INSERT dbo.PhieuKham (MaHS, MaBS, MaKhoa, LoaiKham, TrieuChung, NhietDo, HuyetAp, MaHen)
        VALUES (@MaHS, @MaBS, @MaKhoa, @LoaiKham, @TrieuChung, @NhietDo, @HuyetAp, @MaHen);
        SET @MaPK = SCOPE_IDENTITY();
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CapNhatChanDoan @MaPK int, @ChanDoan nvarchar(300)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TT varchar(10) = (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @MaPK);
    IF @TT IS NULL BEGIN THROW 50012, N'Phiếu khám không tồn tại.', 1; END;
    IF @TT = 'HoanTat' BEGIN THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1; END;
    UPDATE dbo.PhieuKham SET ChanDoan = @ChanDoan WHERE MaPK = @MaPK;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ThemXetNghiem @MaPK int, @MaLoaiXN int, @KetQua nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @TT varchar(10) = (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @MaPK);
    DECLARE @DonGia decimal(18,0) = (SELECT DonGia FROM dbo.LoaiXetNghiem WHERE MaLoaiXN = @MaLoaiXN);
    IF @TT IS NULL BEGIN THROW 50012, N'Phiếu khám không tồn tại.', 1; END;
    IF @TT = 'HoanTat' BEGIN THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1; END;
    IF @DonGia IS NULL BEGIN THROW 50040, N'Loại xét nghiệm không tồn tại.', 1; END;
    IF EXISTS (SELECT 1 FROM dbo.PhieuXetNghiem WHERE MaPK = @MaPK AND MaLoaiXN = @MaLoaiXN)
    BEGIN THROW 50041, N'Phiếu khám đã có xét nghiệm này.', 1; END;
    INSERT dbo.PhieuXetNghiem (MaPK, MaLoaiXN, DonGia, KetQua) VALUES (@MaPK, @MaLoaiXN, @DonGia, @KetQua);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_CapNhatKetQuaXN @MaXN int, @KetQua nvarchar(500)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.PhieuXetNghiem WHERE MaXN = @MaXN)
    BEGIN THROW 50042, N'Không tìm thấy phiếu xét nghiệm.', 1; END;
    UPDATE dbo.PhieuXetNghiem SET KetQua = @KetQua WHERE MaXN = @MaXN;   -- trigger chặn nếu phiếu đã hoàn tất
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_LapToaThuoc
    @MaPK int, @GhiChu nvarchar(300) = NULL, @ChiTiet dbo.ChiTietToaType READONLY, @MaToa int OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @TT varchar(10) = (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @MaPK);
    IF @TT IS NULL BEGIN THROW 50012, N'Phiếu khám không tồn tại.', 1; END;
    IF @TT = 'HoanTat' BEGIN THROW 50010, N'Phiếu khám đã hoàn tất sau thanh toán viện phí, không được chỉnh sửa.', 1; END;
    IF NOT EXISTS (SELECT 1 FROM @ChiTiet) BEGIN THROW 50044, N'Toa thuốc phải có ít nhất một loại thuốc.', 1; END;
    IF EXISTS (SELECT 1 FROM @ChiTiet c WHERE NOT EXISTS (SELECT 1 FROM dbo.Thuoc t WHERE t.MaThuoc = c.MaThuoc))
    BEGIN THROW 50045, N'Toa có loại thuốc không tồn tại trong danh mục.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        IF EXISTS (SELECT 1 FROM dbo.ToaThuoc WITH (UPDLOCK, HOLDLOCK) WHERE MaPK = @MaPK)
        BEGIN THROW 50043, N'Phiếu khám này đã có toa thuốc.', 1; END;
        INSERT dbo.ToaThuoc (MaPK, GhiChu) VALUES (@MaPK, @GhiChu);
        SET @MaToa = SCOPE_IDENTITY();
        INSERT dbo.ChiTietToa (MaToa, MaThuoc, SoLuong, DonGia, LieuDung)
        SELECT @MaToa, c.MaThuoc, c.SoLuong, t.DonGia, c.LieuDung
        FROM @ChiTiet c JOIN dbo.Thuoc t ON t.MaThuoc = c.MaThuoc;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_PhieuDangKhamCuaHoSo @MaHS int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pk.MaPK, pk.NgayGioKham, pk.LoaiKham, bs.HoTen AS BacSi, pk.TrieuChung, pk.NhietDo, pk.HuyetAp, pk.ChanDoan
    FROM dbo.PhieuKham pk JOIN dbo.BacSi bs ON bs.MaBS = pk.MaBS
    WHERE pk.MaHS = @MaHS AND pk.TrangThai = 'DangKham';
END;
GO

/* =====================================================================
   MODULE 5 - THANH TOÁN VIỆN PHÍ VÀ HOÀN TẤT PHIẾU KHÁM (1 transaction)
   ===================================================================== */
CREATE OR ALTER PROCEDURE dbo.sp_DanhSachPhieuChoThanhToan
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pk.MaPK, h.MaHoSo, b.HoTen AS BenhNhan, pk.NgayGioKham, pk.LoaiKham, bs.HoTen AS BacSi
    FROM dbo.PhieuKham pk
    JOIN dbo.HoSoBenhAn h ON h.MaHS = pk.MaHS
    JOIN dbo.BenhNhan b ON b.MaBN = h.MaBN
    JOIN dbo.BacSi bs ON bs.MaBS = pk.MaBS
    WHERE pk.TrangThai = 'DangKham'
    ORDER BY pk.NgayGioKham;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_XemVienPhi @MaPK int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MaPK, TienKham, TienXetNghiem, TienThuoc, TongTien = TienKham + TienXetNghiem + TienThuoc
    FROM dbo.fn_TinhVienPhi(@MaPK);
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ThanhToanVaHoanTat @MaPK int, @PhuongThuc varchar(12), @MaHD int OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @PhuongThuc NOT IN ('TienMat', 'The', 'ChuyenKhoan')
    BEGIN THROW 50051, N'Phương thức thanh toán không hợp lệ.', 1; END;

    BEGIN TRY
        BEGIN TRAN;
        DECLARE @TT varchar(10) = (SELECT TrangThai FROM dbo.PhieuKham WITH (UPDLOCK, HOLDLOCK) WHERE MaPK = @MaPK);
        IF @TT IS NULL BEGIN THROW 50012, N'Phiếu khám không tồn tại.', 1; END;
        IF @TT = 'HoanTat' BEGIN THROW 50010, N'Phiếu khám đã được thanh toán và hoàn tất.', 1; END;

        INSERT dbo.HoaDonVienPhi (MaPK, TienKham, TienXetNghiem, TienThuoc, PhuongThuc)
        SELECT MaPK, TienKham, TienXetNghiem, TienThuoc, @PhuongThuc FROM dbo.fn_TinhVienPhi(@MaPK);
        SET @MaHD = SCOPE_IDENTITY();

        UPDATE dbo.PhieuKham SET TrangThai = 'HoanTat' WHERE MaPK = @MaPK;
        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* ---------- Danh mục cho combobox ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_DsBacSi AS
BEGIN SET NOCOUNT ON;
    SELECT b.MaBS, b.HoTen, k.TenKhoa AS Khoa, Hien = b.HoTen + N' - ' + k.TenKhoa
    FROM dbo.BacSi b JOIN dbo.Khoa k ON k.MaKhoa = b.MaKhoa ORDER BY b.HoTen;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_DsThuoc AS
BEGIN SET NOCOUNT ON; SELECT MaThuoc, TenThuoc, DonVi, DonGia FROM dbo.Thuoc ORDER BY TenThuoc; END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_DsLoaiXetNghiem AS
BEGIN SET NOCOUNT ON; SELECT MaLoaiXN, TenXN, DonGia FROM dbo.LoaiXetNghiem ORDER BY TenXN; END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_LayThamSo @TenThamSo varchar(50) AS
BEGIN SET NOCOUNT ON; SELECT GiaTri FROM dbo.ThamSo WHERE TenThamSo = @TenThamSo; END;
GO
PRINT N'02_procs.sql: hoàn tất.';
GO
