using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmPhanCongHDV
    {
        private TextBox txtMaPC;
        private ComboBox cboHDV, cboLoai, cboDoiTuong;
        private NumericUpDown numThuLao;
        private Button btnPhanCong, btnDong;
        private DataGridView dgv;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Phân công hướng dẫn viên", 980, 540);
            FormHelper.Lb(this, "Mã phân công:", 15, 15); txtMaPC = FormHelper.Tx(this, 105, 15, 140);
            FormHelper.Lb(this, "Hướng dẫn viên:", 300, 15); cboHDV = FormHelper.Cb(this, 400, 15, 280);
            FormHelper.Lb(this, "Loại:", 15, 55);
            cboLoai = FormHelper.Cb(this, 105, 55, 100);
            FormHelper.Lb(this, "Chuyến / đoàn:", 300, 55); cboDoiTuong = FormHelper.Cb(this, 400, 55, 470);
            FormHelper.Lb(this, "Thù lao tour:", 15, 95); numThuLao = FormHelper.Nu(this, 105, 95, 140, 0, 1000000000m, 0);
            FormHelper.Lb(this, "Ngày bắt đầu / kết thúc lấy theo chuyến hoặc phiếu đoàn.", 300, 95);
            btnPhanCong = FormHelper.Bt(this, "Phân công", 780, 93, 100);
            dgv = FormHelper.Dg(this, 15, 135, 945, 340);
            btnDong = FormHelper.Bt(this, "Đóng", 860, 495, 100);

            cboLoai.SelectedIndexChanged += cboLoai_SelectedIndexChanged;
            btnPhanCong.Click += btnPhanCong_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmPhanCongHDV_Load;
            ResumeLayout(false);
        }
    }
}
