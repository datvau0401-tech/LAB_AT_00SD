/* 03_seed.sql : dữ liệu danh mục ban đầu (chạy lại nhiều lần vẫn an toàn) */
USE QuanLyBenhAn;
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

MERGE dbo.ThamSo AS t
USING (VALUES ('LePhiNam', 500000, N'Lệ phí quản lý hồ sơ và khám định kỳ mỗi năm (VNĐ)'),
              ('PhiKham',  150000, N'Phí khám điều trị mỗi lượt (VNĐ)')) AS s(Ten, GiaTri, MoTa)
ON t.TenThamSo = s.Ten
WHEN NOT MATCHED THEN INSERT (TenThamSo, GiaTri, MoTa) VALUES (s.Ten, s.GiaTri, s.MoTa);
GO

INSERT dbo.Khoa (TenKhoa)
SELECT v.TenKhoa FROM (VALUES (N'Nội tổng quát'), (N'Tim mạch'), (N'Nhi'), (N'Da liễu'), (N'Tai mũi họng')) v(TenKhoa)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Khoa k WHERE k.TenKhoa = v.TenKhoa);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BacSi)
INSERT dbo.BacSi (HoTen, MaKhoa, ChuyenMon)
SELECT v.HoTen, k.MaKhoa, v.ChuyenMon
FROM (VALUES (N'BS. Nguyễn Văn An',  N'Nội tổng quát', N'Nội tiêu hóa'),
             (N'BS. Trần Thị Bình',  N'Nội tổng quát', N'Khám sức khỏe tổng quát'),
             (N'BS. Lê Quang Cường', N'Tim mạch',      N'Tăng huyết áp'),
             (N'BS. Phạm Thu Dung',  N'Nhi',           N'Nhi hô hấp'),
             (N'BS. Hoàng Minh Em',  N'Da liễu',       N'Da liễu dị ứng'),
             (N'BS. Võ Thanh Phong', N'Tai mũi họng',  N'Viêm xoang')) v(HoTen, TenKhoa, ChuyenMon)
JOIN dbo.Khoa k ON k.TenKhoa = v.TenKhoa;
GO

INSERT dbo.Thuoc (TenThuoc, DonVi, DonGia)
SELECT v.Ten, v.DonVi, v.DonGia FROM (VALUES
    (N'Paracetamol 500mg', N'viên', 1500), (N'Amoxicillin 500mg', N'viên', 4000),
    (N'Ibuprofen 400mg', N'viên', 2500),   (N'Loratadine 10mg', N'viên', 3000),
    (N'Omeprazole 20mg', N'viên', 3500),   (N'Amlodipine 5mg', N'viên', 3200),
    (N'Vitamin C 500mg', N'viên', 1200),   (N'Oresol', N'gói', 5000)) v(Ten, DonVi, DonGia)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Thuoc t WHERE t.TenThuoc = v.Ten);
GO

INSERT dbo.LoaiXetNghiem (TenXN, DonGia)
SELECT v.Ten, v.DonGia FROM (VALUES
    (N'Công thức máu', 90000), (N'Đường huyết lúc đói', 50000), (N'Mỡ máu (lipid)', 120000),
    (N'Chức năng gan', 150000), (N'Tổng phân tích nước tiểu', 60000), (N'X-quang ngực thẳng', 180000)) v(Ten, DonGia)
WHERE NOT EXISTS (SELECT 1 FROM dbo.LoaiXetNghiem l WHERE l.TenXN = v.Ten);
GO
PRINT N'03_seed.sql: hoàn tất.';
GO
