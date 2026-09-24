namespace QuanLyThuVien.Forms
{
    partial class FrmDanhMuc
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabNV;
        private System.Windows.Forms.TabPage tabTL;
        private System.Windows.Forms.TabPage tabNXB;
        private System.Windows.Forms.Button btnDong;

        // Tab Nhân viên
        private System.Windows.Forms.Label lblNVMa, lblNVHo, lblNVTen, lblNVPhai, lblNVNgaySinh, lblNVChucVu, lblNVSDT;
        private System.Windows.Forms.TextBox txtNVMa, txtNVHo, txtNVTen, txtNVChucVu, txtNVSDT;
        private System.Windows.Forms.ComboBox cboNVPhai;
        private System.Windows.Forms.DateTimePicker dtNVNgaySinh;
        private System.Windows.Forms.Button btnNVThem, btnNVCapNhat, btnNVXoa, btnNVMoi;
        private System.Windows.Forms.DataGridView dgvNV;

        // Tab Thể loại
        private System.Windows.Forms.Label lblTLMa, lblTLTen;
        private System.Windows.Forms.TextBox txtTLMa, txtTLTen;
        private System.Windows.Forms.Button btnTLThem, btnTLCapNhat, btnTLXoa, btnTLMoi;
        private System.Windows.Forms.DataGridView dgvTL;

        // Tab Nhà xuất bản
        private System.Windows.Forms.Label lblNXBMa, lblNXBDiaChi, lblNXBSDT;
        private System.Windows.Forms.TextBox txtNXBMa, txtNXBDiaChi, txtNXBSDT;
        private System.Windows.Forms.Button btnNXBThem, btnNXBCapNhat, btnNXBXoa, btnNXBMoi;
        private System.Windows.Forms.DataGridView dgvNXB;

        private void InitializeComponent()
        {
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabNV = new System.Windows.Forms.TabPage();
            this.tabTL = new System.Windows.Forms.TabPage();
            this.tabNXB = new System.Windows.Forms.TabPage();
            this.btnDong = new System.Windows.Forms.Button();

            // ---------------- Tab Nhân viên ----------------
            this.lblNVMa = new System.Windows.Forms.Label { Text = "Mã nhân viên:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtNVMa = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 22), Width = 150 };
            this.lblNVHo = new System.Windows.Forms.Label { Text = "Họ:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            this.txtNVHo = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 57), Width = 150 };
            this.lblNVTen = new System.Windows.Forms.Label { Text = "Tên:", Location = new System.Drawing.Point(20, 95), AutoSize = true };
            this.txtNVTen = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 92), Width = 150 };

            this.lblNVPhai = new System.Windows.Forms.Label { Text = "Phái:", Location = new System.Drawing.Point(320, 25), AutoSize = true };
            this.cboNVPhai = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(420, 22), Width = 150, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            this.lblNVNgaySinh = new System.Windows.Forms.Label { Text = "Ngày sinh:", Location = new System.Drawing.Point(320, 60), AutoSize = true };
            this.dtNVNgaySinh = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(420, 57), Width = 150, Format = System.Windows.Forms.DateTimePickerFormat.Short };
            this.lblNVChucVu = new System.Windows.Forms.Label { Text = "Chức vụ:", Location = new System.Drawing.Point(320, 95), AutoSize = true };
            this.txtNVChucVu = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(420, 92), Width = 150 };
            this.lblNVSDT = new System.Windows.Forms.Label { Text = "Điện thoại:", Location = new System.Drawing.Point(320, 130), AutoSize = true };
            this.txtNVSDT = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(420, 127), Width = 150 };

            this.btnNVThem = new System.Windows.Forms.Button { Text = "Thêm", Location = new System.Drawing.Point(620, 22), Width = 100 };
            this.btnNVThem.Click += new System.EventHandler(this.btnNVThem_Click);
            this.btnNVCapNhat = new System.Windows.Forms.Button { Text = "Cập nhật", Location = new System.Drawing.Point(730, 22), Width = 100 };
            this.btnNVCapNhat.Click += new System.EventHandler(this.btnNVCapNhat_Click);
            this.btnNVXoa = new System.Windows.Forms.Button { Text = "Xóa", Location = new System.Drawing.Point(620, 60), Width = 100 };
            this.btnNVXoa.Click += new System.EventHandler(this.btnNVXoa_Click);
            this.btnNVMoi = new System.Windows.Forms.Button { Text = "Làm mới", Location = new System.Drawing.Point(730, 60), Width = 100 };
            this.btnNVMoi.Click += new System.EventHandler(this.btnNVMoi_Click);

            this.dgvNV = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 175),
                Size = new System.Drawing.Size(810, 480),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvNV.SelectionChanged += new System.EventHandler(this.dgvNV_SelectionChanged);

            this.tabNV.Controls.Add(this.lblNVMa); this.tabNV.Controls.Add(this.txtNVMa);
            this.tabNV.Controls.Add(this.lblNVHo); this.tabNV.Controls.Add(this.txtNVHo);
            this.tabNV.Controls.Add(this.lblNVTen); this.tabNV.Controls.Add(this.txtNVTen);
            this.tabNV.Controls.Add(this.lblNVPhai); this.tabNV.Controls.Add(this.cboNVPhai);
            this.tabNV.Controls.Add(this.lblNVNgaySinh); this.tabNV.Controls.Add(this.dtNVNgaySinh);
            this.tabNV.Controls.Add(this.lblNVChucVu); this.tabNV.Controls.Add(this.txtNVChucVu);
            this.tabNV.Controls.Add(this.lblNVSDT); this.tabNV.Controls.Add(this.txtNVSDT);
            this.tabNV.Controls.Add(this.btnNVThem); this.tabNV.Controls.Add(this.btnNVCapNhat);
            this.tabNV.Controls.Add(this.btnNVXoa); this.tabNV.Controls.Add(this.btnNVMoi);
            this.tabNV.Controls.Add(this.dgvNV);
            this.tabNV.Text = "Nhân viên";

            // ---------------- Tab Thể loại ----------------
            this.lblTLMa = new System.Windows.Forms.Label { Text = "Mã thể loại:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtTLMa = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 22), Width = 150 };
            this.lblTLTen = new System.Windows.Forms.Label { Text = "Tên thể loại:", Location = new System.Drawing.Point(320, 25), AutoSize = true };
            this.txtTLTen = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(420, 22), Width = 200 };

            this.btnTLThem = new System.Windows.Forms.Button { Text = "Thêm", Location = new System.Drawing.Point(650, 20), Width = 80 };
            this.btnTLThem.Click += new System.EventHandler(this.btnTLThem_Click);
            this.btnTLCapNhat = new System.Windows.Forms.Button { Text = "Cập nhật", Location = new System.Drawing.Point(650, 55), Width = 80 };
            this.btnTLCapNhat.Click += new System.EventHandler(this.btnTLCapNhat_Click);
            this.btnTLXoa = new System.Windows.Forms.Button { Text = "Xóa", Location = new System.Drawing.Point(740, 20), Width = 80 };
            this.btnTLXoa.Click += new System.EventHandler(this.btnTLXoa_Click);
            this.btnTLMoi = new System.Windows.Forms.Button { Text = "Làm mới", Location = new System.Drawing.Point(740, 55), Width = 80 };
            this.btnTLMoi.Click += new System.EventHandler(this.btnTLMoi_Click);

            this.dgvTL = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(810, 555),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvTL.SelectionChanged += new System.EventHandler(this.dgvTL_SelectionChanged);

            this.tabTL.Controls.Add(this.lblTLMa); this.tabTL.Controls.Add(this.txtTLMa);
            this.tabTL.Controls.Add(this.lblTLTen); this.tabTL.Controls.Add(this.txtTLTen);
            this.tabTL.Controls.Add(this.btnTLThem); this.tabTL.Controls.Add(this.btnTLCapNhat);
            this.tabTL.Controls.Add(this.btnTLXoa); this.tabTL.Controls.Add(this.btnTLMoi);
            this.tabTL.Controls.Add(this.dgvTL);
            this.tabTL.Text = "Thể loại";

            // ---------------- Tab Nhà xuất bản ----------------
            this.lblNXBMa = new System.Windows.Forms.Label { Text = "Mã NXB:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtNXBMa = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(130, 22), Width = 150 };
            this.lblNXBDiaChi = new System.Windows.Forms.Label { Text = "Địa chỉ:", Location = new System.Drawing.Point(320, 25), AutoSize = true };
            this.txtNXBDiaChi = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(420, 22), Width = 200 };
            this.lblNXBSDT = new System.Windows.Forms.Label { Text = "Điện thoại:", Location = new System.Drawing.Point(320, 60), AutoSize = true };
            this.txtNXBSDT = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(420, 57), Width = 200 };

            this.btnNXBThem = new System.Windows.Forms.Button { Text = "Thêm", Location = new System.Drawing.Point(650, 20), Width = 80 };
            this.btnNXBThem.Click += new System.EventHandler(this.btnNXBThem_Click);
            this.btnNXBCapNhat = new System.Windows.Forms.Button { Text = "Cập nhật", Location = new System.Drawing.Point(650, 55), Width = 80 };
            this.btnNXBCapNhat.Click += new System.EventHandler(this.btnNXBCapNhat_Click);
            this.btnNXBXoa = new System.Windows.Forms.Button { Text = "Xóa", Location = new System.Drawing.Point(740, 20), Width = 80 };
            this.btnNXBXoa.Click += new System.EventHandler(this.btnNXBXoa_Click);
            this.btnNXBMoi = new System.Windows.Forms.Button { Text = "Làm mới", Location = new System.Drawing.Point(740, 55), Width = 80 };
            this.btnNXBMoi.Click += new System.EventHandler(this.btnNXBMoi_Click);

            this.dgvNXB = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(810, 555),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvNXB.SelectionChanged += new System.EventHandler(this.dgvNXB_SelectionChanged);

            this.tabNXB.Controls.Add(this.lblNXBMa); this.tabNXB.Controls.Add(this.txtNXBMa);
            this.tabNXB.Controls.Add(this.lblNXBDiaChi); this.tabNXB.Controls.Add(this.txtNXBDiaChi);
            this.tabNXB.Controls.Add(this.lblNXBSDT); this.tabNXB.Controls.Add(this.txtNXBSDT);
            this.tabNXB.Controls.Add(this.btnNXBThem); this.tabNXB.Controls.Add(this.btnNXBCapNhat);
            this.tabNXB.Controls.Add(this.btnNXBXoa); this.tabNXB.Controls.Add(this.btnNXBMoi);
            this.tabNXB.Controls.Add(this.dgvNXB);
            this.tabNXB.Text = "Nhà xuất bản";

            // ---------------- TabControl ----------------
            this.tabs.Location = new System.Drawing.Point(15, 15);
            this.tabs.Size = new System.Drawing.Size(860, 650);
            this.tabs.Controls.Add(this.tabNV);
            this.tabs.Controls.Add(this.tabTL);
            this.tabs.Controls.Add(this.tabNXB);

            // ---------------- Nút đóng ----------------
            this.btnDong.Location = new System.Drawing.Point(750, 675);
            this.btnDong.Size = new System.Drawing.Size(120, 35);
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            // ---------------- Form ----------------
            this.ClientSize = new System.Drawing.Size(890, 720);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.btnDong);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "FrmDanhMuc";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Danh mục và nhân viên";
            this.Load += new System.EventHandler(this.FrmDanhMuc_Load);
        }
    }
}
