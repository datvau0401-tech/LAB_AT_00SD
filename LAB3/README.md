# Quản Lý Khách Sạn

Ứng dụng desktop **Windows Forms (.NET Framework 4.8)** quản lý nghiệp vụ khách sạn: danh mục, phòng - tiện nghi, đặt/nhận phòng, sử dụng dịch vụ, trả phòng - đền bù - hóa đơn - thanh toán, và thống kê. Dữ liệu được lưu trữ trên **SQL Server**, thao tác qua ADO.NET (`SqlClient`).

## Công nghệ sử dụng

- **Ngôn ngữ:** C# (LangVersion 7.3)
- **Nền tảng:** .NET Framework 4.8, Windows Forms (`WinExe`)
- **Cơ sở dữ liệu:** Microsoft SQL Server (qua `System.Data.SqlClient`)
- **IDE:** Visual Studio (kèm sẵn file `.sln`)

## Cấu trúc thư mục

```
QuanLyKhachSan/
├── QuanLyKhachSan.sln
└── QuanLyKhachSan/
    ├── Program.cs              # Điểm khởi động ứng dụng
    ├── DbHelper.cs             # Lớp tiện ích dùng chung để kết nối & thao tác SQL Server
    ├── FrmMain.cs              # Form chính (menu điều hướng)
    ├── FrmDanhMuc.cs           # Quản lý danh mục: khu vực, nhân viên, loại tiện nghi, dịch vụ, quy định đền bù
    ├── FrmPhongTienNghi.cs     # Quản lý phòng, tiện nghi, phiếu lắp đặt tiện nghi
    ├── FrmDatPhong.cs          # Quản lý khách hàng, đặt phòng, nhận phòng
    ├── FrmDichVu.cs            # Ghi nhận sử dụng dịch vụ trong thời gian lưu trú
    ├── FrmTraPhong.cs          # Trả phòng, lập phiếu đền bù, xuất hóa đơn, thanh toán
    ├── FrmThongKe.cs           # Thống kê doanh thu, tình trạng phòng theo khoảng thời gian
    ├── App.config              # Cấu hình chuỗi kết nối cơ sở dữ liệu
    └── QuanLyKhachSan.csproj
```

## Chức năng chính

Form chính (`FrmMain`) là menu điều hướng tới 6 phân hệ:

1. **Danh mục** – CRUD các danh mục dùng chung: Khu vực, Nhân viên, Loại tiện nghi, Dịch vụ, Quy định đền bù (theo mức độ thiệt hại của từng loại tiện nghi).
2. **Phòng – Tiện nghi** – Quản lý thông tin phòng (số phòng, khu vực, sức chứa, đơn giá, trạng thái), tiện nghi trong phòng, và phiếu lắp đặt tiện nghi.
3. **Đặt / Nhận phòng** – Quản lý hồ sơ khách hàng, lập phiếu đặt phòng (kênh đặt, tiền cọc, ngày nhận/trả dự kiến), chi tiết phòng đặt, ghi nhận người lưu trú và làm thủ tục nhận phòng (cập nhật trạng thái phòng/phiếu đặt).
4. **Sử dụng dịch vụ** – Ghi nhận các dịch vụ khách sử dụng trong thời gian lưu trú theo từng phiếu đặt phòng, tự động tính thành tiền theo đơn giá dịch vụ.
5. **Trả phòng – Đền bù – Hóa đơn – Thanh toán** – Kiểm tra tình trạng tiện nghi khi trả phòng, lập phiếu đền bù theo quy định (nếu có hư hỏng), xuất hóa đơn (tiền phòng + tiền dịch vụ + đền bù), ghi nhận thanh toán và đóng phiếu đặt phòng khi thanh toán đủ.
6. **Thống kê** – Tổng hợp số lượt đặt phòng, số phòng đang ở, số hóa đơn, doanh thu phòng/dịch vụ/đền bù theo khoảng thời gian tuỳ chọn.

## Kiến trúc & quy ước dữ liệu

- Toàn bộ truy vấn SQL đi qua lớp tĩnh `DbHelper` với 3 phương thức dùng chung: `GetDataTable`, `ExecuteNonQuery`, `ExecuteScalar`, đều dùng **tham số hoá câu lệnh** (`SqlParameter`) để tránh SQL Injection.
- Các bảng dữ liệu chính suy ra từ mã nguồn: `KhuVuc`, `NhanVien`, `LoaiTienNghi`, `DichVu`, `QuyDinhDenBu`, `Phong`, `TienNghi`, `PhieuLapDat`, `KhachHang`, `PhieuDatPhong`, `ChiTietDatPhong`, `NguoiLuuTru`, `PhieuSuDungDV`, `ChiTietPhieuSuDungDV`, `PhieuDenBu`, `ChiTietPhieuDenBu`, `HoaDon`, `ThanhToan`.
- Giao diện được dựng hoàn toàn bằng code (`InitializeComponent()` viết tay), không dùng Designer `.resx`.

## Cấu hình & chạy dự án

1. **Cài đặt SQL Server** và tạo cơ sở dữ liệu `QuanLyKhachSan` cùng các bảng tương ứng ở trên (script tạo bảng chưa có sẵn trong repo — cần tự khởi tạo theo cấu trúc cột được dùng trong mã nguồn).
2. Mở `App.config`, sửa chuỗi kết nối `QuanLyKhachSanDb` cho khớp với máy chủ SQL Server của bạn:
   ```xml
   <add name="QuanLyKhachSanDb"
        connectionString="Data Source=TANDAT;Initial Catalog=QuanLyKhachSan;User ID=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=True"
        providerName="System.Data.SqlClient" />
   ```
3. Mở `QuanLyKhachSan.sln` bằng Visual Studio (có hỗ trợ .NET Framework 4.8).
4. Build và chạy (F5). Ứng dụng khởi động tại `FrmMain`.

## Lưu ý

- Chuỗi kết nối mẫu trong `App.config` chứa thông tin đăng nhập mặc định (`sa` / `123`) — **cần đổi sang thông tin thực tế và không commit mật khẩu thật lên hệ thống quản lý phiên bản**.
- Dự án hiện chưa có script SQL khởi tạo cơ sở dữ liệu (`.sql`) đi kèm; cần tạo thủ công dựa theo tên bảng/cột xuất hiện trong các file `Frm*.cs`.
