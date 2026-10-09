using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmLuongThongKe
    {
        private TabControl tabTK;
        private NumericUpDown numThang, numNam;
        private DateTimePicker dtTu, dtDen;
        private Button btnLuong, btnTongHop, btnDong;
        private DataGridView dgvLuong, dgvTongHop;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Lương hướng dẫn viên - thống kê", 900, 560);
            tabTK = new TabControl { Left = 10, Top = 10, Width = 880, Height = 490 };
            Controls.Add(tabTK);

            var t1 = FormHelper.Tp(tabTK, "Lương hướng dẫn viên");
            FormHelper.Lb(t1, "Tháng:", 10, 15); numThang = FormHelper.Nu(t1, 60, 15, 60, 1, 12, 1);
            FormHelper.Lb(t1, "Năm:", 150, 15); numNam = FormHelper.Nu(t1, 190, 15, 80, 2000, 2100, 2026); numNam.ThousandsSeparator = false;
            btnLuong = FormHelper.Bt(t1, "Tính lương", 300, 13, 110);
            FormHelper.Lb(t1, "Lương = lương căn bản + thù lao các tour kết thúc trong tháng", 430, 15);
            dgvLuong = FormHelper.Dg(t1, 10, 55, 845, 390);

            var t2 = FormHelper.Tp(tabTK, "Thống kê tổng hợp");
            FormHelper.Lb(t2, "Từ ngày:", 10, 15); dtTu = FormHelper.Dt(t2, 75, 15, 120);
            FormHelper.Lb(t2, "Đến ngày:", 230, 15); dtDen = FormHelper.Dt(t2, 300, 15, 120);
            btnTongHop = FormHelper.Bt(t2, "Thống kê", 450, 13, 110);
            dgvTongHop = FormHelper.Dg(t2, 10, 55, 845, 390);

            btnDong = FormHelper.Bt(this, "Đóng", 790, 515, 100);

            btnLuong.Click += btnLuong_Click;
            btnTongHop.Click += btnTongHop_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmLuongThongKe_Load;
            ResumeLayout(false);
        }
    }
}
