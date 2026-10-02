using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eShopping.Services;

namespace eShopping.Forms
{
    public class FrmGioHang : Form
    {
        private readonly GioHangService gio = new GioHangService();

        private DataGridView dgvGio;
        private NumericUpDown numSoLuong;
        private Label lblTong;
        private Button btnCapNhat, btnXoa, btnTinhTien, btnDong;

        public FrmGioHang()
        {
            KhoiTao();
            Load += FrmGioHang_Load;
        }

        private void KhoiTao()
        {
            Text = "Giỏ hàng";
            ClientSize = new Size(860, 480);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9.5F);

            dgvGio = Ui.Luoi(this, "dgvGio", 15, 15, 830, 330);
            Ui.Nhan(this, "Số lượng:", 15, 360, 70);
            numSoLuong = new NumericUpDown { Name = "numSoLuong", Left = 90, Top = 360, Width = 70, Minimum = 1, Maximum = GioHangService.ToiDaMoiSanPham, Value = 1 };
            Controls.Add(numSoLuong);
            btnCapNhat = Ui.Nut(this, "btnCapNhat", "Cập nhật số lượng", 175, 356, 160);
            btnXoa = Ui.Nut(this, "btnXoa", "Xóa khỏi giỏ", 345, 356, 130);
            lblTong = new Label
            {
                Name = "lblTong", Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Left = 15, Top = 405, Width = 500, Height = 30
            };
            Controls.Add(lblTong);
            btnTinhTien = Ui.Nut(this, "btnTinhTien", "Tính tiền / Đặt hàng", 560, 420, 170);
            btnDong = Ui.Nut(this, "btnDong", "Đóng", 745, 420, 100);

            dgvGio.SelectionChanged += dgvGio_SelectionChanged;
            btnCapNhat.Click += btnCapNhat_Click;
            btnXoa.Click += btnXoa_Click;
            btnTinhTien.Click += btnTinhTien_Click;
            btnDong.Click += btnDong_Click;
        }

        private void FrmGioHang_Load(object sender, EventArgs e) { TaiGio(); }

        private void TaiGio()
        {
            DataTable t = new DataTable();
            t.Columns.Add("MaSanPham");
            t.Columns.Add("TenSanPham");
            t.Columns.Add("DonGia");
            t.Columns.Add("SoLuong", typeof(int));
            t.Columns.Add("ThanhTien");
            foreach (MucGioHang m in PhienLamViec.GioHang.Muc)
                t.Rows.Add(m.MaSanPham, m.TenSanPham, Ui.Tien(m.DonGia), m.SoLuong, Ui.Tien(m.ThanhTien));

            dgvGio.DataSource = t;
            dgvGio.Columns["MaSanPham"].HeaderText = "Mã";
            dgvGio.Columns["TenSanPham"].HeaderText = "Tên sản phẩm";
            dgvGio.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvGio.Columns["SoLuong"].HeaderText = "Số lượng";
            dgvGio.Columns["ThanhTien"].HeaderText = "Thành tiền";

            lblTong.Text = "Tổng tiền hàng: " + Ui.Tien(PhienLamViec.GioHang.TongTienHang);
            bool co = !PhienLamViec.GioHang.Rong;
            btnCapNhat.Enabled = co;
            btnXoa.Enabled = co;
            btnTinhTien.Enabled = co;
        }

        private string MaDangChon()
        {
            if (dgvGio.CurrentRow == null) return null;
            return Convert.ToString(dgvGio.CurrentRow.Cells["MaSanPham"].Value);
        }

        private void dgvGio_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvGio.CurrentRow == null) return;
            int sl = Convert.ToInt32(dgvGio.CurrentRow.Cells["SoLuong"].Value);
            numSoLuong.Value = Math.Max(numSoLuong.Minimum, Math.Min(numSoLuong.Maximum, sl));
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            string ma = MaDangChon();
            if (string.IsNullOrEmpty(ma)) return;
            KetQuaXuLy kq = gio.CapNhatSoLuong(ma, (int)numSoLuong.Value);
            if (!kq.ThanhCong) Ui.HienKetQua(kq);
            TaiGio();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            string ma = MaDangChon();
            if (string.IsNullOrEmpty(ma)) return;
            if (MessageBox.Show("Bỏ sản phẩm đang chọn khỏi giỏ hàng?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            gio.Xoa(ma);
            TaiGio();
        }

        private void btnTinhTien_Click(object sender, EventArgs e)
        {
            if (PhienLamViec.GioHang.Rong) { MessageBox.Show("Giỏ hàng đang trống."); return; }

            // Yêu cầu đăng nhập (hoặc đăng ký mới) trước khi đặt hàng
            if (PhienLamViec.KhachHangHienTai == null)
            {
                MessageBox.Show("Vui lòng đăng nhập hoặc đăng ký tài khoản để đặt hàng.", "Yêu cầu đăng nhập");
                using (FrmDangNhap f = new FrmDangNhap())
                    if (f.ShowDialog(this) != DialogResult.OK) return;
            }

            using (FrmThanhToan f = new FrmThanhToan())
            {
                DialogResult kq = f.ShowDialog(this);
                TaiGio();
                if (kq == DialogResult.OK) Close();
            }
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
