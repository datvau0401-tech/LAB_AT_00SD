using System;
using System.Drawing;
using System.Windows.Forms;
using eShopping.Services;

namespace eShopping.Forms
{
    public class FrmDangKy : Form
    {
        private readonly KhachHangService service = new KhachHangService();

        private TextBox txtHoTen, txtSoGiayTo, txtDiaChi, txtDienThoai, txtEmail, txtTenDangNhap, txtMatKhau, txtNhapLai;
        private DateTimePicker dtNgaySinh;
        private Button btnDangKy, btnDong;

        // Form đăng nhập dùng để điền sẵn tên đăng nhập sau khi đăng ký thành công
        public string TenDangNhapMoi { get; private set; }

        public FrmDangKy()
        {
            KhoiTao();
        }

        private void KhoiTao()
        {
            Text = "Đăng ký tài khoản khách hàng";
            ClientSize = new Size(500, 470);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            int y = 15;
            Ui.Nhan(this, "Họ tên (*):", 15, y, 150);
            txtHoTen = Ui.O(this, "txtHoTen", 175, y, 295); y += 40;

            Ui.Nhan(this, "Ngày sinh (*):", 15, y, 150);
            dtNgaySinh = new DateTimePicker
            {
                Name = "dtNgaySinh", Left = 175, Top = y, Width = 160, Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today, Value = DateTime.Today.AddYears(-20)
            };
            Controls.Add(dtNgaySinh); y += 40;

            Ui.Nhan(this, "Số CMND/Passport (*):", 15, y, 155);
            txtSoGiayTo = Ui.O(this, "txtSoGiayTo", 175, y, 295); y += 40;

            Ui.Nhan(this, "Địa chỉ (*):", 15, y, 150);
            txtDiaChi = Ui.O(this, "txtDiaChi", 175, y, 295); y += 40;

            Ui.Nhan(this, "Điện thoại (*):", 15, y, 150);
            txtDienThoai = Ui.O(this, "txtDienThoai", 175, y, 295); y += 40;

            Ui.Nhan(this, "Email (tùy chọn):", 15, y, 150);
            txtEmail = Ui.O(this, "txtEmail", 175, y, 295); y += 40;

            Ui.Nhan(this, "Tên đăng nhập (*):", 15, y, 150);
            txtTenDangNhap = Ui.O(this, "txtTenDangNhap", 175, y, 295); y += 40;

            Ui.Nhan(this, "Mật khẩu (*):", 15, y, 150);
            txtMatKhau = Ui.O(this, "txtMatKhau", 175, y, 295);
            txtMatKhau.UseSystemPasswordChar = true; y += 40;

            Ui.Nhan(this, "Nhập lại mật khẩu (*):", 15, y, 155);
            txtNhapLai = Ui.O(this, "txtNhapLai", 175, y, 295);
            txtNhapLai.UseSystemPasswordChar = true; y += 50;

            btnDangKy = Ui.Nut(this, "btnDangKy", "Đăng ký", 175, y, 120);
            btnDong = Ui.Nut(this, "btnDong", "Đóng", 310, y, 100);

            AcceptButton = btnDangKy;
            CancelButton = btnDong;

            btnDangKy.Click += btnDangKy_Click;
            btnDong.Click += btnDong_Click;
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            if (txtMatKhau.Text != txtNhapLai.Text)
            {
                MessageBox.Show("Mật khẩu nhập lại không khớp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            KhachHang kh = new KhachHang
            {
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dtNgaySinh.Value.Date,
                SoGiayTo = txtSoGiayTo.Text.Trim(),
                DiaChi = txtDiaChi.Text.Trim(),
                DienThoai = txtDienThoai.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                TenDangNhap = txtTenDangNhap.Text.Trim()
            };

            KetQuaXuLy kq = service.DangKy(kh, txtMatKhau.Text);
            Ui.HienKetQua(kq);
            if (!kq.ThanhCong) return;

            TenDangNhapMoi = kh.TenDangNhap;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
