using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class KhenThuongKyLuatService
    {
        // UC11 - Lập quyết định khen thưởng cho cá nhân hoặc tập thể (BR11, BR12) - TC06, TC07, TC08
        // Quy ước: CaNhan -> lưu MaNV ở QuyetDinh, MaPB để NULL;
        //          TapThe -> lưu MaPB ở QuyetDinhKhenThuong, MaNV để NULL.
        public KetQuaXuLy LapQuyetDinhKhenThuong(string loaiDT, string maDT, string hinhThuc,
            string lyDo, decimal soTien, string soQD)
        {
            if (loaiDT != "CaNhan" && loaiDT != "TapThe") return KetQuaXuLy.Loi("Loại đối tượng không hợp lệ.");
            if (string.IsNullOrWhiteSpace(maDT)) return KetQuaXuLy.Loi("Vui lòng chọn đối tượng khen thưởng.");
            if (string.IsNullOrWhiteSpace(lyDo)) return KetQuaXuLy.Loi("Vui lòng nhập lý do khen thưởng.");
            if (soTien <= 0) return KetQuaXuLy.Loi("Số tiền khen thưởng phải lớn hơn 0.");

            string sqlKiemTra = loaiDT == "CaNhan"
                ? "SELECT COUNT(*) FROM NhanVien WHERE MaNV=@Ma"
                : "SELECT COUNT(*) FROM PhongBan WHERE MaPB=@Ma";
            if (Convert.ToInt32(Db.Scalar(sqlKiemTra, new SqlParameter("@Ma", maDT))) == 0)
                return KetQuaXuLy.Loi("Không tìm thấy đối tượng khen thưởng.");

            string maNV = loaiDT == "CaNhan" ? maDT : null;
            string maPB = loaiDT == "TapThe" ? maDT : null;
            string maQD = "KT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinh(MaQD,LoaiQD,SoQD,NgayQD,TrangThaiKy,MaNV)
                        VALUES(@MaQD,'KhenThuong',@SoQD,@Ngay,0,@MaNV)",
                        new SqlParameter("@MaQD", maQD), new SqlParameter("@SoQD", soQD),
                        new SqlParameter("@Ngay", DateTime.Today),
                        new SqlParameter("@MaNV", (object)maNV ?? DBNull.Value));

                    DbTx.Execute(cn, tx, @"INSERT INTO QuyetDinhKhenThuong(MaQD,LoaiDoiTuong,HinhThuc,LyDo,SoTienKhenThuong,MaPB)
                        VALUES(@MaQD,@Loai,@HinhThuc,@LyDo,@Tien,@MaPB)",
                        new SqlParameter("@MaQD", maQD), new SqlParameter("@Loai", loaiDT),
                        new SqlParameter("@HinhThuc", (object)hinhThuc ?? DBNull.Value),
                        new SqlParameter("@LyDo", lyDo), new SqlParameter("@Tien", soTien),
                        new SqlParameter("@MaPB", (object)maPB ?? DBNull.Value));

                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã lập quyết định khen thưởng, chờ giám đốc ký. Mã QĐ: " + maQD);
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Lỗi xử lý: " + ex.Message);
                }
            }
        }

        // UC11 + UC13 - Giám đốc ký quyết định và hệ thống lập phiếu chi (BR12, BR13) - TC06, TC07
        // Kỷ luật (UC12, UC14) lập và ký theo cùng mẫu: LapQuyetDinhKyLuat, KyVaLapPhieuThu.
        public KetQuaXuLy KyVaLapPhieuChi(string maQD, string loaiChi)
        {
            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    DataTable dt = DbTx.Query(cn, tx, @"SELECT kt.SoTienKhenThuong, q.MaNV
                        FROM QuyetDinhKhenThuong kt JOIN QuyetDinh q ON q.MaQD=kt.MaQD
                        WHERE kt.MaQD=@MaQD AND q.TrangThaiKy=0",
                        new SqlParameter("@MaQD", maQD));
                    if (dt.Rows.Count == 0) return KetQuaXuLy.Loi("Quyết định không tồn tại hoặc đã ký.");

                    decimal soTien = Convert.ToDecimal(dt.Rows[0]["SoTienKhenThuong"]);
                    object maNV = dt.Rows[0]["MaNV"];          // DBNull khi là tập thể

                    DbTx.Execute(cn, tx, "UPDATE QuyetDinh SET TrangThaiKy=1, NgayHieuLuc=@Ngay WHERE MaQD=@MaQD",
                        new SqlParameter("@Ngay", DateTime.Today), new SqlParameter("@MaQD", maQD));

                    DbTx.Execute(cn, tx, @"INSERT INTO PhieuChi(MaPhieuChi,MaQD,MaNV,NgayChi,SoTien,LoaiChi)
                        VALUES(@MaPC,@MaQD,@MaNV,@Ngay,@Tien,@Loai)",
                        new SqlParameter("@MaPC", "PC" + DateTime.Now.ToString("yyyyMMddHHmmssfff")),
                        new SqlParameter("@MaQD", maQD), new SqlParameter("@MaNV", maNV),
                        new SqlParameter("@Ngay", DateTime.Today), new SqlParameter("@Tien", soTien),
                        new SqlParameter("@Loai", loaiChi));

                    tx.Commit();
                    return KetQuaXuLy.Ok("Đã ký quyết định và lập phiếu chi " + soTien.ToString("N0") + " đ.");
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
