using System;
using System.Globalization;
using System.Windows.Forms;
using QuanLyThuVien.Services;

namespace QuanLyThuVien.Forms
{
    public partial class FrmThongKe : Form
    {
        private readonly ThongKeService service = new ThongKeService();

        public FrmThongKe() { InitializeComponent(); }

        private void FrmThongKe_Load(object sender, EventArgs e)
        {
            DateTime homNay = DateTime.Today;
            dtTuNgay.Value = new DateTime(homNay.Year, homNay.Month, 1);
            dtDenNgay.Value = homNay;

            ThucHienThongKe();
        }

        private void btnThongKe_Click(object sender, EventArgs e) { ThucHienThongKe(); }

        private void ThucHienThongKe()
        {
            if (dtDenNgay.Value.Date < dtTuNgay.Value.Date)
            {
                MessageBox.Show("Đến ngày phải sau hoặc bằng Từ ngày.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ThongKeTongHop kq = service.TinhTongHop(dtTuNgay.Value, dtDenNgay.Value);

            lblLuotMuon.Text = "Lượt sách mượn: " + kq.LuotSachMuon;
            lblQuaHan.Text = "Sách quá hạn: " + kq.SachQuaHan;
            lblMat.Text = "Sách mất: " + kq.SachMat;
            lblHuHong.Text = "Sách hư hỏng: " + kq.SachHuHong;
            lblTongPhi.Text = "Tổng phí phạt: " + kq.TongPhiPhat.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

            dgvChiTiet.DataSource = service.LayChiTietPhieuPhat(dtTuNgay.Value, dtDenNgay.Value);
            dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvChiTiet.Columns.Contains("Phí phạt"))
                dgvChiTiet.Columns["Phí phạt"].DefaultCellStyle.Format = "N0";
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
