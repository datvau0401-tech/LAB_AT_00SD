using System;
using System.Drawing;
using System.Windows.Forms;

namespace eShopping.Forms
{
    public class FrmMain : Form
    {
        private Label lblTitle, lblXinChao;
        private Button btnSanPham, btnGioHang, btnDangNhap, btnDangKy, btnThoat;

        public FrmMain()
        {
            KhoiTao();
            CapNhatTrangThai();
        }

        private void KhoiTao()
        {
            Text = "e-SHOPPING - Cửa hàng ABC";
            ClientSize = new Size(640, 360);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F);

            lblTitle = new Label
            {
                Name = "lblTitle", Text = "CỬA HÀNG ONLINE e-SHOPPING",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter, Left = 0, Top = 20, Width = 640, Height = 50
            };
            lblXinChao = new Label
            {
                Name = "lblXinChao", TextAlign = ContentAlignment.MiddleCenter, Left = 0, Top = 75, Width = 640, Height = 28
            };
            Controls.Add(lblTitle);
            Controls.Add(lblXinChao);

            btnSanPham = Ui.Nut(this, "btnSanPham", "Xem sản phẩm", 60, 130, 230);
            btnGioHang = Ui.Nut(this, "btnGioHang", "Giỏ hàng", 350, 130, 230);
            btnDangNhap = Ui.Nut(this, "btnDangNhap", "Đăng nhập", 60, 200, 230);
            btnDangKy = Ui.Nut(this, "btnDangKy", "Đăng ký tài khoản", 350, 200, 230);
            btnThoat = Ui.Nut(this, "btnThoat", "Thoát", 205, 270, 230);
            foreach (Button b in new[] { btnSanPham, btnGioHang, btnDangNhap, btnDangKy, btnThoat }) b.Height = 50;

            btnSanPham.Click += btnSanPham_Click;
            btnGioHang.Click += btnGioHang_Click;
            btnDangNhap.Click += btnDangNhap_Click;
            btnDangKy.Click += btnDangKy_Click;
            btnThoat.Click += btnThoat_Click;
        }

        private void CapNhatTrangThai()
        {
            KhachHang kh = PhienLamViec.KhachHangHienTai;
            lblXinChao.Text = kh == null ? "Bạn chưa đăng nhập." : "Xin chào, " + kh.HoTen;
            btnDangNhap.Text = kh == null ? "Đăng nhập" : "Đăng xuất";
            btnDangKy.Enabled = kh == null;
        }

        private void btnSanPham_Click(object sender, EventArgs e)
        {
            using (FrmSanPham f = new FrmSanPham()) f.ShowDialog(this);
        }

        private void btnGioHang_Click(object sender, EventArgs e)
        {
            using (FrmGioHang f = new FrmGioHang()) f.ShowDialog(this);
            CapNhatTrangThai();
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            if (PhienLamViec.KhachHangHienTai != null)
            {
                PhienLamViec.KhachHangHienTai = null;
            }
            else
            {
                using (FrmDangNhap f = new FrmDangNhap()) f.ShowDialog(this);
            }
            CapNhatTrangThai();
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            using (FrmDangKy f = new FrmDangKy()) f.ShowDialog(this);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có thực sự muốn thoát chương trình?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) Close();
        }
    }
}
