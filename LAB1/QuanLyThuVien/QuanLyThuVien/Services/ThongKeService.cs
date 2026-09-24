using System;
using System.Data;
using System.Data.SqlClient;
using QuanLyThuVien.Data;

namespace QuanLyThuVien.Services
{
    public class ThongKeService
    {
        // Quy ước TinhTrangTra khi trả sách: "Bình thường", "Mất", "Hư hỏng"
        // (Trễ hạn được xác định bằng cách so NgayTraThucTe với NgayHenTra, không phụ thuộc TinhTrangTra)

        public ThongKeTongHop TinhTongHop(DateTime tuNgay, DateTime denNgay)
        {
            DateTime tu = tuNgay.Date;
            DateTime den = denNgay.Date;

            var kq = new ThongKeTongHop();

            kq.LuotSachMuon = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon ct
                  JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                  WHERE pm.NgayMuon BETWEEN @Tu AND @Den",
                new SqlParameter("@Tu", tu), new SqlParameter("@Den", den)));

            kq.SachQuaHan = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon ct
                  JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                  WHERE ct.NgayTraThucTe IS NOT NULL AND ct.NgayTraThucTe > pm.NgayHenTra
                        AND ct.NgayTraThucTe BETWEEN @Tu AND @Den",
                new SqlParameter("@Tu", tu), new SqlParameter("@Den", den)));

            kq.SachMat = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon
                  WHERE TinhTrangTra = N'Mất' AND NgayTraThucTe BETWEEN @Tu AND @Den",
                new SqlParameter("@Tu", tu), new SqlParameter("@Den", den)));

            kq.SachHuHong = Convert.ToInt32(Db.Scalar(
                @"SELECT COUNT(*) FROM ChiTietPhieuMuon
                  WHERE TinhTrangTra = N'Hư hỏng' AND NgayTraThucTe BETWEEN @Tu AND @Den",
                new SqlParameter("@Tu", tu), new SqlParameter("@Den", den)));

            object tongPhi = Db.Scalar(
                @"SELECT ISNULL(SUM(PhiPhat),0) FROM PhieuPhat WHERE NgayPhat BETWEEN @Tu AND @Den",
                new SqlParameter("@Tu", tu), new SqlParameter("@Den", den));
            kq.TongPhiPhat = Convert.ToDecimal(tongPhi);

            return kq;
        }

        public DataTable LayChiTietPhieuPhat(DateTime tuNgay, DateTime denNgay)
        {
            string sql = @"SELECT pp.MaPhieuPhat AS [Mã phiếu], pp.NgayPhat AS [Ngày],
                                   dg.MaDocGia + N' - ' + dg.Ho + N' ' + dg.Ten AS [Độc giả],
                                   ct.MaDauSach AS [Mã sách], pp.LyDo AS [Lý do], pp.PhiPhat AS [Phí phạt]
                            FROM PhieuPhat pp
                            JOIN ChiTietPhieuMuon ct ON ct.MaChiTiet = pp.MaChiTiet
                            JOIN PhieuMuon pm ON pm.MaPhieuMuon = ct.MaPhieuMuon
                            JOIN DocGia dg ON dg.MaDocGia = pm.MaDocGia
                            WHERE pp.NgayPhat BETWEEN @Tu AND @Den
                            ORDER BY pp.NgayPhat DESC";
            return Db.Query(sql,
                new SqlParameter("@Tu", tuNgay.Date),
                new SqlParameter("@Den", denNgay.Date));
        }
    }
}
