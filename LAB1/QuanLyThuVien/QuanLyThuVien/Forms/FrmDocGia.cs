using System;
using System.Data;
using System.Windows.Forms;
using QuanLyThuVien.Services;

namespace QuanLyThuVien.Forms
{
    public partial class FrmDocGia : Form
    {
        private readonly DocGiaService service = new DocGiaService();

        public FrmDocGia() { InitializeComponent(); }

        private void FrmDocGia_Load(object sender, EventArgs e)
        {
            cboPhai.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            if (cboPhai.Items.Count > 0) cboPhai.SelectedIndex = 0;

            TaiDuLieu();
            LamMoi();
        }

        private void TaiDuLieu()
        {
            dgvDocGia.DataSource = service.LayDanhSach(txtTim.Text.Trim());
            dgvDocGia.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Ẩn cột kỹ thuật, chỉ hiện các cột theo đúng mockup: Mã, Họ, Tên, Phái, Điện thoại, Email, Hạn thẻ
            if (dgvDocGia.Columns.Contains("MaThe")) dgvDocGia.Columns["MaThe"].Visible = false;
            if (dgvDocGia.Columns.Contains("NgaySinh")) dgvDocGia.Columns["NgaySinh"].Visible = false;
            if (dgvDocGia.Columns.Contains("DiaChi")) dgvDocGia.Columns["DiaChi"].Visible = false;
            if (dgvDocGia.Columns.Contains("Anh3x4")) dgvDocGia.Columns["Anh3x4"].Visible = false;
            if (dgvDocGia.Columns.Contains("NgayCap")) dgvDocGia.Columns["NgayCap"].Visible = false;
            if (dgvDocGia.Columns.Contains("DaDongLePhi")) dgvDocGia.Columns["DaDongLePhi"].Visible = false;
            if (dgvDocGia.Columns.Contains("HanSuDung")) dgvDocGia.Columns["HanSuDung"].HeaderText = "Hạn thẻ";
        }

        private DocGia LayDuLieuForm()
        {
            return new DocGia
            {
                MaDocGia = txtMa.Text.Trim(),
                Ho = txtHo.Text.Trim(),
                Ten = txtTen.Text.Trim(),
                NgaySinh = dtNgaySinh.Value,
                Phai = Convert.ToString(cboPhai.SelectedItem),
                SoDienThoai = txtSDT.Text.Trim(),
                DiaChi = txtDiaChi.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Anh3x4 = string.IsNullOrWhiteSpace(txtAnh.Text) ? null : txtAnh.Text.Trim()
            };
        }

        private void HienKetQua(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi",
                MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (kq.ThanhCong) { TaiDuLieu(); LamMoi(); }
        }

        private void btnThem_Click(object sender, EventArgs e) { HienKetQua(service.Luu(LayDuLieuForm(), false)); }
        private void btnCapNhat_Click(object sender, EventArgs e) { HienKetQua(service.Luu(LayDuLieuForm(), true)); }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) return;
            if (MessageBox.Show("Xóa độc giả đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                HienKetQua(service.Xoa(txtMa.Text.Trim()));
        }

        private void btnCapThe_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text))
            {
                MessageBox.Show("Vui lòng chọn độc giả cần cấp thẻ.", "Thông báo");
                return;
            }
            HienKetQua(service.CapThe(txtMa.Text.Trim(), dtNgayCap.Value, dtHanSuDung.Value, chkDaDongLePhi.Checked));
        }

        private void btnGiaHan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text))
            {
                MessageBox.Show("Vui lòng chọn độc giả cần gia hạn thẻ.", "Thông báo");
                return;
            }
            HienKetQua(service.GiaHan(txtMa.Text.Trim(), dtHanSuDung.Value));
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK) txtAnh.Text = ofd.FileName;
            }
        }

        private void btnTim_Click(object sender, EventArgs e) { TaiDuLieu(); }
        private void btnLamMoi_Click(object sender, EventArgs e) { LamMoi(); }
        private void btnDong_Click(object sender, EventArgs e) { Close(); }

        private void LamMoi()
        {
            txtMa.Clear(); txtHo.Clear(); txtTen.Clear(); txtSDT.Clear();
            txtDiaChi.Clear(); txtEmail.Clear(); txtAnh.Clear();
            dtNgaySinh.Value = DateTime.Today.AddYears(-20);
            if (cboPhai.Items.Count > 0) cboPhai.SelectedIndex = 0;
            dtNgayCap.Value = DateTime.Today;
            dtHanSuDung.Value = DateTime.Today.AddYears(1);
            chkDaDongLePhi.Checked = false;

            txtMa.ReadOnly = false; txtMa.Focus();
            btnThem.Enabled = true; btnCapNhat.Enabled = false; btnXoa.Enabled = false;
        }

        private void dgvDocGia_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDocGia.CurrentRow == null || dgvDocGia.CurrentRow.DataBoundItem == null) return;
            DataRowView r = dgvDocGia.CurrentRow.DataBoundItem as DataRowView;
            if (r == null) return;

            txtMa.Text = Convert.ToString(r["MaDocGia"]);
            txtHo.Text = Convert.ToString(r["Ho"]);
            txtTen.Text = Convert.ToString(r["Ten"]);
            dtNgaySinh.Value = Convert.ToDateTime(r["NgaySinh"]);
            cboPhai.SelectedItem = Convert.ToString(r["Phai"]);
            txtSDT.Text = Convert.ToString(r["SoDienThoai"]);
            txtDiaChi.Text = Convert.ToString(r["DiaChi"]);
            txtEmail.Text = Convert.ToString(r["Email"]);
            txtAnh.Text = Convert.ToString(r["Anh3x4"]);

            if (r["NgayCap"] != DBNull.Value) dtNgayCap.Value = Convert.ToDateTime(r["NgayCap"]);
            if (r["HanSuDung"] != DBNull.Value) dtHanSuDung.Value = Convert.ToDateTime(r["HanSuDung"]);
            else dtHanSuDung.Value = DateTime.Today.AddYears(1);
            chkDaDongLePhi.Checked = r["DaDongLePhi"] != DBNull.Value && Convert.ToBoolean(r["DaDongLePhi"]);

            txtMa.ReadOnly = true;
            btnThem.Enabled = false; btnCapNhat.Enabled = true; btnXoa.Enabled = true;
        }
    }
}
