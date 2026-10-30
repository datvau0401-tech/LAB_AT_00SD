namespace QuanLyVanPhong;

public class FrmKhachHang : Form
{
    TextBox txtTen = Ui.T(110, 15, 280);
    TextBox txtSdt = Ui.T(110, 47, 280);
    TextBox txtDc = Ui.T(110, 79, 280);
    DataGridView g = Ui.Grid(10, 150, 580, 250);

    public FrmKhachHang()
    {
        Ui.Setup(this, "Khách hàng", 600, 410);
        Controls.AddRange(new Control[]
        {
            Ui.L("Tên khách hàng", 10, 18), txtTen,
            Ui.L("Điện thoại", 10, 50), txtSdt,
            Ui.L("Địa chỉ", 10, 82), txtDc,
            Ui.B("Thêm khách hàng", 110, 112, 140, Them),
            g
        });
        Load += (s, e) => Nap();
    }

    void Nap()
    {
        try
        {
            g.DataSource = Db.Table(
                "SELECT MaKH, TenKH, DienThoai, DiaChi FROM KhachHang WHERE DienThoai NOT LIKE 'TEST%' ORDER BY MaKH");
        }
        catch (Exception ex) { Ui.Err(ex); }
    }

    void Them(object sender, EventArgs e)
    {
        try
        {
            if (txtTen.Text.Trim() == "" || txtSdt.Text.Trim() == "")
                throw new BusinessException("Nhập tên và số điện thoại.");

            // Cột DienThoai có ràng buộc UNIQUE -> trùng số sẽ báo lỗi
            Db.Exec("INSERT INTO KhachHang(TenKH, DienThoai, DiaChi) VALUES(@t, @s, @d)",
                Db.P("@t", txtTen.Text.Trim()), Db.P("@s", txtSdt.Text.Trim()), Db.P("@d", txtDc.Text.Trim()));
            txtTen.Clear(); txtSdt.Clear(); txtDc.Clear();
            Nap();
        }
        catch (Exception ex) { Ui.Err(ex); }
    }
}
