namespace QuanLyThuVien.Forms
{
    partial class FrmDocGia
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Label lblTieuDe;
        private System.Windows.Forms.Label lblMa, lblHo, lblTen, lblNgaySinh, lblPhai;
        private System.Windows.Forms.Label lblSDT, lblDiaChi, lblEmail, lblAnh;
        private System.Windows.Forms.Label lblNgayCap, lblHanSuDung;
        private System.Windows.Forms.TextBox txtMa, txtHo, txtTen, txtSDT, txtDiaChi, txtEmail, txtAnh;
        private System.Windows.Forms.DateTimePicker dtNgaySinh, dtNgayCap, dtHanSuDung;
        private System.Windows.Forms.ComboBox cboPhai;
        private System.Windows.Forms.CheckBox chkDaDongLePhi;
        private System.Windows.Forms.Button btnChonAnh;
        private System.Windows.Forms.Button btnThem, btnCapNhat, btnCapThe, btnGiaHan;
        private System.Windows.Forms.Label lblTim;
        private System.Windows.Forms.TextBox txtTim;
        private System.Windows.Forms.Button btnTim, btnLamMoi, btnXoa, btnDong;
        private System.Windows.Forms.DataGridView dgvDocGia;

        private void InitializeComponent()
        {
            this.lblTieuDe = new System.Windows.Forms.Label
            {
                Text = "Độc giả và thẻ thư viện",
                Location = new System.Drawing.Point(20, 15),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold)
            };

            // ---- Cột trái ----
            this.lblMa = new System.Windows.Forms.Label { Text = "Mã độc giả:", Location = new System.Drawing.Point(20, 55), AutoSize = true };
            this.txtMa = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(150, 52), Width = 160 };

            this.lblHo = new System.Windows.Forms.Label { Text = "Họ:", Location = new System.Drawing.Point(20, 90), AutoSize = true };
            this.txtHo = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(150, 87), Width = 160 };

            this.lblTen = new System.Windows.Forms.Label { Text = "Tên:", Location = new System.Drawing.Point(20, 125), AutoSize = true };
            this.txtTen = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(150, 122), Width = 160 };

            this.lblNgaySinh = new System.Windows.Forms.Label { Text = "Ngày sinh:", Location = new System.Drawing.Point(20, 160), AutoSize = true };
            this.dtNgaySinh = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(150, 157), Width = 160, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.lblPhai = new System.Windows.Forms.Label { Text = "Phái:", Location = new System.Drawing.Point(20, 195), AutoSize = true };
            this.cboPhai = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(150, 192), Width = 160, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };

            // ---- Cột giữa ----
            this.lblSDT = new System.Windows.Forms.Label { Text = "Điện thoại:", Location = new System.Drawing.Point(380, 55), AutoSize = true };
            this.txtSDT = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(500, 52), Width = 180 };

            this.lblDiaChi = new System.Windows.Forms.Label { Text = "Địa chỉ:", Location = new System.Drawing.Point(380, 90), AutoSize = true };
            this.txtDiaChi = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(500, 87), Width = 180 };

            this.lblEmail = new System.Windows.Forms.Label { Text = "Email:", Location = new System.Drawing.Point(380, 125), AutoSize = true };
            this.txtEmail = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(500, 122), Width = 180 };

            this.lblAnh = new System.Windows.Forms.Label { Text = "Ảnh 3x4:", Location = new System.Drawing.Point(380, 160), AutoSize = true };
            this.txtAnh = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(500, 157), Width = 140 };
            this.btnChonAnh = new System.Windows.Forms.Button { Text = "...", Location = new System.Drawing.Point(645, 156), Width = 35 };
            this.btnChonAnh.Click += new System.EventHandler(this.btnChonAnh_Click);

            // ---- Thông tin thẻ ----
            this.lblNgayCap = new System.Windows.Forms.Label { Text = "Ngày cấp:", Location = new System.Drawing.Point(380, 195), AutoSize = true };
            this.dtNgayCap = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(500, 192), Width = 140, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.chkDaDongLePhi = new System.Windows.Forms.CheckBox { Text = "Đã đóng lệ phí", Location = new System.Drawing.Point(650, 194), AutoSize = true };

            this.lblHanSuDung = new System.Windows.Forms.Label { Text = "Hạn sử dụng:", Location = new System.Drawing.Point(380, 230), AutoSize = true };
            this.dtHanSuDung = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(500, 227), Width = 140, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            // ---- Các nút chức năng ----
            this.btnThem = new System.Windows.Forms.Button { Text = "Thêm", Location = new System.Drawing.Point(800, 52), Width = 140 };
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);

            this.btnCapNhat = new System.Windows.Forms.Button { Text = "Cập nhật", Location = new System.Drawing.Point(800, 90), Width = 140 };
            this.btnCapNhat.Click += new System.EventHandler(this.btnCapNhat_Click);

            this.btnCapThe = new System.Windows.Forms.Button { Text = "Cấp thẻ", Location = new System.Drawing.Point(800, 192), Width = 140 };
            this.btnCapThe.Click += new System.EventHandler(this.btnCapThe_Click);

            this.btnGiaHan = new System.Windows.Forms.Button { Text = "Gia hạn", Location = new System.Drawing.Point(800, 227), Width = 140 };
            this.btnGiaHan.Click += new System.EventHandler(this.btnGiaHan_Click);

            // ---- Tìm kiếm ----
            this.lblTim = new System.Windows.Forms.Label { Text = "Từ khóa:", Location = new System.Drawing.Point(20, 275), AutoSize = true };
            this.txtTim = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(100, 272), Width = 220 };
            this.btnTim = new System.Windows.Forms.Button { Text = "Tìm", Location = new System.Drawing.Point(330, 270), Width = 80 };
            this.btnTim.Click += new System.EventHandler(this.btnTim_Click);

            this.btnLamMoi = new System.Windows.Forms.Button { Text = "Làm mới", Location = new System.Drawing.Point(650, 270), Width = 140 };
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);

            this.btnXoa = new System.Windows.Forms.Button { Text = "Xóa", Location = new System.Drawing.Point(800, 270), Width = 140 };
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click);

            // ---- Bảng danh sách ----
            this.dgvDocGia = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 310),
                Size = new System.Drawing.Size(920, 350),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvDocGia.SelectionChanged += new System.EventHandler(this.dgvDocGia_SelectionChanged);

            this.btnDong = new System.Windows.Forms.Button { Text = "Đóng", Location = new System.Drawing.Point(830, 675), Width = 110 };
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            this.ClientSize = new System.Drawing.Size(965, 715);
            this.Controls.Add(this.lblTieuDe);
            this.Controls.Add(this.lblMa); this.Controls.Add(this.txtMa);
            this.Controls.Add(this.lblHo); this.Controls.Add(this.txtHo);
            this.Controls.Add(this.lblTen); this.Controls.Add(this.txtTen);
            this.Controls.Add(this.lblNgaySinh); this.Controls.Add(this.dtNgaySinh);
            this.Controls.Add(this.lblPhai); this.Controls.Add(this.cboPhai);
            this.Controls.Add(this.lblSDT); this.Controls.Add(this.txtSDT);
            this.Controls.Add(this.lblDiaChi); this.Controls.Add(this.txtDiaChi);
            this.Controls.Add(this.lblEmail); this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblAnh); this.Controls.Add(this.txtAnh); this.Controls.Add(this.btnChonAnh);
            this.Controls.Add(this.lblNgayCap); this.Controls.Add(this.dtNgayCap); this.Controls.Add(this.chkDaDongLePhi);
            this.Controls.Add(this.lblHanSuDung); this.Controls.Add(this.dtHanSuDung);
            this.Controls.Add(this.btnThem); this.Controls.Add(this.btnCapNhat);
            this.Controls.Add(this.btnCapThe); this.Controls.Add(this.btnGiaHan);
            this.Controls.Add(this.lblTim); this.Controls.Add(this.txtTim); this.Controls.Add(this.btnTim);
            this.Controls.Add(this.btnLamMoi); this.Controls.Add(this.btnXoa);
            this.Controls.Add(this.dgvDocGia);
            this.Controls.Add(this.btnDong);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "FrmDocGia";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Độc giả và thẻ thư viện";
            this.Load += new System.EventHandler(this.FrmDocGia_Load);
        }
    }
}
