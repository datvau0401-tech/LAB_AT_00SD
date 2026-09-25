using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyKhachSan
{
    /// <summary>
    /// Quy trình trả phòng: xem phòng/tiện nghi của phiếu đang ở, lập phiếu đền bù (nếu tiện nghi hư hỏng),
    /// lập hóa đơn, thanh toán và hoàn tất trả phòng (mở lại phòng về trạng thái Trống).
    /// </summary>
    public class FrmTraPhong : Form
    {
        private TextBox txtSoPhieuDat;
        private Button btnTaiPhieu;

        private DataGridView gridPhong, gridTienNghi, gridTienNghiDenBu;

        private TextBox txtSoPhieuDenBu, txtSoTienDenBu;
        private ComboBox cboMucDo, cboNhanVienDenBu;
        private Button btnLapPhieuDenBu;

        private TextBox txtSoHoaDon, txtSoNgayTinhTien;
        private ComboBox cboNhanVienHD;
        private Button btnLapHoaDon;

        private DataGridView gridHoaDon;

        private ComboBox cboHinhThuc;
        private TextBox txtSoTienTT;
        private Button btnThanhToan, btnHoanTat;

        private string _maLoaiTNDangChon, _soPhongDangChonDenBu;
        private DataTable _mucDoTable;
        private string _soHoaDonDangChon;
        private decimal _tongTienHoaDonDangChon;

        public FrmTraPhong()
        {
            InitializeComponent();
            LoadNhanVienCombo();
        }

        private void InitializeComponent()
        {
            this.Text = "Trả phòng - Đền bù - Hóa đơn - Thanh toán";
            this.Size = new Size(1180, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // ----- Phiếu đang ở -----
            Panel pnlPhieu = new Panel { Dock = DockStyle.Top, Height = 40 };
            Label lblPhieu = new Label { Text = "Phiếu đang ở:", Left = 15, Top = 12, AutoSize = true };
            txtSoPhieuDat = new TextBox { Left = 105, Top = 9, Width = 130 };
            btnTaiPhieu = new Button { Text = "Tải phiếu", Left = 245, Top = 7, Width = 90 };
            btnTaiPhieu.Click += BtnTaiPhieu_Click;
            pnlPhieu.Controls.AddRange(new Control[] { lblPhieu, txtSoPhieuDat, btnTaiPhieu });

            // ----- 3 lưới: Phòng | Tiện nghi | Tiện nghi đền bù -----
            TableLayoutPanel grids = new TableLayoutPanel { Dock = DockStyle.Top, Height = 220, ColumnCount = 3, RowCount = 1 };
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
            grids.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));

            gridPhong = MakeGrid();
            gridPhong.Columns.Add("Phong", "Phòng");
            gridPhong.Columns.Add("DonGia", "Đơn giá/ngày");

            gridTienNghi = MakeGrid();
            gridTienNghi.Columns.Add("TienNghi", "Tiện nghi");
            gridTienNghi.Columns.Add("Loai", "Loại");
            gridTienNghi.Columns.Add("TinhTrang", "Tình trạng");
            gridTienNghi.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaLoaiHidden", HeaderText = "MaLoai", Visible = false });
            gridTienNghi.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoPhongHidden", HeaderText = "SoPhong", Visible = false });
            gridTienNghi.CellClick += GridTienNghi_CellClick;

            gridTienNghiDenBu = MakeGrid();
            gridTienNghiDenBu.Columns.Add("TienNghiDB", "Tiện nghi đền bù");
            gridTienNghiDenBu.Columns.Add("MucDo", "Mức độ");
            gridTienNghiDenBu.Columns.Add("SoTien", "Số tiền");

            grids.Controls.Add(gridPhong, 0, 0);
            grids.Controls.Add(gridTienNghi, 1, 0);
            grids.Controls.Add(gridTienNghiDenBu, 2, 0);

            // ----- Lập phiếu đền bù -----
            GroupBox gbDenBu = new GroupBox { Text = "Lập phiếu đền bù (chọn 1 dòng ở lưới Tiện nghi rồi điền bên dưới)", Dock = DockStyle.Top, Height = 75 };
            Label l1 = new Label { Text = "Số phiếu đền bù:", Left = 15, Top = 30, AutoSize = true };
            txtSoPhieuDenBu = new TextBox { Left = 130, Top = 27, Width = 100 };
            Label l2 = new Label { Text = "Mức độ:", Left = 250, Top = 30, AutoSize = true };
            cboMucDo = new ComboBox { Left = 310, Top = 27, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboMucDo.SelectedIndexChanged += CboMucDo_SelectedIndexChanged;
            Label l3 = new Label { Text = "Số tiền:", Left = 475, Top = 30, AutoSize = true };
            txtSoTienDenBu = new TextBox { Left = 530, Top = 27, Width = 100, ReadOnly = true };
            Label l4 = new Label { Text = "Nhân viên:", Left = 650, Top = 30, AutoSize = true };
            cboNhanVienDenBu = new ComboBox { Left = 725, Top = 27, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            btnLapPhieuDenBu = new Button { Text = "Lập phiếu đền bù", Left = 895, Top = 25, Width = 140 };
            btnLapPhieuDenBu.Click += BtnLapPhieuDenBu_Click;
            gbDenBu.Controls.AddRange(new Control[] { l1, txtSoPhieuDenBu, l2, cboMucDo, l3, txtSoTienDenBu, l4, cboNhanVienDenBu, btnLapPhieuDenBu });

            // ----- Lập hóa đơn -----
            GroupBox gbHoaDon = new GroupBox { Text = "Lập hóa đơn", Dock = DockStyle.Top, Height = 60 };
            Label l5 = new Label { Text = "Số hóa đơn:", Left = 15, Top = 25, AutoSize = true };
            txtSoHoaDon = new TextBox { Left = 100, Top = 22, Width = 110 };
            Label l6 = new Label { Text = "Số ngày tính tiền:", Left = 230, Top = 25, AutoSize = true };
            txtSoNgayTinhTien = new TextBox { Left = 350, Top = 22, Width = 60 };
            Label l7 = new Label { Text = "Nhân viên:", Left = 430, Top = 25, AutoSize = true };
            cboNhanVienHD = new ComboBox { Left = 505, Top = 22, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            btnLapHoaDon = new Button { Text = "Lập hóa đơn", Left = 895, Top = 19, Width = 140 };
            btnLapHoaDon.Click += BtnLapHoaDon_Click;
            gbHoaDon.Controls.AddRange(new Control[] { l5, txtSoHoaDon, l6, txtSoNgayTinhTien, l7, cboNhanVienHD, btnLapHoaDon });

            // ----- Lưới hóa đơn -----
            Label lblHD = new Label { Text = "Hóa đơn của phiếu:", Dock = DockStyle.Top, Height = 20, Padding = new Padding(5, 3, 0, 0) };
            gridHoaDon = MakeGrid();
            gridHoaDon.Dock = DockStyle.Top;
            gridHoaDon.Height = 150;
            gridHoaDon.Columns.Add("HoaDon", "Hóa đơn");
            gridHoaDon.Columns.Add("PhieuDat", "Phiếu đặt");
            gridHoaDon.Columns.Add("TienPhong", "Tiền phòng");
            gridHoaDon.Columns.Add("TienDichVu", "Tiền dịch vụ");
            gridHoaDon.Columns.Add("TongTien", "Tổng tiền");
            gridHoaDon.Columns.Add("TrangThai", "Trạng thái");
            gridHoaDon.CellClick += GridHoaDon_CellClick;

            // ----- Thanh toán -----
            GroupBox gbThanhToan = new GroupBox { Text = "Thanh toán (chọn 1 dòng hóa đơn ở trên)", Dock = DockStyle.Top, Height = 65 };
            Label l8 = new Label { Text = "Hình thức:", Left = 15, Top = 28, AutoSize = true };
            cboHinhThuc = new ComboBox { Left = 90, Top = 25, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            cboHinhThuc.Items.AddRange(new object[] { "Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử" });
            Label l9 = new Label { Text = "Số tiền:", Left = 250, Top = 28, AutoSize = true };
            txtSoTienTT = new TextBox { Left = 310, Top = 25, Width = 120 };
            btnThanhToan = new Button { Text = "Thanh toán", Left = 460, Top = 22, Width = 120 };
            btnThanhToan.Click += BtnThanhToan_Click;
            btnHoanTat = new Button { Text = "Hoàn tất trả phòng", Left = 895, Top = 18, Width = 140, Height = 32 };
            btnHoanTat.Click += BtnHoanTat_Click;
            gbThanhToan.Controls.AddRange(new Control[] { l8, cboHinhThuc, l9, txtSoTienTT, btnThanhToan, btnHoanTat });

            // Thêm theo THỨ TỰ NGƯỢC với chiều hiển thị mong muốn (Dock=Top: thêm sau cùng sẽ nổi lên trên).
            this.Controls.Add(gbThanhToan);
            this.Controls.Add(gridHoaDon);
            this.Controls.Add(lblHD);
            this.Controls.Add(gbHoaDon);
            this.Controls.Add(gbDenBu);
            this.Controls.Add(grids);
            this.Controls.Add(pnlPhieu);
        }

        private static DataGridView MakeGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
        }

        private void LoadNhanVienCombo()
        {
            DataTable nv = DbHelper.GetDataTable("SELECT MaNV, HoTen FROM NhanVien ORDER BY MaNV");
            foreach (ComboBox c in new[] { cboNhanVienDenBu, cboNhanVienHD })
            {
                c.DataSource = nv.Copy();
                c.DisplayMember = "HoTen";
                c.ValueMember = "MaNV";
            }
        }

        // ===================== TẢI PHIẾU =====================
        private void BtnTaiPhieu_Click(object sender, EventArgs e)
        {
            string soPhieu = txtSoPhieuDat.Text.Trim();
            if (string.IsNullOrWhiteSpace(soPhieu)) return;

            object dem = DbHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM PhieuDatPhong WHERE SoPhieuDat=@sp AND TrangThai=N'Đang ở'",
                new SqlParameter("@sp", soPhieu));
            if (Convert.ToInt32(dem) == 0)
            {
                MessageBox.Show(this, "Không tìm thấy phiếu đang ở với mã này.", "Không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                gridPhong.Rows.Clear();
                gridTienNghi.Rows.Clear();
                gridTienNghiDenBu.Rows.Clear();
                gridHoaDon.Rows.Clear();
                return;
            }

            // Phòng của phiếu
            gridPhong.Rows.Clear();
            List<string> danhSachPhong = new List<string>();
            DataTable phong = DbHelper.GetDataTable(
                @"SELECT ct.SoPhong, p.DonGiaNgay
                  FROM ChiTietDatPhong ct JOIN Phong p ON ct.SoPhong = p.SoPhong
                  WHERE ct.SoPhieuDat=@sp ORDER BY ct.SoPhong",
                new SqlParameter("@sp", soPhieu));
            foreach (DataRow r in phong.Rows)
            {
                gridPhong.Rows.Add(r["SoPhong"], r["DonGiaNgay"]);
                danhSachPhong.Add(Convert.ToString(r["SoPhong"]));
            }

            // Tiện nghi hiện có trong các phòng đó (lấy phiếu lắp đặt mới nhất của mỗi tiện nghi)
            gridTienNghi.Rows.Clear();
            if (danhSachPhong.Count > 0)
            {
                List<SqlParameter> ps = new List<SqlParameter>();
                List<string> names = new List<string>();
                for (int i = 0; i < danhSachPhong.Count; i++)
                {
                    string pname = "@r" + i;
                    names.Add(pname);
                    ps.Add(new SqlParameter(pname, danhSachPhong[i]));
                }
                string sql = @"SELECT latest.SoPhong, tn.MaTienNghi, lt.TenLoaiTN, tn.MaLoaiTN, tn.TinhTrangHienTai
                               FROM TienNghi tn
                               JOIN LoaiTienNghi lt ON tn.MaLoaiTN = lt.MaLoaiTN
                               JOIN (
                                   SELECT MaTienNghi, SoPhong,
                                          ROW_NUMBER() OVER (PARTITION BY MaTienNghi ORDER BY NgayLap DESC) AS rn
                                   FROM PhieuLapDat
                               ) latest ON latest.MaTienNghi = tn.MaTienNghi AND latest.rn = 1
                               WHERE latest.SoPhong IN (" + string.Join(",", names.ToArray()) + @")
                               ORDER BY latest.SoPhong, tn.MaTienNghi";
                DataTable tn = DbHelper.GetDataTable(sql, ps.ToArray());
                foreach (DataRow r in tn.Rows)
                    gridTienNghi.Rows.Add(r["MaTienNghi"], r["TenLoaiTN"], r["TinhTrangHienTai"], r["MaLoaiTN"], r["SoPhong"]);
            }

            gridTienNghiDenBu.Rows.Clear();
            txtSoPhieuDenBu.Clear();
            cboMucDo.Items.Clear();
            txtSoTienDenBu.Clear();
            _maLoaiTNDangChon = null;
            _soPhongDangChonDenBu = null;

            // Gợi ý số ngày tính tiền = số ngày đã ở tính đến hôm nay
            object ngayNhanObj = DbHelper.ExecuteScalar(
                "SELECT NgayNhanThucTe FROM PhieuDatPhong WHERE SoPhieuDat=@sp", new SqlParameter("@sp", soPhieu));
            int soNgay = 1;
            if (ngayNhanObj != null && ngayNhanObj != DBNull.Value)
            {
                soNgay = (DateTime.Now.Date - Convert.ToDateTime(ngayNhanObj).Date).Days;
                if (soNgay < 1) soNgay = 1;
            }
            txtSoNgayTinhTien.Text = soNgay.ToString();
            txtSoHoaDon.Clear();

            LoadHoaDonCuaPhieu(soPhieu);
            txtSoTienTT.Clear();
            _soHoaDonDangChon = null;
        }

        // ===================== ĐỀN BÙ =====================
        private void GridTienNghi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = gridTienNghi.Rows[e.RowIndex];
            _maLoaiTNDangChon = Convert.ToString(row.Cells["MaLoaiHidden"].Value);
            _soPhongDangChonDenBu = Convert.ToString(row.Cells["SoPhongHidden"].Value);

            cboMucDo.Items.Clear();
            txtSoTienDenBu.Clear();
            _mucDoTable = DbHelper.GetDataTable(
                "SELECT MucDoThietHai, MucDenBu FROM QuyDinhDenBu WHERE MaLoaiTN=@l ORDER BY MucDenBu",
                new SqlParameter("@l", _maLoaiTNDangChon));
            foreach (DataRow r in _mucDoTable.Rows)
                cboMucDo.Items.Add(Convert.ToString(r["MucDoThietHai"]));
        }

        private void CboMucDo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_mucDoTable == null || cboMucDo.SelectedItem == null) return;
            string mucDo = cboMucDo.SelectedItem.ToString();
            foreach (DataRow r in _mucDoTable.Rows)
            {
                if (Convert.ToString(r["MucDoThietHai"]) == mucDo)
                {
                    txtSoTienDenBu.Text = Convert.ToString(r["MucDenBu"]);
                    break;
                }
            }
        }

        private void BtnLapPhieuDenBu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSoPhieuDat.Text) || string.IsNullOrWhiteSpace(txtSoPhieuDenBu.Text) ||
                _maLoaiTNDangChon == null || cboMucDo.SelectedItem == null || cboNhanVienDenBu.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui lòng tải phiếu, chọn 1 tiện nghi ở lưới, chọn mức độ, nhân viên và nhập số phiếu đền bù.",
                    "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string maTienNghi = Convert.ToString(gridTienNghi.CurrentRow.Cells["TienNghi"].Value);
            string soPhieuDenBu = txtSoPhieuDenBu.Text.Trim();
            string mucDo = cboMucDo.SelectedItem.ToString();
            decimal soTien = ParseDecimal(txtSoTienDenBu.Text);

            try
            {
                object daCoPhieu = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM PhieuDenBu WHERE SoPhieuDenBu=@sp", new SqlParameter("@sp", soPhieuDenBu));
                if (Convert.ToInt32(daCoPhieu) == 0)
                {
                    DbHelper.ExecuteNonQuery(
                        @"INSERT INTO PhieuDenBu(SoPhieuDenBu, SoPhieuDat, SoPhong, NgayLap, MaNV, TongTien)
                          VALUES(@sp,@pd,@ph,@ngay,@nv,0)",
                        new SqlParameter("@sp", soPhieuDenBu),
                        new SqlParameter("@pd", txtSoPhieuDat.Text.Trim()),
                        new SqlParameter("@ph", _soPhongDangChonDenBu),
                        new SqlParameter("@ngay", DateTime.Now),
                        new SqlParameter("@nv", cboNhanVienDenBu.SelectedValue));
                }

                object daCoChiTiet = DbHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM ChiTietPhieuDenBu WHERE SoPhieuDenBu=@sp AND MaTienNghi=@tn",
                    new SqlParameter("@sp", soPhieuDenBu), new SqlParameter("@tn", maTienNghi));
                if (Convert.ToInt32(daCoChiTiet) > 0)
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE ChiTietPhieuDenBu SET MucDoThietHai=@md, SoTien=@st WHERE SoPhieuDenBu=@sp AND MaTienNghi=@tn",
                        new SqlParameter("@md", mucDo), new SqlParameter("@st", soTien),
                        new SqlParameter("@sp", soPhieuDenBu), new SqlParameter("@tn", maTienNghi));
                }
                else
                {
                    DbHelper.ExecuteNonQuery(
                        "INSERT INTO ChiTietPhieuDenBu(SoPhieuDenBu, MaTienNghi, MucDoThietHai, SoTien) VALUES(@sp,@tn,@md,@st)",
                        new SqlParameter("@sp", soPhieuDenBu), new SqlParameter("@tn", maTienNghi),
                        new SqlParameter("@md", mucDo), new SqlParameter("@st", soTien));
                }

                DbHelper.ExecuteNonQuery(
                    @"UPDATE PhieuDenBu SET TongTien = (SELECT ISNULL(SUM(SoTien),0) FROM ChiTietPhieuDenBu WHERE SoPhieuDenBu=@sp)
                      WHERE SoPhieuDenBu=@sp",
                    new SqlParameter("@sp", soPhieuDenBu));

                MessageBox.Show(this, "Lập/cập nhật phiếu đền bù thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                gridTienNghiDenBu.Rows.Clear();
                DataTable ct = DbHelper.GetDataTable(
                    "SELECT MaTienNghi, MucDoThietHai, SoTien FROM ChiTietPhieuDenBu WHERE SoPhieuDenBu=@sp ORDER BY MaTienNghi",
                    new SqlParameter("@sp", soPhieuDenBu));
                foreach (DataRow r in ct.Rows)
                    gridTienNghiDenBu.Rows.Add(r["MaTienNghi"], r["MucDoThietHai"], r["SoTien"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== HÓA ĐƠN =====================
        private void LoadHoaDonCuaPhieu(string soPhieuDat)
        {
            gridHoaDon.Rows.Clear();
            DataTable dt = DbHelper.GetDataTable(
                @"SELECT SoHoaDon, SoPhieuDat, TienPhong, TienDichVu, TongTien, TrangThai
                  FROM HoaDon WHERE SoPhieuDat=@sp ORDER BY NgayLap DESC",
                new SqlParameter("@sp", soPhieuDat));
            foreach (DataRow r in dt.Rows)
                gridHoaDon.Rows.Add(r["SoHoaDon"], r["SoPhieuDat"], r["TienPhong"], r["TienDichVu"], r["TongTien"], r["TrangThai"]);
        }

        private void BtnLapHoaDon_Click(object sender, EventArgs e)
        {
            string soPhieu = txtSoPhieuDat.Text.Trim();
            if (string.IsNullOrWhiteSpace(soPhieu) || gridPhong.Rows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng tải phiếu trước khi lập hóa đơn.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtSoHoaDon.Text) || cboNhanVienHD.SelectedValue == null)
            {
                MessageBox.Show(this, "Vui lòng nhập Số hóa đơn và chọn Nhân viên.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int soNgay = ParseInt(txtSoNgayTinhTien.Text);
            if (soNgay <= 0)
            {
                MessageBox.Show(this, "Số ngày tính tiền phải lớn hơn 0.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal tienPhongMotNgay = 0;
                foreach (DataGridViewRow r in gridPhong.Rows)
                    tienPhongMotNgay += ParseDecimal(Convert.ToString(r.Cells["DonGia"].Value));
                decimal tienPhong = tienPhongMotNgay * soNgay;

                object tienDVObj = DbHelper.ExecuteScalar(
                    @"SELECT ISNULL(SUM(ct.ThanhTien),0) FROM ChiTietPhieuSuDungDV ct
                      JOIN PhieuSuDungDV p ON ct.SoPhieuSDDV = p.SoPhieuSDDV
                      WHERE p.SoPhieuDat=@sp",
                    new SqlParameter("@sp", soPhieu));
                object tienDenBuObj = DbHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(TongTien),0) FROM PhieuDenBu WHERE SoPhieuDat=@sp",
                    new SqlParameter("@sp", soPhieu));
                // Tiền dịch vụ trên hóa đơn gồm cả tiền dịch vụ đã dùng và tiền đền bù (thiết bị) của phiếu này,
                // vì bảng HoaDon chỉ có 2 cột tiền: TienPhong và TienDichVu.
                decimal tienDichVu = Convert.ToDecimal(tienDVObj) + Convert.ToDecimal(tienDenBuObj);

                DbHelper.ExecuteNonQuery(
                    @"INSERT INTO HoaDon(SoHoaDon, SoPhieuDat, NgayLap, MaNV, SoNgayTinhTien, TienPhong, TienDichVu)
                      VALUES(@hd,@sp,@ngay,@nv,@sn,@tp,@tdv)",
                    new SqlParameter("@hd", txtSoHoaDon.Text.Trim()),
                    new SqlParameter("@sp", soPhieu),
                    new SqlParameter("@ngay", DateTime.Now),
                    new SqlParameter("@nv", cboNhanVienHD.SelectedValue),
                    new SqlParameter("@sn", soNgay),
                    new SqlParameter("@tp", tienPhong),
                    new SqlParameter("@tdv", tienDichVu));

                MessageBox.Show(this, "Lập hóa đơn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadHoaDonCuaPhieu(soPhieu);
                txtSoTienTT.Text = (tienPhong + tienDichVu).ToString(CultureInfo.InvariantCulture);
                _soHoaDonDangChon = txtSoHoaDon.Text.Trim();
                _tongTienHoaDonDangChon = tienPhong + tienDichVu;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== THANH TOÁN =====================
        private void GridHoaDon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = gridHoaDon.Rows[e.RowIndex];
            _soHoaDonDangChon = Convert.ToString(row.Cells["HoaDon"].Value);
            _tongTienHoaDonDangChon = ParseDecimal(Convert.ToString(row.Cells["TongTien"].Value));
            txtSoTienTT.Text = _tongTienHoaDonDangChon.ToString(CultureInfo.InvariantCulture);
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_soHoaDonDangChon))
            {
                MessageBox.Show(this, "Vui lòng chọn 1 hóa đơn ở lưới bên trên.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cboHinhThuc.SelectedItem == null)
            {
                MessageBox.Show(this, "Vui lòng chọn hình thức thanh toán.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            decimal soTien = ParseDecimal(txtSoTienTT.Text);
            if (soTien <= 0)
            {
                MessageBox.Show(this, "Số tiền phải lớn hơn 0.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string maThanhToan = _soHoaDonDangChon + "-" + DateTime.Now.ToString("HHmmssfff");
                DbHelper.ExecuteNonQuery(
                    @"INSERT INTO ThanhToan(MaThanhToan, SoHoaDon, NgayThanhToan, HinhThuc, SoTien)
                      VALUES(@mtt,@hd,@ngay,@ht,@st)",
                    new SqlParameter("@mtt", maThanhToan),
                    new SqlParameter("@hd", _soHoaDonDangChon),
                    new SqlParameter("@ngay", DateTime.Now),
                    new SqlParameter("@ht", cboHinhThuc.SelectedItem.ToString()),
                    new SqlParameter("@st", soTien));

                object tongDaTraObj = DbHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE SoHoaDon=@hd", new SqlParameter("@hd", _soHoaDonDangChon));
                decimal tongDaTra = Convert.ToDecimal(tongDaTraObj);
                if (tongDaTra >= _tongTienHoaDonDangChon)
                {
                    DbHelper.ExecuteNonQuery(
                        "UPDATE HoaDon SET TrangThai=N'Đã thanh toán' WHERE SoHoaDon=@hd",
                        new SqlParameter("@hd", _soHoaDonDangChon));
                }

                MessageBox.Show(this, "Thanh toán thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadHoaDonCuaPhieu(txtSoPhieuDat.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== HOÀN TẤT TRẢ PHÒNG =====================
        private void BtnHoanTat_Click(object sender, EventArgs e)
        {
            string soPhieu = txtSoPhieuDat.Text.Trim();
            if (string.IsNullOrWhiteSpace(soPhieu) || gridPhong.Rows.Count == 0)
            {
                MessageBox.Show(this, "Vui lòng tải phiếu trước.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object chuaThanhToan = DbHelper.ExecuteScalar(
                @"SELECT COUNT(*) FROM HoaDon WHERE SoPhieuDat=@sp AND TrangThai <> N'Đã thanh toán'",
                new SqlParameter("@sp", soPhieu));
            object coHoaDon = DbHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM HoaDon WHERE SoPhieuDat=@sp", new SqlParameter("@sp", soPhieu));
            if (Convert.ToInt32(coHoaDon) == 0 || Convert.ToInt32(chuaThanhToan) > 0)
            {
                MessageBox.Show(this, "Phiếu này chưa có hóa đơn hoặc còn hóa đơn chưa thanh toán xong.",
                    "Chưa thể trả phòng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(this, "Xác nhận hoàn tất trả phòng cho phiếu " + soPhieu + "?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                DbHelper.ExecuteNonQuery(
                    "UPDATE PhieuDatPhong SET TrangThai=N'Đã trả', NgayTraThucTe=@now WHERE SoPhieuDat=@sp",
                    new SqlParameter("@now", DateTime.Now), new SqlParameter("@sp", soPhieu));

                foreach (DataGridViewRow r in gridPhong.Rows)
                {
                    string soPhong = Convert.ToString(r.Cells["Phong"].Value);
                    DbHelper.ExecuteNonQuery(
                        "UPDATE Phong SET TrangThai=N'Trống' WHERE SoPhong=@ph", new SqlParameter("@ph", soPhong));
                }

                MessageBox.Show(this, "Trả phòng hoàn tất. Các phòng đã được mở lại trạng thái Trống.",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtSoPhieuDat.Clear();
                gridPhong.Rows.Clear();
                gridTienNghi.Rows.Clear();
                gridTienNghiDenBu.Rows.Clear();
                gridHoaDon.Rows.Clear();
                txtSoPhieuDenBu.Clear();
                txtSoHoaDon.Clear();
                txtSoTienTT.Clear();
                _soHoaDonDangChon = null;
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
