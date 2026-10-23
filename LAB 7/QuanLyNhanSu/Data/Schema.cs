using System;

namespace QuanLyNhanSu.Data
{
    // Tạo thêm các bảng phục vụ form Lịch phân công ca và Đánh giá nhân sự (chạy nhiều lần vẫn an toàn)
    public static class Schema
    {
        public static void DamBao()
        {
            Db.Execute(@"IF OBJECT_ID('LichPhanCong','U') IS NULL
                CREATE TABLE LichPhanCong(
                    Id      INT IDENTITY(1,1) PRIMARY KEY,
                    MaNV    VARCHAR(20)   NOT NULL REFERENCES NhanVien(MaNV),
                    NgayLam DATE          NOT NULL,
                    Ca      NVARCHAR(30)  NOT NULL,
                    GhiChu  NVARCHAR(200) NULL)");
            Db.Execute(@"IF OBJECT_ID('DanhGiaNhanSu','U') IS NULL
                CREATE TABLE DanhGiaNhanSu(
                    Id      INT IDENTITY(1,1) PRIMARY KEY,
                    MaNV    VARCHAR(20)   NOT NULL REFERENCES NhanVien(MaNV),
                    Thang   INT           NOT NULL,
                    Nam     INT           NOT NULL,
                    Diem    INT           NOT NULL CHECK (Diem BETWEEN 1 AND 10),
                    XepLoai NVARCHAR(30)  NOT NULL,
                    NhanXet NVARCHAR(300) NULL)");
        }
    }
}
