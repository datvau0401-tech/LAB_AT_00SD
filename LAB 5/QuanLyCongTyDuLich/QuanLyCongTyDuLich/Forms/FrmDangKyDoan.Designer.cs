using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmDangKyDoan
    {
        private GroupBox grpDoan, grpDangKy;
        private TextBox txtMaDoan, txtTenCQ, txtDiaChi, txtDT, txtDaiDien, txtSo, txtDon;
        private ComboBox cboTour;
        private DateTimePicker dtDi;
        private NumericUpDown numNguoi, numCoc;
        private CheckBox chkBH;
        private Label lblKetThuc, lblTong;
        private DataGridView dgvThanhVien, dgv;
        private Button btnDangKy, btnHuy, btnDong;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Phiếu đăng ký theo đoàn", 1080, 720);

            grpDoan = new GroupBox { Text = "Thông tin đoàn khách", Left = 10, Top = 5, Width = 440, Height = 185 };
            Controls.Add(grpDoan);
            FormHelper.Lb(grpDoan, "Mã đoàn:", 10, 22); txtMaDoan = FormHelper.Tx(grpDoan, 115, 22, 150);
            FormHelper.Lb(grpDoan, "Cơ quan / gia đình:", 10, 55); txtTenCQ = FormHelper.Tx(grpDoan, 115, 55, 310);
            FormHelper.Lb(grpDoan, "Địa chỉ:", 10, 88); txtDiaChi = FormHelper.Tx(grpDoan, 115, 88, 310);
            FormHelper.Lb(grpDoan, "Điện thoại:", 10, 121); txtDT = FormHelper.Tx(grpDoan, 115, 121, 150);
            FormHelper.Lb(grpDoan, "Người đại diện:", 10, 154); txtDaiDien = FormHelper.Tx(grpDoan, 115, 154, 310);

            grpDangKy = new GroupBox { Text = "Đăng ký tour", Left = 460, Top = 5, Width = 610, Height = 185 };
            Controls.Add(grpDangKy);
            FormHelper.Lb(grpDangKy, "Số phiếu:", 10, 22); txtSo = FormHelper.Tx(grpDangKy, 80, 22, 120);
            FormHelper.Lb(grpDangKy, "Tour:", 230, 22); cboTour = FormHelper.Cb(grpDangKy, 270, 22, 320);
            FormHelper.Lb(grpDangKy, "Ngày đi:", 10, 55); dtDi = FormHelper.Dt(grpDangKy, 80, 55, 120);
            FormHelper.Lb(grpDangKy, "Số người:", 230, 55); numNguoi = FormHelper.Nu(grpDangKy, 295, 55, 80, 13, 1000, 13);
            FormHelper.Lb(grpDangKy, "Địa điểm đón:", 10, 88); txtDon = FormHelper.Tx(grpDangKy, 100, 88, 490);
            FormHelper.Lb(grpDangKy, "Tiền cọc:", 10, 121); numCoc = FormHelper.Nu(grpDangKy, 80, 121, 150, 0, 100000000000m, 0);
            chkBH = FormHelper.Ck(grpDangKy, "Mua bảo hiểm", 260, 123);
            FormHelper.Lb(grpDangKy, "Kết thúc DK:", 10, 154);
            lblKetThuc = new Label { Left = 90, Top = 158, AutoSize = true, Text = "-", Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            grpDangKy.Controls.Add(lblKetThuc);
            FormHelper.Lb(grpDangKy, "Tổng dự kiến:", 260, 154);
            lblTong = new Label { Left = 345, Top = 158, AutoSize = true, Text = "0 đ", ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            grpDangKy.Controls.Add(lblTong);

            FormHelper.Lb(this, "Danh sách người cùng đi (bắt buộc đủ số người khi mua bảo hiểm):", 10, 198);
            dgvThanhVien = FormHelper.Dg(this, 10, 225, 1060, 170, false);
            btnDangKy = FormHelper.Bt(this, "Lập phiếu đăng ký", 760, 402, 160);
            btnHuy = FormHelper.Bt(this, "Hủy phiếu (mất cọc)", 930, 402, 140);
            FormHelper.Lb(this, "Các phiếu đăng ký đoàn:", 10, 410);
            dgv = FormHelper.Dg(this, 10, 440, 1060, 230);
            btnDong = FormHelper.Bt(this, "Đóng", 970, 680, 100);

            cboTour.SelectedIndexChanged += TinhTong;
            dtDi.ValueChanged += TinhTong;
            numNguoi.ValueChanged += TinhTong;
            chkBH.CheckedChanged += chkBH_CheckedChanged;
            btnDangKy.Click += btnDangKy_Click;
            btnHuy.Click += btnHuy_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmDangKyDoan_Load;
            ResumeLayout(false);
        }
    }
}
