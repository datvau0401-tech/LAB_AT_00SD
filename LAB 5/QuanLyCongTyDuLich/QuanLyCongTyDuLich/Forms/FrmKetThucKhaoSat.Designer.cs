using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmKetThucKhaoSat
    {
        private TabControl tabKT;
        private DataGridView dgvDoan, dgvKS;
        private TextBox txtSoTT, txtSoDK, txtGhiChu, txtMaKS, txtKSChon, txtGopY;
        private DateTimePicker dtTT, dtGui, dtPH;
        private NumericUpDown numTien, numDiem;
        private ComboBox cboLoaiKS, cboDangKy;
        private Button btnThanhToan, btnGui, btnGhiPH, btnDong;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Kết thúc tour - thanh toán đoàn - khảo sát", 1060, 620);
            tabKT = new TabControl { Left = 10, Top = 10, Width = 1040, Height = 560 };
            Controls.Add(tabKT);

            var t1 = FormHelper.Tp(tabKT, "Thanh toán sau tour (đoàn)");
            dgvDoan = FormHelper.Dg(t1, 10, 10, 1005, 360);
            FormHelper.Lb(t1, "Số thanh toán:", 10, 390); txtSoTT = FormHelper.Tx(t1, 100, 390, 120);
            FormHelper.Lb(t1, "Phiếu đoàn:", 250, 390); txtSoDK = FormHelper.Tx(t1, 325, 390, 120); txtSoDK.ReadOnly = true;
            FormHelper.Lb(t1, "Ngày thanh toán:", 480, 390); dtTT = FormHelper.Dt(t1, 580, 390, 120);
            FormHelper.Lb(t1, "Số tiền:", 10, 430); numTien = FormHelper.Nu(t1, 100, 430, 140, 0, 100000000000m, 0);
            FormHelper.Lb(t1, "Ghi chú:", 270, 430); txtGhiChu = FormHelper.Tx(t1, 330, 430, 370);
            btnThanhToan = FormHelper.Bt(t1, "Ghi nhận thanh toán", 780, 428, 170);

            var t2 = FormHelper.Tp(tabKT, "Khảo sát khách hàng");
            FormHelper.Lb(t2, "Loại khách:", 10, 15); cboLoaiKS = FormHelper.Cb(t2, 85, 15, 90);
            FormHelper.Lb(t2, "Đăng ký đã kết thúc:", 210, 15); cboDangKy = FormHelper.Cb(t2, 330, 15, 340);
            FormHelper.Lb(t2, "Mã KS:", 700, 15); txtMaKS = FormHelper.Tx(t2, 750, 15, 120);
            FormHelper.Lb(t2, "Ngày gửi:", 10, 55); dtGui = FormHelper.Dt(t2, 85, 55, 120);
            btnGui = FormHelper.Bt(t2, "Gửi phiếu khảo sát", 750, 53, 170);
            dgvKS = FormHelper.Dg(t2, 10, 95, 1005, 270);
            FormHelper.Lb(t2, "Phiếu chọn:", 10, 385); txtKSChon = FormHelper.Tx(t2, 90, 385, 110); txtKSChon.ReadOnly = true;
            FormHelper.Lb(t2, "Ngày phản hồi:", 230, 385); dtPH = FormHelper.Dt(t2, 320, 385, 120);
            FormHelper.Lb(t2, "Điểm (1-5):", 470, 385); numDiem = FormHelper.Nu(t2, 545, 385, 60, 1, 5, 5);
            FormHelper.Lb(t2, "Góp ý:", 10, 425); txtGopY = FormHelper.Tx(t2, 90, 425, 640);
            btnGhiPH = FormHelper.Bt(t2, "Ghi nhận góp ý", 780, 423, 150);

            btnDong = FormHelper.Bt(this, "Đóng", 950, 578, 100);

            dgvDoan.SelectionChanged += dgvDoan_SelectionChanged;
            btnThanhToan.Click += btnThanhToan_Click;
            cboLoaiKS.SelectedIndexChanged += cboLoaiKS_SelectedIndexChanged;
            btnGui.Click += btnGui_Click;
            dgvKS.SelectionChanged += dgvKS_SelectionChanged;
            btnGhiPH.Click += btnGhiPH_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmKetThucKhaoSat_Load;
            ResumeLayout(false);
        }
    }
}
