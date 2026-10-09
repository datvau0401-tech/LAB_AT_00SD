using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmDangKyLe
    {
        private TextBox txtSo, txtTen, txtDT;
        private ComboBox cboChuyen, cboDiemBan;
        private NumericUpDown numNguoi;
        private Label lblThanhTien;
        private Button btnDangKy, btnDong;
        private DataGridView dgv;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Đăng ký khách lẻ theo chuyến", 1000, 580);
            FormHelper.Lb(this, "Số đăng ký:", 15, 15); txtSo = FormHelper.Tx(this, 100, 15, 140);
            FormHelper.Lb(this, "Chuyến:", 290, 15); cboChuyen = FormHelper.Cb(this, 345, 15, 420);
            FormHelper.Lb(this, "Điểm bán vé:", 15, 55); cboDiemBan = FormHelper.Cb(this, 100, 55, 200);
            FormHelper.Lb(this, "Người đăng ký:", 340, 55); txtTen = FormHelper.Tx(this, 430, 55, 220);
            FormHelper.Lb(this, "Điện thoại:", 690, 55); txtDT = FormHelper.Tx(this, 760, 55, 140);
            FormHelper.Lb(this, "Số người:", 15, 95); numNguoi = FormHelper.Nu(this, 100, 95, 70, 1, 11, 1);
            FormHelper.Lb(this, "Thành tiền:", 220, 95);
            lblThanhTien = new Label { Left = 300, Top = 99, AutoSize = true, Text = "0 đ", ForeColor = Color.DarkRed, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Controls.Add(lblThanhTien);
            btnDangKy = FormHelper.Bt(this, "Đăng ký và thanh toán vé", 690, 93, 210);
            dgv = FormHelper.Dg(this, 15, 140, 965, 380);
            btnDong = FormHelper.Bt(this, "Đóng", 880, 535, 100);

            cboChuyen.SelectedIndexChanged += TinhTien;
            numNguoi.ValueChanged += TinhTien;
            btnDangKy.Click += btnDangKy_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmDangKyLe_Load;
            ResumeLayout(false);
        }
    }
}
