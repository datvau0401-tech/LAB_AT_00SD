using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyThuVien.Data;

namespace QuanLyThuVien.Services
{
    public class MuonTraService
    {
        public const int SO_SACH_TOI_DA_MOI_PHIEU = 3;
        private const decimal PHI_TRE_MOI_NGAY = 5000m;
        public const decimal PHI_MAT_SACH = 100000m;
        public const decimal PHI_HU_HONG = 50000m;

        // ---------- Tab Mượn sách ----------

        public DataRow LayDocGia(string maDocGia)
        {
            DataTable t = Db.Query(
                @"SELECT dg.MaDocGia, dg.Ho, dg.Ten, the.HanSuDung, the.DaDongLePhi, the.TrangThai
                  FROM DocGia dg
                  LEFT JOIN TheDocGia the ON the.MaDocGia = dg.MaDocGia AND the.TrangThai = 1
                  WHERE dg.MaDocGia = @Ma",
                new SqlParameter("@Ma", (maDocGia ?? "").Trim()));
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public KetQuaXuLy KiemTraDieuKienMuon(string maDocGia)
        {
            DataRow r = LayDocGia(maDocGia);
            if (r == null) return KetQuaXuLy.Loi("Không tìm thấy độc giả với mã đã nhập.");

            if (r["TrangThai"] == DBNull.Value)
                return KetQuaXuLy.Loi("Độc giả chưa có thẻ đang hoạt động.");

            if (Convert.ToDateTime(r["HanSuDung"]) < DateTime.Today)
                return KetQuaXuLy.Loi("Thẻ độc giả đã hết hạn sử dụng.");

            if (!Convert.ToBoolean(r["DaDongLePhi"]))
                return KetQuaXuLy.Loi("Độc giả chưa đóng lệ phí thẻ.");

            int soQuaHan = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon ct
                  JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                  WHERE pm.MaDocGia=@Ma AND ct.NgayTraThucTe IS NULL AND pm.NgayHenTra < @HomNay",
                new SqlParameter("@Ma", maDocGia.Trim()), new SqlParameter("@HomNay", DateTime.Today)));
            if (soQuaHan > 0)
                return KetQuaXuLy.Loi("Độc giả đang có sách quá hạn chưa trả, không thể mượn thêm.");

            int dangMuon = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon ct
                  JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                  WHERE pm.MaDocGia=@Ma AND ct.NgayTraThucTe IS NULL",
                new SqlParameter("@Ma", maDocGia.Trim())));
            if (dangMuon >= SO_SACH_TOI_DA_MOI_PHIEU)
                return KetQuaXuLy.Loi("Độc giả đã mượn đủ số sách tối đa (" + SO_SACH_TOI_DA_MOI_PHIEU + " cuốn) chưa trả.");

            return KetQuaXuLy.Ok(r["Ho"] + " " + r["Ten"] + " - Đủ điều kiện mượn sách.");
        }

        public DataTable LaySachConTrongKho(string tuKhoa)
        {
            string key = (tuKhoa ?? string.Empty).Trim();
            return Db.Query(
                @"SELECT MaDauSach, TenSach, NamXuatBan, SoLuongHienCo
                  FROM DauSach
                  WHERE SoLuongHienCo > 0 AND (@TuKhoa='' OR MaDauSach LIKE @Like OR TenSach LIKE @Like)
                  ORDER BY TenSach",
                new SqlParameter("@TuKhoa", key), new SqlParameter("@Like", "%" + key + "%"));
        }

        public KetQuaXuLy LapPhieuMuon(string maDocGia, string maNhanVien, DateTime ngayMuon, DateTime henTra, string[] maSachDaChon)
        {
            if (string.IsNullOrWhiteSpace(maDocGia)) return KetQuaXuLy.Loi("Vui lòng nhập mã độc giả.");
            if (string.IsNullOrWhiteSpace(maNhanVien)) return KetQuaXuLy.Loi("Vui lòng nhập mã nhân viên lập phiếu.");
            if (maSachDaChon == null || maSachDaChon.Length == 0) return KetQuaXuLy.Loi("Vui lòng chọn ít nhất 1 đầu sách.");
            if (maSachDaChon.Length > SO_SACH_TOI_DA_MOI_PHIEU) return KetQuaXuLy.Loi("Chỉ được chọn tối đa " + SO_SACH_TOI_DA_MOI_PHIEU + " đầu sách mỗi phiếu.");
            if (henTra < ngayMuon) return KetQuaXuLy.Loi("Hẹn trả phải sau hoặc bằng ngày mượn.");

            KetQuaXuLy dieuKien = KiemTraDieuKienMuon(maDocGia);
            if (!dieuKien.ThanhCong) return dieuKien;

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tran = cn.BeginTransaction())
            {
                try
                {
                    string maPhieu = "PM_" + maDocGia.Trim() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO PhieuMuon(MaPhieuMuon,MaDocGia,MaNhanVien,NgayMuon,NgayHenTra)
                          VALUES(@Ma,@DocGia,@NhanVien,@Muon,@Han)", cn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Ma", maPhieu);
                        cmd.Parameters.AddWithValue("@DocGia", maDocGia.Trim());
                        cmd.Parameters.AddWithValue("@NhanVien", maNhanVien.Trim());
                        cmd.Parameters.AddWithValue("@Muon", ngayMuon.Date);
                        cmd.Parameters.AddWithValue("@Han", henTra.Date);
                        cmd.ExecuteNonQuery();
                    }

                    int stt = 1;
                    foreach (string maSach in maSachDaChon)
                    {
                        using (SqlCommand cmdKiemTra = new SqlCommand(
                            "SELECT SoLuongHienCo FROM DauSach WHERE MaDauSach=@Ma", cn, tran))
                        {
                            cmdKiemTra.Parameters.AddWithValue("@Ma", maSach);
                            object soLuong = cmdKiemTra.ExecuteScalar();
                            if (soLuong == null || Convert.ToInt32(soLuong) <= 0)
                            {
                                tran.Rollback();
                                return KetQuaXuLy.Loi("Đầu sách " + maSach + " không còn trong kho.");
                            }
                        }

                        string maChiTiet = maPhieu + "_CT" + stt;
                        using (SqlCommand cmdCT = new SqlCommand(
                            @"INSERT INTO ChiTietPhieuMuon(MaChiTiet,MaPhieuMuon,MaDauSach)
                              VALUES(@MaCT,@MaPM,@MaSach)", cn, tran))
                        {
                            cmdCT.Parameters.AddWithValue("@MaCT", maChiTiet);
                            cmdCT.Parameters.AddWithValue("@MaPM", maPhieu);
                            cmdCT.Parameters.AddWithValue("@MaSach", maSach);
                            cmdCT.ExecuteNonQuery();
                        }

                        using (SqlCommand cmdGiam = new SqlCommand(
                            "UPDATE DauSach SET SoLuongHienCo = SoLuongHienCo - 1 WHERE MaDauSach=@Ma", cn, tran))
                        {
                            cmdGiam.Parameters.AddWithValue("@Ma", maSach);
                            cmdGiam.ExecuteNonQuery();
                        }
                        stt++;
                    }

                    tran.Commit();
                    return KetQuaXuLy.Ok("Lập phiếu mượn thành công. Mã phiếu: " + maPhieu);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return KetQuaXuLy.Loi("Lỗi khi lập phiếu mượn: " + ex.Message);
                }
            }
        }

        // ---------- Tab Trả sách ----------

        public DataTable LaySachDangMuon(string maDocGia)
        {
            return Db.Query(
                @"SELECT ct.MaChiTiet, ct.MaDauSach, ds.TenSach, pm.NgayMuon, pm.NgayHenTra,
                         CASE WHEN pm.NgayHenTra < @HomNay THEN DATEDIFF(day, pm.NgayHenTra, @HomNay) ELSE 0 END AS SoNgayTre
                  FROM ChiTietPhieuMuon ct
                  JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                  JOIN DauSach ds ON ds.MaDauSach = ct.MaDauSach
                  WHERE pm.MaDocGia = @Ma AND ct.NgayTraThucTe IS NULL
                  ORDER BY pm.NgayHenTra",
                new SqlParameter("@Ma", (maDocGia ?? "").Trim()),
                new SqlParameter("@HomNay", DateTime.Today));
        }

        public decimal TinhPhiPhatDuKien(int soNgayTre, string tinhTrang)
        {
            decimal phi = soNgayTre > 0 ? soNgayTre * PHI_TRE_MOI_NGAY : 0m;
            if (tinhTrang == "Mất") phi += PHI_MAT_SACH;
            else if (tinhTrang == "Hư hỏng") phi += PHI_HU_HONG;
            return phi;
        }

        public KetQuaXuLy TraSach(string maChiTiet, string tinhTrang, string maNhanVien)
        {
            if (string.IsNullOrWhiteSpace(maChiTiet)) return KetQuaXuLy.Loi("Vui lòng chọn sách cần trả.");
            if (string.IsNullOrWhiteSpace(maNhanVien)) return KetQuaXuLy.Loi("Vui lòng nhập mã nhân viên xử lý trả sách.");

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tran = cn.BeginTransaction())
            {
                try
                {
                    string maDauSach; DateTime ngayHenTra;
                    using (SqlCommand cmdInfo = new SqlCommand(
                        @"SELECT ct.MaDauSach, pm.NgayHenTra FROM ChiTietPhieuMuon ct
                          JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                          WHERE ct.MaChiTiet=@Ma AND ct.NgayTraThucTe IS NULL", cn, tran))
                    {
                        cmdInfo.Parameters.AddWithValue("@Ma", maChiTiet);
                        using (SqlDataReader reader = cmdInfo.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                tran.Rollback();
                                return KetQuaXuLy.Loi("Không tìm thấy chi tiết phiếu mượn cần trả (có thể đã trả trước đó).");
                            }
                            maDauSach = Convert.ToString(reader["MaDauSach"]);
                            ngayHenTra = Convert.ToDateTime(reader["NgayHenTra"]);
                        }
                    }

                    using (SqlCommand cmdTra = new SqlCommand(
                        @"UPDATE ChiTietPhieuMuon SET NgayTraThucTe=@Ngay, TinhTrangTra=@TinhTrang WHERE MaChiTiet=@Ma", cn, tran))
                    {
                        cmdTra.Parameters.AddWithValue("@Ngay", DateTime.Today);
                        cmdTra.Parameters.AddWithValue("@TinhTrang", (object)tinhTrang ?? DBNull.Value);
                        cmdTra.Parameters.AddWithValue("@Ma", maChiTiet);
                        cmdTra.ExecuteNonQuery();
                    }

                    int soNgayTre = ngayHenTra < DateTime.Today ? (DateTime.Today - ngayHenTra).Days : 0;
                    int sttPhat = 1;

                    if (soNgayTre > 0)
                    {
                        ThemPhieuPhat(cn, tran, maChiTiet, maNhanVien, "Trả trễ " + soNgayTre + " ngày",
                            soNgayTre * PHI_TRE_MOI_NGAY, ref sttPhat);
                    }
                    if (tinhTrang == "Mất")
                    {
                        ThemPhieuPhat(cn, tran, maChiTiet, maNhanVien, "Làm mất sách", PHI_MAT_SACH, ref sttPhat);
                    }
                    else if (tinhTrang == "Hư hỏng")
                    {
                        ThemPhieuPhat(cn, tran, maChiTiet, maNhanVien, "Sách bị hư hỏng", PHI_HU_HONG, ref sttPhat);
                    }

                    // Sách mất thì không nhập lại kho; bình thường/hư hỏng thì nhập lại kho
                    if (tinhTrang != "Mất")
                    {
                        using (SqlCommand cmdTang = new SqlCommand(
                            "UPDATE DauSach SET SoLuongHienCo = SoLuongHienCo + 1 WHERE MaDauSach=@Ma", cn, tran))
                        {
                            cmdTang.Parameters.AddWithValue("@Ma", maDauSach);
                            cmdTang.ExecuteNonQuery();
                        }
                    }

                    tran.Commit();
                    return KetQuaXuLy.Ok("Trả sách thành công.");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return KetQuaXuLy.Loi("Lỗi khi trả sách: " + ex.Message);
                }
            }
        }

        private void ThemPhieuPhat(SqlConnection cn, SqlTransaction tran, string maChiTiet, string maNhanVien,
            string lyDo, decimal phi, ref int sttPhat)
        {
            string maPhieuPhat = "PP_" + maChiTiet + "_" + sttPhat;
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO PhieuPhat(MaPhieuPhat,MaChiTiet,MaNhanVien,NgayPhat,LyDo,PhiPhat)
                  VALUES(@Ma,@CT,@NV,@Ngay,@LyDo,@Phi)", cn, tran))
            {
                cmd.Parameters.AddWithValue("@Ma", maPhieuPhat);
                cmd.Parameters.AddWithValue("@CT", maChiTiet);
                cmd.Parameters.AddWithValue("@NV", maNhanVien.Trim());
                cmd.Parameters.AddWithValue("@Ngay", DateTime.Today);
                cmd.Parameters.AddWithValue("@LyDo", lyDo);
                cmd.Parameters.AddWithValue("@Phi", phi);
                cmd.ExecuteNonQuery();
            }
            sttPhat++;
        }
    }
}
