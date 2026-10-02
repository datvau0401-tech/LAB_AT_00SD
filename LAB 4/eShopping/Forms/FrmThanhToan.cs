using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eShopping.Services;

namespace eShopping.Forms
{
    public class FrmThanhToan : Form
    {
        private readonly DatHangService service = new DatHangService();

        private DataGridView dgvTomTat;
        private GroupBox grpGiao, grpThe;
        private ComboBox cboLoaiGiaoHang, cboKhuVuc, cboLoaiThe;
        private CheckBox chkNguoiNhanLaToi;
        private TextBox txtNNHoTen, txtNNDiaChi, txtNNDienThoai, txtSoThe, txtCSV, txtChuThe;
        private DateTimePicker dtHetHan;
        private Label lblTienHang, lblPhiGiao, lblLePhi, lblTong;
        private Button btnXacNhan, btnHuy;
        private bool daTai;

        public FrmThanhToan()
        {
            KhoiTao();
            Load += FrmThanhToan_Load;
        }

        private void KhoiTao()
        {
            Text = "Đặt hàng và thanh toán";
            ClientSize = new Size(900, 630);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            dgvTomTat = Ui.Luoi(this, "dgvTomTat", 15, 15, 870, 140);

            // ---- Giao hàng ----
            grpGiao = new GroupBox { Name = "grpGiao", Text = "Giao hàng", Left = 15, Top = 170, Width = 430, Height = 290 };
            Controls.Add(grpGiao);
            Ui.Nhan(grpGiao, "Loại phiếu đặt hàng:", 15, 25, 130);
            cboLoaiGiaoHang = Ui.Combo(grpGiao, "cboLoaiGiaoHang", 150, 25, 265);
            Ui.Nhan(grpGiao, "Khu vực giao hàng:", 15, 65, 130);
            cboKhuVuc = Ui.Combo(grpGiao, "cboKhuVuc", 150, 65, 265);
            chkNguoiNhanLaToi = new CheckBox { Name = "chkNguoiNhanLaToi", Text = "Người nhận là chính tôi", Left = 15, Top = 105, Width = 300 };
            grpGiao.Controls.Add(chkNguoiNhanLaToi);
            Ui.Nhan(grpGiao, "Họ tên người nhận:", 15, 145, 130);
            txtNNHoTen = Ui.O(grpGiao, "txtNNHoTen", 150, 145, 265);
            Ui.Nhan(grpGiao, "Địa chỉ:", 15, 185, 130);
            txtNNDiaChi = Ui.O(grpGiao, "txtNNDiaChi", 150, 185, 265);
            Ui.Nhan(grpGiao, "Điện thoại:", 15, 225, 130);
            txtNNDienThoai = Ui.O(grpGiao, "txtNNDienThoai", 150, 225, 265);

            // ---- Thẻ tín dụng ----
            grpThe = new GroupBox { Name = "grpThe", Text = "Thanh toán bằng thẻ tín dụng", Left = 460, Top = 170, Width = 425, Height = 290 };
            Controls.Add(grpThe);
            Ui.Nhan(grpThe, "Loại thẻ:", 15, 25, 125);
            cboLoaiThe = Ui.Combo(grpThe, "cboLoaiThe", 145, 25, 265);
            Ui.Nhan(grpThe, "Số thẻ:", 15, 65, 125);
            txtSoThe = Ui.O(grpThe, "txtSoThe", 145, 65, 265);
            Ui.Nhan(grpThe, "Mã an ninh (CSV):", 15, 105, 125);
            txtCSV = Ui.O(grpThe, "txtCSV", 145, 105, 100);
            txtCSV.UseSystemPasswordChar = true;
            Ui.Nhan(grpThe, "Hết hạn (MM/yyyy):", 15, 145, 125);
            dtHetHan = new DateTimePicker
            {
                Name = "dtHetHan", Left = 145, Top = 145, Width = 120,
                Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true,
                Value = DateTime.Today.AddYears(2)
            };
            grpThe.Controls.Add(dtHetHan);
            Ui.Nhan(grpThe, "Họ tên chủ thẻ:", 15, 185, 125);
            txtChuThe = Ui.O(grpThe, "txtChuThe", 145, 185, 265);
            Label ghiChu = new Label
            {
                Text = "Hệ thống chỉ lưu 4 số cuối của thẻ; số thẻ đầy đủ và CSV không được lưu.",
                Left = 15, Top = 230, Width = 395, Height = 45, ForeColor = Color.DimGray
            };
            grpThe.Controls.Add(ghiChu);

            // ---- Tổng kết ----
            lblTienHang = new Label { Name = "lblTienHang", Left = 15, Top = 475, Width = 600, Height = 26 };
            lblPhiGiao = new Label { Name = "lblPhiGiao", Left = 15, Top = 503, Width = 600, Height = 26 };
            lblLePhi = new Label { Name = "lblLePhi", Left = 15, Top = 531, Width = 600, Height = 26 };
            lblTong = new Label { Name = "lblTong", Left = 15, Top = 562, Width = 600, Height = 30, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            Controls.Add(lblTienHang);
            Controls.Add(lblPhiGiao);
            Controls.Add(lblLePhi);
            Controls.Add(lblTong);

            btnXacNhan = Ui.Nut(this, "btnXacNhan", "Xác nhận đặt hàng", 640, 575, 160);
            btnHuy = Ui.Nut(this, "btnHuy", "Hủy", 810, 575, 75);
            CancelButton = btnHuy;

            cboLoaiGiaoHang.SelectedIndexChanged += CapNhatTien_Event;
            cboKhuVuc.SelectedIndexChanged += CapNhatTien_Event;
            cboLoaiThe.SelectedIndexChanged += CapNhatTien_Event;
            chkNguoiNhanLaToi.CheckedChanged += chkNguoiNhanLaToi_CheckedChanged;
            btnXacNhan.Click += btnXacNhan_Click;
            btnHuy.Click += btnHuy_Click;
        }

        private void FrmThanhToan_Load(object sender, EventArgs e)
        {
            cboLoaiGiaoHang.DisplayMember = "HienThi";
            cboLoaiGiaoHang.ValueMember = "MaLoaiGiaoHang";
            cboLoaiGiaoHang.DataSource = service.LayLoaiGiaoHang();

            cboKhuVuc.DisplayMember = "TenKhuVuc";
            cboKhuVuc.ValueMember = "MaKhuVuc";
            cboKhuVuc.DataSource = service.LayKhuVuc();

            cboLoaiThe.DisplayMember = "TenLoaiThe";
            cboLoaiThe.ValueMember = "MaLoaiThe";
            cboLoaiThe.DataSource = service.LayLoaiThe();

            TaiTomTat();
            daTai = true;
            CapNhatTien();
        }

        private void TaiTomTat()
        {
            DataTable t = new DataTable();
            t.Columns.Add("TenSanPham");
            t.Columns.Add("DonGia");
            t.Columns.Add("SoLuong", typeof(int));
            t.Columns.Add("ThanhTien");
            foreach (MucGioHang m in PhienLamViec.GioHang.Muc)
                t.Rows.Add(m.TenSanPham, Ui.Tien(m.DonGia), m.SoLuong, Ui.Tien(m.ThanhTien));
            dgvTomTat.DataSource = t;
            dgvTomTat.Columns["TenSanPham"].HeaderText = "Sản phẩm";
            dgvTomTat.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvTomTat.Columns["SoLuong"].HeaderText = "Số lượng";
            dgvTomTat.Columns["ThanhTien"].HeaderText = "Thành tiền";
            btnXacNhan.Enabled = !PhienLamViec.GioHang.Rong;
        }

        private static string Gia(ComboBox c)
        {
            return c.SelectedValue == null ? null : Convert.ToString(c.SelectedValue);
        }

        private void CapNhatTien_Event(object sender, EventArgs e) { CapNhatTien(); }

        private void CapNhatTien()
        {
            if (!daTai) return;
            KetQuaTinhTien t = service.TinhTien(PhienLamViec.GioHang.TongTienHang, Gia(cboKhuVuc), Gia(cboLoaiGiaoHang), Gia(cboLoaiThe));
            lblTienHang.Text = "Tiền hàng: " + Ui.Tien(t.TongTienHang);
            lblPhiGiao.Text = "Phí giao hàng: " + (t.ThieuBangGia ? "(chưa có bảng giá)" : t.MienPhiGiaoHang ? "Miễn phí" : Ui.Tien(t.PhiGiaoHang));
            lblLePhi.Text = "Lệ phí thanh toán thẻ: " + Ui.Tien(t.LePhiThe);
            lblTong.Text = "TỔNG TRỊ GIÁ: " + Ui.Tien(t.TongTriGia);
        }

        // Người nhận có thể khác người mua: chỉ điền sẵn khi khách tích "là chính tôi".
        private void chkNguoiNhanLaToi_CheckedChanged(object sender, EventArgs e)
        {
            KhachHang kh = PhienLamViec.KhachHangHienTai;
            if (chkNguoiNhanLaToi.Checked && kh != null)
            {
                txtNNHoTen.Text = kh.HoTen;
                txtNNDiaChi.Text = kh.DiaChi;
                txtNNDienThoai.Text = kh.DienThoai;
            }
            else
            {
                txtNNHoTen.Clear();
                txtNNDiaChi.Clear();
                txtNNDienThoai.Clear();
            }
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            NguoiNhan nn = new NguoiNhan
            {
                HoTen = txtNNHoTen.Text.Trim(),
                DiaChi = txtNNDiaChi.Text.Trim(),
                DienThoai = txtNNDienThoai.Text.Trim(),
                MaKhuVuc = Gia(cboKhuVuc)
            };

            // Thẻ hết hạn vào cuối tháng đã chọn
            DateTime han = new DateTime(dtHetHan.Value.Year, dtHetHan.Value.Month, 1).AddMonths(1).AddDays(-1);
            TheTinDung the = new TheTinDung
            {
                MaLoaiThe = Gia(cboLoaiThe),
                SoThe = txtSoThe.Text,
                CSV = txtCSV.Text,
                NgayHetHan = han,
                TenChuThe = txtChuThe.Text.Trim()
            };

            KetQuaXuLy kq = service.DatHang(PhienLamViec.KhachHangHienTai, PhienLamViec.GioHang, nn, Gia(cboLoaiGiaoHang), the);
            txtCSV.Clear(); // không giữ mã an ninh thẻ trên màn hình sau mỗi lần thử
            Ui.HienKetQua(kq);

            if (kq.ThanhCong)
            {
                PhienLamViec.GioHang.Muc.Clear();
                txtSoThe.Clear();
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                // Giỏ hàng có thể đã được đối chiếu lại (đổi giá / hết hàng)
                TaiTomTat();
                CapNhatTien();
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
