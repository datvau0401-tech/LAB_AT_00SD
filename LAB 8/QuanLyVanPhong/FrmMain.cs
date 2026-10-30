namespace QuanLyVanPhong;

public class FrmMain : Form
{
    public FrmMain()
    {
        Ui.Setup(this, "Quản lý cho thuê văn phòng cao ốc", 380, 360);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;

        var menu = new (string Ten, Func<Form> Tao)[]
        {
            ("1. Danh sách phòng",          () => new FrmPhong()),
            ("2. Khách hàng",               () => new FrmKhachHang()),
            ("3. Lập hợp đồng thuê",        () => new FrmHopDong()),
            ("4. Thanh toán thuê / dịch vụ",() => new FrmThanhToan()),
            ("5. Lập phiếu gia hạn",        () => new FrmGiaHan()),
            ("6. Kiểm thử quy tắc",         () => new FrmKiemThu()),
        };

        int y = 20;
        foreach (var (ten, tao) in menu)
        {
            Controls.Add(Ui.B(ten, 50, y, 280, (s, e) => tao().ShowDialog(this)));
            y += 52;
        }
    }
}
