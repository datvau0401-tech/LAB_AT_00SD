using System;
using System.Windows.Forms;

namespace QuanLyThuVien.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain() { InitializeComponent(); }

        private void btnDanhMuc_Click(object sender, EventArgs e) { using (FrmDanhMuc f = new FrmDanhMuc()) f.ShowDialog(this); }
        private void btnSach_Click(object sender, EventArgs e) { using (FrmSach f = new FrmSach()) f.ShowDialog(this); }

        // Nếu bạn đã tạo thêm FrmDocGia, FrmMuonTra, FrmThongKe thì mở comment các dòng dưới
        // và thay các MessageBox tương ứng.
        private void btnDocGia_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form Độc giả và thẻ chưa được thêm vào project này.", "Thông báo");
        }
        private void btnMuonTra_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form Mượn - Trả sách chưa được thêm vào project này.", "Thông báo");
        }
        private void btnThongKe_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form Thống kê chưa được thêm vào project này.", "Thông báo");
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có thực sự muốn thoát chương trình?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) Close();
        }
    }
}
