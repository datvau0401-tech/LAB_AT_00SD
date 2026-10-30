using System.Data;

namespace QuanLyVanPhong;

public class FrmHopDong : Form
{
    class PhongItem
    {
        public string Ma;
        public string Text;
        public override string ToString() => Text;
    }

    ComboBox cbKH = Ui.Combo(120, 15, 300);
    DateTimePicker dtp = new DateTimePicker { Left = 120, Top = 50, Width = 130, Format = DateTimePickerFormat.Short };
    NumericUpDown nud = new NumericUpDown { Left = 120, Top = 85, Width = 70, Minimum = 1, Maximum = 120, Value = 6 };
    CheckedListBox clb = new CheckedListBox { Left = 120, Top = 120, Width = 300, Height = 130, CheckOnClick = true };
    DataGridView g = Ui.Grid(10, 300, 600, 170);

    public FrmHopDong()
    {
        Ui.Setup(this, "Lập hợp đồng thuê văn phòng", 630, 480);
        Controls.AddRange(new Control[]
        {
            Ui.L("Khách hàng", 10, 18), cbKH,
            Ui.L("Ngày bắt đầu", 10, 53), dtp,
            Ui.L("Số tháng thuê", 10, 88), nud,
            Ui.L("Chọn phòng", 10, 123), clb,
            Ui.B("Lập hợp đồng", 120, 260, 150, Lap),
            g
        });
        Load += (s, e) => Nap();
    }

    void Nap()
    {
        try
        {
            Ui.Bind(cbKH, Db.Table(
                "SELECT MaKH, TenKH FROM KhachHang WHERE DienThoai NOT LIKE 'TEST%' ORDER BY TenKH"), "MaKH", "TenKH");

            clb.Items.Clear();
            var phong = Db.Table(
                "SELECT MaPhong, DienTich, Huong, GiaThue FROM Phong " +
                "WHERE TrangThai = N'Trong' AND MaPhong NOT LIKE 'TST%' ORDER BY MaPhong");
            foreach (DataRow r in phong.Rows)
                clb.Items.Add(new PhongItem
                {
                    Ma = (string)r["MaPhong"],
                    Text = $"{r["MaPhong"]} - {r["DienTich"]:0.#} m² - {r["Huong"]} - {r["GiaThue"]:N0} đ/tháng"
                });

            g.DataSource = Db.Table(
                "SELECT hd.MaHD, kh.TenKH, hd.NgayBatDau, hd.SoThang, STRING_AGG(ct.MaPhong, ', ') AS Phong " +
                "FROM HopDong hd JOIN KhachHang kh ON kh.MaKH = hd.MaKH " +
                "JOIN ChiTietHopDong ct ON ct.MaHD = hd.MaHD " +
                "WHERE kh.DienThoai NOT LIKE 'TEST%' " +
                "GROUP BY hd.MaHD, kh.TenKH, hd.NgayBatDau, hd.SoThang ORDER BY hd.MaHD DESC");
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void Lap(object sender, EventArgs e)
    {
        try
        {
            int kh = Ui.Sel(cbKH);
            if (kh < 0) throw new BusinessException("Chọn khách hàng.");

            var ds = clb.CheckedItems.Cast<PhongItem>().Select(x => x.Ma).ToList();
            int hd = RentalService.TaoHopDong(kh, dtp.Value, (int)nud.Value, ds);

            Ui.Info($"Đã lập hợp đồng #{hd}.\nTháng 1–6 đã thu trước; từ tháng 7 (nếu có) thu vào đầu mỗi tháng.");
            Nap();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }
}
