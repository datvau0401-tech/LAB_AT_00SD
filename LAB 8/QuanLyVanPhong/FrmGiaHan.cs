namespace QuanLyVanPhong;

public class FrmGiaHan : Form
{
    ComboBox cbHD = Ui.Combo(100, 10, 320);
    ComboBox cbPhong = Ui.Combo(100, 45, 120);
    NumericUpDown nud = new NumericUpDown { Left = 100, Top = 80, Width = 60, Minimum = 1, Maximum = 60, Value = 1 };
    Label lblInfo = new Label { Left = 10, Top = 112, Width = 560, Height = 36, AutoSize = false };
    DataGridView g = Ui.Grid(10, 190, 560, 170);

    public FrmGiaHan()
    {
        Ui.Setup(this, "Lập phiếu gia hạn", 580, 370);
        Controls.AddRange(new Control[]
        {
            Ui.L("Hợp đồng", 10, 14), cbHD,
            Ui.L("Phòng", 10, 49), cbPhong,
            Ui.L("Gia hạn (tháng)", 10, 84), nud,
            lblInfo,
            Ui.B("Lập phiếu gia hạn", 100, 152, 160, Lap),
            g
        });

        cbHD.SelectedIndexChanged += (s, e) => NapHD();
        cbPhong.SelectedIndexChanged += (s, e) => HienInfo();
        Load += (s, e) =>
        {
            try { Ui.NapHopDong(cbHD); NapHD(); }
            catch (Exception ex) { Ui.Err(ex); }
        };
    }

    void NapHD()
    {
        try
        {
            int hd = Ui.Sel(cbHD);
            if (hd < 0) { g.DataSource = null; return; }

            Ui.Bind(cbPhong, Db.Table("SELECT MaPhong FROM ChiTietHopDong WHERE MaHD=@hd ORDER BY MaPhong",
                Db.P("@hd", hd)), "MaPhong", "MaPhong");
            g.DataSource = Db.Table(
                "SELECT MaPG, MaPhong, NgayLap, SoThangGiaHan FROM PhieuGiaHan WHERE MaHD=@hd ORDER BY MaPG DESC",
                Db.P("@hd", hd));
            HienInfo();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void HienInfo()
    {
        try
        {
            int hd = Ui.Sel(cbHD);
            string p = cbPhong.SelectedValue as string;
            if (hd < 0 || p == null) { lblInfo.Text = ""; return; }

            var dt = Db.Table(
                "SELECT hd.SoThang, ct.SoThangGiaHan FROM HopDong hd JOIN ChiTietHopDong ct ON ct.MaHD = hd.MaHD " +
                "WHERE hd.MaHD=@hd AND ct.MaPhong=@p", Db.P("@hd", hd), Db.P("@p", p));
            if (dt.Rows.Count == 0) return;

            int n = (int)dt.Rows[0][0], k = (int)dt.Rows[0][1];
            lblInfo.Text = $"Thuê {n} tháng, đã gia hạn {k} tháng. Mỗi phiếu gia hạn tối đa {n / 2.0:0.#} tháng; " +
                           "chỉ gia hạn khi đã hết nợ tiền thuê và dịch vụ.";
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void Lap(object sender, EventArgs e)
    {
        try
        {
            int hd = Ui.Sel(cbHD);
            string p = cbPhong.SelectedValue as string;
            if (hd < 0 || p == null) throw new BusinessException("Chọn hợp đồng và phòng.");

            int maPG = RentalService.LapPhieuGiaHan(hd, p, (int)nud.Value);
            Ui.Info($"Đã lập phiếu gia hạn #{maPG}.");
            NapHD();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }
}
