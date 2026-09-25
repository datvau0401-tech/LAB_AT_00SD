using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    public class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quản lý khách sạn";
            this.Size = new Size(820, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            Label lblTitle = new Label();
            lblTitle.Text = "HỆ THỐNG QUẢN LÝ KHÁCH SẠN";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 70;

            TableLayoutPanel panel = new TableLayoutPanel();
            panel.ColumnCount = 3;
            panel.RowCount = 3;
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(40, 10, 40, 40);
            for (int i = 0; i < 3; i++) panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            for (int i = 0; i < 3; i++) panel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));

            panel.Controls.Add(MakeButton("Danh mục", OnDanhMuc), 0, 0);
            panel.Controls.Add(MakeButton("Phòng - Tiện nghi", OnPhongTienNghi), 1, 0);
            panel.Controls.Add(MakeButton("Đặt / Nhận phòng", OnDatPhong), 2, 0);
            panel.Controls.Add(MakeButton("Sử dụng dịch vụ", OnDichVu), 0, 1);
            panel.Controls.Add(MakeButton("Trả phòng - Thanh toán", OnTraPhong), 1, 1);
            panel.Controls.Add(MakeButton("Thống kê", OnThongKe), 2, 1);
            panel.Controls.Add(MakeButton("Thoát", OnThoat), 1, 2);

            this.Controls.Add(panel);
            this.Controls.Add(lblTitle);
        }

        private Button MakeButton(string text, EventHandler onClick)
        {
            Button b = new Button();
            b.Text = text;
            b.Dock = DockStyle.Fill;
            b.Margin = new Padding(15);
            b.Font = new Font("Segoe UI", 11);
            b.Click += onClick;
            return b;
        }

        private void OnDanhMuc(object sender, EventArgs e)
        {
            using (FrmDanhMuc f = new FrmDanhMuc()) f.ShowDialog(this);
        }

        private void OnPhongTienNghi(object sender, EventArgs e)
        {
            using (FrmPhongTienNghi f = new FrmPhongTienNghi()) f.ShowDialog(this);
        }

        private void OnDatPhong(object sender, EventArgs e)
        {
            using (FrmDatPhong f = new FrmDatPhong()) f.ShowDialog(this);
        }

        private void OnDichVu(object sender, EventArgs e)
        {
            using (FrmDichVu f = new FrmDichVu()) f.ShowDialog(this);
        }

        private void OnTraPhong(object sender, EventArgs e)
        {
            using (FrmTraPhong f = new FrmTraPhong()) f.ShowDialog(this);
        }

        private void OnThongKe(object sender, EventArgs e)
        {
            using (FrmThongKe f = new FrmThongKe()) f.ShowDialog(this);
        }

        private void OnThoat(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
