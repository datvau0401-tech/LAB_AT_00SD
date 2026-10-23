using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.Services
{
    public class LuongService
    {
        // UC10 - Tính lương tháng (BR05, BR06, BR07, BR10) - TC12, TC13, TC14
        // Công thức (GT03 - tham số lưu trong bảng ThamSo):
        //   LuongChinh       = LuongCoBan / SoNgayCongChuan * SoNgayCong
        //   TongPhuCap       = PhuCapLuong + PhuCapDocHai + PhuCapChucVu
        //   LuongGio         = LuongCoBan / (SoNgayCongChuan * SoGioMotNgay)
        //   TienPhuTrong     = LuongGio * SUM(SoGioNV * HeSo)
        //   TongKhauTru      = MucDongBH * (TyLeBHXH + TyLeBHYT)   (chỉ hợp đồng dài hạn)
        //   TienPhatTruLuong = SUM(SoTienNop) của phiếu thu "Trừ lương" trong tháng
        //   LuongThucNhan    = LuongChinh + TongPhuCap + TienPhuTrong - TongKhauTru - TienPhatTruLuong
        public KetQuaXuLy TinhLuongThang(string maNV, int thang, int nam)
        {
            if (string.IsNullOrWhiteSpace(maNV)) return KetQuaXuLy.Loi("Vui lòng chọn nhân viên.");
            if (thang < 1 || thang > 12) return KetQuaXuLy.Loi("Tháng không hợp lệ.");

            DateTime dau = new DateTime(nam, thang, 1);
            DateTime cuoi = dau.AddMonths(1).AddDays(-1);

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 1. Mỗi nhân viên chỉ được tính một bảng lương cho một tháng
                    if (Convert.ToInt32(DbTx.Scalar(cn, tx,
                        "SELECT COUNT(*) FROM BangLuong WHERE MaNV=@MaNV AND Thang=@T AND Nam=@N",
                        new SqlParameter("@MaNV", maNV), new SqlParameter("@T", thang),
                        new SqlParameter("@N", nam))) > 0)
                        return KetQuaXuLy.Loi("Đã tính lương tháng " + thang + "/" + nam + " cho nhân viên này.");

                    // 2. Bảng chấm công của tháng và số ngày công của nhân viên
                    DataTable bcc = DbTx.Query(cn, tx, "SELECT MaBCC FROM BangChamCong WHERE Thang=@T AND Nam=@N",
                        new SqlParameter("@T", thang), new SqlParameter("@N", nam));
                    if (bcc.Rows.Count == 0) return KetQuaXuLy.Loi("Chưa có bảng chấm công tháng này.");
                    string maBCC = Convert.ToString(bcc.Rows[0]["MaBCC"]);

                    decimal soNgayCong = Convert.ToDecimal(DbTx.Scalar(cn, tx,
                        "SELECT COUNT(*) FROM ChiTietChamCong WHERE MaBCC=@B AND MaNV=@MaNV",
                        new SqlParameter("@B", maBCC), new SqlParameter("@MaNV", maNV)));
                    if (soNgayCong == 0) return KetQuaXuLy.Loi("Nhân viên chưa có dữ liệu chấm công tháng này.");

                    // 3. Quyết định lương và chức vụ đang hiệu lực (BR06)
                    DataTable ql = DbTx.Query(cn, tx, @"SELECT TOP 1 q.MaQD, l.LuongCoBan, l.PhuCapLuong, l.PhuCapDocHai
                        FROM QuyetDinhLuong l JOIN QuyetDinh q ON q.MaQD=l.MaQD
                        WHERE q.MaNV=@MaNV AND q.TrangThaiKy=1 AND q.NgayHieuLuc<=@Cuoi
                        ORDER BY q.NgayHieuLuc DESC",
                        new SqlParameter("@MaNV", maNV), new SqlParameter("@Cuoi", cuoi));
                    if (ql.Rows.Count == 0) return KetQuaXuLy.Loi("Chưa có quyết định lương hiệu lực.");

                    DataRow r = ql.Rows[0];
                    string maQDLuong = Convert.ToString(r["MaQD"]);
                    decimal luongCoBan = Convert.ToDecimal(r["LuongCoBan"]);
                    decimal phuCapLuong = Convert.ToDecimal(r["PhuCapLuong"]) + Convert.ToDecimal(r["PhuCapDocHai"]);

                    decimal phuCapChucVu = 0;
                    string maQDChucVu = null;
                    DataTable cv = DbTx.Query(cn, tx, @"SELECT TOP 1 q.MaQD, c.PhuCapChucVu
                        FROM QuyetDinhChucVu c JOIN QuyetDinh q ON q.MaQD=c.MaQD
                        WHERE q.MaNV=@MaNV AND q.TrangThaiKy=1 AND q.NgayHieuLuc<=@Cuoi
                        ORDER BY q.NgayHieuLuc DESC",
                        new SqlParameter("@MaNV", maNV), new SqlParameter("@Cuoi", cuoi));
                    if (cv.Rows.Count > 0)
                    {
                        maQDChucVu = Convert.ToString(cv.Rows[0]["MaQD"]);
                        phuCapChucVu = Convert.ToDecimal(cv.Rows[0]["PhuCapChucVu"]);
                    }
                    decimal tongPhuCap = phuCapLuong + phuCapChucVu;

                    // 4. Tham số tính lương (đọc từ ThamSo, không cố định trong mã)
                    decimal soNgayChuan = ThamSo(cn, tx, "SoNgayCongChuan");
                    decimal soGioNgay = ThamSo(cn, tx, "SoGioMotNgay");
                    decimal tyLeBH = ThamSo(cn, tx, "TyLeBHXH") + ThamSo(cn, tx, "TyLeBHYT");

                    // 5. Lương chính và tiền phụ trội (BR07: giờ phụ trội nhân hệ số 1.5)
                    decimal luongChinh = Math.Round(luongCoBan / soNgayChuan * soNgayCong, 0, MidpointRounding.AwayFromZero);
                    decimal luongGio = luongCoBan / (soNgayChuan * soGioNgay);
                    decimal gioQuyDoi = Convert.ToDecimal(DbTx.Scalar(cn, tx, @"SELECT ISNULL(SUM(ct.SoGioNV * p.HeSo), 0)
                        FROM ChiTietPhuTrong ct JOIN PhieuGhiNhanPhuTrong p ON p.SoPhieu=ct.SoPhieu
                        WHERE ct.MaNV=@MaNV AND p.NgayPhuTrong BETWEEN @Dau AND @Cuoi",
                        new SqlParameter("@MaNV", maNV), new SqlParameter("@Dau", dau),
                        new SqlParameter("@Cuoi", cuoi)));
                    decimal tienPhuTrong = Math.Round(luongGio * gioQuyDoi, 0, MidpointRounding.AwayFromZero);

                    // 6. Khấu trừ bảo hiểm: chỉ hợp đồng dài hạn còn hiệu lực (BR05)
                    decimal tongKhauTru = 0;
                    object maHS = null;
                    DataTable bh = DbTx.Query(cn, tx, @"SELECT TOP 1 hs.MaHS, hs.MucDongBH
                        FROM HoSoBaoHiem hs JOIN HopDong hd ON hd.MaHD=hs.MaHD
                        WHERE hd.MaNV=@MaNV AND hd.TrangThaiHieuLuc=1 AND hd.LoaiHopDong='DaiHan'",
                        new SqlParameter("@MaNV", maNV));
                    if (bh.Rows.Count > 0)
                    {
                        maHS = bh.Rows[0]["MaHS"];
                        decimal mucDongBH = Convert.ToDecimal(bh.Rows[0]["MucDongBH"]);
                        tongKhauTru = Math.Round(mucDongBH * tyLeBH, 0, MidpointRounding.AwayFromZero);
                    }

                    // 7. Phạt trừ lương trong tháng (BR12)
                    decimal tienPhat = Convert.ToDecimal(DbTx.Scalar(cn, tx, @"SELECT ISNULL(SUM(SoTienNop), 0)
                        FROM PhieuThuPhat
                        WHERE MaNV=@MaNV AND HinhThucThu=N'Trừ lương' AND NgayThu BETWEEN @Dau AND @Cuoi",
                        new SqlParameter("@MaNV", maNV), new SqlParameter("@Dau", dau),
                        new SqlParameter("@Cuoi", cuoi)));

                    decimal thucNhan = luongChinh + tongPhuCap + tienPhuTrong - tongKhauTru - tienPhat;

                    // 8. Ghi bảng lương và các quyết định đã dùng để tính
                    string maBL = "BL" + nam + thang.ToString("00") + "-" + maNV;
                    DbTx.Execute(cn, tx, @"INSERT INTO BangLuong(MaBL,MaNV,MaBCC,MaHS,Thang,Nam,LuongChinh,TongPhuCap,
                        TienPhuTrong,TongKhauTru,TienPhatTruLuong,LuongThucNhan)
                        VALUES(@MaBL,@MaNV,@MaBCC,@MaHS,@T,@N,@LC,@PC,@PT,@KT,@Phat,@TN)",
                        new SqlParameter("@MaBL", maBL), new SqlParameter("@MaNV", maNV),
                        new SqlParameter("@MaBCC", maBCC), new SqlParameter("@MaHS", maHS ?? DBNull.Value),
                        new SqlParameter("@T", thang), new SqlParameter("@N", nam),
                        new SqlParameter("@LC", luongChinh), new SqlParameter("@PC", tongPhuCap),
                        new SqlParameter("@PT", tienPhuTrong), new SqlParameter("@KT", tongKhauTru),
                        new SqlParameter("@Phat", tienPhat), new SqlParameter("@TN", thucNhan));

                    DbTx.Execute(cn, tx, "INSERT INTO BangLuongQuyetDinh(MaBL,MaQD) VALUES(@MaBL,@MaQD)",
                        new SqlParameter("@MaBL", maBL), new SqlParameter("@MaQD", maQDLuong));
                    if (maQDChucVu != null)
                        DbTx.Execute(cn, tx, "INSERT INTO BangLuongQuyetDinh(MaBL,MaQD) VALUES(@MaBL,@MaQD)",
                            new SqlParameter("@MaBL", maBL), new SqlParameter("@MaQD", maQDChucVu));

                    tx.Commit();
                    return KetQuaXuLy.Ok("Thực nhận: " + thucNhan.ToString("N0") + " đ. Mã bảng lương: " + maBL);
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return KetQuaXuLy.Loi("Không thể tính lương: " + ex.Message);
                }
            }
        }

        private static decimal ThamSo(SqlConnection cn, SqlTransaction tx, string ten)
        {
            return Convert.ToDecimal(DbTx.Scalar(cn, tx,
                "SELECT GiaTri FROM ThamSo WHERE TenThamSo=@Ten", new SqlParameter("@Ten", ten)));
        }
    }
}
