using System.Data;

namespace QuanLyVanPhong;

/// <summary>
/// Kiểm thử tự động các quy tắc nghiệp vụ. Tạo dữ liệu tạm (khách TEST-0000, phòng TST1..TST4)
/// rồi xóa sạch khi xong, không đụng tới dữ liệu thật.
/// </summary>
public static class Tester
{
    const string TestPhone = "TEST-0000";

    static int Count(string sql) => Convert.ToInt32(Db.Scalar(sql));

    static void Cleanup()
    {
        string hd = $"SELECT MaHD FROM HopDong WHERE MaKH IN (SELECT MaKH FROM KhachHang WHERE DienThoai='{TestPhone}')";
        foreach (var bang in new[] { "HoaDonDichVu", "ThanhToanThue", "PhieuGiaHan", "ChiTietHopDong" })
            Db.Exec($"DELETE FROM {bang} WHERE MaHD IN ({hd})");
        Db.Exec($"DELETE FROM HopDong WHERE MaKH IN (SELECT MaKH FROM KhachHang WHERE DienThoai='{TestPhone}')");
        Db.Exec("DELETE FROM Phong WHERE MaPhong LIKE 'TST%'");
        Db.Exec($"DELETE FROM KhachHang WHERE DienThoai='{TestPhone}'");
    }

    public static List<string> Run()
    {
        var log = new List<string>();
        int dat = 0, loi = 0;

        void Check(string ten, bool ok, string ghiChu = "")
        {
            log.Add($"{(ok ? "[ĐẠT]" : "[LỖI]")} {ten}" + (ghiChu == "" ? "" : $"\r\n         -> {ghiChu}"));
            if (ok) dat++; else loi++;
        }

        // Chạy 1 hành động, trả về true nếu bị từ chối bởi quy tắc nghiệp vụ
        bool BiTuChoi(Action a, out string msg)
        {
            try { a(); msg = "(không có lỗi nào)"; return false; }
            catch (BusinessException ex) { msg = ex.Message; return true; }
        }

        try
        {
            Cleanup();
            Db.Exec($"INSERT INTO KhachHang(TenKH, DienThoai) VALUES(N'KH kiểm thử', '{TestPhone}')");
            int kh = Convert.ToInt32(Db.Scalar("SELECT MaKH FROM KhachHang WHERE DienThoai=@s", Db.P("@s", TestPhone)));
            foreach (var p in new[] { "TST1", "TST2", "TST3", "TST4" })
                Db.Exec("INSERT INTO Phong(MaPhong, MaTang, DienTich, Huong, GiaThue) VALUES(@p, 1, 50, N'Đông', 10000000)",
                    Db.P("@p", p));

            var today = DateTime.Today;
            string m;
            bool r;

            // ---------- Quy tắc 1: thuê tối thiểu 6 tháng ----------
            r = BiTuChoi(() => RentalService.TaoHopDong(kh, today, 5, new List<string> { "TST1" }), out m);
            Check("Thuê 5 tháng (< 6) phải bị từ chối", r, m);

            int hd1 = RentalService.TaoHopDong(kh, today, 6, new List<string> { "TST1" });
            int paid = Count($"SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD={hd1} AND DaThu=1");
            int all = Count($"SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD={hd1}");
            Check("Thuê đúng 6 tháng được chấp nhận, trả trước đủ 6 tháng", paid == 6 && all == 6, $"đã thu {paid}/{all} tháng");

            // ---------- Quy tắc 2: trả trước 6 tháng đầu, tháng 7+ trả đầu tháng ----------
            int hd2 = RentalService.TaoHopDong(kh, today, 12, new List<string> { "TST2" });
            paid = Count($"SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD={hd2} AND DaThu=1");
            int chua = Count($"SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD={hd2} AND DaThu=0");
            Check("Thuê 12 tháng: 6 tháng đầu đã trả, tháng 7-12 chưa trả", paid == 6 && chua == 6, $"đã trả {paid}, chưa trả {chua}");

            var han7 = (DateTime)Db.Scalar($"SELECT HanThu FROM ThanhToanThue WHERE MaHD={hd2} AND ThangThu=7");
            Check("Tháng 7 đến hạn vào đầu tháng thuê thứ 7", han7 == today.AddMonths(6), $"hạn thu: {han7:dd/MM/yyyy}");

            // ---------- Phòng đã thuê + transaction (rollback) ----------
            r = BiTuChoi(() => RentalService.TaoHopDong(kh, today, 6, new List<string> { "TST1" }), out m);
            Check("Thuê phòng đã có người thuê phải bị từ chối", r, m);

            int soHD = Count($"SELECT COUNT(*) FROM HopDong WHERE MaKH={kh}");
            r = BiTuChoi(() => RentalService.TaoHopDong(kh, today, 6, new List<string> { "TST3", "TST1" }), out m);
            string tt3 = (string)Db.Scalar("SELECT TrangThai FROM Phong WHERE MaPhong='TST3'");
            int soHD2 = Count($"SELECT COUNT(*) FROM HopDong WHERE MaKH={kh}");
            Check("Thuê 2 phòng, 1 phòng lỗi -> ROLLBACK toàn bộ (TST3 vẫn trống, không sinh hợp đồng)",
                r && tt3 == "Trong" && soHD == soHD2, $"TST3 = '{tt3}', số hợp đồng {soHD} -> {soHD2}");

            // ---------- Quy tắc 3: dịch vụ thanh toán cuối tháng ----------
            int dv = Convert.ToInt32(Db.Scalar("SELECT TOP 1 MaDV FROM DichVu ORDER BY MaDV"));
            int maHDDV = RentalService.LapHoaDonDichVu(hd2, "TST2", dv, today, 500000);
            var han = (DateTime)Db.Scalar($"SELECT HanThu FROM HoaDonDichVu WHERE MaHDDV={maHDDV}");
            var cuoiThang = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            Check("Hóa đơn dịch vụ đến hạn vào cuối tháng", han == cuoiThang, $"hạn thu: {han:dd/MM/yyyy}");

            r = BiTuChoi(() => RentalService.LapHoaDonDichVu(hd2, "TST2", dv, today, 600000), out m);
            Check("Lập trùng hóa đơn cùng dịch vụ/tháng bị từ chối (UNIQUE)", r, m);

            // ---------- Quy tắc 4: gia hạn <= 1/2 và không còn nợ ----------
            r = BiTuChoi(() => RentalService.LapPhieuGiaHan(hd2, "TST2", 3), out m);
            Check("Gia hạn khi còn hóa đơn dịch vụ chưa trả bị từ chối", r && m.Contains("dịch vụ"), m);

            RentalService.ThuDichVu(maHDDV);

            r = BiTuChoi(() => RentalService.LapPhieuGiaHan(hd2, "TST2", 7), out m);
            Check("Gia hạn 7 tháng (> 1/2 của 12 tháng) bị từ chối", r && m.Contains("tối đa"), m);

            r = !BiTuChoi(() => RentalService.LapPhieuGiaHan(hd2, "TST2", 6), out m);
            int tong = Count($"SELECT COUNT(*) FROM ThanhToanThue WHERE MaHD={hd2} AND MaPhong='TST2'");
            int gh = Count($"SELECT SoThangGiaHan FROM ChiTietHopDong WHERE MaHD={hd2} AND MaPhong='TST2'");
            Check("Gia hạn 6 tháng (= 1/2) khi hết nợ thì thành công", r && tong == 18 && gh == 6,
                r ? $"lịch thu {tong} tháng, đã gia hạn {gh} tháng" : m);

            // Nợ tiền thuê: hợp đồng bắt đầu cách đây 8 tháng, tháng 7+ đã đến hạn mà chưa trả
            int hd3 = RentalService.TaoHopDong(kh, today.AddMonths(-8), 12, new List<string> { "TST4" });
            r = BiTuChoi(() => RentalService.LapPhieuGiaHan(hd3, "TST4", 2), out m);
            Check("Gia hạn khi còn nợ tiền thuê quá hạn bị từ chối", r && m.Contains("tiền thuê"), m);

            var no = Db.Table($"SELECT MaTT FROM ThanhToanThue WHERE MaHD={hd3} AND DaThu=0 AND HanThu<=@t", Db.P("@t", today));
            foreach (DataRow row in no.Rows) RentalService.ThuTienThue((int)row["MaTT"]);

            r = !BiTuChoi(() => RentalService.LapPhieuGiaHan(hd3, "TST4", 2), out m);
            Check("Trả hết nợ rồi gia hạn 2 tháng thì thành công", r, r ? $"đã thu {no.Rows.Count} khoản nợ" : m);
        }
        catch (Exception ex)
        {
            log.Add("[LỖI HỆ THỐNG] " + ex.Message);
            loi++;
        }
        finally
        {
            try { Cleanup(); } catch { /* bỏ qua */ }
        }

        log.Add("");
        log.Add($"TỔNG KẾT: {dat} đạt, {loi} lỗi.");
        return log;
    }
}
