using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Ghi nhận Phiếu sử dụng dịch vụ cho khách đang lưu trú.
    /// Mỗi tổ hợp (Phiếu đặt, Phòng, Ngày sử dụng) chỉ có một PhieuSuDungDV (theo ràng buộc UNIQUE
    /// của CSDL); form tự sinh mã phiếu này ở phía sau, người dùng chỉ cần chọn Phiếu lưu trú - Phòng -
    /// Dịch vụ - Ngày - Số lượng rồi bấm "Ghi nhận". Nếu dịch vụ đó đã được ghi trong cùng ngày, số
    /// lượng sẽ được cộng dồn thay vì tạo dòng mới.
    /// </summary>
    public class FrmDichVu : Form
    {
        private ComboBox cboPhieu, cboPhong, cboDichVu, cboNhanVien;
        private DateTimePicker dtpNgaySuDung;
        private TextBox txtSoLuong;
        private Button btnGhiNhan;
        private DataGridView grid;

        public FrmDichVu()
        {
            InitializeComponent();
            LoadCombo();
            LoadDanhSachSuDung();
        }

        private void InitializeComponent()
        {
            this.Text = "Sử dụng dịch vụ";
            this.Size = new Size(950, 620);
            this.StartPosition = FormStartPosition.CenterScreen;

            Panel input = new Panel { Dock = DockStyle.Top, Height = 90 };

            Label l1 = new Label { Text = "Phiếu lưu trú:", Left = 15, Top = 15, AutoSize = true };
            cboPhieu = new ComboBox { Left = 105, Top = 12, Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            cboPhieu.SelectedIndexChanged += CboPhieu_SelectedIndexChanged;

            Label l2 = new Label { Text = "Phòng:", Left = 345, Top = 15, AutoSize = true };
            cboPhong = new ComboBox { Left = 400, Top = 12, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };

            Label l3 = new Label { Text = "Dịch vụ:", Left = 520, Top = 15, AutoSize = true };
            cboDichVu = new ComboBox { Left = 580, Top = 12, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label l4 = new Label { Text = "Ngày sử dụng:", Left = 15, Top = 50, AutoSize = true };
            dtpNgaySuDung = new DateTimePicker { Left = 105, Top = 47, Width = 120, Format = DateTimePickerFormat.Short };

            Label l5 = new Label { Text = "Số lượng:", Left = 250, Top = 50, AutoSize = true };
            txtSoLuong = new TextBox { Left = 320, Top = 47, Width = 60, Text = "1" };

            Label l6 = new Label { Text = "Nhân viên:", Left = 400, Top = 50, AutoSize = true };
            cboNhanVien = new ComboBox { Left = 470, Top = 47, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            btnGhiNhan = new Button { Text = "Ghi nhận", Left = 800, Top = 20, Width = 110, Height = 40 };
            btnGhiNhan.Click += BtnGhiNhan_Click;

            input.Controls.AddRange(new Control[]
            {
                l1, cboPhieu, l2, cboPhong, l3, cboDichVu,
                l4, dtpNgaySuDung, l5, txtSoLuong, l6, cboNhanVien, btnGhiNhan
            });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add("SoPhieu", "Số phiếu");
            grid.Columns.Add("Phong", "Phòng");
            grid.Columns.Add("Ngay", "Ngày");
            grid.Columns.Add("DichVu", "Dịch vụ");
            grid.Columns.Add("SoLuong", "Số lượng");
            grid.Columns.Add("DonGia", "Đơn giá");
            grid.Columns.Add("ThanhTien", "Thành tiền");

            this.Controls.Add(grid);
            this.Controls.Add(input);
        }

        private void LoadCombo()
        {
            DataTable phieu = DbHelper.GetDataTable(
                @"SELECT pd.SoPhieuDat, (pd.SoPhieuDat + N' - ' + kh.HoTen) AS Hien
                  FROM PhieuDatPhong pd JOIN KhachHang kh ON pd.MaKhach = kh.MaKhach
                  WHERE pd.TrangThai = N'Đang ở' ORDER BY pd.SoPhieuDat");
            cboPhieu.DataSource = phieu.Copy();
            cboPhieu.DisplayMember = "Hien";
            cboPhieu.ValueMember = "SoPhieuDat";

            DataTable dv = DbHelper.GetDataTable("SELECT MaDV, TenDV FROM DichVu ORDER BY MaDV");
            cboDichVu.DataSource = dv.Copy();
            cboDichVu.DisplayMember = "TenDV";
            cboDichVu.ValueMember = "MaDV";

            DataTable nv = DbHelper.GetDataTable("SELECT MaNV, HoTen FROM NhanVien ORDER BY MaNV");
            cboNhanVien.DataSource = nv.Copy();
            cboNhanVien.DisplayMember = "HoTen";
            cboNhanVien.ValueMember = "MaNV";
        }

        private void CboPhieu_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboPhong.DataSource = null;
            if (cboPhieu.SelectedValue == null) return;
            try
            {
                DataTable phong = DbHelper.GetDataTable(
                    "SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat=@sp ORDER BY SoPhong",
                    new SqlParameter("@sp", cboPhieu.SelectedValue));
                cboPhong.DataSource = phong.Copy();
                cboPhong.DisplayMember = "SoPhong";
                cboPhong.ValueMember = "SoPhong";

                if (phong.Rows.Count == 0)
                    MessageBox.Show(this, "Không tìm thấy phòng nào cho phiếu: " + cboPhieu.SelectedValue,
                        "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi khi tải danh sách phòng: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDanhSachSuDung()
        {
            grid.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT p.SoPhieuSDDV, p.SoPhong, p.NgaySuDung, dv.TenDV, ct.SoLuong, ct.DonGia, ct.ThanhTien
                  FROM ChiTietPhieuSuDungDV ct
                  JOIN PhieuSuDungDV p ON ct.SoPhieuSDDV = p.SoPhieuSDDV
                  JOIN DichVu dv ON ct.MaDV = dv.MaDV
                  ORDER BY p.NgaySuDung DESC, p.SoPhieuSDDV");
            foreach (DataRow r in dt.Rows)
                grid.Rows.Add(r["SoPhieuSDDV"], r["SoPhong"],
                    Convert.ToDateTime(r["NgaySuDung"]).ToString("dd/MM/yyyy"),
                    r["TenDV"], r["SoLuong"], r["DonGia"], r["ThanhTien"]);
        }

        private void BtnGhiNhan_Click(object sender, EventArgs e)
        {
            if (cboPhieu.SelectedValue == null || cboPhong.SelectedValue == null ||
                cboDichVu.SelectedValue == null || cboNhanVien.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui lòng chọn đủ Phiếu lưu trú, Phòng, Dịch vụ và Nhân viên.",
                    "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int soLuong = ParseInt(txtSoLuong.Text);
            if (soLuong <= 0)
            {
                MessageBox.Show(this, "Số lượng phải lớn hơn 0.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string soPhieuDat = Convert.ToString(cboPhieu.SelectedValue);
            string soPhong = Convert.ToString(cboPhong.SelectedValue);
            string maDV = Convert.ToString(cboDichVu.SelectedValue);
            string maNV = Convert.ToString(cboNhanVien.SelectedValue);
            DateTime ngay = dtpNgaySuDung.Value.Date;

            // Mã phiếu sử dụng dịch vụ được sinh tự động theo (Phiếu đặt, Phòng, Ngày)
            // để đảm bảo đúng ràng buộc UNIQUE(SoPhieuDat, SoPhong, NgaySuDung) của bảng PhieuSuDungDV.
            string soPhieuSDDV = soPhieuDat + "-" + soPhong + "-" + ngay.ToString("yyyyMMdd");

            try
            {
                object daCoPhieu = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM PhieuSuDungDV WHERE SoPhieuSDDV=@sp", new SqlParameter("@sp", soPhieuSDDV));
                if (Convert.ToInt32(daCoPhieu) == 0)
                {
                    DbHelper.ExecuteNonQuery(
                        @"INSERT INTO PhieuSuDungDV(SoPhieuSDDV, SoPhieuDat, SoPhong, NgaySuDung, MaNV)
                          VALUES(@sp,@pd,@ph,@ngay,@nv)",
                        new SqlParameter("@sp", soPhieuSDDV),
                        new SqlParameter("@pd", soPhieuDat),
                        new SqlParameter("@ph", soPhong),
                        new SqlParameter("@ngay", ngay),
                        new SqlParameter("@nv", maNV));
                }

                object giaObj = DbHelper.ExecuteScalar("SELECT DonGia FROM DichVu WHERE MaDV=@dv", new SqlParameter("@dv", maDV));
                decimal donGia = Convert.ToDecimal(giaObj);

                object daCoChiTiet = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM ChiTietPhieuSuDungDV WHERE SoPhieuSDDV=@sp AND MaDV=@dv",
                    new SqlParameter("@sp", soPhieuSDDV), new SqlParameter("@dv", maDV));

                if (Convert.ToInt32(daCoChiTiet) > 0)
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE ChiTietPhieuSuDungDV SET SoLuong = SoLuong + @sl WHERE SoPhieuSDDV=@sp AND MaDV=@dv",
                        new SqlParameter("@sl", soLuong),
                        new SqlParameter("@sp", soPhieuSDDV),
                        new SqlParameter("@dv", maDV));
                }
                else
                {
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO ChiTietPhieuSuDungDV(SoPhieuSDDV, MaDV, SoLuong, DonGia) VALUES(@sp,@dv,@sl,@dg)",
                        new SqlParameter("@sp", soPhieuSDDV),
                        new SqlParameter("@dv", maDV),
                        new SqlParameter("@sl", soLuong),
                        new SqlParameter("@dg", donGia));
                }

                MessageBox.Show(this, "Ghi nhận sử dụng dịch vụ thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSoLuong.Text = "1";
                LoadDanhSachSuDung();
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
    }
}