# Bài 4 – Quản lý cho thuê văn phòng ở cao ốc (C# WinForms + SQL Server)

## Cách chạy (Visual Studio 2022)
1. Mở SSMS → mở `Database\Database.sql` → **Execute** (tạo DB `QuanLyVanPhong` + dữ liệu mẫu).
2. Mở `QuanLyVanPhong.sln` bằng Visual Studio 2022 (lần đầu VS tự tải gói NuGet `Microsoft.Data.SqlClient`).
3. Mở `Db.cs`, sửa `ConnStr` nếu server của bạn không phải `.\SQLEXPRESS`
   (ví dụ LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=QuanLyVanPhong;Trusted_Connection=True;`).
4. Nhấn **F5**. Vào menu **6. Kiểm thử quy tắc** → **Chạy kiểm thử**.

> Nếu VS báo thiếu .NET 8: đổi `net8.0-windows` thành `net6.0-windows` trong file `.csproj`.

## Cấu trúc code (làm theo thứ tự này để tự code lại từng bước)
| Bước | File | Việc cần làm |
|---|---|---|
| 0 | New Project → *Windows Forms App* (C#, .NET 8) → NuGet cài `Microsoft.Data.SqlClient` | tạo khung |
| 1 | `Db.cs` | Chuỗi kết nối + hàm `Exec / Scalar / Table`. Có bản nhận `SqlTransaction` để dùng trong transaction |
| 2 | `RentalService.cs` | **Toàn bộ quy tắc nghiệp vụ** + transaction (`InTx`: lỗi → `Rollback`) |
| 3 | `Ui.cs` | Hàm tạo Label/TextBox/Button/Grid nhanh bằng code |
| 4 | `FrmPhong` | Form chỉ đọc: `DataGridView` ← `SELECT ... JOIN Tang` |
| 5 | `FrmKhachHang` | 3 TextBox + nút Thêm. Trùng SĐT → lỗi **UNIQUE** |
| 6 | `FrmHopDong` | Chọn khách, ngày, số tháng, tick nhiều phòng → `TaoHopDong` |
| 7 | `FrmThanhToan` | Tab 1: thu tiền thuê từng tháng. Tab 2: lập/thu hóa đơn dịch vụ cuối tháng |
| 8 | `FrmGiaHan` | Chọn hợp đồng + phòng + số tháng → `LapPhieuGiaHan` |
| 9 | `FrmMain` + `Program.cs` | Menu mở các form; `Program.Main` chạy `FrmMain` |
| 10 | `Tester.cs` + `FrmKiemThu` | Kiểm thử tự động |

## Quy tắc ↔ nơi cài đặt ↔ kiểm thử
| Quy tắc | Cài ở | Test |
|---|---|---|
| Thuê tối thiểu 6 tháng | `TaoHopDong` + `CHECK (SoThang >= 6)` | thuê 5 tháng bị từ chối, 6 tháng OK |
| Trả trước 6 tháng đầu | `ThemLichThu` (tháng 1–6 `DaThu = 1`) | 6/6 tháng đã thu |
| Tháng 7+ trả đầu tháng | `ThemLichThu` (`HanThu = ngày bắt đầu + (m-1) tháng`) | tháng 7 hạn = +6 tháng |
| Dịch vụ cuối tháng | `LapHoaDonDichVu` (`HanThu` = ngày cuối tháng) | hạn = cuối tháng; trùng bị UNIQUE chặn |
| Gia hạn ≤ 1/2 thời gian thuê | `LapPhieuGiaHan` (`soThangGH * 2 > soThang` → lỗi) | 7/12 bị từ chối, 6/12 OK |
| Gia hạn phải hết nợ | `KiemTraKhongNo` (tiền thuê quá hạn + dịch vụ chưa trả) | còn nợ → từ chối; trả xong → OK |
| Nhiều bảng cùng lúc | `InTx` (BEGIN TRAN / COMMIT / ROLLBACK) | thuê 2 phòng, 1 phòng lỗi → rollback sạch |

## Ví dụ transaction (trong `RentalService.InTx`)
```csharp
using var c = Db.Open();
using var t = c.BeginTransaction();
try   { /* nhiều lệnh INSERT/UPDATE dùng chung c, t */ t.Commit(); }
catch { t.Rollback(); throw; }
```

## Lưu ý
- Bảng `NhanVien`, `TheGiuXe` có trong CSDL (đúng đề) nhưng chưa làm form, vì không nằm trong các quy tắc cần kiểm thử.
- Dữ liệu kiểm thử (phòng `TST*`, khách `TEST-0000`) được xóa sạch sau mỗi lần chạy.
