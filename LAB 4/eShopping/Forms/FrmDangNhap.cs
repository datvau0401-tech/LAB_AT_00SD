using System;
using System.Drawing;
using System.Windows.Forms;
using eShopping.Services;

namespace eShopping.Forms
{
    public class FrmDangNhap : Form
    {
        private readonly KhachHangService service = new KhachHangService();

        private TextBox txtTenDangNhap, txtMatKhau;
        private Button btnDangNhap, btnDangKy, btnHuy;

        public FrmDangNhap()
        {
            KhoiTao();
        }

        private void KhoiTao()
        {
            Text = "Đăng nhập";
            ClientSize = new Size(400, 200);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            Ui.Nhan(this, "Tên đăng nhập:", 20, 20, 110);
            txtTenDangNhap = Ui.O(this, "txtTenDangNhap", 140, 20, 230);
            Ui.Nhan(this, "Mật khẩu:", 20, 65, 110);
            txtMatKhau = Ui.O(this, "txtMatKhau", 140, 65, 230);
            txtMatKhau.UseSystemPasswordChar = true;

            btnDangNhap = Ui.Nut(this, "btnDangNhap", "Đăng nhập", 20, 125, 110);
            btnDangKy = Ui.Nut(this, "btnDangKy", "Đăng ký mới", 145, 125, 110);
            btnHuy = Ui.Nut(this, "btnHuy", "Hủy", 270, 125, 100);

            AcceptButton = btnDangNhap;
            CancelButton = btnHuy;

            btnDangNhap.Click += btnDangNhap_Click;
            btnDangKy.Click += btnDangKy_Click;
            btnHuy.Click += btnHuy_Click;
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            KetQuaXuLy kq = service.DangNhap(txtTenDangNhap.Text, txtMatKhau.Text);
            if (!kq.ThanhCong)
            {
                Ui.HienKetQua(kq);
                txtMatKhau.Clear();
                txtMatKhau.Focus();
                return;
            }
            PhienLamViec.KhachHangHienTai = (KhachHang)kq.DuLieu;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            using (FrmDangKy f = new FrmDangKy())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    txtTenDangNhap.Text = f.TenDangNhapMoi;
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                }
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
