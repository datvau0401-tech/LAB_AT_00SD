# eShopping – Cửa hàng ABC

Ứng dụng desktop **Windows Forms (.NET Framework 4.7.2)** mô phỏng hệ thống mua sắm trực tuyến e-Shopping: xem sản phẩm, giỏ hàng, đăng ký/đăng nhập khách hàng, đặt hàng và thanh toán bằng thẻ. Dữ liệu lưu trên **SQL Server LocalDB**.

## Chức năng

- Đăng ký, đăng nhập khách hàng
- Xem sản phẩm theo nhóm, xem chi tiết (mô tả, thông số kỹ thuật, hình ảnh, tình trạng hàng)
- Quản lý giỏ hàng (thêm, sửa số lượng, xóa)
- Đặt hàng: nhập người nhận, chọn khu vực và loại giao hàng, tính phí giao hàng
- Thanh toán bằng thẻ (cổng thanh toán giả lập), ghi nhật ký email xác nhận

## Cấu trúc thư mục

```
eShopping.sln
eShopping/
├── eShopping.csproj
├── App.config              # Chuỗi kết nối CSDL
├── Program.cs              # Điểm vào, kiểm tra kết nối DB khi khởi động
├── Models.cs               # Các lớp dữ liệu (SanPham, KhachHang, GioHang, ...)
├── Data/Db.cs              # Truy cập SQL Server (System.Data.SqlClient)
├── Services/               # Nghiệp vụ: KhachHang, GioHang, DatHang
├── Adapters/ExternalSystems.cs
│                           # Adapter tới hệ thống ngoài:
│                           #   IProductSystemAdapter (chỉ đọc schema ext)
│                           #   IPaymentGateway  -> MockPaymentGateway
│                           #   IEmailService    -> MockEmailService
├── Forms/                  # Giao diện: FrmMain, FrmDangNhap, FrmDangKy,
│                           #            FrmSanPham, FrmGioHang, FrmThanhToan
└── Database/eShopping.sql  # Script tạo CSDL và dữ liệu mẫu
```

## Kiến trúc

Ứng dụng chia 4 lớp: **Forms** (giao diện) → **Services** (nghiệp vụ) → **Adapters** (hệ thống ngoài) / **Data** (CSDL).

- Hệ thống quản lý sản phẩm bên ngoài được mô phỏng bằng schema `ext`. e-Shopping **chỉ đọc** schema này qua `ProductSystemAdapter`.
- Cổng thanh toán và dịch vụ email dùng bản giả lập (`Mock...`), có thể thay bằng bản thật mà không ảnh hưởng tầng nghiệp vụ nhờ interface.
- Dữ liệu của e-Shopping nằm trong schema `dbo`: `KhachHang`, `NguoiNhan`, `DonHang`, `ChiTietDonHang`, `TheThanhToan`, `LoaiThe`, `KhuVucGiaoHang`, `LoaiGiaoHang`, `BangGiaGiaoHang`, `NhatKyEmail`.

## Yêu cầu

- Windows, **Visual Studio 2022** (workload *.NET desktop development*)
- **.NET Framework 4.7.2 Developer Pack**
- **SQL Server LocalDB** (đi kèm Visual Studio, có thể chọn thêm trong Visual Studio Installer ở mục *Data storage and processing*)

## Hướng dẫn chạy

1. **Giải nén** zip, giữ nguyên `eShopping.sln` cạnh thư mục `eShopping`.
2. **Tạo CSDL:** mở `eShopping/Database/eShopping.sql` trong SSMS hoặc Visual Studio (*View → SQL Server Object Explorer*, kết nối `(localdb)\MSSQLLocalDB`) rồi chạy toàn bộ script. Script sẽ tạo database `eShoppingDB` cùng dữ liệu mẫu.
3. **Mở solution:** Visual Studio → *File → Open → Project/Solution* → chọn `eShopping.sln` (không dùng *Open Folder*).
4. **Restore và build:** chuột phải Solution → *Restore NuGet Packages*, sau đó *Build → Rebuild Solution*.
5. Bấm **F5** để chạy.

> Script `eShopping.sql` sẽ **xóa và tạo lại** các bảng, nên chạy lại sẽ mất dữ liệu cũ.

## Cấu hình kết nối

Chuỗi kết nối nằm trong `eShopping/App.config`:

```xml
<add name="eShoppingDb"
     connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=eShoppingDB;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
     providerName="System.Data.SqlClient" />
```

Nếu dùng SQL Server khác, đổi `Data Source` cho phù hợp.

## Xử lý lỗi thường gặp

| Lỗi | Cách sửa |
|---|---|
| `NETSDK1004: Assets file 'project.assets.json' not found` | Restore NuGet Packages, hoặc xóa thư mục `bin`, `obj` rồi Rebuild |
| `Could not find file ...\bin\Debug\net472\eShopping.exe` | Do build lỗi (thường là lỗi trên); sửa lỗi build rồi chạy lại |
| Không thấy *Restore NuGet Packages* | Đang ở *Folder View*; mở bằng `eShopping.sln` thay vì *Open Folder* |
| "Không kết nối được CSDL eShoppingDB" | Chưa chạy `eShopping.sql`, hoặc LocalDB chưa cài / sai `connectionString` |
