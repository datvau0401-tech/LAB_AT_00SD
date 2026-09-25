using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Quản lý các danh mục nền: Khu vực, Nhân viên, Loại tiện nghi, Dịch vụ, Quy định đền bù.
    /// Dùng chung một bộ điều khiển nhập liệu, chỉ đổi nhãn/ẩn-hiện theo danh mục đang chọn.
    /// </summary>
    public class FrmDanhMuc : Form
    {
        private enum Category { KhuVuc, NhanVien, LoaiTienNghi, DichVu, QuyDinhDenBu }
        private Category _current = Category.KhuVuc;

        private Label lblMa, lblTen, lblExtra1, lblExtra2;
        private TextBox txtMa, txtTen, txtExtra1, txtExtra2;
        private ComboBox cboLoaiTN;
        private Button btnThem, btnSua, btnXoa;
        private DataGridView grid;

        public FrmDanhMuc()
        {
            InitializeComponent();
            LoadCategory(Category.KhuVuc);
        }

        private void InitializeComponent()
        {
            this.Text = "Danh mục khách sạn";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel topNav = new Panel();
            topNav.Dock = DockStyle.Top;
            topNav.Height = 40;

            string[] names = { "Khu vực", "Nhân viên", "Loại tiện nghi", "Dịch vụ", "Quy định đền bù" };
            Category[] cats = { Category.KhuVuc, Category.NhanVien, Category.LoaiTienNghi, Category.DichVu, Category.QuyDinhDenBu };
            int x = 10;
            for (int i = 0; i < names.Length; i++)
            {
                Category cat = cats[i];
                Button btn = new Button();
                btn.Text = "[ " + names[i] + " ]";
                btn.FlatStyle = FlatStyle.Flat;
                btn.Left = x;
                btn.Top = 5;
                btn.AutoSize = true;
                btn.Click += delegate { LoadCategory(cat); };
                topNav.Controls.Add(btn);
                x += btn.Width + 10;
            }

            Panel inputPanel = new Panel();
            inputPanel.Dock = DockStyle.Top;
            inputPanel.Height = 80;

            lblMa = new Label { Text = "Mã:", Left = 15, Top = 15, AutoSize = true };
            txtMa = new TextBox { Left = 60, Top = 12, Width = 110 };

            lblTen = new Label { Text = "Tên:", Left = 190, Top = 15, AutoSize = true };
            txtTen = new TextBox { Left = 235, Top = 12, Width = 190 };
            cboLoaiTN = new ComboBox { Left = 235, Top = 12, Width = 190, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };

            lblExtra1 = new Label { Text = "Đơn vị / Vai trò:", Left = 440, Top = 15, AutoSize = true };
            txtExtra1 = new TextBox { Left = 560, Top = 12, Width = 150 };

            lblExtra2 = new Label { Text = "Giá trị:", Left = 440, Top = 45, AutoSize = true };
            txtExtra2 = new TextBox { Left = 560, Top = 42, Width = 150 };

            btnThem = new Button { Text = "Thêm", Left = 730, Top = 10, Width = 90 };
            btnThem.Click += BtnThem_Click;
            btnSua = new Button { Text = "Sửa", Left = 730, Top = 42, Width = 90 };
            btnSua.Click += BtnSua_Click;
            btnXoa = new Button { Text = "Xóa", Left = 826, Top = 42, Width = 90 };
            btnXoa.Click += BtnXoa_Click;

            inputPanel.Controls.AddRange(new Control[]
            {
                lblMa, txtMa, lblTen, txtTen, cboLoaiTN,
                lblExtra1, txtExtra1, lblExtra2, txtExtra2,
                btnThem, btnSua, btnXoa
            });

            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.CellClick += Grid_CellClick;

            this.Controls.Add(grid);
            this.Controls.Add(inputPanel);
            this.Controls.Add(topNav);
        }

        private void LoadCategory(Category cat)
        {
            _current = cat;
            txtMa.Clear();
            txtTen.Clear();
            txtExtra1.Clear();
            txtExtra2.Clear();
            txtTen.Visible = true;
            cboLoaiTN.Visible = false;

            grid.Columns.Clear();
            grid.Columns.Add("Ma", "Mã");
            grid.Columns.Add("Ten", "Tên");
            grid.Columns.Add("Loai", "Loại / Vai trò");
            grid.Columns.Add("DonVi", "Đơn vị");
            grid.Columns.Add("DonGia", "Đơn giá / Mức");

            switch (cat)
            {
                case Category.KhuVuc:
                    lblTen.Text = "Tên khu vực:";
                    SetExtraVisible(false, false);
                    LoadKhuVuc();
                    break;
                case Category.NhanVien:
                    lblTen.Text = "Họ tên:";
                    lblExtra1.Text = "Vai trò:";
                    lblExtra2.Text = "Số điện thoại:";
                    SetExtraVisible(true, true);
                    LoadNhanVien();
                    break;
                case Category.LoaiTienNghi:
                    lblTen.Text = "Tên loại tiện nghi:";
                    SetExtraVisible(false, false);
                    LoadLoaiTienNghi();
                    break;
                case Category.DichVu:
                    lblTen.Text = "Tên dịch vụ:";
                    lblExtra1.Text = "Đơn vị tính:";
                    lblExtra2.Text = "Đơn giá:";
                    SetExtraVisible(true, true);
                    LoadDichVu();
                    break;
                case Category.QuyDinhDenBu:
                    txtTen.Visible = false;
                    cboLoaiTN.Visible = true;
                    cboLoaiTN.DataSource = DbHelper.GetDataTable("SELECT MaLoaiTN, TenLoaiTN FROM LoaiTienNghi");
                    cboLoaiTN.DisplayMember = "TenLoaiTN";
                    cboLoaiTN.ValueMember = "MaLoaiTN";
                    lblExtra1.Text = "Mức độ thiệt hại:";
                    lblExtra2.Text = "Mức đền bù:";
                    SetExtraVisible(true, true);
                    LoadQuyDinhDenBu();
                    break;
            }
        }

        private void SetExtraVisible(bool v1, bool v2)
        {
            lblExtra1.Visible = v1; txtExtra1.Visible = v1;
            lblExtra2.Visible = v2; txtExtra2.Visible = v2;
        }

        private void LoadKhuVuc()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable("SELECT MaKhuVuc, TenKhuVuc FROM KhuVuc ORDER BY MaKhuVuc");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaKhuVuc"], r["TenKhuVuc"], "", "", "");
        }

        private void LoadNhanVien()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable("SELECT MaNV, HoTen, VaiTro, SoDienThoai FROM NhanVien ORDER BY MaNV");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaNV"], r["HoTen"], r["VaiTro"], r["SoDienThoai"], "");
        }

        private void LoadLoaiTienNghi()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable("SELECT MaLoaiTN, TenLoaiTN FROM LoaiTienNghi ORDER BY MaLoaiTN");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaLoaiTN"], r["TenLoaiTN"], "", "", "");
        }

        private void LoadDichVu()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable("SELECT MaDV, TenDV, DonViTinh, DonGia FROM DichVu ORDER BY MaDV");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaDV"], r["TenDV"], "", r["DonViTinh"], r["DonGia"]);
        }

        private void LoadQuyDinhDenBu()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT q.MaQuyDinh, l.TenLoaiTN, q.MucDoThietHai, q.MucDenBu
                  FROM QuyDinhDenBu q JOIN LoaiTienNghi l ON q.MaLoaiTN = l.MaLoaiTN
                  ORDER BY q.MaQuyDinh");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["MaQuyDinh"], r["TenLoaiTN"], r["MucDoThietHai"], "", r["MucDenBu"]);
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = grid.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(row.Cells["Ma"].Value);

            switch (_current)
            {
                case Category.KhuVuc:
                case Category.LoaiTienNghi:
                    txtTen.Text = Convert.ToString(row.Cells["Ten"].Value);
                    break;
                case Category.NhanVien:
                    txtTen.Text = Convert.ToString(row.Cells["Ten"].Value);
                    txtExtra1.Text = Convert.ToString(row.Cells["Loai"].Value);
                    txtExtra2.Text = Convert.ToString(row.Cells["DonVi"].Value);
                    break;
                case Category.DichVu:
                    txtTen.Text = Convert.ToString(row.Cells["Ten"].Value);
                    txtExtra1.Text = Convert.ToString(row.Cells["DonVi"].Value);
                    txtExtra2.Text = Convert.ToString(row.Cells["DonGia"].Value);
                    break;
                case Category.QuyDinhDenBu:
                    txtExtra1.Text = Convert.ToString(row.Cells["Loai"].Value);
                    txtExtra2.Text = Convert.ToString(row.Cells["DonGia"].Value);
                    break;
            }
        }

        private bool ValidateMa()
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text))
            {
                MessageBox.Show(this, "Vui lòng nhập Mã.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateMa()) return;
            try
            {
                switch (_current)
                {
                    case Category.KhuVuc:
                        DbHelper.ExecuteNonQuery(
                            "INSERT INTO KhuVuc(MaKhuVuc, TenKhuVuc) VALUES(@ma,@ten)",
                            new SqlParameter("@ma", txtMa.Text.Trim()),
                            new SqlParameter("@ten", txtTen.Text.Trim()));
                        LoadKhuVuc();
                        break;

                    case Category.NhanVien:
                        DbHelper.ExecuteNonQuery(
                            "INSERT INTO NhanVien(MaNV, HoTen, VaiTro, SoDienThoai) VALUES(@ma,@ten,@vt,@dt)",
                            new SqlParameter("@ma", txtMa.Text.Trim()),
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@vt", txtExtra1.Text.Trim()),
                            new SqlParameter("@dt", txtExtra2.Text.Trim()));
                        LoadNhanVien();
                        break;

                    case Category.LoaiTienNghi:
                        DbHelper.ExecuteNonQuery(
                            "INSERT INTO LoaiTienNghi(MaLoaiTN, TenLoaiTN) VALUES(@ma,@ten)",
                            new SqlParameter("@ma", txtMa.Text.Trim()),
                            new SqlParameter("@ten", txtTen.Text.Trim()));
                        LoadLoaiTienNghi();
                        break;

                    case Category.DichVu:
                        DbHelper.ExecuteNonQuery(
                            "INSERT INTO DichVu(MaDV, TenDV, DonViTinh, DonGia) VALUES(@ma,@ten,@dv,@dg)",
                            new SqlParameter("@ma", txtMa.Text.Trim()),
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@dv", txtExtra1.Text.Trim()),
                            new SqlParameter("@dg", ParseDecimal(txtExtra2.Text)));
                        LoadDichVu();
                        break;

                    case Category.QuyDinhDenBu:
                        DbHelper.ExecuteNonQuery(
                            "INSERT INTO QuyDinhDenBu(MaQuyDinh, MaLoaiTN, MucDoThietHai, MucDenBu) VALUES(@ma,@loai,@md,@mdb)",
                            new SqlParameter("@ma", txtMa.Text.Trim()),
                            new SqlParameter("@loai", cboLoaiTN.SelectedValue),
                            new SqlParameter("@md", txtExtra1.Text.Trim()),
                            new SqlParameter("@mdb", ParseDecimal(txtExtra2.Text)));
                        LoadQuyDinhDenBu();
                        break;
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
            if (!ValidateMa()) return;
            try
            {
                switch (_current)
                {
                    case Category.KhuVuc:
                        DbHelper.ExecuteNonQuery(
                            "UPDATE KhuVuc SET TenKhuVuc=@ten WHERE MaKhuVuc=@ma",
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@ma", txtMa.Text.Trim()));
                        LoadKhuVuc();
                        break;

                    case Category.NhanVien:
                        DbHelper.ExecuteNonQuery(
                            "UPDATE NhanVien SET HoTen=@ten, VaiTro=@vt, SoDienThoai=@dt WHERE MaNV=@ma",
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@vt", txtExtra1.Text.Trim()),
                            new SqlParameter("@dt", txtExtra2.Text.Trim()),
                            new SqlParameter("@ma", txtMa.Text.Trim()));
                        LoadNhanVien();
                        break;

                    case Category.LoaiTienNghi:
                        DbHelper.ExecuteNonQuery(
                            "UPDATE LoaiTienNghi SET TenLoaiTN=@ten WHERE MaLoaiTN=@ma",
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@ma", txtMa.Text.Trim()));
                        LoadLoaiTienNghi();
                        break;

                    case Category.DichVu:
                        DbHelper.ExecuteNonQuery(
                            "UPDATE DichVu SET TenDV=@ten, DonViTinh=@dv, DonGia=@dg WHERE MaDV=@ma",
                            new SqlParameter("@ten", txtTen.Text.Trim()),
                            new SqlParameter("@dv", txtExtra1.Text.Trim()),
                            new SqlParameter("@dg", ParseDecimal(txtExtra2.Text)),
                            new SqlParameter("@ma", txtMa.Text.Trim()));
                        LoadDichVu();
                        break;

                    case Category.QuyDinhDenBu:
                        DbHelper.ExecuteNonQuery(
                            "UPDATE QuyDinhDenBu SET MaLoaiTN=@loai, MucDoThietHai=@md, MucDenBu=@mdb WHERE MaQuyDinh=@ma",
                            new SqlParameter("@loai", cboLoaiTN.SelectedValue),
                            new SqlParameter("@md", txtExtra1.Text.Trim()),
                            new SqlParameter("@mdb", ParseDecimal(txtExtra2.Text)),
                            new SqlParameter("@ma", txtMa.Text.Trim()));
                        LoadQuyDinhDenBu();
                        break;
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
            if (!ValidateMa()) return;
            if (MessageBox.Show(this, "Xóa mục đã chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                string sql;
                switch (_current)
                {
                    case Category.KhuVuc: sql = "DELETE FROM KhuVuc WHERE MaKhuVuc=@ma"; break;
                    case Category.NhanVien: sql = "DELETE FROM NhanVien WHERE MaNV=@ma"; break;
                    case Category.LoaiTienNghi: sql = "DELETE FROM LoaiTienNghi WHERE MaLoaiTN=@ma"; break;
                    case Category.DichVu: sql = "DELETE FROM DichVu WHERE MaDV=@ma"; break;
                    case Category.QuyDinhDenBu: sql = "DELETE FROM QuyDinhDenBu WHERE MaQuyDinh=@ma"; break;
                    default: return;
                }
                DbHelper.ExecuteNonQuery(sql, new SqlParameter("@ma", txtMa.Text.Trim()));
                LoadCategory(_current);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi (có thể do dữ liệu đang được tham chiếu): " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static decimal ParseDecimal(string s)
        {
            decimal result;
            decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            return result;
        }
    }
}
