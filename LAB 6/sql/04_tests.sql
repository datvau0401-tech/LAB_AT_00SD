/* =====================================================================
   04_tests.sql : kiểm thử các quy tắc nghiệp vụ trực tiếp trên SQL Server
   - Dữ liệu thử dùng SĐT 09900000xx và được dọn sạch trước/sau khi chạy.
   - Chạy sau 01, 02, 03:  sqlcmd -S . -E -I -f 65001 -d QuanLyBenhAn -i 04_tests.sql
   - Kết quả: bảng PASS/FAIL ở cuối. (Test lịch hẹn dùng "5 phút nữa" nên đừng chạy
     đúng vào phút cuối cùng của một quý.)
   ===================================================================== */
USE QuanLyBenhAn;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_DonDepDuLieuTest
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID('tempdb..#D') IS NOT NULL DROP TABLE #D;
    SELECT h.MaHS, b.MaBN INTO #D
    FROM dbo.BenhNhan b LEFT JOIN dbo.HoSoBenhAn h ON h.MaBN = b.MaBN
    WHERE b.DienThoai LIKE '09900000__';

    DISABLE TRIGGER dbo.trg_ChiTietToa_Khoa ON dbo.ChiTietToa;
    DISABLE TRIGGER dbo.trg_ToaThuoc_Khoa   ON dbo.ToaThuoc;
    DISABLE TRIGGER dbo.trg_PhieuXN_Khoa    ON dbo.PhieuXetNghiem;
    BEGIN TRY
        DELETE c FROM dbo.ChiTietToa c JOIN dbo.ToaThuoc t ON t.MaToa = c.MaToa
               JOIN dbo.PhieuKham p ON p.MaPK = t.MaPK WHERE p.MaHS IN (SELECT MaHS FROM #D);
        DELETE t FROM dbo.ToaThuoc t JOIN dbo.PhieuKham p ON p.MaPK = t.MaPK WHERE p.MaHS IN (SELECT MaHS FROM #D);
        DELETE x FROM dbo.PhieuXetNghiem x JOIN dbo.PhieuKham p ON p.MaPK = x.MaPK WHERE p.MaHS IN (SELECT MaHS FROM #D);
        DELETE h FROM dbo.HoaDonVienPhi h JOIN dbo.PhieuKham p ON p.MaPK = h.MaPK WHERE p.MaHS IN (SELECT MaHS FROM #D);
        DELETE FROM dbo.PhieuKham WHERE MaHS IN (SELECT MaHS FROM #D);
        DELETE FROM dbo.LichHen   WHERE MaHS IN (SELECT MaHS FROM #D);
        DELETE FROM dbo.LePhiNam  WHERE MaHS IN (SELECT MaHS FROM #D);
        DELETE FROM dbo.HoSoBenhAn WHERE MaHS IN (SELECT MaHS FROM #D);
        DELETE FROM dbo.BenhNhan  WHERE MaBN IN (SELECT MaBN FROM #D);
    END TRY
    BEGIN CATCH
        ENABLE TRIGGER dbo.trg_ChiTietToa_Khoa ON dbo.ChiTietToa;
        ENABLE TRIGGER dbo.trg_ToaThuoc_Khoa   ON dbo.ToaThuoc;
        ENABLE TRIGGER dbo.trg_PhieuXN_Khoa    ON dbo.PhieuXetNghiem;
        THROW;
    END CATCH;
    ENABLE TRIGGER dbo.trg_ChiTietToa_Khoa ON dbo.ChiTietToa;
    ENABLE TRIGGER dbo.trg_ToaThuoc_Khoa   ON dbo.ToaThuoc;
    ENABLE TRIGGER dbo.trg_PhieuXN_Khoa    ON dbo.PhieuXetNghiem;
END;
GO

EXEC dbo.sp_DonDepDuLieuTest;
GO

IF OBJECT_ID('tempdb..#KQ') IS NOT NULL DROP TABLE #KQ;
CREATE TABLE #KQ (STT int IDENTITY(1,1), Ten nvarchar(200), Dat bit, GhiChu nvarchar(500));
IF OBJECT_ID('tempdb..#TC') IS NOT NULL DROP TABLE #TC;
CREATE TABLE #TC (MaHS int, MaHoSo varchar(10), HoTen nvarchar(100), NgaySinh date, DienThoai varchar(15),
                  DiaChi nvarchar(200), NgayCap date, ConHan bit);
IF OBJECT_ID('tempdb..#LS') IS NOT NULL DROP TABLE #LS;
CREATE TABLE #LS (MaPK int, NgayGioKham datetime2(0), LoaiKham varchar(10), BacSi nvarchar(100), Khoa nvarchar(100),
                  TrieuChung nvarchar(500), NhietDo decimal(4,1), HuyetAp varchar(10), ChanDoan nvarchar(300),
                  TrangThai varchar(10), TongTien decimal(19,0));
GO

SET NOCOUNT ON;
DECLARE @bnA int, @hsA int, @maA varchar(10), @bnB int, @hsB int, @maB varchar(10), @bnC int, @hsC int, @maC varchar(10);
DECLARE @bs int = (SELECT TOP 1 MaBS FROM dbo.BacSi ORDER BY MaBS);
DECLARE @pkA int, @pkB int, @toa int, @hd int, @henA int, @henB int, @henB2 int, @n int, @tong decimal(19,0);
DECLARE @xnCTM int = (SELECT MaLoaiXN FROM dbo.LoaiXetNghiem WHERE TenXN = N'Công thức máu');
DECLARE @xnDH  int = (SELECT MaLoaiXN FROM dbo.LoaiXetNghiem WHERE TenXN = N'Đường huyết lúc đói');
DECLARE @para int = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Paracetamol 500mg');
DECLARE @amox int = (SELECT MaThuoc FROM dbo.Thuoc WHERE TenThuoc = N'Amoxicillin 500mg');
DECLARE @t5 datetime2(0) = DATEADD(MINUTE, 5, SYSDATETIME());
DECLARE @t6 datetime2(0) = DATEADD(MINUTE, 6, SYSDATETIME());
DECLARE @toaCT dbo.ChiTietToaType;
DECLARE @toaBad dbo.ChiTietToaType;
INSERT @toaCT VALUES (@para, 10, N'Ngày 3 lần, mỗi lần 1 viên'), (@amox, 14, N'Ngày 2 lần, mỗi lần 1 viên');
INSERT @toaBad VALUES (@para, 5, NULL), (999999, 1, NULL);

/* ---------------- ĐĂNG KÝ LẦN ĐẦU + LỆ PHÍ NĂM + CẤP HỒ SƠ ---------------- */
BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'Nguyễn Test A', N'1 Lê Lợi, Q1', '1990-01-01', '0990000001', NULL, 500000, @bnA OUT, @hsA OUT, @maA OUT;
    INSERT #KQ VALUES (N'T01 Đăng ký lần đầu: cấp mã hồ sơ + ghi nhận lệ phí năm',
        IIF(@maA LIKE 'HS[0-9][0-9][0-9][0-9][0-9][0-9]'
            AND EXISTS (SELECT 1 FROM dbo.LePhiNam WHERE MaHS = @hsA AND Nam = YEAR(GETDATE()) AND SoTien = 500000), 1, 0), @maA);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T01 Đăng ký lần đầu', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'Nguyễn Test A2', N'2 Lê Lợi', '1991-01-01', '0990000001', NULL, 500000, @bnC OUT, @hsC OUT, @maC OUT;
    INSERT #KQ VALUES (N'T02 Trùng SĐT bị từ chối', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T02 Trùng SĐT bị từ chối', IIF(ERROR_NUMBER() = 50002, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'Test Thiếu Phí', N'3 Lê Lợi', '1992-01-01', '0990000099', NULL, 100000, @bnC OUT, @hsC OUT, @maC OUT;
    INSERT #KQ VALUES (N'T03 Lệ phí thiếu: không tạo hồ sơ (rollback)', 0, N'Không báo lỗi');
END TRY BEGIN CATCH
    INSERT #KQ VALUES (N'T03 Lệ phí thiếu: không tạo hồ sơ (rollback)',
        IIF(ERROR_NUMBER() = 50003 AND NOT EXISTS (SELECT 1 FROM dbo.BenhNhan WHERE DienThoai = '0990000099'), 1, 0), ERROR_MESSAGE());
END CATCH;

BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'   ', N'4 Lê Lợi', '1992-01-01', '0990000098', NULL, 500000, @bnC OUT, @hsC OUT, @maC OUT;
    INSERT #KQ VALUES (N'T04 Họ tên trống bị từ chối', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T04 Họ tên trống bị từ chối', IIF(ERROR_NUMBER() = 50001, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_GiaHanLePhi @hsA, 500000;
    INSERT #KQ VALUES (N'T05 Đóng lệ phí trùng năm bị từ chối', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T05 Đóng lệ phí trùng năm bị từ chối', IIF(ERROR_NUMBER() = 50006, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    DECLARE @namSau smallint = YEAR(GETDATE()) + 1;
    EXEC dbo.sp_GiaHanLePhi @hsA, 500000, @namSau;
    INSERT #KQ VALUES (N'T06 Gia hạn lệ phí cho năm sau', IIF(dbo.fn_HoSoConHan(@hsA, @namSau) = 1, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T06 Gia hạn lệ phí cho năm sau', 0, ERROR_MESSAGE()); END CATCH;

/* ---------------- PHIẾU KHÁM, XÉT NGHIỆM, TOA, THANH TOÁN ---------------- */
BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsA, @bs, 'DieuTri', N'Nhức đầu, sốt nhẹ', 37.8, '120/80', NULL, @pkA OUT;
    INSERT #KQ VALUES (N'T07 Tạo phiếu khám điều trị', IIF(@pkA IS NOT NULL, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T07 Tạo phiếu khám điều trị', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsA, @bs, 'DieuTri', N'Ho', 36.8, '110/70', NULL, @n OUT;
    INSERT #KQ VALUES (N'T08 Không mở phiếu thứ 2 khi phiếu cũ chưa thanh toán', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T08 Không mở phiếu thứ 2 khi phiếu cũ chưa thanh toán', IIF(ERROR_NUMBER() = 50021, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThemXetNghiem @pkA, @xnCTM, N'Bạch cầu tăng nhẹ';
    INSERT #KQ VALUES (N'T09 Liên kết xét nghiệm với phiếu khám', IIF(EXISTS (SELECT 1 FROM dbo.PhieuXetNghiem WHERE MaPK = @pkA AND MaLoaiXN = @xnCTM), 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T09 Liên kết xét nghiệm với phiếu khám', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThemXetNghiem @pkA, @xnCTM;
    INSERT #KQ VALUES (N'T10 Xét nghiệm trùng loại trong phiếu bị từ chối', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T10 Xét nghiệm trùng loại trong phiếu bị từ chối', IIF(ERROR_NUMBER() = 50041, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_LapToaThuoc @pkA, N'Uống sau ăn', @toaBad, @toa OUT;
    INSERT #KQ VALUES (N'T11 Toa có thuốc không tồn tại bị từ chối, không tạo toa dở dang', 0, N'Không báo lỗi');
END TRY BEGIN CATCH
    INSERT #KQ VALUES (N'T11 Toa có thuốc không tồn tại bị từ chối, không tạo toa dở dang',
        IIF(ERROR_NUMBER() = 50045 AND NOT EXISTS (SELECT 1 FROM dbo.ToaThuoc WHERE MaPK = @pkA), 1, 0), ERROR_MESSAGE());
END CATCH;

BEGIN TRY
    EXEC dbo.sp_LapToaThuoc @pkA, N'Uống sau ăn', @toaCT, @toa OUT;
    INSERT #KQ VALUES (N'T12 Lập toa thuốc gắn với phiếu khám',
        IIF((SELECT COUNT(*) FROM dbo.ChiTietToa WHERE MaToa = @toa) = 2, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T12 Lập toa thuốc gắn với phiếu khám', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_LapToaThuoc @pkA, NULL, @toaCT, @n OUT;
    INSERT #KQ VALUES (N'T13 Mỗi phiếu khám chỉ có 1 toa', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T13 Mỗi phiếu khám chỉ có 1 toa', IIF(ERROR_NUMBER() = 50043, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    UPDATE dbo.PhieuKham SET TrangThai = 'HoanTat' WHERE MaPK = @pkA;
    INSERT #KQ VALUES (N'T14 Không hoàn tất phiếu khi chưa thanh toán', 0, N'Không báo lỗi');
END TRY BEGIN CATCH
    INSERT #KQ VALUES (N'T14 Không hoàn tất phiếu khi chưa thanh toán',
        IIF(ERROR_NUMBER() = 50011 AND (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @pkA) = 'DangKham', 1, 0), ERROR_MESSAGE());
END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThanhToanVaHoanTat @pkA, 'TienMat', @hd OUT;
    -- 150.000 (khám) + 90.000 (XN) + 10*1.500 + 14*4.000 = 311.000
    INSERT #KQ VALUES (N'T15 Thanh toán viện phí rồi hoàn tất phiếu (311.000đ)',
        IIF((SELECT TongTien FROM dbo.HoaDonVienPhi WHERE MaHD = @hd) = 311000
            AND (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @pkA) = 'HoanTat', 1, 0),
        CONVERT(nvarchar(30), (SELECT TongTien FROM dbo.HoaDonVienPhi WHERE MaHD = @hd)));
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T15 Thanh toán viện phí rồi hoàn tất phiếu', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThanhToanVaHoanTat @pkA, 'TienMat', @n OUT;
    INSERT #KQ VALUES (N'T16 Không thanh toán 2 lần', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T16 Không thanh toán 2 lần', IIF(ERROR_NUMBER() = 50010, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThemXetNghiem @pkA, @xnDH;
    INSERT #KQ VALUES (N'T17 Không thêm xét nghiệm sau khi hoàn tất', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T17 Không thêm xét nghiệm sau khi hoàn tất', IIF(ERROR_NUMBER() = 50010, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    UPDATE dbo.PhieuXetNghiem SET KetQua = N'Sửa trái phép' WHERE MaPK = @pkA;
    INSERT #KQ VALUES (N'T18 Không sửa kết quả xét nghiệm sau khi hoàn tất', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T18 Không sửa kết quả xét nghiệm sau khi hoàn tất', IIF(ERROR_NUMBER() = 50010, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    DELETE FROM dbo.ChiTietToa WHERE MaToa = @toa;
    INSERT #KQ VALUES (N'T19 Không xóa chi tiết toa sau khi hoàn tất', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T19 Không xóa chi tiết toa sau khi hoàn tất', IIF(ERROR_NUMBER() = 50010, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_CapNhatChanDoan @pkA, N'Sửa trái phép';
    INSERT #KQ VALUES (N'T20 Không sửa chẩn đoán sau khi hoàn tất', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T20 Không sửa chẩn đoán sau khi hoàn tất', IIF(ERROR_NUMBER() = 50010, 1, 0), ERROR_MESSAGE()); END CATCH;

/* ---------------- TRA CỨU HỒ SƠ / LỊCH SỬ ---------------- */
BEGIN TRY
    DELETE #TC; INSERT #TC EXEC dbo.sp_TraCuuHoSo @maA;
    SELECT @n = COUNT(*) FROM #TC WHERE MaHS = @hsA;
    INSERT #KQ VALUES (N'T21a Tra cứu theo mã hồ sơ', IIF(@n = 1 AND (SELECT COUNT(*) FROM #TC) = 1, 1, 0), @maA);
    DELETE #TC; INSERT #TC EXEC dbo.sp_TraCuuHoSo '0990000001';
    INSERT #KQ VALUES (N'T21b Tra cứu theo số điện thoại', IIF((SELECT COUNT(*) FROM #TC WHERE MaHS = @hsA) = 1, 1, 0), NULL);
    DELETE #TC; INSERT #TC EXEC dbo.sp_TraCuuHoSo N'Test A';
    INSERT #KQ VALUES (N'T21c Tra cứu theo một phần họ tên', IIF((SELECT COUNT(*) FROM #TC WHERE MaHS = @hsA) = 1, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T21 Tra cứu hồ sơ', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    DELETE #LS; INSERT #LS EXEC dbo.sp_LichSuKham @hsA;
    INSERT #KQ VALUES (N'T22 Lịch sử khám hiển thị phiếu, trạng thái và viện phí',
        IIF((SELECT COUNT(*) FROM #LS WHERE MaPK = @pkA AND TrangThai = 'HoanTat' AND TongTien = 311000) = 1, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T22 Lịch sử khám', 0, ERROR_MESSAGE()); END CATCH;

/* ---------------- LỊCH ĐỊNH KỲ THEO QUÝ ---------------- */
BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'Lê Test B', N'5 Hai Bà Trưng', '1985-05-05', '0990000002', NULL, 600000, @bnB OUT, @hsB OUT, @maB OUT;
    EXEC dbo.sp_TaoLichHen @hsB, @t5, @bs, @henB OUT;
    INSERT #KQ VALUES (N'T23 Đặt lịch khám định kỳ cho quý hiện tại',
        IIF(EXISTS (SELECT 1 FROM dbo.LichHen WHERE MaHen = @henB AND Quy = (MONTH(GETDATE()) + 2) / 3 AND TrangThai = 'DaHen'), 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T23 Đặt lịch khám định kỳ', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoLichHen @hsB, @t6, NULL, @n OUT;
    INSERT #KQ VALUES (N'T24 Mỗi quý chỉ 1 lịch định kỳ miễn phí', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T24 Mỗi quý chỉ 1 lịch định kỳ miễn phí', IIF(ERROR_NUMBER() = 50032, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_HuyLichHen @henB;
    EXEC dbo.sp_TaoLichHen @hsB, @t5, @bs, @henB2 OUT;
    INSERT #KQ VALUES (N'T25 Hủy lịch rồi đặt lại trong cùng quý', IIF(@henB2 <> @henB, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T25 Hủy lịch rồi đặt lại trong cùng quý', 0, ERROR_MESSAGE()); END CATCH;
SET @henB = @henB2;

BEGIN TRY
    EXEC dbo.sp_TaoLichHen @hsB, '2000-01-01', NULL, @n OUT;
    INSERT #KQ VALUES (N'T26 Không đặt lịch hẹn trong quá khứ', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T26 Không đặt lịch hẹn trong quá khứ', IIF(ERROR_NUMBER() = 50030, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    DECLARE @dauNamSau datetime2(0) = DATEFROMPARTS(YEAR(GETDATE()) + 1, 1, 2);
    EXEC dbo.sp_TaoLichHen @hsB, @dauNamSau, NULL, @n OUT;
    INSERT #KQ VALUES (N'T27 Chưa đóng lệ phí năm sau thì không được đặt lịch năm sau', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T27 Chưa đóng lệ phí năm sau thì không được đặt lịch năm sau', IIF(ERROR_NUMBER() = 50031, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsB, @bs, 'DinhKy', NULL, NULL, NULL, NULL, @n OUT;
    INSERT #KQ VALUES (N'T28 Khám định kỳ bắt buộc có lịch hẹn', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T28 Khám định kỳ bắt buộc có lịch hẹn', IIF(ERROR_NUMBER() = 50023, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoLichHen @hsA, @t6, NULL, @henA OUT;
    EXEC dbo.sp_TaoPhieuKham @hsB, @bs, 'DinhKy', NULL, NULL, NULL, @henA, @n OUT;
    INSERT #KQ VALUES (N'T29 Không dùng lịch hẹn của hồ sơ khác', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T29 Không dùng lịch hẹn của hồ sơ khác', IIF(ERROR_NUMBER() = 50024, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsB, @bs, 'DieuTri', NULL, NULL, NULL, @henB, @n OUT;
    INSERT #KQ VALUES (N'T30 Khám điều trị không được gắn lịch hẹn định kỳ', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T30 Khám điều trị không được gắn lịch hẹn định kỳ', IIF(ERROR_NUMBER() = 50025, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsB, @bs, 'DinhKy', N'Khám sức khỏe định kỳ', 36.6, '115/75', @henB, @pkB OUT;
    INSERT #KQ VALUES (N'T31 Khám định kỳ theo lịch hẹn: lịch chuyển sang "Đã khám"',
        IIF((SELECT TrangThai FROM dbo.LichHen WHERE MaHen = @henB) = 'DaKham'
            AND (SELECT MaHen FROM dbo.PhieuKham WHERE MaPK = @pkB) = @henB, 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T31 Khám định kỳ theo lịch hẹn', 0, ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_TaoLichHen @hsB, @t6, NULL, @n OUT;
    INSERT #KQ VALUES (N'T32 Quý đã khám định kỳ thì không đặt thêm', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T32 Quý đã khám định kỳ thì không đặt thêm', IIF(ERROR_NUMBER() = 50032, 1, 0), ERROR_MESSAGE()); END CATCH;

BEGIN TRY
    EXEC dbo.sp_ThemXetNghiem @pkB, @xnCTM, N'Bình thường';
    EXEC dbo.sp_ThanhToanVaHoanTat @pkB, 'TienMat', @hd OUT;
    INSERT #KQ VALUES (N'T33 Khám định kỳ miễn phí (hóa đơn 0đ) và hoàn tất sau thanh toán',
        IIF((SELECT TongTien FROM dbo.HoaDonVienPhi WHERE MaHD = @hd) = 0
            AND (SELECT TrangThai FROM dbo.PhieuKham WHERE MaPK = @pkB) = 'HoanTat', 1, 0), NULL);
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T33 Khám định kỳ miễn phí', 0, ERROR_MESSAGE()); END CATCH;

/* ---------------- HỒ SƠ HẾT HẠN LỆ PHÍ ---------------- */
BEGIN TRY
    EXEC dbo.sp_DangKyLanDau N'Phạm Test C', N'7 Nguyễn Huệ', '2000-02-02', '0990000003', NULL, 500000, @bnC OUT, @hsC OUT, @maC OUT;
    DELETE FROM dbo.LePhiNam WHERE MaHS = @hsC;           -- giả lập hồ sơ chưa gia hạn năm nay
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T34 Chuẩn bị hồ sơ hết hạn', 0, ERROR_MESSAGE()); END CATCH;
BEGIN TRY
    EXEC dbo.sp_TaoPhieuKham @hsC, @bs, 'DieuTri', N'Đau họng', NULL, NULL, NULL, @n OUT;
    INSERT #KQ VALUES (N'T34a Hồ sơ chưa đóng lệ phí năm nay không được khám', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T34a Hồ sơ chưa đóng lệ phí năm nay không được khám', IIF(ERROR_NUMBER() = 50022, 1, 0), ERROR_MESSAGE()); END CATCH;
BEGIN TRY
    EXEC dbo.sp_TaoLichHen @hsC, @t5, NULL, @n OUT;
    INSERT #KQ VALUES (N'T34b Hồ sơ chưa đóng lệ phí không được đặt lịch định kỳ', 0, N'Không báo lỗi');
END TRY BEGIN CATCH INSERT #KQ VALUES (N'T34b Hồ sơ chưa đóng lệ phí không được đặt lịch định kỳ', IIF(ERROR_NUMBER() = 50031, 1, 0), ERROR_MESSAGE()); END CATCH;

/* ---------------- KẾT QUẢ ---------------- */
SELECT STT, Ten, Kq = IIF(Dat = 1, N'PASS', N'FAIL'), GhiChu FROM #KQ ORDER BY STT;
SELECT Tong = COUNT(*), Pass = SUM(CAST(Dat AS int)), Fail = SUM(1 - CAST(Dat AS int)) FROM #KQ;
GO

EXEC dbo.sp_DonDepDuLieuTest;
PRINT N'04_tests.sql: xong - đã dọn dữ liệu thử.';
GO
