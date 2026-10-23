# LAB 7 - Quản lý nhân sự (Công ty sơn Đông Á)

Ứng dụng WinForms (C#, .NET Framework 4.7.2) quản lý nhân sự: tuyển dụng - thử việc, quyết định, hợp đồng và bảo hiểm, chấm công - phụ trội, khen thưởng, tính lương, thống kê.

- **Sinh viên:** Nguyễn Tấn Đạt
- **MSSV:** 1250080032
- **Môn:** Thực hành Phân tích thiết kế hướng đối tượng (HUTECH) - Bài 7

## Chức năng

Chương trình mở đầu bằng form menu chính, mỗi nút mở một form nghiệp vụ:

| Nút | Chức năng |
|---|---|
| Danh mục phòng ban | Thêm, sửa, xóa phòng ban |
| Tuyển dụng - Thử việc | Lập quyết định tuyển dụng (thời gian, lương thử việc, điều khoản), giám đốc ký quyết định |
| Khen thưởng - Kỷ luật | Lập quyết định khen thưởng cho cá nhân hoặc tập thể phòng ban, ký và tự lập phiếu chi |
| Lịch phân công ca | Phân công nhân viên theo ngày và ca làm việc |
| Chấm công & Phụ trội | Chấm công theo ngày hoặc cả tháng, phiếu ghi nhận giờ phụ trội (hệ số 1.5) |
| Hợp đồng & Bảo hiểm | Ký hợp đồng chính thức (dài hạn, ngắn hạn), tự lập hồ sơ BHYT, BHXH cho hợp đồng dài hạn và quyết định lương |
| Lương - Thống kê | Tính lương tháng, thống kê lương theo tháng |
| Đánh giá nhân sự | Chấm điểm 1-10 theo tháng, tự xếp loại |

### Công thức tính lương

```
Lương chính     = Lương cơ bản / Số ngày công chuẩn * Số ngày công
Tiền phụ trội   = Lương giờ * Tổng (số giờ * hệ số 1.5)
Khấu trừ BH     = Mức đóng BH * (Tỷ lệ BHXH + Tỷ lệ BHYT)   (chỉ hợp đồng dài hạn)
Thực nhận       = Lương chính + Phụ cấp + Phụ trội - Khấu trừ BH - Phạt trừ lương
```

Các tham số (số ngày công chuẩn, số giờ một ngày, tỷ lệ BH) lưu trong bảng `ThamSo`.

## Cấu trúc project

```
QuanLyNhanSu.sln
Database/
  QuanLyNhanSuDB.sql        Tạo CSDL, bảng và dữ liệu mẫu
QuanLyNhanSu/
  Program.cs                Điểm khởi chạy
  App.config                Chuỗi kết nối CSDL
  Models.cs                 Kết quả trả về từ service
  Data/                     Truy cập dữ liệu (Db, DbTx, Schema)
  Services/                 Nghiệp vụ (NhanSuService, LuongService, KhenThuongKyLuatService)
  UI/                       Form menu chính, form host và các tab
```

## Cách chạy

**Yêu cầu:** Visual Studio 2022, SQL Server (hoặc SQL Server Express), .NET Framework 4.7.2.

1. Mở SQL Server Management Studio, chạy file `Database/QuanLyNhanSuDB.sql` để tạo CSDL `QuanLyNhanSuDB` cùng dữ liệu mẫu. Lưu ý: chạy lại file này sẽ xóa CSDL cũ và tạo mới.
2. Mở `App.config`, sửa `Data Source` cho đúng tên server của máy bạn:

   ```xml
   <add name="QuanLyNhanSuDb"
        connectionString="Data Source=.;Initial Catalog=QuanLyNhanSuDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
        providerName="System.Data.SqlClient" />
   ```

   Một số giá trị `Data Source` thường gặp: `.`, `.\SQLEXPRESS`, `(LocalDB)\MSSQLLocalDB`.
3. Mở `QuanLyNhanSu.sln` bằng Visual Studio, chọn `QuanLyNhanSu` làm Startup Project rồi nhấn **F5**.

## Dữ liệu mẫu để thử

| Nhân viên | Tình trạng | Dùng để thử |
|---|---|---|
| NV001 | Hợp đồng dài hạn, có BHYT, BHXH | Tính lương tháng 9/2026 |
| NV002 | Hợp đồng ngắn hạn, có phạt trừ lương | Tính lương tháng 9/2026 |
| NV003 | Đã có quyết định tuyển dụng được ký | Ký hợp đồng chính thức |
| NV004 | Có quyết định tuyển dụng chờ ký | Giám đốc ký quyết định |
| NV005 | Chưa có gì | Lập quyết định tuyển dụng mới |

## Ghi chú

- Hai bảng `LichPhanCong` và `DanhGiaNhanSu` được chương trình tự tạo khi khởi động nếu chưa có.
- Chức năng lập quyết định kỷ luật chưa được cài đặt trong phần nghiệp vụ.
