using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmTour
    {
        private ComboBox cboTour, cboPT, cboDTQ;
        private TabControl tabTour;
        private DataGridView dgvTour, dgvDiemDung, dgvChang, dgvTQ;
        private TextBox txtMa, txtTen, txtMoTa, txtDiemDung, txtGhiChuDD, txtGhiChuPT;
        private NumericUpDown numNgay, numDem, numGia, numThuTu, numSao, numChang, numThuTuTQ;
        private CheckBox chkDoiPT, chkAn, chkKS;
        private Button btnThemTour, btnThemDD, btnThemChang, btnThemTQ, btnDong;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Tour - hành trình", 1000, 640);
            FormHelper.Lb(this, "Tour đang chọn (cho các tab hành trình):", 10, 12);
            cboTour = FormHelper.Cb(this, 290, 10, 320);
            tabTour = new TabControl { Left = 10, Top = 45, Width = 980, Height = 540 };
            Controls.Add(tabTour);

            var t1 = FormHelper.Tp(tabTour, "Tour");
            dgvTour = FormHelper.Dg(t1, 10, 10, 945, 340);
            FormHelper.Lb(t1, "Mã tour:", 10, 365); txtMa = FormHelper.Tx(t1, 80, 365, 120);
            FormHelper.Lb(t1, "Tên tour:", 250, 365); txtTen = FormHelper.Tx(t1, 320, 365, 340);
            FormHelper.Lb(t1, "Số ngày:", 10, 405); numNgay = FormHelper.Nu(t1, 80, 405, 80, 1, 60, 3);
            FormHelper.Lb(t1, "Số đêm:", 250, 405); numDem = FormHelper.Nu(t1, 320, 405, 80, 0, 60, 2);
            FormHelper.Lb(t1, "Đơn giá / khách:", 450, 405); numGia = FormHelper.Nu(t1, 550, 405, 140, 0, 1000000000m, 0);
            FormHelper.Lb(t1, "Mô tả:", 10, 445); txtMoTa = FormHelper.Tx(t1, 80, 445, 610);
            btnThemTour = FormHelper.Bt(t1, "Thêm tour", 790, 443, 120);

            var t2 = FormHelper.Tp(tabTour, "Điểm dừng");
            dgvDiemDung = FormHelper.Dg(t2, 10, 10, 945, 330);
            FormHelper.Lb(t2, "Thứ tự:", 10, 355); numThuTu = FormHelper.Nu(t2, 70, 355, 70, 1, 100, 1);
            FormHelper.Lb(t2, "Tên điểm dừng:", 180, 355); txtDiemDung = FormHelper.Tx(t2, 275, 355, 250);
            chkDoiPT = FormHelper.Ck(t2, "Đổi phương tiện", 10, 395);
            chkAn = FormHelper.Ck(t2, "Có nơi ăn", 160, 395);
            chkKS = FormHelper.Ck(t2, "Có khách sạn", 270, 395);
            FormHelper.Lb(t2, "Hạng sao:", 400, 395); numSao = FormHelper.Nu(t2, 470, 395, 60, 2, 5, 3); numSao.Enabled = false;
            FormHelper.Lb(t2, "Ghi chú:", 10, 435); txtGhiChuDD = FormHelper.Tx(t2, 70, 435, 600);
            btnThemDD = FormHelper.Bt(t2, "Thêm điểm dừng", 790, 433, 140);

            var t3 = FormHelper.Tp(tabTour, "Phương tiện theo chặng");
            dgvChang = FormHelper.Dg(t3, 10, 10, 945, 330);
            FormHelper.Lb(t3, "Chặng thứ:", 10, 355); numChang = FormHelper.Nu(t3, 80, 355, 70, 1, 100, 1);
            FormHelper.Lb(t3, "Phương tiện:", 200, 355); cboPT = FormHelper.Cb(t3, 285, 355, 200);
            FormHelper.Lb(t3, "Ghi chú:", 10, 395); txtGhiChuPT = FormHelper.Tx(t3, 80, 395, 600);
            btnThemChang = FormHelper.Bt(t3, "Gắn phương tiện", 790, 393, 140);
            FormHelper.Lb(t3, "Chặng k là đoạn đi tới điểm dừng thứ k; một chặng có thể dùng nhiều phương tiện.", 10, 440);

            var t4 = FormHelper.Tp(tabTour, "Điểm tham quan");
            dgvTQ = FormHelper.Dg(t4, 10, 10, 945, 330);
            FormHelper.Lb(t4, "Điểm tham quan:", 10, 355); cboDTQ = FormHelper.Cb(t4, 115, 355, 250);
            FormHelper.Lb(t4, "Thứ tự:", 400, 355); numThuTuTQ = FormHelper.Nu(t4, 455, 355, 70, 1, 100, 1);
            btnThemTQ = FormHelper.Bt(t4, "Gắn điểm TQ", 790, 353, 140);

            btnDong = FormHelper.Bt(this, "Đóng", 890, 598, 100);

            cboTour.SelectedIndexChanged += cboTour_SelectedIndexChanged;
            chkKS.CheckedChanged += chkKS_CheckedChanged;
            btnThemTour.Click += btnThemTour_Click;
            btnThemDD.Click += btnThemDD_Click;
            btnThemChang.Click += btnThemChang_Click;
            btnThemTQ.Click += btnThemTQ_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmTour_Load;
            ResumeLayout(false);
        }
    }
}
