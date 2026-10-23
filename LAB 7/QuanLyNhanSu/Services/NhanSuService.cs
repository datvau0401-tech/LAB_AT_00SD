using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class NhanSuService
    {
        // UC02 - Lập quyết định tuyển dụng thử việc (BR01, BR02) - TC01, TC02
        public KetQuaXuLy LapQuyetDinhTuyenDung(string maNV, string maPB, int thoiGianThang,
            decimal mucLuongThuViec, string dieuKhoan, string soQD)
        {
            if (string.IsNullOrWhiteSpace(maNV)) return KetQuaXuLy.Loi("Vui lòng chọn nhân viên.");
            if (string.IsNullOrWhiteSpace(maPB)) return KetQuaXuLy.Loi("Vui lòng chọn phòng ban thử việc.");
            if (thoiGianThang <= 0) return KetQuaXuLy.Loi("Thời gian thử việc phải lớn hơn 0 tháng.");
            if (mucLuongThuViec < 0) return KetQuaXuLy.Loi("Mức lương thử việc không hợp lệ.");
            if (string.IsNullOrWhiteSpace(dieuKhoan)) return KetQuaXuLy.Loi("Vui lòng nhập điều khoản hai bên.");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    if (Convert.ToInt32(DbTx.Scalar(cn, tx, "SELECT COUNT(*) FROM NhanVien WHERE MaNV=@MaNV",
                        new SqlParameter("@MaNV", maNV))) == 0)
                        return KetQuaXuLy.Loi("Không tìm thấy nhân viên.");

                    string maQD = "QD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinh(MaQD,LoaiQD,SoQD,NgayQD,TrangThaiKy,MaNV)
                        VALUES(@MaQD,'TuyenDung',@SoQD,@NgayQD,0,@MaNV)",
                        new SqlParameter("@MaQD", maQD), new SqlParameter("@SoQD", soQD),
                        new SqlParameter("@NgayQD", DateTime.Today), new SqlParameter("@MaNV", maNV));

                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinhTuyenDung(MaQD,ThoiGianThuViecThang,MucLuongThuViec,DieuKhoan,MaPB)
                        VALUES(@MaQD,@Thang,@Luong,@DieuKhoan,@MaPB)",
                        new SqlParameter("@MaQD", maQD), new SqlParameter("@Thang", thoiGianThang),
                        new SqlParameter("@Luong", mucLuongThuViec), new SqlParameter("@DieuKhoan", dieuKhoan),
                        new SqlParameter("@MaPB", maPB));

                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định tuyển dụng, chờ giám đốc ký. Mã QĐ: " + maQD);
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Số quyết định đã tồn tại.");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Lỗi xử lý: " + ex.Message);
                }
            }
        }

        // UC03, UC05, UC06 - Giám đốc ký quyết định (BR03, GT01) - TC03
        public KetQuaXuLy KyQuyetDinh(string maQD, DateTime ngayHieuLuc)
        {
            int n = Db.Execute(@"UPDATE QuyetDinh SET TrangThaiKy=1, NgayHieuLuc=@Ngay
                                 WHERE MaQD=@MaQD AND TrangThaiKy=0",
                new SqlParameter("@Ngay", ngayHieuLuc.Date), new SqlParameter("@MaQD", maQD));
            return n == 1 ? KetQuaXuLy.Ok("Đã ký quyết định " + maQD + ".")
                          : KetQuaXuLy.Loi("Quyết định không tồn tại hoặc đã được ký.");
        }

        // UC03, UC04, UC06 - Ký hợp đồng chính thức sau thử việc (BR03, BR04, BR05) - TC04, TC05
        public KetQuaXuLy KyHopDongChinhThuc(string maNV, string maQDTuyenDung, string loaiHD,
            DateTime ngayBatDau, DateTime? ngayKetThuc, decimal luongCoBan, decimal phuCapDocHai,
            decimal mucDongBH)
        {
            if (loaiHD != "DaiHan" && loaiHD != "NganHan") return KetQuaXuLy.Loi("Loại hợp đồng không hợp lệ.");
            if (luongCoBan < 0) return KetQuaXuLy.Loi("Lương cơ bản không hợp lệ.");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // Quyết định tuyển dụng phải đã được ký và thuộc đúng nhân viên
                    object kiemTra = DbTx.Scalar(cn, tx, @"SELECT COUNT(*) FROM QuyetDinhTuyenDung t
                        JOIN QuyetDinh q ON q.MaQD=t.MaQD
                        WHERE t.MaQD=@MaQD AND q.MaNV=@MaNV AND q.TrangThaiKy=1",
                        new SqlParameter("@MaQD", maQDTuyenDung), new SqlParameter("@MaNV", maNV));
                    if (Convert.ToInt32(kiemTra) == 0)
                        return KetQuaXuLy.Loi("Quyết định tuyển dụng chưa được ký hoặc không thuộc nhân viên này.");

                    string maHD = "HD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    object ketThuc = ngayKetThuc.HasValue ? (object)ngayKetThuc.Value.Date : DBNull.Value;
                    DbTx.Execute(cn, tx, @"INSERT INTO HopDong(MaHD,MaNV,MaQD,LoaiHopDong,NgayKy,NgayBatDau,NgayKetThuc,TrangThaiHieuLuc)
                        VALUES(@MaHD,@MaNV,@MaQD,@Loai,@NgayKy,@Bat,@Ket,1)",
                        new SqlParameter("@MaHD", maHD), new SqlParameter("@MaNV", maNV),
                        new SqlParameter("@MaQD", maQDTuyenDung), new SqlParameter("@Loai", loaiHD),
                        new SqlParameter("@NgayKy", DateTime.Today), new SqlParameter("@Bat", ngayBatDau.Date),
                        new SqlParameter("@Ket", ketThuc));

                    // Hợp đồng dài hạn: lập hồ sơ BHYT, BHXH (BR05)
                    if (loaiHD == "DaiHan")
                    {
                        DbTx.Execute(cn, tx, @"INSERT INTO HoSoBaoHiem(MaHS,MaHD,SoBHYT,SoBHXH,MucDongBH,NgayLapHS)
                            VALUES(@MaHS,@MaHD,NULL,NULL,@Muc,@Ngay)",
                            new SqlParameter("@MaHS", "HS" + DateTime.Now.ToString("yyyyMMddHHmmssfff")),
                            new SqlParameter("@MaHD", maHD), new SqlParameter("@Muc", mucDongBH),
                            new SqlParameter("@Ngay", DateTime.Today));
                    }

                    // Quyết định lương đi kèm hợp đồng chính thức (BR03)
                    string maQDLuong = "QD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinh(MaQD,LoaiQD,SoQD,NgayQD,NgayHieuLuc,TrangThaiKy,MaNV)
                        VALUES(@MaQD,'Luong',@SoQD,@Ngay,@Ngay,1,@MaNV)",
                        new SqlParameter("@MaQD", maQDLuong), new SqlParameter("@SoQD", "LUONG-" + maQDLuong),
                        new SqlParameter("@Ngay", ngayBatDau.Date), new SqlParameter("@MaNV", maNV));
                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinhLuong(MaQD,LuongCoBan,PhuCapLuong,PhuCapDocHai)
                        VALUES(@MaQD,@Luong,0,@PhuCap)",
                        new SqlParameter("@MaQD", maQDLuong), new SqlParameter("@Luong", luongCoBan),
                        new SqlParameter("@PhuCap", phuCapDocHai));

                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã ký hợp đồng " + maHD + " và quyết định lương.");
                }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Không thể ký: nhân viên đang có hợp đồng còn hiệu lực, " +
                                          "hoặc quyết định này đã dùng cho hợp đồng khác.");
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Lỗi xử lý: " + ex.Message);
                }
            }
        }
    }
}
