namespace QuanLyThuVien.Forms
{
    partial class FrmThongKe
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label lblTuNgay, lblDenNgay;
        private System.Windows.Forms.DateTimePicker dtTuNgay, dtDenNgay;
        private System.Windows.Forms.Button btnThongKe, btnDong;

        private System.Windows.Forms.Label lblLuotMuon, lblQuaHan, lblMat, lblHuHong, lblTongPhi;
        private System.Windows.Forms.Label lblChiTiet;
        private System.Windows.Forms.DataGridView dgvChiTiet;

        private void InitializeComponent()
        {
            this.lblTuNgay = new System.Windows.Forms.Label { Text = "Từ ngày:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.dtTuNgay = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(100, 22), Width = 150, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.lblDenNgay = new System.Windows.Forms.Label { Text = "Đến ngày:", Location = new System.Drawing.Point(280, 25), AutoSize = true };
            this.dtDenNgay = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(360, 22), Width = 150, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.btnThongKe = new System.Windows.Forms.Button { Text = "Thống kê", Location = new System.Drawing.Point(540, 20), Width = 120 };
            this.btnThongKe.Click += new System.EventHandler(this.btnThongKe_Click);

            this.lblLuotMuon = new System.Windows.Forms.Label { Text = "Lượt sách mượn: 0", Location = new System.Drawing.Point(40, 75), AutoSize = true };
            this.lblQuaHan = new System.Windows.Forms.Label { Text = "Sách quá hạn: 0", Location = new System.Drawing.Point(340, 75), AutoSize = true };

            this.lblMat = new System.Windows.Forms.Label { Text = "Sách mất: 0", Location = new System.Drawing.Point(40, 110), AutoSize = true };
            this.lblHuHong = new System.Windows.Forms.Label { Text = "Sách hư hỏng: 0", Location = new System.Drawing.Point(340, 110), AutoSize = true };

            this.lblTongPhi = new System.Windows.Forms.Label
            {
                Text = "Tổng phí phạt: 0 đ",
                Location = new System.Drawing.Point(40, 150),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold)
            };

            this.lblChiTiet = new System.Windows.Forms.Label { Text = "Chi tiết phiếu phạt:", Location = new System.Drawing.Point(40, 195), AutoSize = true };

            this.dgvChiTiet = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(40, 220),
                Size = new System.Drawing.Size(900, 320),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };

            this.btnDong = new System.Windows.Forms.Button { Text = "Đóng", Location = new System.Drawing.Point(810, 560), Width = 130 };
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            this.ClientSize = new System.Drawing.Size(985, 610);
            this.Controls.Add(this.lblTuNgay); this.Controls.Add(this.dtTuNgay);
            this.Controls.Add(this.lblDenNgay); this.Controls.Add(this.dtDenNgay);
            this.Controls.Add(this.btnThongKe);
            this.Controls.Add(this.lblLuotMuon); this.Controls.Add(this.lblQuaHan);
            this.Controls.Add(this.lblMat); this.Controls.Add(this.lblHuHong);
            this.Controls.Add(this.lblTongPhi);
            this.Controls.Add(this.lblChiTiet);
            this.Controls.Add(this.dgvChiTiet);
            this.Controls.Add(this.btnDong);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "FrmThongKe";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Thống kê thư viện";
            this.Load += new System.EventHandler(this.FrmThongKe_Load);
        }
    }
}
