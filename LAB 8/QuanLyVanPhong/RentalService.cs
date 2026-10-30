using Microsoft.Data.SqlClient;

namespace QuanLyVanPhong;

/// <summary>Lỗi vi phạm quy tắc nghiệp vụ (hiện cho người dùng đọc).</summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

/// <summary>
/// Toàn bộ quy tắc nghiệp vụ nằm ở đây:
///  - Thuê tối thiểu 6 tháng
///  - Trả trước 6 tháng đầu; tháng 7+ trả vào đầu mỗi tháng
///  - Dịch vụ thanh toán cuối tháng
///  - Gia hạn &lt;= 1/2 thời gian thuê và không còn nợ
/// Mỗi nghiệp vụ nhiều bảng chạy trong 1 TRANSACTION: lỗi là ROLLBACK hết.
/// </summary>
public static class RentalService
{
    static SqlParameter P(string n, object v) => Db.P(n, v);

    static T InTx<T>(Func<SqlConnection, SqlTransaction, T> work)
    {
        using var c = Db.Open();
        using var t = c.BeginTransaction();
        try
        {
            var r = work(c, t);
            t.Commit();
            return r;
        }
        catch (SqlException ex) { t.Rollback(); throw Db.Translate(ex); }
        catch { t.Rollback(); throw; }
    }

    // Tạo lịch thu tiền thuê từng tháng. Tháng 1-6: đã thu (trả trước). Tháng 7+: chưa thu, hạn = đầu tháng thuê.
    static void ThemLichThu(SqlConnection c, SqlTransaction t, int maHD, string maPhong,
                            decimal gia, DateTime ngayBD, int tuThang, int denThang)
    {
        for (int m = tuThang; m <= denThang; m++)
        {
            bool truoc = m <= 6;
            Db.Exec(c, t,
                "INSERT INTO ThanhToanThue(MaHD,MaPhong,ThangThu,SoTien,HanThu,DaThu,NgayThu) " +
                "VALUES(@hd,@p,@m,@g,@h,@d,@n)",
                P("@hd", maHD), P("@p", maPhong), P("@m", m), P("@g", gia),
                P("@h", truoc ? ngayBD : ngayBD.AddMonths(m - 1)),
                P("@d", truoc),
                P("@n", truoc ? (object)DateTime.Today : null));
        }
    }

    // ===================== 1. LẬP HỢP ĐỒNG (nhiều phòng) =====================
    public static int TaoHopDong(int maKH, DateTime ngayBD, int soThang, List<string> phongs)
    {
        if (soThang < 6) throw new BusinessException("Thời gian thuê tối thiểu là 6 tháng.");
        if (phongs == null || phongs.Count == 0) throw new BusinessException("Chọn ít nhất 1 phòng.");

        return InTx((c, t) =>
        {
            int maHD = Convert.ToInt32(Db.Scalar(c, t,
                "INSERT INTO HopDong(MaKH,NgayLap,NgayBatDau,SoThang) OUTPUT INSERTED.MaHD VALUES(@kh,@nl,@bd,@st)",
                P("@kh", maKH), P("@nl", DateTime.Today), P("@bd", ngayBD.Date), P("@st", soThang)));

            foreach (var p in phongs)
            {
                object gia = Db.Scalar(c, t,
                    "SELECT GiaThue FROM Phong WITH (UPDLOCK, ROWLOCK) WHERE MaPhong=@p AND TrangThai=N'Trong'",
                    P("@p", p));
                if (gia == null)
                    throw new BusinessException($"Phòng {p} không tồn tại hoặc đã có người thuê.");

                decimal g = Convert.ToDecimal(gia);
                Db.Exec(c, t, "INSERT INTO ChiTietHopDong(MaHD,MaPhong,GiaThue) VALUES(@hd,@p,@g)",
                    P("@hd", maHD), P("@p", p), P("@g", g));
                Db.Exec(c, t, "UPDATE Phong SET TrangThai=N'DangThue' WHERE MaPhong=@p", P("@p", p));
                ThemLichThu(c, t, maHD, p, g, ngayBD.Date, 1, soThang);
            }
            return maHD;
        });
    }

    // ===================== 2. THU TIỀN THUÊ =====================
    public static void ThuTienThue(int maTT) => InTx<int>((c, t) =>
    {
        int n = Db.Exec(c, t, "UPDATE ThanhToanThue SET DaThu=1, NgayThu=@d WHERE MaTT=@id AND DaThu=0",
            P("@d", DateTime.Today), P("@id", maTT));
        if (n == 0) throw new BusinessException("Khoản này đã thu rồi hoặc không tồn tại.");
        return n;
    });

    // ===================== 3. HÓA ĐƠN DỊCH VỤ (thanh toán cuối tháng) =====================
    public static int LapHoaDonDichVu(int maHD, string maPhong, int maDV, DateTime thang, decimal soTien)
    {
        if (soTien <= 0) throw new BusinessException("Số tiền dịch vụ phải lớn hơn 0.");
        var dauThang = new DateTime(thang.Year, thang.Month, 1);
        var cuoiThang = dauThang.AddMonths(1).AddDays(-1);

        return InTx((c, t) => Convert.ToInt32(Db.Scalar(c, t,
            "INSERT INTO HoaDonDichVu(MaHD,MaPhong,MaDV,Thang,SoTien,HanThu) OUTPUT INSERTED.MaHDDV " +
            "VALUES(@hd,@p,@dv,@th,@st,@h)",
            P("@hd", maHD), P("@p", maPhong), P("@dv", maDV), P("@th", dauThang), P("@st", soTien), P("@h", cuoiThang))));
    }

    public static void ThuDichVu(int maHDDV) => InTx<int>((c, t) =>
    {
        int n = Db.Exec(c, t, "UPDATE HoaDonDichVu SET DaThu=1, NgayThu=@d WHERE MaHDDV=@id AND DaThu=0",
            P("@d", DateTime.Today), P("@id", maHDDV));
        if (n == 0) throw new BusinessException("Hóa đơn này đã thu rồi hoặc không tồn tại.");
        return n;
    });

    // ===================== 4. LẬP PHIẾU GIA HẠN =====================
    static void KiemTraKhongNo(SqlConnection c, SqlTransaction t, int maHD, string maPhong)
    {
        int noThue = Convert.ToInt32(Db.Scalar(c, t,
            "SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD=@hd AND MaPhong=@p AND DaThu=0 AND HanThu<=@today",
            P("@hd", maHD), P("@p", maPhong), P("@today", DateTime.Today)));
        if (noThue > 0)
            throw new BusinessException($"Không thể gia hạn: còn nợ tiền thuê quá hạn ({noThue} khoản).");

        int noDV = Convert.ToInt32(Db.Scalar(c, t,
            "SELECT COUNT(*) FROM HoaDonDichVu WHERE MaHD=@hd AND MaPhong=@p AND DaThu=0",
            P("@hd", maHD), P("@p", maPhong)));
        if (noDV > 0)
            throw new BusinessException($"Không thể gia hạn: còn {noDV} hóa đơn dịch vụ chưa thanh toán.");
    }

    public static int LapPhieuGiaHan(int maHD, string maPhong, int soThangGH)
    {
        if (soThangGH <= 0) throw new BusinessException("Số tháng gia hạn phải lớn hơn 0.");

        return InTx((c, t) =>
        {
            var dt = Db.Table(c, t,
                "SELECT hd.NgayBatDau, hd.SoThang, ct.SoThangGiaHan, ct.GiaThue " +
                "FROM HopDong hd JOIN ChiTietHopDong ct ON ct.MaHD=hd.MaHD " +
                "WHERE hd.MaHD=@hd AND ct.MaPhong=@p",
                P("@hd", maHD), P("@p", maPhong));
            if (dt.Rows.Count == 0) throw new BusinessException("Phòng này không thuộc hợp đồng đã chọn.");

            var r = dt.Rows[0];
            DateTime bd = (DateTime)r["NgayBatDau"];
            int soThang = (int)r["SoThang"];
            int daGH = (int)r["SoThangGiaHan"];
            decimal gia = (decimal)r["GiaThue"];

            if (bd.AddMonths(soThang + daGH) < DateTime.Today)
                throw new BusinessException("Hợp đồng đã hết hạn, không thể gia hạn.");

            if (soThangGH * 2 > soThang)
                throw new BusinessException(
                    $"Gia hạn tối đa {soThang / 2.0:0.#} tháng (không quá 1/2 thời gian thuê {soThang} tháng).");

            KiemTraKhongNo(c, t, maHD, maPhong);

            int maPG = Convert.ToInt32(Db.Scalar(c, t,
                "INSERT INTO PhieuGiaHan(MaHD,MaPhong,NgayLap,SoThangGiaHan) OUTPUT INSERTED.MaPG VALUES(@hd,@p,@nl,@k)",
                P("@hd", maHD), P("@p", maPhong), P("@nl", DateTime.Today), P("@k", soThangGH)));

            Db.Exec(c, t, "UPDATE ChiTietHopDong SET SoThangGiaHan = SoThangGiaHan + @k WHERE MaHD=@hd AND MaPhong=@p",
                P("@k", soThangGH), P("@hd", maHD), P("@p", maPhong));

            int tong = soThang + daGH;
            ThemLichThu(c, t, maHD, maPhong, gia, bd, tong + 1, tong + soThangGH);
            return maPG;
        });
    }
}
