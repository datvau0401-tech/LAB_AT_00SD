namespace QuanLyVanPhong;

public class FrmPhong : Form
{
    DataGridView g = Ui.Grid(10, 10, 680, 380);

    public FrmPhong()
    {
        Ui.Setup(this, "Danh sách phòng", 700, 400);
        Controls.Add(g);
        Load += (s, e) =>
        {
            try
            {
                g.DataSource = Db.Table(
                    "SELECT p.MaPhong, t.TenTang, p.DienTich, p.Huong, p.GiaThue, p.TrangThai " +
                    "FROM Phong p JOIN Tang t ON t.MaTang = p.MaTang " +
                    "WHERE p.MaPhong NOT LIKE 'TST%' ORDER BY p.MaPhong");
            }
            catch (Exception ex) { Ui.Err(ex); }
        };
    }
}
