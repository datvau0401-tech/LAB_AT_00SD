using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Quản lý Khách hàng, lập Phiếu đặt phòng và Nhận phòng / Người lưu trú.
    /// </summary>
    public class FrmDatPhong : Form
    {
        private enum View { KhachHang, DatPhong, NhanPhong }
        private View _current = View.KhachHang;

        private Panel topNav;
        private Panel pnlKhachHang, pnlDatPhong, pnlNhanPhong;

        // ---- Khách hàng ----
        private TextBox txtMaKhach, txtHoTenKH, txtCmndKH, txtQuocTichKH, txtSdtKH;
        private Button btnThemKH, btnSuaKH, btnXoaKH;
        private DataGridView gridKhachHang;

        // ---- Đặt phòng ----
        private TextBox txtSoPhieuDat, txtTienCoc;
        private ComboBox cboKhachDP, cboNhanVienDP, cboKenhDat;
        private DateTimePicker dtpNgayNhan, dtpNgayTraDuKien;
        private DataGridView gridPhongTrong, gridPhongChon;
        private Button btnThemPhong, btnBoChonPhong, btnLapPhieuDat;
        private DataGridView gridDanhSachPhieuDat;

        // ---- Nhận phòng ----
        private DataGridView gridPhieuChoNhan, gridPhongCuaPhieu;
        private TextBox txtHoTenNLT, txtCmndNLT, txtQuocTichNLT;
        private Button btnThemNguoiLuuTru, btnNhanPhong;

        public FrmDatPhong()
        {
            InitializeComponent();
            LoadView(View.KhachHang);
        }

        private void InitializeComponent()
        {
            this.Text = "Khách hàng - Đặt phòng - Nhận phòng";
            this.Size = new Size(1100, 720);
            this.StartPosition = FormStartPosition.CenterScreen;

            topNav = new Panel { Dock = DockStyle.Top, Height = 40 };
            string[] names = { "Khách hàng", "Đặt phòng", "Nhận phòng / Người lưu trú" };
            View[] views = { View.KhachHang, View.DatPhong, View.NhanPhong };
            int x = 10;
            for (int i = 0; i < names.Length; i++)
            {
                View v = views[i];
                Button btn = new Button { Text = "[ " + names[i] + " ]", FlatStyle = FlatStyle.Flat, Left = x, Top = 5, AutoSize = true };
                btn.Click += delegate { LoadView(v); };
                topNav.Controls.Add(btn);
                x += btn.Width + 10;
            }
            this.Controls.Add(topNav);

            BuildKhachHangPanel();
            BuildDatPhongPanel();
            BuildNhanPhongPanel();

            this.Controls.Add(pnlNhanPhong);
            this.Controls.Add(pnlDatPhong);
            this.Controls.Add(pnlKhachHang);
        }

        // ===================== KHÁCH HÀNG =====================
        private void BuildKhachHangPanel()
        {
            pnlKhachHang = new Panel { Dock = DockStyle.Fill };

            Panel input = new Panel { Dock = DockStyle.Top, Height = 80 };
            Label l1 = new Label { Text = "Mã khách:", Left = 15, Top = 15, AutoSize = true };
            txtMaKhach = new TextBox { Left = 95, Top = 12, Width = 100 };
            Label l2 = new Label { Text = "Họ tên:", Left = 210, Top = 15, AutoSize = true };
            txtHoTenKH = new TextBox { Left = 265, Top = 12, Width = 180 };
            Label l3 = new Label { Text = "Số CMND:", Left = 460, Top = 15, AutoSize = true };
            txtCmndKH = new TextBox { Left = 535, Top = 12, Width = 130 };
            Label l4 = new Label { Text = "Quốc tịch:", Left = 15, Top = 45, AutoSize = true };
            txtQuocTichKH = new TextBox { Left = 95, Top = 42, Width = 150 };
            Label l5 = new Label { Text = "Số điện thoại:", Left = 260, Top = 45, AutoSize = true };
            txtSdtKH = new TextBox { Left = 355, Top = 42, Width = 140 };

            btnThemKH = new Button { Text = "Thêm", Left = 685, Top = 10, Width = 85 };
            btnThemKH.Click += BtnThemKH_Click;
            btnSuaKH = new Button { Text = "Sửa", Left = 685, Top = 42, Width = 85 };
            btnSuaKH.Click += BtnSuaKH_Click;
            btnXoaKH = new Button { Text = "Xóa", Left = 780, Top = 42, Width = 85 };
            btnXoaKH.Click += BtnXoaKH_Click;

            input.Controls.AddRange(new Control[] { l1, txtMaKhach, l2, txtHoTenKH, l3, txtCmndKH, l4, txtQuocTichKH, l5, txtSdtKH, btnThemKH, btnSuaKH, btnXoaKH });

            gridKhachHang = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridKhachHang.Columns.Add("Ma", "Mã khách");
            gridKhachHang.Columns.Add("Ten", "Họ tên");
            gridKhachHang.Columns.Add("Cmnd", "Số CMND");
            gridKhachHang.Columns.Add("QuocTich", "Quốc tịch");
            gridKhachHang.Columns.Add("Sdt", "Số điện thoại");
            gridKhachHang.CellClick += GridKhachHang_CellClick;

            pnlKhachHang.Controls.Add(gridKhachHang);
            pnlKhachHang.Controls.Add(input);
        }

        private void LoadKhachHang()
        {
            gridKhachHang.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable("SELECT MaKhach, HoTen, SoCMND, QuocTich, SoDienThoai FROM KhachHang ORDER BY MaKhach");
            foreach (DataRow r in dt.Rows)
                gridKhachHang.Rows.Add(r["MaKhach"], r["HoTen"], r["SoCMND"], r["QuocTich"], r["SoDienThoai"]);
        }

        private void GridKhachHang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = gridKhachHang.Rows[e.RowIndex];
            txtMaKhach.Text = Convert.ToString(row.Cells["Ma"].Value);
            txtHoTenKH.Text = Convert.ToString(row.Cells["Ten"].Value);
            txtCmndKH.Text = Convert.ToString(row.Cells["Cmnd"].Value);
            txtQuocTichKH.Text = Convert.ToString(row.Cells["QuocTich"].Value);
            txtSdtKH.Text = Convert.ToString(row.Cells["Sdt"].Value);
        }

        private void BtnThemKH_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) return;
            try
            {
                DbHelper.ExecuteNonQuery(
                    "INSERT INTO KhachHang(MaKhach, HoTen, SoCMND, QuocTich, SoDienThoai) VALUES(@ma,@ten,@cmnd,@qt,@dt)",
                    new SqlParameter("@ma", txtMaKhach.Text.Trim()),
                    new SqlParameter("@ten", txtHoTenKH.Text.Trim()),
                    new SqlParameter("@cmnd", txtCmndKH.Text.Trim()),
                    new SqlParameter("@qt", txtQuocTichKH.Text.Trim()),
                    new SqlParameter("@dt", txtSdtKH.Text.Trim()));
                LoadKhachHang();
                MessageBox.Show(this, "Thêm khách hàng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnSuaKH_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) return;
            try
            {
                DbHelper.ExecuteNonQuery(
                    "UPDATE KhachHang SET HoTen=@ten, SoCMND=@cmnd, QuocTich=@qt, SoDienThoai=@dt WHERE MaKhach=@ma",
                    new SqlParameter("@ten", txtHoTenKH.Text.Trim()),
                    new SqlParameter("@cmnd", txtCmndKH.Text.Trim()),
                    new SqlParameter("@qt", txtQuocTichKH.Text.Trim()),
                    new SqlParameter("@dt", txtSdtKH.Text.Trim()),
                    new SqlParameter("@ma", txtMaKhach.Text.Trim()));
                LoadKhachHang();
                MessageBox.Show(this, "Cập nhật thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnXoaKH_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKhach.Text)) return;
            if (MessageBox.Show(this, "Xóa khách hàng đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                DbHelper.ExecuteNonQuery("DELETE FROM KhachHang WHERE MaKhach=@ma", new SqlParameter("@ma", txtMaKhach.Text.Trim()));
                LoadKhachHang();
            }
            catch (Exception ex) { MessageBox.Show(this, "Lỗi (có thể do dữ liệu đang được tham chiếu): " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ===================== ĐẶT PHÒNG =====================
        private void BuildDatPhongPanel()
        {
            pnlDatPhong = new Panel { Dock = DockStyle.Fill };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 75 };
            Label l1 = new Label { Text = "Số phiếu đặt:", Left = 15, Top = 15, AutoSize = true };
            txtSoPhieuDat = new TextBox { Left = 105, Top = 12, Width = 100 };
            Label l2 = new Label { Text = "Khách:", Left = 220, Top = 15, AutoSize = true };
            cboKhachDP = new ComboBox { Left = 270, Top = 12, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
            Label l3 = new Label { Text = "Kênh đặt:", Left = 455, Top = 15, AutoSize = true };
            cboKenhDat = new ComboBox { Left = 520, Top = 12, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cboKenhDat.Items.AddRange(new object[] { "Điện thoại", "Website", "Trực tiếp" });
            Label l4 = new Label { Text = "Tiền cọc:", Left = 655, Top = 15, AutoSize = true };
            txtTienCoc = new TextBox { Left = 720, Top = 12, Width = 100 };

            Label l5 = new Label { Text = "Ngày nhận:", Left = 15, Top = 45, AutoSize = true };
            dtpNgayNhan = new DateTimePicker { Left = 90, Top = 42, Width = 120, Format = DateTimePickerFormat.Short };
            Label l6 = new Label { Text = "Ngày trả dự kiến:", Left = 225, Top = 45, AutoSize = true };
            dtpNgayTraDuKien = new DateTimePicker { Left = 335, Top = 42, Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(1) };
            Label l7 = new Label { Text = "Nhân viên lễ tân:", Left = 470, Top = 45, AutoSize = true };
            cboNhanVienDP = new ComboBox { Left = 580, Top = 42, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            header.Controls.AddRange(new Control[] { l1, txtSoPhieuDat, l2, cboKhachDP, l3, cboKenhDat, l4, txtTienCoc, l5, dtpNgayNhan, l6, dtpNgayTraDuKien, l7, cboNhanVienDP });

            TableLayoutPanel grids = new TableLayoutPanel { Dock = DockStyle.Top, Height = 260, ColumnCount = 3, RowCount = 1 };
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12f));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));

            gridPhongTrong = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridPhongTrong.Columns.Add("Phong", "Phòng");
            gridPhongTrong.Columns.Add("Khu", "Khu");
            gridPhongTrong.Columns.Add("SucChua", "Sức chứa");
            gridPhongTrong.Columns.Add("DonGia", "Đơn giá");

            Panel midButtons = new Panel { Dock = DockStyle.Fill };
            btnThemPhong = new Button { Text = "Thêm phòng >>", Left = 5, Top = 90, Width = 130 };
            btnThemPhong.Click += BtnThemPhong_Click;
            btnBoChonPhong = new Button { Text = "<< Bỏ chọn", Left = 5, Top = 130, Width = 130 };
            btnBoChonPhong.Click += BtnBoChonPhong_Click;
            midButtons.Controls.Add(btnThemPhong);
            midButtons.Controls.Add(btnBoChonPhong);

            gridPhongChon = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridPhongChon.Columns.Add("Phong", "Phòng chọn");
            DataGridViewTextBoxColumn colSoNguoi = new DataGridViewTextBoxColumn { Name = "SoNguoi", HeaderText = "Số người" };
            gridPhongChon.Columns.Add(colSoNguoi);
            gridPhongChon.Columns.Add("DonGia", "Đơn giá/ngày");
            gridPhongChon.Columns["Phong"].ReadOnly = true;
            gridPhongChon.Columns["DonGia"].ReadOnly = true;

            grids.Controls.Add(gridPhongTrong, 0, 0);
            grids.Controls.Add(midButtons, 1, 0);
            grids.Controls.Add(gridPhongChon, 2, 0);

            btnLapPhieuDat = new Button { Text = "Lập phiếu đặt", Dock = DockStyle.Top, Height = 32 };
            btnLapPhieuDat.Click += BtnLapPhieuDat_Click;

            Label lblDs = new Label { Text = "Phiếu đặt phòng:", Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(5, 0, 0, 0) };

            gridDanhSachPhieuDat = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridDanhSachPhieuDat.Columns.Add("SoPhieu", "Số phiếu");
            gridDanhSachPhieuDat.Columns.Add("Khach", "Khách");
            gridDanhSachPhieuDat.Columns.Add("NgayNhan", "Ngày nhận");
            gridDanhSachPhieuDat.Columns.Add("NgayTra", "Ngày trả dự kiến");
            gridDanhSachPhieuDat.Columns.Add("Coc", "Cọc");
            gridDanhSachPhieuDat.Columns.Add("Kenh", "Kênh");
            gridDanhSachPhieuDat.Columns.Add("TrangThai", "Trạng thái");

            pnlDatPhong.Controls.Add(gridDanhSachPhieuDat);
            pnlDatPhong.Controls.Add(lblDs);
            pnlDatPhong.Controls.Add(btnLapPhieuDat);
            pnlDatPhong.Controls.Add(grids);
            pnlDatPhong.Controls.Add(header);
        }

        private void LoadComboDatPhong()
        {
            DataTable khach = DbHelper.GetDataTable("SELECT MaKhach, HoTen FROM KhachHang ORDER BY HoTen");
            cboKhachDP.DataSource = khach.Copy();
            cboKhachDP.DisplayMember = "HoTen";
            cboKhachDP.ValueMember = "MaKhach";

            DataTable nv = DbHelper.GetDataTable("SELECT MaNV, HoTen FROM NhanVien ORDER BY MaNV");
            cboNhanVienDP.DataSource = nv.Copy();
            cboNhanVienDP.DisplayMember = "HoTen";
            cboNhanVienDP.ValueMember = "MaNV";
        }

        private void LoadPhongTrong()
        {
            gridPhongTrong.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT p.SoPhong, k.TenKhuVuc, p.SoNguoiToiDa, p.DonGiaNgay
                  FROM Phong p JOIN KhuVuc k ON p.MaKhuVuc = k.MaKhuVuc
                  WHERE p.TrangThai = N'Trống' ORDER BY p.SoPhong");
            foreach (DataRow r in dt.Rows)
                gridPhongTrong.Rows.Add(r["SoPhong"], r["TenKhuVuc"], r["SoNguoiToiDa"], r["DonGiaNgay"]);
        }

        private void LoadDanhSachPhieuDat()
        {
            gridDanhSachPhieuDat.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT pd.SoPhieuDat, kh.HoTen, pd.NgayNhan, pd.NgayTraDuKien, pd.TienCoc, pd.KenhDat, pd.TrangThai
                  FROM PhieuDatPhong pd JOIN KhachHang kh ON pd.MaKhach = kh.MaKhach
                  ORDER BY pd.NgayLap DESC");
            foreach (DataRow r in dt.Rows)
                gridDanhSachPhieuDat.Rows.Add(r["SoPhieuDat"], r["HoTen"],
                    Convert.ToDateTime(r["NgayNhan"]).ToString("dd/MM/yyyy"),
                    Convert.ToDateTime(r["NgayTraDuKien"]).ToString("dd/MM/yyyy"),
                    r["TienCoc"], r["KenhDat"], r["TrangThai"]);
        }

        private void BtnThemPhong_Click(object sender, EventArgs e)
        {
            if (gridPhongTrong.CurrentRow == null) return;
            string soPhong = Convert.ToString(gridPhongTrong.CurrentRow.Cells["Phong"].Value);
            foreach (DataGridViewRow r in gridPhongChon.Rows)
                if (Convert.ToString(r.Cells["Phong"].Value) == soPhong) return; // đã chọn rồi

            object donGia = gridPhongTrong.CurrentRow.Cells["DonGia"].Value;
            gridPhongChon.Rows.Add(soPhong, 1, donGia);
        }

        private void BtnBoChonPhong_Click(object sender, EventArgs e)
        {
            if (gridPhongChon.CurrentRow != null) gridPhongChon.Rows.Remove(gridPhongChon.CurrentRow);
        }

        private void BtnLapPhieuDat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSoPhieuDat.Text) || cboKhachDP.SelectedValue == null || gridPhongChon.Rows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng nhập Số phiếu đặt, chọn Khách và ít nhất một Phòng.",
                    "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                DbHelper.ExecuteNonQuery(
                    @"INSERT INTO PhieuDatPhong(SoPhieuDat, MaKhach, MaNVLeTan, NgayLap, NgayNhan, NgayTraDuKien, TienCoc, KenhDat, TrangThai)
                      VALUES(@sp,@kh,@nv,@ngaylap,@nhan,@tra,@coc,@kenh,N'Đã đặt')",
                    new SqlParameter("@sp", txtSoPhieuDat.Text.Trim()),
                    new SqlParameter("@kh", cboKhachDP.SelectedValue),
                    new SqlParameter("@nv", cboNhanVienDP.SelectedValue),
                    new SqlParameter("@ngaylap", DateTime.Now),
                    new SqlParameter("@nhan", dtpNgayNhan.Value.Date),
                    new SqlParameter("@tra", dtpNgayTraDuKien.Value.Date),
                    new SqlParameter("@coc", ParseDecimal(txtTienCoc.Text)),
                    new SqlParameter("@kenh", cboKenhDat.Text == "" ? "Website" : cboKenhDat.Text));

                foreach (DataGridViewRow r in gridPhongChon.Rows)
                {
                    string soPhong = Convert.ToString(r.Cells["Phong"].Value);
                    int soNguoi = ParseInt(Convert.ToString(r.Cells["SoNguoi"].Value));
                    if (soNguoi <= 0) soNguoi = 1;

                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO ChiTietDatPhong(SoPhieuDat, SoPhong, SoNguoi) VALUES(@sp,@ph,@sn)",
                        new SqlParameter("@sp", txtSoPhieuDat.Text.Trim()),
                        new SqlParameter("@ph", soPhong),
                        new SqlParameter("@sn", soNguoi));

                    DbHelper.ExecuteNonQuery(
                        "UPDATE Phong SET TrangThai=N'Đã đặt' WHERE SoPhong=@ph",
                        new SqlParameter("@ph", soPhong));
                }

                MessageBox.Show(this, "Lập phiếu đặt phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSoPhieuDat.Clear();
                txtTienCoc.Clear();
                gridPhongChon.Rows.Clear();
                LoadPhongTrong();
                LoadDanhSachPhieuDat();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== NHẬN PHÒNG / NGƯỜI LƯU TRÚ =====================
        private void BuildNhanPhongPanel()
        {
            pnlNhanPhong = new Panel { Dock = DockStyle.Fill };

            Label lbl1 = new Label { Text = "Phiếu đặt phòng đang chờ nhận:", Dock = DockStyle.Top, Height = 22, Padding = new Padding(5, 3, 0, 0) };
            gridPhieuChoNhan = new DataGridView { Dock = DockStyle.Top, Height = 180, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridPhieuChoNhan.Columns.Add("SoPhieu", "Số phiếu");
            gridPhieuChoNhan.Columns.Add("Khach", "Khách");
            gridPhieuChoNhan.Columns.Add("NgayNhan", "Ngày nhận dự kiến");
            gridPhieuChoNhan.CellClick += GridPhieuChoNhan_CellClick;

            btnNhanPhong = new Button { Text = "Nhận phòng (Check-in)", Dock = DockStyle.Top, Height = 32 };
            btnNhanPhong.Click += BtnNhanPhong_Click;

            Label lbl2 = new Label { Text = "Phòng thuộc phiếu đã chọn:", Dock = DockStyle.Top, Height = 22, Padding = new Padding(5, 3, 0, 0) };
            gridPhongCuaPhieu = new DataGridView { Dock = DockStyle.Top, Height = 130, ReadOnly = true, AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            gridPhongCuaPhieu.Columns.Add("Phong", "Phòng");
            gridPhongCuaPhieu.Columns.Add("SoNguoi", "Số người");

            GroupBox gbNguoiLuuTru = new GroupBox { Text = "Thêm người lưu trú cho phòng đã chọn ở trên", Dock = DockStyle.Fill };
            Label l1 = new Label { Text = "Họ tên:", Left = 15, Top = 30, AutoSize = true };
            txtHoTenNLT = new TextBox { Left = 75, Top = 27, Width = 180 };
            Label l2 = new Label { Text = "Số CMND:", Left = 270, Top = 30, AutoSize = true };
            txtCmndNLT = new TextBox { Left = 340, Top = 27, Width = 130 };
            Label l3 = new Label { Text = "Quốc tịch:", Left = 485, Top = 30, AutoSize = true };
            txtQuocTichNLT = new TextBox { Left = 550, Top = 27, Width = 150 };
            btnThemNguoiLuuTru = new Button { Text = "Thêm người", Left = 715, Top = 25, Width = 110 };
            btnThemNguoiLuuTru.Click += BtnThemNguoiLuuTru_Click;
            gbNguoiLuuTru.Controls.AddRange(new Control[] { l1, txtHoTenNLT, l2, txtCmndNLT, l3, txtQuocTichNLT, btnThemNguoiLuuTru });

            pnlNhanPhong.Controls.Add(gbNguoiLuuTru);
            pnlNhanPhong.Controls.Add(gridPhongCuaPhieu);
            pnlNhanPhong.Controls.Add(lbl2);
            pnlNhanPhong.Controls.Add(btnNhanPhong);
            pnlNhanPhong.Controls.Add(gridPhieuChoNhan);
            pnlNhanPhong.Controls.Add(lbl1);
        }

        private void LoadPhieuChoNhan()
        {
            gridPhieuChoNhan.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT pd.SoPhieuDat, kh.HoTen, pd.NgayNhan
                  FROM PhieuDatPhong pd JOIN KhachHang kh ON pd.MaKhach = kh.MaKhach
                  WHERE pd.TrangThai = N'Đã đặt' ORDER BY pd.NgayNhan");
            foreach (DataRow r in dt.Rows)
                gridPhieuChoNhan.Rows.Add(r["SoPhieuDat"], r["HoTen"], Convert.ToDateTime(r["NgayNhan"]).ToString("dd/MM/yyyy"));
            gridPhongCuaPhieu.Rows.Clear();
        }

        private void GridPhieuChoNhan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string soPhieu = Convert.ToString(gridPhieuChoNhan.Rows[e.RowIndex].Cells["SoPhieu"].Value);
            gridPhongCuaPhieu.Rows.Clear();
            try
            {
                DataTable dt = DbHelper.GetDataTable(
                    "SELECT SoPhong, SoNguoi FROM ChiTietDatPhong WHERE SoPhieuDat=@sp",
                    new SqlParameter("@sp", soPhieu));
                foreach (DataRow r in dt.Rows)
                    gridPhongCuaPhieu.Rows.Add(r["SoPhong"], r["SoNguoi"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi khi tải phòng của phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnThemNguoiLuuTru_Click(object sender, EventArgs e)
        {
            if (gridPhieuChoNhan.CurrentRow == null || gridPhongCuaPhieu.CurrentRow == null)
            {
                MessageBox.Show(this, "Vui lòng chọn Phiếu đặt và Phòng trước.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtHoTenNLT.Text)) return;

            string soPhieu = Convert.ToString(gridPhieuChoNhan.CurrentRow.Cells["SoPhieu"].Value);
            string soPhong = Convert.ToString(gridPhongCuaPhieu.CurrentRow.Cells["Phong"].Value);
            try
            {
                DbHelper.ExecuteNonQuery(
                    "INSERT INTO NguoiLuuTru(SoPhieuDat, SoPhong, HoTen, SoCMND, QuocTich) VALUES(@sp,@ph,@ten,@cmnd,@qt)",
                    new SqlParameter("@sp", soPhieu),
                    new SqlParameter("@ph", soPhong),
                    new SqlParameter("@ten", txtHoTenNLT.Text.Trim()),
                    new SqlParameter("@cmnd", txtCmndNLT.Text.Trim()),
                    new SqlParameter("@qt", txtQuocTichNLT.Text.Trim()));

                MessageBox.Show(this, "Đã thêm người lưu trú.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtHoTenNLT.Clear(); txtCmndNLT.Clear(); txtQuocTichNLT.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNhanPhong_Click(object sender, EventArgs e)
        {
            if (gridPhieuChoNhan.CurrentRow == null) return;
            string soPhieu = Convert.ToString(gridPhieuChoNhan.CurrentRow.Cells["SoPhieu"].Value);
            if (MessageBox.Show(this, "Xác nhận nhận phòng cho phiếu " + soPhieu + "?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                DbHelper.ExecuteNonQuery(
                    "UPDATE PhieuDatPhong SET TrangThai=N'Đang ở', NgayNhanThucTe=@now WHERE SoPhieuDat=@sp",
                    new SqlParameter("@now", DateTime.Now),
                    new SqlParameter("@sp", soPhieu));

                DataTable phongs = DbHelper.GetDataTable(
                    "SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat=@sp",
                    new SqlParameter("@sp", soPhieu));
                foreach (DataRow r in phongs.Rows)
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@ph",
                        new SqlParameter("@ph", r["SoPhong"]));
                }

                MessageBox.Show(this, "Nhận phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadPhieuChoNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== ĐIỀU HƯỚNG =====================
        private void LoadView(View v)
        {
            _current = v;
            pnlKhachHang.Visible = v == View.KhachHang;
            pnlDatPhong.Visible = v == View.DatPhong;
            pnlNhanPhong.Visible = v == View.NhanPhong;

            try
            {
                if (v == View.KhachHang) LoadKhachHang();
                else if (v == View.DatPhong)
                {
                    LoadComboDatPhong();
                    LoadPhongTrong();
                    LoadDanhSachPhieuDat();
                    gridPhongChon.Rows.Clear();
                }
                else LoadPhieuChoNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static int ParseInt(string s)
        {
            int result;
            int.TryParse(s, out result);
            return result;
        }

        private static decimal ParseDecimal(string s)
        {
            decimal result;
            decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }
    }
}