# LAB1 - Ứng dụng Quản Lý Thư Viện (Windows Forms)

## Mô tả
Ứng dụng quản lý thư viện viết bằng C# Windows Forms (.NET Framework 4.7.2), sử dụng SQL Server làm cơ sở dữ liệu.

## Các chức năng chính (3 Form)
- **FrmMain**: Form chính, điều hướng đến các chức năng khác.
- **FrmDanhMuc**: Quản lý (CRUD) Nhân viên / Thể loại / Nhà xuất bản (dùng TabControl).
- **FrmSach**: Quản lý (CRUD) đầu sách.

## Công nghệ sử dụng
- C# / .NET Framework 4.7.2
- Windows Forms
- SQL Server (ADO.NET, System.Data.SqlClient)

## Cách chạy chương trình
1. Mở file `QuanLyThuVien.sln` bằng Visual Studio.
2. Chạy script `Database/QuanLyThuVien.sql` để tạo database `QuanLyThuVienDB`.
3. Mở `App.config` trong project, chỉnh lại `connectionString` cho đúng SQL Server đang dùng.
4. Nhấn `F5` hoặc Build > Rebuild Solution để chạy chương trình.

## Cấu trúc thư mục
```
QuanLyThuVien/
├── Models.cs
├── Program.cs
├── App.config
├── Data/
│   └── Db.cs
├── Database/
│   └── QuanLyThuVien.sql
├── Forms/
│   ├── FrmMain.cs / .Designer.cs
│   ├── FrmDanhMuc.cs / .Designer.cs
│   └── FrmSach.cs / .Designer.cs
└── Services/
    ├── DanhMucService.cs
    └── SachService.cs
```
