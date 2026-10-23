# Bài 5 - Quản lý hồ sơ bệnh án

Bài thực hành phân tích và thiết kế hệ thống quản lý hồ sơ bệnh án của bệnh nhân: từ phân tích nghiệp vụ, mô hình UML, thiết kế cơ sở dữ liệu đến giao diện C# WinForms.


- **Sinh viên:** Nguyễn Tấn Đạt
- **Lớp:** 12_DH_CNPM1
- **Giảng viên hướng dẫn:** Phạm Trọng Huynh
- **Thời gian thực hiện:** Tháng 10/2026

## 1. Bài toán

Một bệnh viện đa khoa quốc tế cần phần mềm để lưu trữ thống nhất lịch sử khám và điều trị của từng bệnh nhân qua nhiều năm. Phần mềm giúp bác sĩ tra cứu tiền sử bệnh trước khi chọn phác đồ, theo dõi diễn biến bệnh cùng phản ứng phụ của thuốc, đồng thời quản lý lệ phí thường niên và quyền lợi khám sức khỏe định kỳ.

## 2. Phạm vi

Bài phân tích tám nghiệp vụ: đăng ký bệnh nhân, lệ phí năm, hồ sơ bệnh án, khám bệnh, xét nghiệm, toa thuốc, lịch khám định kỳ và hoàn tất đợt điều trị. Phần quản trị danh mục (khoa, bác sĩ, thuốc, tài khoản) được xem là chức năng hỗ trợ và không phân tích chi tiết.

## 3. Tác nhân

| Mã | Tác nhân | Vai trò |
|---|---|---|
| BN | Bệnh nhân | Đăng ký lần đầu, đóng lệ phí, đặt lịch khám định kỳ, thực hiện xét nghiệm, nhận toa thuốc |
| CS | Nhân viên chăm sóc khách hàng | Tiếp nhận đăng ký, thu lệ phí, cấp hồ sơ, đặt lịch, thanh toán viện phí, ghi nhận dữ liệu sau xuất viện |
| BS | Bác sĩ | Khám bệnh, ghi triệu chứng, chỉ định và cập nhật xét nghiệm, kê toa, ghi phản ứng phụ, theo dõi điều trị |
| QT | Quản trị viên | Quản lý danh mục khoa, bác sĩ, thuốc, mức lệ phí, tài khoản và báo cáo (không có trong đề bài, được thêm vào để quản lý danh mục) |

## 4. Mô hình hóa

### 4.1 Class Diagram

Mô hình gồm 13 lớp. Mỗi thuộc tính có ID theo tiền tố tên lớp (ví dụ BN01, PD01), trong đó PK là khóa chính và FK là khóa ngoại.

| Lớp | Thuộc tính chính | Phương thức chính |
|---|---|---|
| BenhNhan | MaBenhNhan (PK), HoTen, DiaChi, NgaySinh, DienThoai, AnhChup3x4 | CapNhatThongTin(), XemLichSuKham() |
| PhieuDangKy | MaPhieuDK (PK), NgayDangKy, MaBenhNhan (FK) | TaoPhieuDangKy(), XacNhanThongTin() |
| LePhiThuongNien | MaLePhi (PK), NamApDung, SoTien, NgayDong, NgayHetHan, MaBenhNhan (FK) | KiemTraConHieuLuc(), GiaHanLePhi() |
| HoSoBenhAn | MaHoSo (PK), NgayCap, MaBenhNhan (FK) | ThemPhieuKham(), LayLichSuKham(), TimKiemHoSo() |
| PhieuKhamBenh | MaPhieuKham (PK), NgayGioKham, LoaiKham, KetLuan, MaHoSo, MaBacSi, MaKhoa (FK) | ThemTrieuChung(), ThemToaThuoc(), ThemXetNghiem() |
| TrieuChung | MaTrieuChung (PK), TenTrieuChung, GiaTri, DonVi, MaPhieuKham (FK) | CapNhatGiaTri(), KiemTraBatThuong() |
| ToaThuoc | MaToaThuoc (PK), NgayKe, MaPhieuKham (FK) | ThemChiTiet(), XoaChiTiet() |
| ChiTietToaThuoc | MaChiTiet (PK), MaToaThuoc (FK), MaThuoc (FK), LieuDung, CachDung, PhanUngPhu | GhiNhanPhanUngPhu() |
| Thuoc | MaThuoc (PK), TenThuoc, DonVi, CongDung | TimKiemThuoc() |
| XetNghiem | MaXetNghiem (PK), TenXetNghiem, NgayChiDinh, KetQua, MaPhieuKham (FK) | CapNhatKetQua() |
| BacSi | MaBacSi (PK), HoTen, ChuyenKhoa, MaKhoa (FK) | XemDanhSachBenhNhan() |
| KhoaDieuTri | MaKhoa (PK), TenKhoa | ThemBacSi(), LayDanhSachBacSi() |
| LichHen | MaLichHen (PK), NgayGioHen, TrangThai, MaBenhNhan (FK) | DatLich(), HuyLich(), CapNhatTrangThai() |

#### Quan hệ và bản số

| # | Quan hệ | Loại | Bản số | Ý nghĩa |
|---|---|---|---|---|
| 1 | BenhNhan - PhieuDangKy | Association | 1 - 1 | Mỗi bệnh nhân có đúng một phiếu đăng ký lần đầu |
| 2 | BenhNhan - LePhiThuongNien | Association | 1 - 0..* | Bệnh nhân đóng lệ phí nhiều năm |
| 3 | BenhNhan - HoSoBenhAn | Association | 0..1 - 1 | Bệnh nhân có thể chưa có hồ sơ; mỗi hồ sơ thuộc đúng một bệnh nhân |
| 4 | HoSoBenhAn - PhieuKhamBenh | Composition | 1 - 0..* | Phiếu khám là thành phần của hồ sơ |
| 5 | PhieuKhamBenh - TrieuChung | Composition | 1 - 0..* | Triệu chứng ghi trong từng phiếu khám |
| 6 | PhieuKhamBenh - ToaThuoc | Composition | 1 - 0..1 | Mỗi phiếu khám có tối đa một toa |
| 7 | PhieuKhamBenh - XetNghiem | Composition | 1 - 0..* | Mỗi phiếu khám có thể có nhiều xét nghiệm |
| 8 | ToaThuoc - ChiTietToaThuoc | Composition | 1 - 1..* | Toa thuốc có ít nhất một dòng thuốc |
| 9 | ChiTietToaThuoc - Thuoc | Association | 0..* - 1 | Mỗi dòng chi tiết kê đúng một loại thuốc |
| 10 | PhieuKhamBenh - BacSi | Association | 0..* - 1 | Mỗi phiếu khám do đúng một bác sĩ thực hiện |
| 11 | PhieuKhamBenh - KhoaDieuTri | Association | 0..* - 1 | Mỗi phiếu khám thuộc đúng một khoa |
| 12 | BacSi - KhoaDieuTri | Association | 0..* - 1 | Mỗi bác sĩ công tác tại một khoa |
| 13 | BenhNhan - LichHen | Association | 1 - 0..* | Bệnh nhân có nhiều lịch hẹn |
| 14 | LichHen - PhieuKhamBenh | Association | 0..1 - 0..1 | Lịch hẹn có thể dẫn đến một phiếu khám |

Composition (hình thoi đen) đặt ở phía lớp toàn thể. Các khóa ngoại được thể hiện bằng đường nối nên không ghi lại trên sơ đồ.

### 4.2 Use Case tổng quát

Hệ thống có 4 tác nhân và 24 use case, chia thành bốn nhóm:

| Nhóm | Use case | Nội dung |
|---|---|---|
| A. Đăng ký và hồ sơ | UC01 - UC08 | Đăng ký lần đầu, đóng lệ phí, cấp hồ sơ, cập nhật thông tin, tra cứu hồ sơ, xem lịch sử khám, đặt và quản lý lịch hẹn |
| B. Khám và điều trị | UC09 - UC17 | Lập phiếu khám, ghi triệu chứng, kê toa, chỉ định và cập nhật xét nghiệm, ghi phản ứng phụ, khám định kỳ, thanh toán, xuất viện |
| C. Theo dõi và báo cáo | UC18 - UC19 | Theo dõi diễn biến điều trị, xem báo cáo hiệu quả điều trị |
| D. Quản lý danh mục | UC20 - UC24 | Quản lý khoa, bác sĩ, thuốc, mức lệ phí, tài khoản người dùng |

Các quan hệ chính: UC03 include UC02; UC06 và UC09 include UC05; UC09 include UC10; UC11, UC12 extend UC09; UC13 extend UC12; UC14 extend UC11; UC15 include UC09; UC17 include UC16.

### 4.3 Use Case phân rã theo giai đoạn

Sơ đồ phân rã chia quy trình từ lúc bệnh nhân đăng ký đến khi nhận toa thuốc thành bốn giai đoạn:

1. **Đăng ký khám bệnh:** đăng ký khám lần đầu (nhập thông tin, chụp ảnh 3x4), đóng lệ phí, cấp hồ sơ bệnh án.
2. **Khám bệnh:** tra cứu hồ sơ, lập phiếu khám, ghi nhận triệu chứng.
3. **Xét nghiệm:** chỉ định xét nghiệm, thực hiện xét nghiệm (use case mới chỉ có trong phân rã), cập nhật kết quả.
4. **Nhận toa thuốc:** kê toa thuốc (chọn thuốc, liều dùng, cách dùng), nhận toa thuốc (use case mới chỉ có trong phân rã).

### 4.4 Activity Diagram

Activity diagram "Từ đăng ký khám bệnh đến nhận toa thuốc" có ba làn: Bệnh nhân, Nhân viên CSKH và Bác sĩ. Luồng gồm hai nhánh quyết định: bệnh nhân đã có hồ sơ hay chưa (nếu chưa, tạo hồ sơ sau khi đóng lệ phí), và có cần xét nghiệm hay không trước khi kê toa.

### 4.5 Sequence Diagram: Lập toa thuốc

Sequence diagram mô tả bác sĩ lập toa cho bệnh nhân sau khi khám, qua các đối tượng FrmToaThuoc, ToaThuocController, PhieuKhamBenh, ToaThuoc, ChiTietToaThuoc, Thuoc và CSDL SQL Server. Quy trình gồm:

- Mở phiếu khám và form lập toa.
- Tìm thuốc theo từ khóa, thêm từng dòng thuốc (kiểm tra liều dùng và cách dùng).
- Xóa dòng thuốc nếu cần.
- Lưu toa trong một transaction: nếu phiếu chưa có toa thì ghi ToaThuoc và các ChiTietToaThuoc, commit khi thành công, rollback khi lỗi. Nếu phiếu đã có toa, hệ thống đề nghị dùng chức năng cập nhật toa.

## 5. Đặc tả Use Case trọng tâm

Năm use case được đặc tả chi tiết: UC01 Đăng ký khám lần đầu, UC03 Cấp hồ sơ bệnh án, UC09 Lập phiếu khám bệnh, UC11 Kê toa thuốc và UC17 Xuất viện và ghi nhận dữ liệu. Mỗi đặc tả gồm actor, tiền điều kiện, hậu điều kiện, luồng chính, luồng thay thế, ngoại lệ và quy tắc liên quan.

## 6. Quy tắc nghiệp vụ

| Mã | Quy tắc |
|---|---|
| BR01 | Phiếu đăng ký chỉ cấp cho bệnh nhân khám lần đầu |
| BR02 | Phiếu đăng ký phải có họ tên, địa chỉ, ngày sinh, điện thoại và ảnh 3x4 |
| BR03 | Chỉ cấp hồ sơ khi bệnh nhân đã đóng lệ phí của năm hiện hành |
| BR04 | Mỗi bệnh nhân có một mã hồ sơ duy nhất |
| BR05 | Lệ phí có hiệu lực một năm kể từ ngày đóng |
| BR06 | Mỗi phiếu khám thuộc một hồ sơ, một bác sĩ và một khoa |
| BR07 | Phiếu khám ghi rõ loại khám: khám bệnh hoặc khám định kỳ |
| BR08 | Khám định kỳ miễn phí, diễn ra theo quý, không kèm chẩn đoán bệnh cụ thể |
| BR09 | Mỗi phiếu khám có tối đa một toa thuốc; toa có ít nhất một dòng thuốc |
| BR10 | Phản ứng phụ gắn với từng dòng thuốc |
| BR11 | Một phiếu khám có thể có nhiều xét nghiệm; kết quả cập nhật sau khi có |
| BR12 | Lịch khám định kỳ đặt qua CSKH, theo thời gian rảnh của bệnh nhân |
| BR13 | Dữ liệu đợt điều trị được ghi nhận sau khi xuất viện và thanh toán viện phí |
| BR14 | Bác sĩ khám lại được xem toàn bộ lịch sử trong hồ sơ |

## 7. Giả định và điểm cần xác nhận

Đề bài còn một số chỗ chưa rõ, nhóm đã chọn cách hiểu sau để tiếp tục mô hình hóa:

1. **Thời điểm ghi nhận dữ liệu khám:** đề bài nói CSKH ghi nhận sau khi xuất viện, nhưng sequence và activity diagram cho thấy bác sĩ nhập toa và kết quả trực tiếp khi khám. Giả định: bác sĩ nhập dữ liệu khi khám, CSKH chỉ hoàn tất và khóa đợt điều trị sau thanh toán.
2. **Hiệu lực của hồ sơ:** giả định hồ sơ được giữ lại khi lệ phí hết hạn, nhưng quyền khám định kỳ miễn phí chỉ có khi lệ phí còn hiệu lực.
3. **Tần suất khám định kỳ:** giả định tối đa một đợt khám định kỳ mỗi quý.
4. **Đợt điều trị ngoại trú:** giả định đợt điều trị bắt đầu khi làm thủ tục khám và kết thúc khi xuất viện và thanh toán.
5. **Viện phí:** chưa có cách tính viện phí và chưa có thực thể thanh toán trong class diagram. UC16 và UC17 chưa được thể hiện đầy đủ trong dữ liệu.
6. **Quyền sửa dữ liệu:** giả định chỉ quản trị viên được sửa phiếu khám sau khi đã đóng đợt điều trị, và phải ghi lại lịch sử thay đổi.
7. **Sinh mã:** giả định hệ thống tự sinh mã duy nhất cho bệnh nhân và hồ sơ.
8. **Tìm kiếm:** giả định tìm theo họ tên, ngày sinh và số điện thoại, ngoài mã hồ sơ.

Một số điểm nhất quán cần sửa thêm: nhánh "chưa đóng lệ phí" chưa có trong activity diagram dù UC03 đã mô tả; UC11 và UC12 là extend của UC09 trong use case tổng quát, nhưng Nhận toa thuốc (4.2) và Thực hiện xét nghiệm (3.2) chỉ xuất hiện trong sơ đồ phân rã.

## 8. Ánh xạ nghiệp vụ sang mô hình

| Nghiệp vụ | Use case | Class chính | Sơ đồ đã làm |
|---|---|---|---|
| Đăng ký bệnh nhân | UC01 | BenhNhan, PhieuDangKy | Use Case phân rã giai đoạn 1 |
| Lệ phí năm | UC02, UC23 | LePhiThuongNien | Use Case phân rã giai đoạn 1 |
| Hồ sơ bệnh án | UC03, UC04, UC05, UC06 | HoSoBenhAn | Use Case tổng quát |
| Khám bệnh | UC09, UC10 | PhieuKhamBenh, TrieuChung | Activity diagram, Use Case phân rã giai đoạn 2 |
| Xét nghiệm | UC12, UC13 | XetNghiem | Activity diagram, Use Case phân rã giai đoạn 3 |
| Toa thuốc | UC11, UC14 | ToaThuoc, ChiTietToaThuoc, Thuoc | Sequence diagram Lập toa thuốc |
| Lịch khám định kỳ | UC07, UC08, UC15 | LichHen | Use Case tổng quát |
| Hoàn tất đợt điều trị | UC16, UC17 | HoSoBenhAn, PhieuKhamBenh | Use Case tổng quát |

## 9. Tài liệu đi kèm

- **Báo cáo:** `1250080048_TangMinhHao.docx`, gồm phân tích nghiệp vụ, class diagram, use case, activity diagram, sequence diagram, đặc tả use case và ánh xạ mô hình.
- **Mã PlantUML:** class diagram, các sơ đồ use case phân rã (Bai5_UC_GiaiDoan1_DangKy, Bai5_UC_GiaiDoan2_KhamBenh, Bai5_UC_GiaiDoan3_XetNghiem, Bai5_UC_GiaiDoan4_NhanToaThuoc), Bai5_ActivityDiagram_KhamDenToaThuoc và Bai5_SequenceDiagram_LapToaThuoc.
- **Mô hình StarUML:** use case tổng quát của toàn bộ hệ thống.
