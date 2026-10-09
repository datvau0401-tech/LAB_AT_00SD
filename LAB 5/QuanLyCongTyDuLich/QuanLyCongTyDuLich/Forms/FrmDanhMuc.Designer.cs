using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmDanhMuc
    {
        private TabControl tabDanhMuc;
        private DataGridView dgvPT, dgvDB, dgvHDV, dgvDTQ;
        private TextBox txtPTMa, txtPTTen, txtPTGhiChu, txtDBMa, txtDBTen, txtDBDiaChi, txtDBDT, txtHDVMa, txtHDVTen, txtHDVDT,
                        txtDTQMa, txtDTQTen, txtDTQDiaDiem, txtDTQNoiDung, txtDTQYNghia;
        private NumericUpDown numLuong;
        private Button btnThemPT, btnThemDB, btnThemHDV, btnThemDTQ, btnDong;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Danh mục", 900, 560);
            tabDanhMuc = new TabControl { Left = 10, Top = 10, Width = 880, Height = 490 };
            Controls.Add(tabDanhMuc);

            var t1 = FormHelper.Tp(tabDanhMuc, "Phương tiện");
            dgvPT = FormHelper.Dg(t1, 10, 10, 845, 320);
            FormHelper.Lb(t1, "Mã PT:", 10, 345); txtPTMa = FormHelper.Tx(t1, 80, 345, 120);
            FormHelper.Lb(t1, "Tên PT:", 250, 345); txtPTTen = FormHelper.Tx(t1, 310, 345, 250);
            FormHelper.Lb(t1, "Ghi chú:", 10, 385); txtPTGhiChu = FormHelper.Tx(t1, 80, 385, 480);
            btnThemPT = FormHelper.Bt(t1, "Thêm", 600, 383, 120);

            var t2 = FormHelper.Tp(tabDanhMuc, "Điểm bán vé");
            dgvDB = FormHelper.Dg(t2, 10, 10, 845, 320);
            FormHelper.Lb(t2, "Mã điểm bán:", 10, 345); txtDBMa = FormHelper.Tx(t2, 100, 345, 120);
            FormHelper.Lb(t2, "Tên điểm bán:", 250, 345); txtDBTen = FormHelper.Tx(t2, 340, 345, 250);
            FormHelper.Lb(t2, "Địa chỉ:", 10, 385); txtDBDiaChi = FormHelper.Tx(t2, 100, 385, 340);
            FormHelper.Lb(t2, "Điện thoại:", 470, 385); txtDBDT = FormHelper.Tx(t2, 545, 385, 130);
            btnThemDB = FormHelper.Bt(t2, "Thêm", 700, 383, 120);

            var t3 = FormHelper.Tp(tabDanhMuc, "Hướng dẫn viên");
            dgvHDV = FormHelper.Dg(t3, 10, 10, 845, 320);
            FormHelper.Lb(t3, "Mã HDV:", 10, 345); txtHDVMa = FormHelper.Tx(t3, 80, 345, 120);
            FormHelper.Lb(t3, "Họ tên:", 250, 345); txtHDVTen = FormHelper.Tx(t3, 310, 345, 250);
            FormHelper.Lb(t3, "Điện thoại:", 10, 385); txtHDVDT = FormHelper.Tx(t3, 80, 385, 120);
            FormHelper.Lb(t3, "Lương căn bản:", 250, 385); numLuong = FormHelper.Nu(t3, 350, 385, 150, 0, 1000000000m, 0);
            btnThemHDV = FormHelper.Bt(t3, "Thêm", 600, 383, 120);

            var t4 = FormHelper.Tp(tabDanhMuc, "Điểm tham quan");
            dgvDTQ = FormHelper.Dg(t4, 10, 10, 845, 300);
            FormHelper.Lb(t4, "Mã điểm TQ:", 10, 325); txtDTQMa = FormHelper.Tx(t4, 100, 325, 120);
            FormHelper.Lb(t4, "Tên điểm TQ:", 250, 325); txtDTQTen = FormHelper.Tx(t4, 340, 325, 250);
            FormHelper.Lb(t4, "Địa điểm:", 620, 325); txtDTQDiaDiem = FormHelper.Tx(t4, 685, 325, 160);
            FormHelper.Lb(t4, "Nội dung:", 10, 365); txtDTQNoiDung = FormHelper.Tx(t4, 100, 365, 745);
            FormHelper.Lb(t4, "Ý nghĩa:", 10, 405); txtDTQYNghia = FormHelper.Tx(t4, 100, 405, 600);
            btnThemDTQ = FormHelper.Bt(t4, "Thêm", 725, 403, 120);

            btnDong = FormHelper.Bt(this, "Đóng", 790, 515, 100);

            btnThemPT.Click += btnThemPT_Click;
            btnThemDB.Click += btnThemDB_Click;
            btnThemHDV.Click += btnThemHDV_Click;
            btnThemDTQ.Click += btnThemDTQ_Click;
            btnDong.Click += btnDong_Click;
            Load += FrmDanhMuc_Load;
            ResumeLayout(false);
        }
    }
}
