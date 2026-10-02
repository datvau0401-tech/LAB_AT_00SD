using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using eShopping.Data;

namespace eShopping.Adapters
{
    // =====================================================================
    // 1) HỆ THỐNG QUẢN LÝ SẢN PHẨM (ngoài) - e-Shopping chỉ ĐỌC
    // =====================================================================
    public interface IProductSystemAdapter
    {
        IList<NhomSanPham> LayNhomSanPham();
        IList<SanPham> LaySanPhamTheoNhom(string maNhom);
        SanPham LayChiTiet(string maSanPham);
    }

    public class ProductSystemAdapter : IProductSystemAdapter
    {
        private const string Cot =
            "MaSanPham, TenSanPham, TenNhaSanXuat, MoTa, ThongSoKyThuat, GiaBan, ConHang, MaNhom";

        public IList<NhomSanPham> LayNhomSanPham()
        {
            DataTable t = Db.Query("SELECT MaNhom, TenNhom FROM ext.NhomSanPham ORDER BY TenNhom");
            List<NhomSanPham> ds = new List<NhomSanPham>();
            foreach (DataRow r in t.Rows)
                ds.Add(new NhomSanPham { MaNhom = Convert.ToString(r["MaNhom"]), TenNhom = Convert.ToString(r["TenNhom"]) });
            return ds;
        }

        public IList<SanPham> LaySanPhamTheoNhom(string maNhom)
        {
            DataTable t = Db.Query("SELECT " + Cot + " FROM ext.SanPham WHERE MaNhom=@Nhom ORDER BY TenSanPham",
                new SqlParameter("@Nhom", maNhom));
            List<SanPham> ds = new List<SanPham>();
            foreach (DataRow r in t.Rows) ds.Add(Map(r));
            return ds;
        }

        public SanPham LayChiTiet(string maSanPham)
        {
            DataTable t = Db.Query("SELECT " + Cot + " FROM ext.SanPham WHERE MaSanPham=@Ma",
                new SqlParameter("@Ma", maSanPham));
            if (t.Rows.Count == 0) return null;
            SanPham sp = Map(t.Rows[0]);
            DataTable h = Db.Query("SELECT DuongDan FROM ext.HinhAnhSanPham WHERE MaSanPham=@Ma ORDER BY MaHinh",
                new SqlParameter("@Ma", maSanPham));
            foreach (DataRow r in h.Rows) sp.HinhAnh.Add(Convert.ToString(r["DuongDan"]));
            return sp;
        }

        private static SanPham Map(DataRow r)
        {
            return new SanPham
            {
                MaSanPham = Convert.ToString(r["MaSanPham"]),
                TenSanPham = Convert.ToString(r["TenSanPham"]),
                TenNhaSanXuat = Convert.ToString(r["TenNhaSanXuat"]),
                MoTa = Convert.ToString(r["MoTa"]),
                ThongSoKyThuat = Convert.ToString(r["ThongSoKyThuat"]),
                GiaBan = Convert.ToDecimal(r["GiaBan"]),
                ConHang = Convert.ToBoolean(r["ConHang"]),
                MaNhom = Convert.ToString(r["MaNhom"])
            };
        }
    }

    // =====================================================================
    // 2) DỊCH VỤ THANH TOÁN TRỰC TUYẾN (ngoài) - bản mô phỏng
    //    Quy tắc giả lập: số thẻ phải qua Luhn; thẻ phải còn hạn;
    //    số thẻ kết thúc bằng '0' coi như không đủ khả năng thanh toán.
    // =====================================================================
    public interface IPaymentGateway
    {
        KetQuaThanhToan XacThuc(TheTinDung the, decimal soTien);
    }

    public class MockPaymentGateway : IPaymentGateway
    {
        public KetQuaThanhToan XacThuc(TheTinDung the, decimal soTien)
        {
            if (the == null || string.IsNullOrEmpty(the.SoThe) || !QuaLuhn(the.SoThe))
                return new KetQuaThanhToan { HopLe = false, ThongBao = "Dịch vụ thanh toán: thông tin thẻ không hợp lệ." };

            if (the.NgayHetHan.Date < DateTime.Today)
                return new KetQuaThanhToan { HopLe = false, ThongBao = "Dịch vụ thanh toán: thẻ đã hết hạn." };

            if (the.SoThe.EndsWith("0"))
                return new KetQuaThanhToan { HopLe = false, ThongBao = "Dịch vụ thanh toán: thẻ không đủ khả năng thanh toán." };

            return new KetQuaThanhToan
            {
                HopLe = true,
                MaXacNhan = "PAY" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant(),
                ThongBao = "Giao dịch được chấp nhận."
            };
        }

        private static bool QuaLuhn(string so)
        {
            int tong = 0;
            bool nhanDoi = false;
            for (int i = so.Length - 1; i >= 0; i--)
            {
                char c = so[i];
                if (c < '0' || c > '9') return false;
                int d = c - '0';
                if (nhanDoi)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                tong += d;
                nhanDoi = !nhanDoi;
            }
            return tong % 10 == 0;
        }
    }

    // =====================================================================
    // 3) DỊCH VỤ EMAIL (ngoài) - bản mô phỏng: ghi vào bảng NhatKyEmail
    // =====================================================================
    public interface IEmailService
    {
        KetQuaXuLy Gui(string maDonHang, string emailNhan, string tieuDe, string noiDung);
    }

    public class MockEmailService : IEmailService
    {
        public KetQuaXuLy Gui(string maDonHang, string emailNhan, string tieuDe, string noiDung)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emailNhan) || !emailNhan.Contains("@"))
                    return KetQuaXuLy.Loi("Địa chỉ email không hợp lệ.");

                Db.Execute(@"INSERT INTO NhatKyEmail(MaDonHang,EmailNhan,TieuDe,NoiDung)
                             VALUES(@Ma,@Email,@TieuDe,@NoiDung)",
                    new SqlParameter("@Ma", maDonHang),
                    new SqlParameter("@Email", emailNhan.Trim()),
                    new SqlParameter("@TieuDe", tieuDe),
                    new SqlParameter("@NoiDung", noiDung));
                return KetQuaXuLy.Ok("Đã gửi email.");
            }
            catch (Exception ex)
            {
                return KetQuaXuLy.Loi("Dịch vụ email lỗi: " + ex.Message);
            }
        }
    }
}
