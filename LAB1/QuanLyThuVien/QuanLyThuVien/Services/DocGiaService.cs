using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyThuVien.Data;

namespace QuanLyThuVien.Services
{
    public class DocGiaService
    {
        // Lấy danh sách độc giả kèm thông tin thẻ đang hoạt động (nếu có)
        public DataTable LayDanhSach(string tuKhoa)
        {
            string sql = @"SELECT dg.MaDocGia, dg.Ho, dg.Ten, dg.NgaySinh, dg.Phai,
                                   dg.SoDienThoai, dg.DiaChi, dg.Email, dg.Anh3x4,
                                   the.MaThe, the.NgayCap, the.HanSuDung, the.DaDongLePhi
                            FROM DocGia dg
                            LEFT JOIN TheDocGia the ON the.MaDocGia = dg.MaDocGia AND the.TrangThai = 1
                            WHERE (@TuKhoa='' OR dg.MaDocGia LIKE @Like OR dg.Ho LIKE @Like OR dg.Ten LIKE @Like)
                            ORDER BY dg.MaDocGia";
            string key = (tuKhoa ?? string.Empty).Trim();
            return Db.Query(sql,
                new SqlParameter("@TuKhoa", key),
                new SqlParameter("@Like", "%" + key + "%"));
        }

        public KetQuaXuLy Luu(DocGia dg, bool capNhat)
        {
            if (dg == null || string.IsNullOrWhiteSpace(dg.MaDocGia) ||
                string.IsNullOrWhiteSpace(dg.Ho) || string.IsNullOrWhiteSpace(dg.Ten) ||
                string.IsNullOrWhiteSpace(dg.Phai) || string.IsNullOrWhiteSpace(dg.DiaChi) ||
                string.IsNullOrWhiteSpace(dg.Email))
                return KetQuaXuLy.Loi("Vui lòng nhập đầy đủ thông tin bắt buộc của độc giả.");

            if (dg.NgaySinh >= DateTime.Today)
                return KetQuaXuLy.Loi("Ngày sinh không hợp lệ.");

            try
            {
                string sql = capNhat
                    ? @"UPDATE DocGia SET Ho=@Ho,Ten=@Ten,NgaySinh=@NgaySinh,Phai=@Phai,
                                          SoDienThoai=@SDT,DiaChi=@DiaChi,Email=@Email,Anh3x4=@Anh
                        WHERE MaDocGia=@Ma"
                    : @"INSERT INTO DocGia(MaDocGia,Ho,Ten,NgaySinh,Phai,SoDienThoai,DiaChi,Email,Anh3x4)
                        VALUES(@Ma,@Ho,@Ten,@NgaySinh,@Phai,@SDT,@DiaChi,@Email,@Anh)";

                int n = Db.Execute(sql,
                    new SqlParameter("@Ma", dg.MaDocGia.Trim()),
                    new SqlParameter("@Ho", dg.Ho.Trim()),
                    new SqlParameter("@Ten", dg.Ten.Trim()),
                    new SqlParameter("@NgaySinh", dg.NgaySinh.Date),
                    new SqlParameter("@Phai", dg.Phai.Trim()),
                    new SqlParameter("@SDT", (object)(dg.SoDienThoai ?? string.Empty)),
                    new SqlParameter("@DiaChi", dg.DiaChi.Trim()),
                    new SqlParameter("@Email", dg.Email.Trim()),
                    new SqlParameter("@Anh", (object)dg.Anh3x4 ?? DBNull.Value));

                return n > 0 ? KetQuaXuLy.Ok(capNhat ? "Cập nhật độc giả thành công." : "Thêm độc giả thành công.")
                              : KetQuaXuLy.Loi("Không có dữ liệu được thay đổi.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) return KetQuaXuLy.Loi("Mã độc giả đã tồn tại.");
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }

        public KetQuaXuLy Xoa(string maDocGia)
        {
            try
            {
                int n = Db.Execute("DELETE FROM DocGia WHERE MaDocGia=@Ma", new SqlParameter("@Ma", maDocGia));
                return n > 0 ? KetQuaXuLy.Ok("Xóa độc giả thành công.") : KetQuaXuLy.Loi("Không tìm thấy độc giả.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) return KetQuaXuLy.Loi("Không thể xóa độc giả đã có thẻ hoặc phiếu mượn.");
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }

        // Cấp thẻ mới (chỉ cho phép khi độc giả chưa có thẻ đang hoạt động)
        public KetQuaXuLy CapThe(string maDocGia, DateTime ngayCap, DateTime hanSuDung, bool daDongLePhi)
        {
            if (string.IsNullOrWhiteSpace(maDocGia))
                return KetQuaXuLy.Loi("Vui lòng chọn độc giả cần cấp thẻ.");
            if (hanSuDung < ngayCap)
                return KetQuaXuLy.Loi("Hạn sử dụng phải sau hoặc bằng ngày cấp.");

            try
            {
                string maThe = "THE_" + maDocGia.Trim() + "_" + ngayCap.Year;
                int n = Db.Execute(
                    @"INSERT INTO TheDocGia(MaThe,MaDocGia,NgayCap,HanSuDung,DaDongLePhi,TrangThai)
                      VALUES(@MaThe,@Ma,@NgayCap,@Han,@LePhi,1)",
                    new SqlParameter("@MaThe", maThe),
                    new SqlParameter("@Ma", maDocGia.Trim()),
                    new SqlParameter("@NgayCap", ngayCap.Date),
                    new SqlParameter("@Han", hanSuDung.Date),
                    new SqlParameter("@LePhi", daDongLePhi));

                return n > 0 ? KetQuaXuLy.Ok("Cấp thẻ thành công.") : KetQuaXuLy.Loi("Không thể cấp thẻ.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                    return KetQuaXuLy.Loi("Độc giả này đã có thẻ đang hoạt động.");
                if (ex.Number == 547)
                    return KetQuaXuLy.Loi("Độc giả không hợp lệ.");
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }

        // Gia hạn thẻ đang hoạt động của độc giả
        public KetQuaXuLy GiaHan(string maDocGia, DateTime hanSuDungMoi)
        {
            if (string.IsNullOrWhiteSpace(maDocGia))
                return KetQuaXuLy.Loi("Vui lòng chọn độc giả cần gia hạn thẻ.");

            try
            {
                int n = Db.Execute(
                    "UPDATE TheDocGia SET HanSuDung=@Han WHERE MaDocGia=@Ma AND TrangThai=1",
                    new SqlParameter("@Han", hanSuDungMoi.Date),
                    new SqlParameter("@Ma", maDocGia.Trim()));

                return n > 0 ? KetQuaXuLy.Ok("Gia hạn thẻ thành công.")
                              : KetQuaXuLy.Loi("Độc giả chưa có thẻ đang hoạt động để gia hạn.");
            }
            catch (SqlException ex)
            {
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }
    }
}
