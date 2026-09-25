using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Quản lý Phòng, Tiện nghi và lập Phiếu lắp đặt / luân chuyển tiện nghi.
    /// Khung trên: đổi giữa "Phòng" và "Tiện nghi" (kèm lưới + Thêm/Sửa/Xóa).
    /// Khung dưới: luôn hiển thị, dùng để lập nhanh phiếu lắp đặt tiện nghi vào phòng.
    /// </summary>
    public class FrmPhongTienNghi : Form
    {
        private enum View { Phong, TienNghi, LichSuLapDat }
        private View _current = View.Phong;

        // Vùng nhập chính (đổi nhãn theo view)
        private Label lblF1, lblF2, lblF3, lblF4, lblF5;
        private TextBox txtF1, txtF3, txtF4;
        private ComboBox cboF2, cboF5;
        private Button btnThem, btnSua, btnXoa;
        private DataGridView grid;

        // Khung lập phiếu lắp đặt (luôn hiển thị)
        private TextBox txtSoPhieuLapDat, txtTinhTrangLD, txtGhiChuLD;
        private ComboBox cboTienNghiLD, cboPhongLD, cboNhanVienLD;
        private DateTimePicker dtpNgayLapLD;
        private Button btnLapPhieu;

        public FrmPhongTienNghi()
        {
            InitializeComponent();
            LoadView(View.Phong);
        }

        private void InitializeComponent()
        {
            this.Text = "Phòng - Tiện nghi - Phiếu lắp đặt";
            this.Size = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ----- Top nav -----
            Panel topNav = new Panel { Dock = DockStyle.Top, Height = 40 };
            string[] names = { "Phòng", "Tiện nghi", "Lắp đặt / luân chuyển" };
            View[] views = { View.Phong, View.TienNghi, View.LichSuLapDat };
            int x = 10;
            for (int i = 0; i < names.Length; i++)
            {
                View v = views[i];
                Button btn = new Button { Text = "[ " + names[i] + " ]", FlatStyle = FlatStyle.Flat, Left = x, Top = 5, AutoSize = true };
                btn.Click += delegate { LoadView(v); };
                topNav.Controls.Add(btn);
                x += btn.Width + 10;
            }

            // ----- Input panel chính -----
            Panel inputPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            lblF1 = new Label { Text = "Số phòng:", Left = 15, Top = 12, AutoSize = true };
            txtF1 = new TextBox { Left = 95, Top = 9, Width = 100 };
            lblF2 = new Label { Text = "Khu vực:", Left = 210, Top = 12, AutoSize = true };
            cboF2 = new ComboBox { Left = 280, Top = 9, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            lblF3 = new Label { Text = "Số người tối đa:", Left = 425, Top = 12, AutoSize = true };
            txtF3 = new TextBox { Left = 535, Top = 9, Width = 70 };
            lblF4 = new Label { Text = "Đơn giá/ngày:", Left = 620, Top = 12, AutoSize = true };
            txtF4 = new TextBox { Left = 715, Top = 9, Width = 100 };
            lblF5 = new Label { Text = "Trạng thái:", Left = 15, Top = 12, AutoSize = true, Visible = false };
            cboF5 = new ComboBox { Left = 95, Top = 9, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            cboF5.Items.AddRange(new object[] { "Trống", "Đã đặt", "Đang ở", "Bảo trì" });

            btnThem = new Button { Text = "Thêm", Left = 825, Top = 7, Width = 75 };
            btnThem.Click += BtnThem_Click;
            btnSua = new Button { Text = "Sửa", Left = 905, Top = 7, Width = 60 };
            btnSua.Click += BtnSua_Click;

            inputPanel.Controls.AddRange(new Control[]
            {
                lblF1, txtF1, lblF2, cboF2, lblF3, txtF3, lblF4, txtF4, lblF5, cboF5, btnThem, btnSua
            });

            Panel deletePanel = new Panel { Dock = DockStyle.Top, Height = 30 };
            btnXoa = new Button { Text = "Xóa mục đang chọn", Left = 15, Top = 2, Width = 150 };
            btnXoa.Click += BtnXoa_Click;
            deletePanel.Controls.Add(btnXoa);

            // ----- Grid -----
            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.CellClick += Grid_CellClick;

            // ----- Khung lập phiếu lắp đặt (đáy, luôn hiển thị) -----
            GroupBox gbLapDat = new GroupBox { Text = "Lập phiếu lắp đặt / luân chuyển tiện nghi", Dock = DockStyle.Bottom, Height = 110 };

            Label l1 = new Label { Text = "Phiếu lắp đặt:", Left = 15, Top = 28, AutoSize = true };
            txtSoPhieuLapDat = new TextBox { Left = 115, Top = 25, Width = 100 };
            Label l2 = new Label { Text = "Tiện nghi:", Left = 235, Top = 28, AutoSize = true };
            cboTienNghiLD = new ComboBox { Left = 305, Top = 25, Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            Label l3 = new Label { Text = "Phòng:", Left = 435, Top = 28, AutoSize = true };
            cboPhongLD = new ComboBox { Left = 485, Top = 25, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            Label l4 = new Label { Text = "Tình trạng:", Left = 600, Top = 28, AutoSize = true };
            txtTinhTrangLD = new TextBox { Left = 675, Top = 25, Width = 110 };

            Label l5 = new Label { Text = "Ngày lập:", Left = 15, Top = 65, AutoSize = true };
            dtpNgayLapLD = new DateTimePicker { Left = 85, Top = 62, Width = 120, Format = DateTimePickerFormat.Short };
            Label l6 = new Label { Text = "Nhân viên:", Left = 220, Top = 65, AutoSize = true };
            cboNhanVienLD = new ComboBox { Left = 295, Top = 62, Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            Label l7 = new Label { Text = "Ghi chú:", Left = 440, Top = 65, AutoSize = true };
            txtGhiChuLD = new TextBox { Left = 500, Top = 62, Width = 285 };

            btnLapPhieu = new Button { Text = "Lập phiếu", Left = 800, Top = 60, Width = 100, Height = 30 };
            btnLapPhieu.Click += BtnLapPhieu_Click;

            gbLapDat.Controls.AddRange(new Control[]
            {
                l1, txtSoPhieuLapDat, l2, cboTienNghiLD, l3, cboPhongLD, l4, txtTinhTrangLD,
                l5, dtpNgayLapLD, l6, cboNhanVienLD, l7, txtGhiChuLD, btnLapPhieu
            });

            this.Controls.Add(grid);
            this.Controls.Add(deletePanel);
            this.Controls.Add(inputPanel);
            this.Controls.Add(topNav);
            this.Controls.Add(gbLapDat);

            LoadComboDanhMucChung();
        }

        private void LoadComboDanhMucChung()
        {
            DataTable khuVuc = DbHelper.GetDataTable("SELECT MaKhuVuc, TenKhuVuc FROM KhuVuc ORDER BY MaKhuVuc");
            cboF2.DataSource = khuVuc.Copy();
            cboF2.DisplayMember = "TenKhuVuc";
            cboF2.ValueMember = "MaKhuVuc";

            DataTable phong = DbHelper.GetDataTable("SELECT SoPhong FROM Phong ORDER BY SoPhong");
            cboPhongLD.DataSource = phong.Copy();
            cboPhongLD.DisplayMember = "SoPhong";
            cboPhongLD.ValueMember = "SoPhong";

            DataTable tienNghi = DbHelper.GetDataTable("SELECT MaTienNghi FROM TienNghi ORDER BY MaTienNghi");
            cboTienNghiLD.DataSource = tienNghi.Copy();
            cboTienNghiLD.DisplayMember = "MaTienNghi";
            cboTienNghiLD.ValueMember = "MaTienNghi";

            DataTable nv = DbHelper.GetDataTable("SELECT MaNV, HoTen FROM NhanVien ORDER BY MaNV");
            cboNhanVienLD.DataSource = nv.Copy();
            cboNhanVienLD.DisplayMember = "HoTen";
            cboNhanVienLD.ValueMember = "MaNV";
        }

        private void LoadView(View v)
        {
            _current = v;
            txtF1.Clear(); txtF3.Clear(); txtF4.Clear();
            grid.Columns.Clear();

            bool isPhong = v == View.Phong;
            bool isTienNghi = v == View.TienNghi;
            bool isLichSu = v == View.LichSuLapDat;

            lblF1.Visible = !isLichSu;
            txtF1.Visible = !isLichSu;
            lblF2.Visible = !isLichSu;
            cboF2.Visible = !isLichSu;
            lblF3.Visible = !isLichSu;
            txtF3.Visible = !isLichSu;
            lblF4.Visible = !isLichSu;
            txtF4.Visible = !isLichSu;
            lblF5.Visible = isPhong;
            cboF5.Visible = isPhong;
            btnThem.Visible = !isLichSu;
            btnSua.Visible = !isLichSu;
            btnXoa.Visible = !isLichSu;

            if (isPhong)
            {
                lblF1.Text = "Số phòng:";
                lblF2.Text = "Khu vực:";
                cboF2.Visible = true;
                lblF3.Text = "Số người tối đa:";
                lblF4.Text = "Đơn giá/ngày:";
                grid.Columns.Add("C1", "Phòng");
                grid.Columns.Add("C2", "Khu");
                grid.Columns.Add("C3", "Sức chứa");
                grid.Columns.Add("C4", "Đơn giá");
                grid.Columns.Add("C5", "Trạng thái");
                LoadPhong();
            }
            else if (isTienNghi)
            {
                lblF1.Text = "Mã tiện nghi:";
                lblF2.Text = "Loại tiện nghi:";
                cboF2.DataSource = DbHelper.GetDataTable("SELECT MaLoaiTN, TenLoaiTN FROM LoaiTienNghi ORDER BY MaLoaiTN").Copy();
                cboF2.DisplayMember = "TenLoaiTN";
                cboF2.ValueMember = "MaLoaiTN";
                lblF3.Text = "Số thứ tự:";
                lblF4.Text = "Tình trạng hiện tại:";
                grid.Columns.Add("C1", "Mã tiện nghi");
                grid.Columns.Add("C2", "Loại");
                grid.Columns.Add("C3", "STT");
                grid.Columns.Add("C4", "Tình trạng");
                LoadTienNghi();
            }
            else
            {
                grid.Columns.Add("C1", "Phiếu lắp đặt");
                grid.Columns.Add("C2", "Tiện nghi");
                grid.Columns.Add("C3", "Phòng");
                grid.Columns.Add("C4", "Ngày lập");
                grid.Columns.Add("C5", "Tình trạng");
                LoadLichSuLapDat();
            }
        }

        private void LoadPhong()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT p.SoPhong, k.TenKhuVuc, p.SoNguoiToiDa, p.DonGiaNgay, p.TrangThai
                  FROM Phong p JOIN KhuVuc k ON p.MaKhuVuc = k.MaKhuVuc
                  ORDER BY p.SoPhong");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["SoPhong"], r["TenKhuVuc"], r["SoNguoiToiDa"], r["DonGiaNgay"], r["TrangThai"]);
        }

        private void LoadTienNghi()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT t.MaTienNghi, l.TenLoaiTN, t.SoThuTu, t.TinhTrangHienTai
                  FROM TienNghi t JOIN LoaiTienNghi l ON t.MaLoaiTN = l.MaLoaiTN
                  ORDER BY t.MaTienNghi");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaTienNghi"], r["TenLoaiTN"], r["SoThuTu"], r["TinhTrangHienTai"]);
        }

        private void LoadLichSuLapDat()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT SoPhieuLapDat, MaTienNghi, SoPhong, NgayLap, TinhTrang
                  FROM PhieuLapDat ORDER BY NgayLap DESC");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["SoPhieuLapDat"], r["MaTienNghi"], r["SoPhong"],
                    Convert.ToDateTime(r["NgayLap"]).ToString("dd/MM/yyyy"), r["TinhTrang"]);
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _current == View.LichSuLapDat) return;
            DataGridViewRow row = grid.Rows[e.RowIndex];
            txtF1.Text = Convert.ToString(row.Cells["C1"].Value);

            if (_current == View.Phong)
            {
                cboF2.Text = Convert.ToString(row.Cells["C2"].Value);
                txtF3.Text = Convert.ToString(row.Cells["C3"].Value);
                txtF4.Text = Convert.ToString(row.Cells["C4"].Value);
                cboF5.Text = Convert.ToString(row.Cells["C5"].Value);
            }
            else
            {
                cboF2.Text = Convert.ToString(row.Cells["C2"].Value);
                txtF3.Text = Convert.ToString(row.Cells["C3"].Value);
                txtF4.Text = Convert.ToString(row.Cells["C4"].Value);
            }
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtF1.Text))
            {
                MessageBox.Show(this, "Vui lòng nhập mã.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (_current == View.Phong)
                {
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO Phong(SoPhong, MaKhuVuc, SoNguoiToiDa, DonGiaNgay, TrangThai) VALUES(@sp,@khu,@sn,@dg,@tt)",
                        new SqlParameter("@sp", txtF1.Text.Trim()),
                        new SqlParameter("@khu", cboF2.SelectedValue),
                        new SqlParameter("@sn", ParseInt(txtF3.Text)),
                        new SqlParameter("@dg", ParseDecimal(txtF4.Text)),
                        new SqlParameter("@tt", cboF5.Text == "" ? "Trống" : cboF5.Text));
                    LoadPhong();
                    LoadComboDanhMucChung();
                }
                else
                {
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO TienNghi(MaTienNghi, MaLoaiTN, SoThuTu, TinhTrangHienTai) VALUES(@ma,@loai,@stt,@tt)",
                        new SqlParameter("@ma", txtF1.Text.Trim()),
                        new SqlParameter("@loai", cboF2.SelectedValue),
                        new SqlParameter("@stt", ParseInt(txtF3.Text)),
                        new SqlParameter("@tt", txtF4.Text.Trim()));
                    LoadTienNghi();
                    LoadComboDanhMucChung();
                }
                MessageBox.Show(this, "Thêm thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtF1.Text)) return;
            try
            {
                if (_current == View.Phong)
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE Phong SET MaKhuVuc=@khu, SoNguoiToiDa=@sn, DonGiaNgay=@dg, TrangThai=@tt WHERE SoPhong=@sp",
                        new SqlParameter("@khu", cboF2.SelectedValue),
                        new SqlParameter("@sn", ParseInt(txtF3.Text)),
                        new SqlParameter("@dg", ParseDecimal(txtF4.Text)),
                        new SqlParameter("@tt", cboF5.Text == "" ? "Trống" : cboF5.Text),
                        new SqlParameter("@sp", txtF1.Text.Trim()));
                    LoadPhong();
                }
                else
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE TienNghi SET MaLoaiTN=@loai, SoThuTu=@stt, TinhTrangHienTai=@tt WHERE MaTienNghi=@ma",
                        new SqlParameter("@loai", cboF2.SelectedValue),
                        new SqlParameter("@stt", ParseInt(txtF3.Text)),
                        new SqlParameter("@tt", txtF4.Text.Trim()),
                        new SqlParameter("@ma", txtF1.Text.Trim()));
                    LoadTienNghi();
                }
                MessageBox.Show(this, "Cập nhật thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtF1.Text)) return;
            if (MessageBox.Show(this, "Xóa mục đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                if (_current == View.Phong)
                {
                    DbHelper.ExecuteNonQuery("DELETE FROM Phong WHERE SoPhong=@sp", new SqlParameter("@sp", txtF1.Text.Trim()));
                    LoadPhong();
                }
                else
                {
                    DbHelper.ExecuteNonQuery("DELETE FROM TienNghi WHERE MaTienNghi=@ma", new SqlParameter("@ma", txtF1.Text.Trim()));
                    LoadTienNghi();
                }
                LoadComboDanhMucChung();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi (có thể do dữ liệu đang được tham chiếu): " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnLapPhieu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSoPhieuLapDat.Text) || cboTienNghiLD.SelectedValue == null || cboPhongLD.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui lòng nhập đủ Phiếu lắp đặt, Tiện nghi và Phòng.",
                    "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                DbHelper.ExecuteNonQuery(
                    @"INSERT INTO PhieuLapDat(SoPhieuLapDat, MaTienNghi, SoPhong, NgayLap, TinhTrang, MaNV, GhiChu)
                      VALUES(@sp,@tn,@ph,@ngay,@tt,@nv,@gc)",
                    new SqlParameter("@sp", txtSoPhieuLapDat.Text.Trim()),
                    new SqlParameter("@tn", cboTienNghiLD.SelectedValue),
                    new SqlParameter("@ph", cboPhongLD.SelectedValue),
                    new SqlParameter("@ngay", dtpNgayLapLD.Value.Date),
                    new SqlParameter("@tt", txtTinhTrangLD.Text.Trim()),
                    new SqlParameter("@nv", cboNhanVienLD.SelectedValue),
                    new SqlParameter("@gc", string.IsNullOrEmpty(txtGhiChuLD.Text) ? (object)DBNull.Value : txtGhiChuLD.Text.Trim()));

                MessageBox.Show(this, "Lập phiếu lắp đặt thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSoPhieuLapDat.Clear();
                txtTinhTrangLD.Clear();
                txtGhiChuLD.Clear();
                if (_current == View.LichSuLapDat) LoadLichSuLapDat();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
