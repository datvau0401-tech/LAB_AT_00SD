using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Thống kê nhanh theo khoảng ngày: số phiếu đặt, số phòng đang ở (thời điểm hiện tại),
    /// số hóa đơn và doanh thu, tổng tiền đền bù, và tổng hợp dịch vụ đã sử dụng.
    /// </summary>
    public class FrmThongKe : Form
    {
        private DateTimePicker dtpTuNgay, dtpDenNgay;
        private Button btnThongKe;

        private Label lblPhieuDat, lblDangO, lblHoaDon, lblDoanhThu, lblDenBu;
        private DataGridView gridDichVu;

        public FrmThongKe()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thống kê khách sạn";
            this.Size = new Size(820, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel pnlBoLoc = new Panel { Dock = DockStyle.Top, Height = 45 };
            Label l1 = new Label { Text = "Từ ngày:", Left = 15, Top = 15, AutoSize = true };
            dtpTuNgay = new DateTimePicker { Left = 80, Top = 11, Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
            Label l2 = new Label { Text = "Đến ngày:", Left = 225, Top = 15, AutoSize = true };
            dtpDenNgay = new DateTimePicker { Left = 295, Top = 11, Width = 130, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            btnThongKe = new Button { Text = "Thống kê", Left = 440, Top = 9, Width = 100 };
            btnThongKe.Click += BtnThongKe_Click;
            pnlBoLoc.Controls.AddRange(new Control[] { l1, dtpTuNgay, l2, dtpDenNgay, btnThongKe });

            Panel pnlTongHop = new Panel { Dock = DockStyle.Top, Height = 75 };
            Font fBold = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPhieuDat = new Label { Text = "Phiếu đặt: 0", Left = 20, Top = 10, AutoSize = true, Font = fBold, ForeColor = Color.DarkBlue };
            lblDangO = new Label { Text = "Đang ở: 0", Left = 420, Top = 10, AutoSize = true, Font = fBold, ForeColor = Color.DarkBlue };
            lblHoaDon = new Label { Text = "Hóa đơn: 0", Left = 20, Top = 40, AutoSize = true, Font = fBold, ForeColor = Color.DarkBlue };
            lblDoanhThu = new Label { Text = "Doanh thu HĐ: 0 đ", Left = 420, Top = 40, AutoSize = true, Font = fBold, ForeColor = Color.DarkBlue };
            lblDenBu = new Label { Text = "Tổng đền bù: 0 đ", Left = 20, Top = 70, AutoSize = true, Font = fBold, ForeColor = Color.DarkBlue };
            pnlTongHop.Height = 100;
            pnlTongHop.Controls.AddRange(new Control[] { lblPhieuDat, lblDangO, lblHoaDon, lblDoanhThu, lblDenBu });

            Label lblDV = new Label { Text = "Dịch vụ sử dụng:", Dock = DockStyle.Top, Height = 22, Padding = new Padding(5, 3, 0, 0) };

            gridDichVu = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            gridDichVu.Columns.Add("MaDV", "Mã DV");
            gridDichVu.Columns.Add("TenDV", "Tên dịch vụ");
            gridDichVu.Columns.Add("TongSoLuong", "Tổng số lượng");
            gridDichVu.Columns.Add("TongTien", "Tổng tiền");

            this.Controls.Add(gridDichVu);
            this.Controls.Add(lblDV);
            this.Controls.Add(pnlTongHop);
            this.Controls.Add(pnlBoLoc);
        }

        private void BtnThongKe_Click(object sender, EventArgs e)
        {
            DateTime tuNgay = dtpTuNgay.Value.Date;
            DateTime denNgay = dtpDenNgay.Value.Date;
            DateTime denNgayLoaiTru = denNgay.AddDays(1); // để so sánh với cột kiểu datetime (NgayLap)

            try
            {
                object soPhieuDat = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM PhieuDatPhong WHERE NgayLap >= @tu AND NgayLap < @den",
                    new SqlParameter("@tu", tuNgay), new SqlParameter("@den", denNgayLoaiTru));
                lblPhieuDat.Text = "Phiếu đặt: " + Convert.ToInt32(soPhieuDat);

                object dangO = DbHelper.ExecuteScalar("SELECT COUNT(*) FROM PhieuDatPhong WHERE TrangThai=N'Đang ở'");
                lblDangO.Text = "Đang ở: " + Convert.ToInt32(dangO);

                object soHoaDon = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM HoaDon WHERE NgayLap >= @tu AND NgayLap < @den",
                    new SqlParameter("@tu", tuNgay), new SqlParameter("@den", denNgayLoaiTru));
                lblHoaDon.Text = "Hóa đơn: " + Convert.ToInt32(soHoaDon);

                object doanhThu = DbHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE NgayLap >= @tu AND NgayLap < @den",
                    new SqlParameter("@tu", tuNgay), new SqlParameter("@den", denNgayLoaiTru));
                lblDoanhThu.Text = "Doanh thu HĐ: " + string.Format("{0:N0}", Convert.ToDecimal(doanhThu)) + " đ";

                object tongDenBu = DbHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TongTien),0) FROM PhieuDenBu WHERE NgayLap >= @tu AND NgayLap < @den",
                    new SqlParameter("@tu", tuNgay), new SqlParameter("@den", denNgayLoaiTru));
                lblDenBu.Text = "Tổng đền bù: " + string.Format("{0:N0}", Convert.ToDecimal(tongDenBu)) + " đ";

                gridDichVu.Rows.Clear();
                DataTable dt = DbHelper.GetDataTable(
                    @"SELECT dv.MaDV, dv.TenDV, SUM(ct.SoLuong) AS TongSoLuong, SUM(ct.ThanhTien) AS TongTien
                      FROM ChiTietPhieuSuDungDV ct
                      JOIN PhieuSuDungDV p ON ct.SoPhieuSDDV = p.SoPhieuSDDV
                      JOIN DichVu dv ON ct.MaDV = dv.MaDV
                      WHERE p.NgaySuDung >= @tu AND p.NgaySuDung <= @den
                      GROUP BY dv.MaDV, dv.TenDV
                      ORDER BY dv.MaDV",
                    new SqlParameter("@tu", tuNgay), new SqlParameter("@den", denNgay));
                foreach (DataRow r in dt.Rows)
                    gridDichVu.Rows.Add(r["MaDV"], r["TenDV"], r["TongSoLuong"], r["TongTien"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
