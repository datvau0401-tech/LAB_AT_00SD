using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmChuyenLe
    {
        private TextBox txtMa, txtDon;
        private ComboBox cboTour;
        private DateTimePicker dtDi;
        private Label lblNgayVe;
        private Button btnThem, btnDongDK, btnDong;
        private DataGridView dgv;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Lịch chuyến khách lẻ", 960, 560);
            FormHelper.Lb(this, "Mã chuyến:", 15, 15); txtMa = FormHelper.Tx(this, 95, 15, 140);
            FormHelper.Lb(this, "Tour:", 300, 15); cboTour = FormHelper.Cb(this, 345, 15, 330);
            FormHelper.Lb(this, "Ngày đi:", 15, 55); dtDi = FormHelper.Dt(this, 95, 55, 140);
            FormHelper.Lb(this, "Ngày về:", 300, 55);
            lblNgayVe = new Label { Left = 360, Top = 59, AutoSize = true, Text = "-", Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Controls.Add(lblNgayVe);
            FormHelper.Lb(this, "Địa điểm đón:", 15, 95); txtDon = FormHelper.Tx(this, 95, 95, 580);
            btnThem = FormHelper.Bt(this, "Tạo chuyến", 720, 93, 100);
            btnDongDK = FormHelper.Bt(this, "Đóng đăng ký", 830, 93, 110);
            dgv = FormHelper.Dg(this, 15, 135, 925, 360);
            btnDong = FormHelper.Bt(this, "Đóng", 840, 515, 100);

            cboTour.SelectedIndexChanged += TinhNgayVe;
            dtDi.ValueChanged += TinhNgayVe;
            btnThem.Click += btnThem_Click;
            btnDongDK.Click += btnDongDK_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmChuyenLe_Load;
            ResumeLayout(false);
        }
    }
}
