using System.Data;
using Microsoft.Data.SqlClient;

namespace BenhAn.Data;

public record DangKyRequest(string HoTen, string DiaChi, DateTime NgaySinh, string DienThoai, byte[]? Anh, decimal LePhi);
public record KetQuaDangKy(int MaBN, int MaHS, string MaHoSo);
public record DongToa(int MaThuoc, int SoLuong, string? LieuDung);

/// <summary>Danh mục dùng chung (bác sĩ, thuốc, loại xét nghiệm, tham số).</summary>
public static class DanhMucService
{
    public static DataTable BacSi() => Db.Table("dbo.sp_DsBacSi");
    public static DataTable Thuoc() => Db.Table("dbo.sp_DsThuoc");
    public static DataTable LoaiXetNghiem() => Db.Table("dbo.sp_DsLoaiXetNghiem");

    public static decimal ThamSo(string ten)
    {
        var v = Db.Scalar("dbo.sp_LayThamSo", Db.P("@TenThamSo", ten, SqlDbType.VarChar));
        return v is null ? 0 : Convert.ToDecimal(v);
    }
}

/// <summary>Module 1: đăng ký lần đầu + lệ phí năm + cấp hồ sơ; gia hạn lệ phí.</summary>
public static class DangKyService
{
    public static KetQuaDangKy DangKyLanDau(DangKyRequest r)
    {
        var bn = Db.Out("@MaBN", SqlDbType.Int);
        var hs = Db.Out("@MaHS", SqlDbType.Int);
        var ma = Db.Out("@MaHoSo", SqlDbType.VarChar, 10);
        Db.Exec("dbo.sp_DangKyLanDau",
            Db.P("@HoTen", r.HoTen, SqlDbType.NVarChar),
            Db.P("@DiaChi", r.DiaChi, SqlDbType.NVarChar),
            Db.P("@NgaySinh", r.NgaySinh.Date, SqlDbType.Date),
            Db.P("@DienThoai", r.DienThoai, SqlDbType.VarChar),
            Db.P("@Anh", r.Anh, SqlDbType.VarBinary),
            Db.Dec("@SoTienLePhi", r.LePhi, 18, 0),
            bn, hs, ma);
        return new KetQuaDangKy((int)bn.Value, (int)hs.Value, (string)ma.Value);
    }

    public static void GiaHanLePhi(int maHS, decimal soTien, short? nam = null) =>
        Db.Exec("dbo.sp_GiaHanLePhi",
            Db.P("@MaHS", maHS, SqlDbType.Int),
            Db.Dec("@SoTien", soTien, 18, 0),
            Db.P("@Nam", nam, SqlDbType.SmallInt));

    public static DataTable LePhiTheoHoSo(int maHS) =>
        Db.Table("dbo.sp_LePhiTheoHoSo", Db.P("@MaHS", maHS, SqlDbType.Int));
}

/// <summary>Module 2: tra cứu hồ sơ và lịch sử khám.</summary>
public static class TraCuuService
{
    public static DataTable TraCuuHoSo(string? tuKhoa) =>
        Db.Table("dbo.sp_TraCuuHoSo", Db.P("@TuKhoa", tuKhoa, SqlDbType.NVarChar));

    /// <summary>Tìm đúng 1 hồ sơ theo mã hồ sơ (HS000001) hoặc số điện thoại. Trả về null nếu không có.</summary>
    public static DataRow? TimHoSo(string maHoSoHoacSdt)
    {
        var dt = TraCuuHoSo(maHoSoHoacSdt);
        var key = maHoSoHoacSdt.Trim();
        foreach (DataRow row in dt.Rows)
        {
            if (string.Equals((string)row["MaHoSo"], key, StringComparison.OrdinalIgnoreCase) ||
                (string)row["DienThoai"] == key)
                return row;
        }
        return null;
    }

    public static DataTable LichSuKham(int maHS) =>
        Db.Table("dbo.sp_LichSuKham", Db.P("@MaHS", maHS, SqlDbType.Int));

    public static DataTable XetNghiem(int maPK) =>
        Db.Table("dbo.sp_XetNghiemTheoPhieu", Db.P("@MaPK", maPK, SqlDbType.Int));

    public static DataTable Toa(int maPK) =>
        Db.Table("dbo.sp_ToaTheoPhieu", Db.P("@MaPK", maPK, SqlDbType.Int));
}

/// <summary>Module 3: lịch khám sức khỏe định kỳ theo quý.</summary>
public static class LichHenService
{
    public static int Tao(int maHS, DateTime ngayHen, int? maBS)
    {
        var out1 = Db.Out("@MaHen", SqlDbType.Int);
        Db.Exec("dbo.sp_TaoLichHen",
            Db.P("@MaHS", maHS, SqlDbType.Int),
            Db.P("@NgayHen", ngayHen, SqlDbType.DateTime2),
            Db.P("@MaBS", maBS, SqlDbType.Int),
            out1);
        return (int)out1.Value;
    }

    public static void Huy(int maHen) =>
        Db.Exec("dbo.sp_HuyLichHen", Db.P("@MaHen", maHen, SqlDbType.Int));

    public static DataTable TheoHoSo(int maHS, bool chiConHieuLuc = false) =>
        Db.Table("dbo.sp_LichHenTheoHoSo",
            Db.P("@MaHS", maHS, SqlDbType.Int),
            Db.P("@ChiConHieuLuc", chiConHieuLuc, SqlDbType.Bit));
}

/// <summary>Module 4: phiếu khám, xét nghiệm, toa thuốc.</summary>
public static class KhamBenhService
{
    public static int TaoPhieu(int maHS, int maBS, string loaiKham, string? trieuChung,
                               decimal? nhietDo, string? huyetAp, int? maHen)
    {
        var o = Db.Out("@MaPK", SqlDbType.Int);
        Db.Exec("dbo.sp_TaoPhieuKham",
            Db.P("@MaHS", maHS, SqlDbType.Int),
            Db.P("@MaBS", maBS, SqlDbType.Int),
            Db.P("@LoaiKham", loaiKham, SqlDbType.VarChar),
            Db.P("@TrieuChung", trieuChung, SqlDbType.NVarChar),
            Db.Dec("@NhietDo", nhietDo, 4, 1),
            Db.P("@HuyetAp", huyetAp, SqlDbType.VarChar),
            Db.P("@MaHen", maHen, SqlDbType.Int),
            o);
        return (int)o.Value;
    }

    public static DataTable PhieuDangKham(int maHS) =>
        Db.Table("dbo.sp_PhieuDangKhamCuaHoSo", Db.P("@MaHS", maHS, SqlDbType.Int));

    public static void CapNhatChanDoan(int maPK, string chanDoan) =>
        Db.Exec("dbo.sp_CapNhatChanDoan",
            Db.P("@MaPK", maPK, SqlDbType.Int),
            Db.P("@ChanDoan", chanDoan, SqlDbType.NVarChar));

    public static void ThemXetNghiem(int maPK, int maLoaiXN, string? ketQua) =>
        Db.Exec("dbo.sp_ThemXetNghiem",
            Db.P("@MaPK", maPK, SqlDbType.Int),
            Db.P("@MaLoaiXN", maLoaiXN, SqlDbType.Int),
            Db.P("@KetQua", ketQua, SqlDbType.NVarChar));

    public static void CapNhatKetQuaXN(int maXN, string ketQua) =>
        Db.Exec("dbo.sp_CapNhatKetQuaXN",
            Db.P("@MaXN", maXN, SqlDbType.Int),
            Db.P("@KetQua", ketQua, SqlDbType.NVarChar));

    public static int LapToa(int maPK, string? ghiChu, IEnumerable<DongToa> dong)
    {
        var tvp = new DataTable();
        tvp.Columns.Add("MaThuoc", typeof(int));
        tvp.Columns.Add("SoLuong", typeof(int));
        tvp.Columns.Add("LieuDung", typeof(string));
        foreach (var d in dong)
            tvp.Rows.Add(d.MaThuoc, d.SoLuong, (object?)d.LieuDung ?? DBNull.Value);

        var pTvp = new SqlParameter("@ChiTiet", SqlDbType.Structured)
        {
            TypeName = "dbo.ChiTietToaType",
            Value = tvp
        };
        var o = Db.Out("@MaToa", SqlDbType.Int);
        Db.Exec("dbo.sp_LapToaThuoc",
            Db.P("@MaPK", maPK, SqlDbType.Int),
            Db.P("@GhiChu", ghiChu, SqlDbType.NVarChar),
            pTvp, o);
        return (int)o.Value;
    }
}

/// <summary>Module 5: thanh toán viện phí và hoàn tất phiếu khám.</summary>
public static class ThanhToanService
{
    public static DataTable PhieuChoThanhToan() => Db.Table("dbo.sp_DanhSachPhieuChoThanhToan");

    public static DataTable XemVienPhi(int maPK) =>
        Db.Table("dbo.sp_XemVienPhi", Db.P("@MaPK", maPK, SqlDbType.Int));

    public static int ThanhToan(int maPK, string phuongThuc)
    {
        var o = Db.Out("@MaHD", SqlDbType.Int);
        Db.Exec("dbo.sp_ThanhToanVaHoanTat",
            Db.P("@MaPK", maPK, SqlDbType.Int),
            Db.P("@PhuongThuc", phuongThuc, SqlDbType.VarChar),
            o);
        return (int)o.Value;
    }
}
