using System.Data;
using BenhAn.Data;
using Xunit;

namespace BenhAn.Tests;

/// <summary>[Fact] chỉ chạy khi đặt biến môi trường BENHAN_TEST_CONNECTION (CSDL đã chạy 01..03). Không có thì bỏ qua.</summary>
public sealed class FactSqlAttribute : FactAttribute
{
    public FactSqlAttribute()
    {
        var cs = Environment.GetEnvironmentVariable("BENHAN_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(cs))
            Skip = "Đặt biến môi trường BENHAN_TEST_CONNECTION để chạy test tích hợp SQL Server.";
        else
            Db.ConnectionString = cs;
    }
}

public class NghiepVuTests
{
    private static readonly Random Rnd = new();

    private static string SdtMoi() => "0" + Rnd.Next(100_000_000, 999_999_999);

    private static KetQuaDangKy DangKy(string? sdt = null, decimal lePhi = 500_000) =>
        DangKyService.DangKyLanDau(new DangKyRequest("Test " + Guid.NewGuid().ToString("N")[..6], "1 Đường Test",
            new DateTime(1995, 5, 5), sdt ?? SdtMoi(), null, lePhi));

    private static int BacSi() => Convert.ToInt32(DanhMucService.BacSi().Rows[0]["MaBS"]);

    private static int ThuocId(string ten) =>
        DanhMucService.Thuoc().AsEnumerable().Where(r => (string)r["TenThuoc"] == ten).Select(r => Convert.ToInt32(r["MaThuoc"])).First();

    private static int XnId(string ten) =>
        DanhMucService.LoaiXetNghiem().AsEnumerable().Where(r => (string)r["TenXN"] == ten).Select(r => Convert.ToInt32(r["MaLoaiXN"])).First();

    private static NghiepVuException Loi(Action a) => Assert.Throws<NghiepVuException>(a);

    [FactSql]
    public void DangKyLanDau_CapMaHoSo_VaGhiNhanLePhiNam()
    {
        var kq = DangKy();
        Assert.Matches(@"^HS\d{6}$", kq.MaHoSo);
        var lePhi = DangKyService.LePhiTheoHoSo(kq.MaHS);
        Assert.Single(lePhi.Rows);
        Assert.Equal((short)DateTime.Today.Year, Convert.ToInt16(lePhi.Rows[0]["Nam"]));
    }

    [FactSql]
    public void DangKyLanDau_TrungSoDienThoai_BiTuChoi()
    {
        var sdt = SdtMoi();
        DangKy(sdt);
        Assert.Equal(50002, Loi(() => DangKy(sdt)).MaLoi);
    }

    [FactSql]
    public void DangKyLanDau_LePhiThieu_KhongTaoHoSo()
    {
        var sdt = SdtMoi();
        Assert.Equal(50003, Loi(() => DangKy(sdt, 1000)).MaLoi);
        Assert.Empty(TraCuuService.TraCuuHoSo(sdt).Rows);
    }

    [FactSql]
    public void GiaHanLePhi_TrungNam_BiTuChoi()
    {
        var kq = DangKy();
        Assert.Equal(50006, Loi(() => DangKyService.GiaHanLePhi(kq.MaHS, 500_000)).MaLoi);
    }

    [FactSql]
    public void TraCuu_TheoMaHoSo_SoDienThoai_VaHoTen()
    {
        var sdt = SdtMoi();
        var kq = DangKy(sdt);
        Assert.NotNull(TraCuuService.TimHoSo(kq.MaHoSo));
        Assert.NotNull(TraCuuService.TimHoSo(sdt));
        var theoTen = TraCuuService.TraCuuHoSo("Test");
        Assert.Contains(theoTen.AsEnumerable(), r => (string)r["MaHoSo"] == kq.MaHoSo);
    }

    [FactSql]
    public void PhieuKham_LienKetXetNghiemVaToa_ThanhToanRoiHoanTat_KhoaChinhSua()
    {
        var kq = DangKy();
        var maPK = KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DieuTri", "Sốt", 38.2m, "120/80", null);

        // chỉ 1 phiếu đang khám cho mỗi hồ sơ
        Assert.Equal(50021, Loi(() => KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DieuTri", null, null, null, null)).MaLoi);

        KhamBenhService.ThemXetNghiem(maPK, XnId("Công thức máu"), "Bình thường");
        Assert.Equal(50041, Loi(() => KhamBenhService.ThemXetNghiem(maPK, XnId("Công thức máu"), null)).MaLoi);

        var maToa = KhamBenhService.LapToa(maPK, "Uống sau ăn", new[]
        {
            new DongToa(ThuocId("Paracetamol 500mg"), 10, "3 lần/ngày"),
            new DongToa(ThuocId("Amoxicillin 500mg"), 14, "2 lần/ngày")
        });
        Assert.True(maToa > 0);
        Assert.Equal(50043, Loi(() => KhamBenhService.LapToa(maPK, null, new[] { new DongToa(ThuocId("Oresol"), 1, null) })).MaLoi);

        // viện phí = 150.000 (khám) + 90.000 (XN) + 10*1.500 + 14*4.000 = 311.000
        var vp = ThanhToanService.XemVienPhi(maPK).Rows[0];
        Assert.Equal(311_000m, Convert.ToDecimal(vp["TongTien"]));

        ThanhToanService.ThanhToan(maPK, "TienMat");

        // sau thanh toán: hoàn tất và bị khóa
        var ls = TraCuuService.LichSuKham(kq.MaHS).Rows[0];
        Assert.Equal("HoanTat", (string)ls["TrangThai"]);
        Assert.Equal(311_000m, Convert.ToDecimal(ls["TongTien"]));
        Assert.Equal(50010, Loi(() => ThanhToanService.ThanhToan(maPK, "TienMat")).MaLoi);
        Assert.Equal(50010, Loi(() => KhamBenhService.ThemXetNghiem(maPK, XnId("Đường huyết lúc đói"), null)).MaLoi);
        Assert.Equal(50010, Loi(() => KhamBenhService.CapNhatChanDoan(maPK, "Sửa trái phép")).MaLoi);
    }

    [FactSql]
    public void ToaCoThuocKhongTonTai_BiTuChoi_VaKhongTaoToaDoDang()
    {
        var kq = DangKy();
        var maPK = KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DieuTri", null, null, null, null);
        Assert.Equal(50045, Loi(() => KhamBenhService.LapToa(maPK, null, new[]
        {
            new DongToa(ThuocId("Oresol"), 1, null), new DongToa(999_999, 1, null)
        })).MaLoi);
        Assert.Empty(TraCuuService.Toa(maPK).Rows);
        // vẫn lập được toa đúng sau đó
        Assert.True(KhamBenhService.LapToa(maPK, null, new[] { new DongToa(ThuocId("Oresol"), 1, null) }) > 0);
    }

    [FactSql]
    public void LichDinhKy_MoiQuyMotLan_HuyRoiDatLai_KhamMienPhi()
    {
        var kq = DangKy();
        var gio = DateTime.Now.AddMinutes(5);
        var hen = LichHenService.Tao(kq.MaHS, gio, BacSi());

        Assert.Equal(50032, Loi(() => LichHenService.Tao(kq.MaHS, gio.AddMinutes(1), null)).MaLoi);   // cùng quý
        LichHenService.Huy(hen);
        var hen2 = LichHenService.Tao(kq.MaHS, gio, BacSi());                                          // hủy rồi đặt lại
        Assert.NotEqual(hen, hen2);

        // khám định kỳ bắt buộc có lịch hẹn; khám điều trị thì không được gắn lịch hẹn
        Assert.Equal(50023, Loi(() => KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DinhKy", null, null, null, null)).MaLoi);
        Assert.Equal(50025, Loi(() => KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DieuTri", null, null, null, hen2)).MaLoi);

        var maPK = KhamBenhService.TaoPhieu(kq.MaHS, BacSi(), "DinhKy", "Khám định kỳ", 36.6m, "115/75", hen2);
        var lich = LichHenService.TheoHoSo(kq.MaHS).AsEnumerable().Single(r => Convert.ToInt32(r["MaHen"]) == hen2);
        Assert.Equal("DaKham", (string)lich["TrangThai"]);

        KhamBenhService.ThemXetNghiem(maPK, XnId("Công thức máu"), null);
        ThanhToanService.ThanhToan(maPK, "TienMat");
        Assert.Equal(0m, Convert.ToDecimal(TraCuuService.LichSuKham(kq.MaHS).Rows[0]["TongTien"]));   // miễn phí

        Assert.Equal(50032, Loi(() => LichHenService.Tao(kq.MaHS, gio.AddMinutes(2), null)).MaLoi);  // quý đã khám
    }

    [FactSql]
    public void LichDinhKy_KhongDatTrongQuaKhu_VaKhongDatKhiChuaDongLePhiNamSau()
    {
        var kq = DangKy();
        Assert.Equal(50030, Loi(() => LichHenService.Tao(kq.MaHS, DateTime.Now.AddDays(-1), null)).MaLoi);
        var dauNamSau = new DateTime(DateTime.Today.Year + 1, 1, 2, 8, 0, 0);
        Assert.Equal(50031, Loi(() => LichHenService.Tao(kq.MaHS, dauNamSau, null)).MaLoi);

        DangKyService.GiaHanLePhi(kq.MaHS, 500_000, (short)(DateTime.Today.Year + 1));
        Assert.True(LichHenService.Tao(kq.MaHS, dauNamSau, null) > 0);
    }

    [FactSql]
    public void KhamDinhKy_DungLichHenCuaHoSoKhac_BiTuChoi()
    {
        var a = DangKy();
        var b = DangKy();
        var henA = LichHenService.Tao(a.MaHS, DateTime.Now.AddMinutes(5), null);
        Assert.Equal(50024, Loi(() => KhamBenhService.TaoPhieu(b.MaHS, BacSi(), "DinhKy", null, null, null, henA)).MaLoi);
    }
}
