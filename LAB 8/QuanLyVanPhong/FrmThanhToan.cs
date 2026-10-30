namespace QuanLyVanPhong;

public class FrmThanhToan : Form
{
    ComboBox cbHD = Ui.Combo(80, 10, 320);

    // Tab 1: tiền thuê
    DataGridView gThue = Ui.Grid(5, 5, 740, 360);

    // Tab 2: dịch vụ
    ComboBox cbPhong = Ui.Combo(60, 10, 110);
    ComboBox cbDV = Ui.Combo(250, 10, 110);
    DateTimePicker dtpThang = new DateTimePicker
    {
        Left = 430, Top = 10, Width = 100, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy"
    };
    TextBox txtTien = Ui.T(60, 45, 110);
    DataGridView gDV = Ui.Grid(5, 80, 740, 280);

    public FrmThanhToan()
    {
        Ui.Setup(this, "Thanh toán tiền thuê & dịch vụ", 780, 520);

        var tab = new TabControl { Left = 10, Top = 45, Width = 760, Height = 465 };
        var p1 = new TabPage("Tiền thuê (tháng 1-6 trả trước, 7+ đầu tháng)");
        var p2 = new TabPage("Dịch vụ (thanh toán cuối tháng)");

        p1.Controls.AddRange(new Control[]
        {
            gThue,
            Ui.B("Thu tiền khoản đang chọn", 5, 372, 200, ThuThue)
        });
        p2.Controls.AddRange(new Control[]
        {
            Ui.L("Phòng", 5, 13), cbPhong,
            Ui.L("Dịch vụ", 190, 13), cbDV,
            Ui.L("Tháng", 380, 13), dtpThang,
            Ui.L("Số tiền", 5, 48), txtTien,
            Ui.B("Lập hóa đơn dịch vụ", 190, 43, 150, LapDV),
            Ui.B("Thu hóa đơn đang chọn", 350, 43, 170, ThuDV),
            gDV
        });
        tab.TabPages.Add(p1);
        tab.TabPages.Add(p2);

        Controls.AddRange(new Control[] { Ui.L("Hợp đồng", 10, 14), cbHD, tab });

        cbHD.SelectedIndexChanged += (s, e) => NapHD();
        Load += (s, e) =>
        {
            try
            {
                Ui.Bind(cbDV, Db.Table("SELECT MaDV, TenDV FROM DichVu ORDER BY MaDV"), "MaDV", "TenDV");
                Ui.NapHopDong(cbHD);
                NapHD();
            }
            catch (Exception ex) { Ui.Err(ex); }
        };
    }

    void NapHD()
    {
        try
        {
            int hd = Ui.Sel(cbHD);
            if (hd < 0) { gThue.DataSource = null; gDV.DataSource = null; return; }

            gThue.DataSource = Db.Table(
                "SELECT MaTT, MaPhong, ThangThu, SoTien, HanThu, DaThu, NgayThu FROM ThanhToanThue " +
                "WHERE MaHD=@hd ORDER BY MaPhong, ThangThu", Db.P("@hd", hd));

            Ui.Bind(cbPhong, Db.Table("SELECT MaPhong FROM ChiTietHopDong WHERE MaHD=@hd ORDER BY MaPhong",
                Db.P("@hd", hd)), "MaPhong", "MaPhong");

            gDV.DataSource = Db.Table(
                "SELECT h.MaHDDV, h.MaPhong, v.TenDV, h.Thang, h.SoTien, h.HanThu, h.DaThu " +
                "FROM HoaDonDichVu h JOIN DichVu v ON v.MaDV = h.MaDV " +
                "WHERE h.MaHD=@hd ORDER BY h.Thang, h.MaPhong", Db.P("@hd", hd));
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void ThuThue(object sender, EventArgs e)
    {
        try
        {
            int id = Ui.SelId(gThue, "MaTT");
            if (id < 0) throw new BusinessException("Chọn một dòng cần thu.");
            RentalService.ThuTienThue(id);
            NapHD();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void LapDV(object sender, EventArgs e)
    {
        try
        {
            int hd = Ui.Sel(cbHD);
            string phong = cbPhong.SelectedValue as string;
            int dv = Ui.Sel(cbDV);
            if (hd < 0 || phong == null || dv < 0) throw new BusinessException("Chọn hợp đồng, phòng và dịch vụ.");
            if (!decimal.TryParse(txtTien.Text, out var tien)) throw new BusinessException("Số tiền không hợp lệ.");

            RentalService.LapHoaDonDichVu(hd, phong, dv, dtpThang.Value, tien);
            txtTien.Clear();
            NapHD();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void ThuDV(object sender, EventArgs e)
    {
        try
        {
            int id = Ui.SelId(gDV, "MaHDDV");
            if (id < 0) throw new BusinessException("Chọn một hóa đơn cần thu.");
            RentalService.ThuDichVu(id);
            NapHD();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }
}
