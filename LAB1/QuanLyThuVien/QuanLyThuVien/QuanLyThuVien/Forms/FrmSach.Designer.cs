namespace QuanLyThuVien.Forms
{
    partial class FrmSach
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label lblMa, lblTen, lblNam, lblSoLuong, lblTheLoai, lblNXB, lblTim;
        private System.Windows.Forms.TextBox txtMa, txtTen, txtTim;
        private System.Windows.Forms.NumericUpDown numNam, numSoLuong;
        private System.Windows.Forms.ComboBox cboTheLoai, cboNXB;
        private System.Windows.Forms.Button btnTim, btnThem, btnCapNhat, btnXoa, btnLamMoi, btnDong;
        private System.Windows.Forms.DataGridView dgvSach;

        private void InitializeComponent()
        {
            this.lblMa = new System.Windows.Forms.Label { Text = "Mã đầu sách:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtMa = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(140, 22), Width = 160 };

            this.lblTen = new System.Windows.Forms.Label { Text = "Tên sách:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            this.txtTen = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(140, 57), Width = 300 };

            this.lblNam = new System.Windows.Forms.Label { Text = "Năm xuất bản:", Location = new System.Drawing.Point(20, 95), AutoSize = true };
            this.numNam = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(140, 92), Width = 160, Minimum = 1000, Maximum = 3000 };

            this.lblSoLuong = new System.Windows.Forms.Label { Text = "Số lượng hiện có:", Location = new System.Drawing.Point(20, 130), AutoSize = true };
            this.numSoLuong = new System.Windows.Forms.NumericUpDown { Location = new System.Drawing.Point(140, 127), Width = 160, Maximum = 100000 };

            this.lblTheLoai = new System.Windows.Forms.Label { Text = "Thể loại:", Location = new System.Drawing.Point(480, 25), AutoSize = true };
            this.cboTheLoai = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(580, 22), Width = 200, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };

            this.lblNXB = new System.Windows.Forms.Label { Text = "Nhà xuất bản:", Location = new System.Drawing.Point(480, 60), AutoSize = true };
            this.cboNXB = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(580, 57), Width = 200, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };

            this.lblTim = new System.Windows.Forms.Label { Text = "Từ khóa:", Location = new System.Drawing.Point(480, 95), AutoSize = true };
            this.txtTim = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(580, 92), Width = 140 };
            this.btnTim = new System.Windows.Forms.Button { Text = "Tìm", Location = new System.Drawing.Point(730, 90), Width = 60 };
            this.btnTim.Click += new System.EventHandler(this.btnTim_Click);

            this.btnThem = new System.Windows.Forms.Button { Text = "Thêm", Location = new System.Drawing.Point(830, 22), Width = 130 };
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            this.btnCapNhat = new System.Windows.Forms.Button { Text = "Cập nhật", Location = new System.Drawing.Point(830, 60), Width = 130 };
            this.btnCapNhat.Click += new System.EventHandler(this.btnCapNhat_Click);
            this.btnXoa = new System.Windows.Forms.Button { Text = "Xóa", Location = new System.Drawing.Point(830, 98), Width = 130 };
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);
            this.btnLamMoi = new System.Windows.Forms.Button { Text = "Làm mới", Location = new System.Drawing.Point(830, 136), Width = 130 };
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);

            this.dgvSach = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 180),
                Size = new System.Drawing.Size(940, 500),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvSach.SelectionChanged += new System.EventHandler(this.dgvSach_SelectionChanged);

            this.btnDong = new System.Windows.Forms.Button { Text = "Đóng", Location = new System.Drawing.Point(850, 695), Width = 110 };
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            this.ClientSize = new System.Drawing.Size(985, 740);
            this.Controls.Add(this.lblMa); this.Controls.Add(this.txtMa);
            this.Controls.Add(this.lblTen); this.Controls.Add(this.txtTen);
            this.Controls.Add(this.lblNam); this.Controls.Add(this.numNam);
            this.Controls.Add(this.lblSoLuong); this.Controls.Add(this.numSoLuong);
            this.Controls.Add(this.lblTheLoai); this.Controls.Add(this.cboTheLoai);
            this.Controls.Add(this.lblNXB); this.Controls.Add(this.cboNXB);
            this.Controls.Add(this.lblTim); this.Controls.Add(this.txtTim); this.Controls.Add(this.btnTim);
            this.Controls.Add(this.btnThem); this.Controls.Add(this.btnCapNhat);
            this.Controls.Add(this.btnXoa); this.Controls.Add(this.btnLamMoi);
            this.Controls.Add(this.dgvSach);
            this.Controls.Add(this.btnDong);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "FrmSach";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quản lý đầu sách";
            this.Load += new System.EventHandler(this.FrmSach_Load);
        }
    }
}
