namespace QuanLyThuVien.Forms
{
    partial class FrmMuonTra
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.TabControl tabChinh;
        private System.Windows.Forms.TabPage tabMuon, tabTra;
        private System.Windows.Forms.Button btnDong;

        // ---- Tab Mượn sách ----
        private System.Windows.Forms.Label lblDocGia, lblDieuKien;
        private System.Windows.Forms.TextBox txtDocGia;
        private System.Windows.Forms.Button btnKiemTra;
        private System.Windows.Forms.Label lblNhanVien, lblNgayMuon, lblHenTra;
        private System.Windows.Forms.TextBox txtNhanVien;
        private System.Windows.Forms.DateTimePicker dtNgayMuon, dtHenTra;
        private System.Windows.Forms.Label lblKho, lblDaChon;
        private System.Windows.Forms.DataGridView dgvKho, dgvDaChon;
        private System.Windows.Forms.Button btnThem, btnBo, btnLapPhieuMuon;

        // ---- Tab Trả sách ----
        private System.Windows.Forms.Label lblDocGiaTra;
        private System.Windows.Forms.TextBox txtDocGiaTra;
        private System.Windows.Forms.Button btnTimSachDangMuon;
        private System.Windows.Forms.Label lblNhanVienTra;
        private System.Windows.Forms.TextBox txtNhanVienTra;
        private System.Windows.Forms.DataGridView dgvDangMuon;
        private System.Windows.Forms.Label lblTinhTrang, lblPhiPhat;
        private System.Windows.Forms.ComboBox cboTinhTrang;
        private System.Windows.Forms.Label lblPhiPhatGiaTri;
        private System.Windows.Forms.Button btnXacNhanTra;

        private void InitializeComponent()
        {
            this.tabChinh = new System.Windows.Forms.TabControl
            {
                Location = new System.Drawing.Point(15, 15),
                Size = new System.Drawing.Size(950, 620)
            };
            this.tabMuon = new System.Windows.Forms.TabPage("Mượn sách");
            this.tabTra = new System.Windows.Forms.TabPage("Trả sách");
            this.tabChinh.TabPages.Add(this.tabMuon);
            this.tabChinh.TabPages.Add(this.tabTra);

            XayDungTabMuon();
            XayDungTabTra();

            this.btnDong = new System.Windows.Forms.Button { Text = "Đóng", Location = new System.Drawing.Point(855, 650), Width = 110 };
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);

            this.ClientSize = new System.Drawing.Size(985, 700);
            this.Controls.Add(this.tabChinh);
            this.Controls.Add(this.btnDong);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Name = "FrmMuonTra";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Mượn - Trả sách";
            this.Load += new System.EventHandler(this.FrmMuonTra_Load);
        }

        private void XayDungTabMuon()
        {
            this.lblDocGia = new System.Windows.Forms.Label { Text = "Độc giả:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtDocGia = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(110, 22), Width = 140 };
            this.btnKiemTra = new System.Windows.Forms.Button { Text = "Kiểm tra điều kiện", Location = new System.Drawing.Point(260, 20), Width = 160 };
            this.btnKiemTra.Click += new System.EventHandler(this.btnKiemTra_Click);
            this.lblDieuKien = new System.Windows.Forms.Label { Text = "", Location = new System.Drawing.Point(435, 25), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen };

            this.lblNhanVien = new System.Windows.Forms.Label { Text = "Nhân viên lập phiếu:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            this.txtNhanVien = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(160, 57), Width = 100 };

            this.lblNgayMuon = new System.Windows.Forms.Label { Text = "Ngày mượn:", Location = new System.Drawing.Point(300, 60), AutoSize = true };
            this.dtNgayMuon = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(400, 57), Width = 130, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.lblHenTra = new System.Windows.Forms.Label { Text = "Hẹn trả:", Location = new System.Drawing.Point(560, 60), AutoSize = true };
            this.dtHenTra = new System.Windows.Forms.DateTimePicker { Location = new System.Drawing.Point(630, 57), Width = 130, Format = System.Windows.Forms.DateTimePickerFormat.Short };

            this.lblKho = new System.Windows.Forms.Label { Text = "Sách còn trong kho:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            this.dgvKho = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 125),
                Size = new System.Drawing.Size(400, 410),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AllowUserToAddRows = false
            };

            this.btnThem = new System.Windows.Forms.Button { Text = "Thêm >>", Location = new System.Drawing.Point(440, 220), Width = 90 };
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            this.btnBo = new System.Windows.Forms.Button { Text = "<< Bỏ", Location = new System.Drawing.Point(440, 255), Width = 90 };
            this.btnBo.Click += new System.EventHandler(this.btnBo_Click);

            this.lblDaChon = new System.Windows.Forms.Label { Text = "Sách đã chọn (tối đa 3):", Location = new System.Drawing.Point(550, 100), AutoSize = true };
            this.dgvDaChon = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(550, 125),
                Size = new System.Drawing.Size(380, 230),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AllowUserToAddRows = false
            };

            this.btnLapPhieuMuon = new System.Windows.Forms.Button { Text = "Lập phiếu mượn", Location = new System.Drawing.Point(760, 375), Width = 170 };
            this.btnLapPhieuMuon.Click += new System.EventHandler(this.btnLapPhieuMuon_Click);

            this.tabMuon.Controls.Add(this.lblDocGia); this.tabMuon.Controls.Add(this.txtDocGia);
            this.tabMuon.Controls.Add(this.btnKiemTra);
            this.tabMuon.Controls.Add(this.lblDieuKien);
            this.tabMuon.Controls.Add(this.lblNhanVien); this.tabMuon.Controls.Add(this.txtNhanVien);
            this.tabMuon.Controls.Add(this.lblNgayMuon); this.tabMuon.Controls.Add(this.dtNgayMuon);
            this.tabMuon.Controls.Add(this.lblHenTra); this.tabMuon.Controls.Add(this.dtHenTra);
            this.tabMuon.Controls.Add(this.lblKho); this.tabMuon.Controls.Add(this.dgvKho);
            this.tabMuon.Controls.Add(this.btnThem); this.tabMuon.Controls.Add(this.btnBo);
            this.tabMuon.Controls.Add(this.lblDaChon); this.tabMuon.Controls.Add(this.dgvDaChon);
            this.tabMuon.Controls.Add(this.btnLapPhieuMuon);
        }

        private void XayDungTabTra()
        {
            this.lblDocGiaTra = new System.Windows.Forms.Label { Text = "Mã độc giả:", Location = new System.Drawing.Point(20, 25), AutoSize = true };
            this.txtDocGiaTra = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(120, 22), Width = 120 };
            this.btnTimSachDangMuon = new System.Windows.Forms.Button { Text = "Tìm sách đang mượn", Location = new System.Drawing.Point(260, 20), Width = 180 };
            this.btnTimSachDangMuon.Click += new System.EventHandler(this.btnTimSachDangMuon_Click);

            this.lblNhanVienTra = new System.Windows.Forms.Label { Text = "NV xử lý:", Location = new System.Drawing.Point(460, 25), AutoSize = true };
            this.txtNhanVienTra = new System.Windows.Forms.TextBox { Location = new System.Drawing.Point(540, 22), Width = 100 };

            this.dgvDangMuon = new System.Windows.Forms.DataGridView
            {
                Location = new System.Drawing.Point(20, 65),
                Size = new System.Drawing.Size(900, 300),
                ReadOnly = true,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            this.dgvDangMuon.SelectionChanged += new System.EventHandler(this.dgvDangMuon_SelectionChanged);

            this.lblTinhTrang = new System.Windows.Forms.Label { Text = "Tình trạng trả:", Location = new System.Drawing.Point(20, 385), AutoSize = true };
            this.cboTinhTrang = new System.Windows.Forms.ComboBox { Location = new System.Drawing.Point(140, 382), Width = 160, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            this.cboTinhTrang.SelectedIndexChanged += new System.EventHandler(this.cboTinhTrang_SelectedIndexChanged);

            this.lblPhiPhat = new System.Windows.Forms.Label { Text = "Phí phạt dự kiến:", Location = new System.Drawing.Point(330, 385), AutoSize = true };
            this.lblPhiPhatGiaTri = new System.Windows.Forms.Label
            {
                Text = "0 đ",
                Location = new System.Drawing.Point(460, 385),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold)
            };

            this.btnXacNhanTra = new System.Windows.Forms.Button { Text = "Xác nhận trả sách", Location = new System.Drawing.Point(740, 380), Width = 180 };
            this.btnXacNhanTra.Click += new System.EventHandler(this.btnXacNhanTra_Click);

            this.tabTra.Controls.Add(this.lblDocGiaTra); this.tabTra.Controls.Add(this.txtDocGiaTra);
            this.tabTra.Controls.Add(this.btnTimSachDangMuon);
            this.tabTra.Controls.Add(this.lblNhanVienTra); this.tabTra.Controls.Add(this.txtNhanVienTra);
            this.tabTra.Controls.Add(this.dgvDangMuon);
            this.tabTra.Controls.Add(this.lblTinhTrang); this.tabTra.Controls.Add(this.cboTinhTrang);
            this.tabTra.Controls.Add(this.lblPhiPhat); this.tabTra.Controls.Add(this.lblPhiPhatGiaTri);
            this.tabTra.Controls.Add(this.btnXacNhanTra);
        }
    }
}
