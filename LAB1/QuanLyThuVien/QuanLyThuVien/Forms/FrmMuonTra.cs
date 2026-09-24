using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using QuanLyThuVien.Services;

namespace QuanLyThuVien.Forms
{
    public partial class FrmMuonTra : Form
    {
        private readonly MuonTraService service = new MuonTraService();

        // Danh sách sách đã chọn để mượn (lưu tạm trong bộ nhớ, chưa ghi DB)
        private readonly DataTable bangDaChon = new DataTable();

        public FrmMuonTra() { InitializeComponent(); }

        private void FrmMuonTra_Load(object sender, EventArgs e)
        {
            bangDaChon.Columns.Add("MaDauSach");
            bangDaChon.Columns.Add("TenSach");
            dgvDaChon.DataSource = bangDaChon;
            dgvDaChon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDaChon.Columns["MaDauSach"].HeaderText = "Mã";
            dgvDaChon.Columns["TenSach"].HeaderText = "Tên sách";

            dtNgayMuon.Value = DateTime.Today;
            dtHenTra.Value = DateTime.Today.AddDays(7);

            cboTinhTrang.Items.AddRange(new object[] { "Bình thường", "Mất", "Hư hỏng" });
            cboTinhTrang.SelectedIndex = 0;

            TaiKho();
        }

        // ================= TAB MƯỢN SÁCH =================

        private void TaiKho()
        {
            dgvKho.DataSource = service.LaySachConTrongKho(null);
            dgvKho.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvKho.Columns.Contains("MaDauSach")) dgvKho.Columns["MaDauSach"].HeaderText = "Mã";
            if (dgvKho.Columns.Contains("TenSach")) dgvKho.Columns["TenSach"].HeaderText = "Tên sách";
            if (dgvKho.Columns.Contains("NamXuatBan")) dgvKho.Columns["NamXuatBan"].HeaderText = "Năm XB";
            if (dgvKho.Columns.Contains("SoLuongHienCo")) dgvKho.Columns["SoLuongHienCo"].HeaderText = "Còn";
        }

        private void btnKiemTra_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDocGia.Text))
            {
                MessageBox.Show("Vui lòng nhập mã độc giả.", "Thông báo");
                return;
            }

            DataRow r = service.LayDocGia(txtDocGia.Text.Trim());
            string tenDocGia = r != null ? txtDocGia.Text.Trim() + " - " + r["Ho"] + " " + r["Ten"] : txtDocGia.Text.Trim();

            KetQuaXuLy kq = service.KiemTraDieuKienMuon(txtDocGia.Text.Trim());
            lblDieuKien.ForeColor = kq.ThanhCong ? System.Drawing.Color.DarkGreen : System.Drawing.Color.DarkRed;
            lblDieuKien.Text = kq.ThanhCong
                ? tenDocGia + " - Đủ điều kiện mượn sách"
                : "Không đủ điều kiện: " + kq.ThongBao;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (dgvKho.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 đầu sách trong kho.", "Thông báo");
                return;
            }

            foreach (DataGridViewRow row in dgvKho.SelectedRows)
            {
                if (bangDaChon.Rows.Count >= MuonTraService.SO_SACH_TOI_DA_MOI_PHIEU)
                {
                    MessageBox.Show("Chỉ được chọn tối đa " + MuonTraService.SO_SACH_TOI_DA_MOI_PHIEU + " đầu sách mỗi phiếu.", "Thông báo");
                    break;
                }

                DataRowView rv = row.DataBoundItem as DataRowView;
                if (rv == null) continue;
                string maSach = Convert.ToString(rv["MaDauSach"]);

                bool daCo = bangDaChon.AsEnumerable().Any(x => Convert.ToString(x["MaDauSach"]) == maSach);
                if (daCo) continue;

                DataRow moi = bangDaChon.NewRow();
                moi["MaDauSach"] = maSach;
                moi["TenSach"] = rv["TenSach"];
                bangDaChon.Rows.Add(moi);
            }
        }

        private void btnBo_Click(object sender, EventArgs e)
        {
            if (dgvDaChon.SelectedRows.Count == 0) return;
            List<DataRowView> canXoa = new List<DataRowView>();
            foreach (DataGridViewRow row in dgvDaChon.SelectedRows)
            {
                DataRowView rv = row.DataBoundItem as DataRowView;
                if (rv != null) canXoa.Add(rv);
            }
            foreach (DataRowView rv in canXoa) rv.Row.Delete();
            bangDaChon.AcceptChanges();
        }

        private void btnLapPhieuMuon_Click(object sender, EventArgs e)
        {
            string[] dsMaSach = bangDaChon.AsEnumerable()
                .Select(r => Convert.ToString(r["MaDauSach"])).ToArray();

            KetQuaXuLy kq = service.LapPhieuMuon(
                txtDocGia.Text.Trim(), txtNhanVien.Text.Trim(),
                dtNgayMuon.Value, dtHenTra.Value, dsMaSach);

            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi",
                MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (kq.ThanhCong)
            {
                bangDaChon.Rows.Clear();
                lblDieuKien.Text = "";
                txtDocGia.Clear();
                TaiKho();
            }
        }

        // ================= TAB TRẢ SÁCH =================

        private void btnTimSachDangMuon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDocGiaTra.Text))
            {
                MessageBox.Show("Vui lòng nhập mã độc giả.", "Thông báo");
                return;
            }

            DataTable t = service.LaySachDangMuon(txtDocGiaTra.Text.Trim());
            dgvDangMuon.DataSource = t;
            dgvDangMuon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (dgvDangMuon.Columns.Contains("MaChiTiet")) dgvDangMuon.Columns["MaChiTiet"].Visible = false;
            if (dgvDangMuon.Columns.Contains("MaDauSach")) dgvDangMuon.Columns["MaDauSach"].HeaderText = "Mã sách";
            if (dgvDangMuon.Columns.Contains("TenSach")) dgvDangMuon.Columns["TenSach"].HeaderText = "Tên sách";
            if (dgvDangMuon.Columns.Contains("NgayMuon")) dgvDangMuon.Columns["NgayMuon"].HeaderText = "Ngày mượn";
            if (dgvDangMuon.Columns.Contains("NgayHenTra")) dgvDangMuon.Columns["NgayHenTra"].HeaderText = "Hạn trả";
            if (dgvDangMuon.Columns.Contains("SoNgayTre")) dgvDangMuon.Columns["SoNgayTre"].HeaderText = "Số ngày trễ";

            if (t.Rows.Count == 0)
                MessageBox.Show("Độc giả này hiện không có sách nào đang mượn.", "Thông báo");

            CapNhatPhiPhatDuKien();
        }

        private void dgvDangMuon_SelectionChanged(object sender, EventArgs e) { CapNhatPhiPhatDuKien(); }
        private void cboTinhTrang_SelectedIndexChanged(object sender, EventArgs e) { CapNhatPhiPhatDuKien(); }

        private void CapNhatPhiPhatDuKien()
        {
            int soNgayTre = 0;
            if (dgvDangMuon.CurrentRow != null && dgvDangMuon.CurrentRow.DataBoundItem is DataRowView rv)
                soNgayTre = Convert.ToInt32(rv["SoNgayTre"]);

            string tinhTrang = Convert.ToString(cboTinhTrang.SelectedItem);
            decimal phi = service.TinhPhiPhatDuKien(soNgayTre, tinhTrang);
            lblPhiPhatGiaTri.Text = phi.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        }

        private void btnXacNhanTra_Click(object sender, EventArgs e)
        {
            if (dgvDangMuon.CurrentRow == null || !(dgvDangMuon.CurrentRow.DataBoundItem is DataRowView rv))
            {
                MessageBox.Show("Vui lòng chọn sách cần trả trong danh sách.", "Thông báo");
                return;
            }

            string maChiTiet = Convert.ToString(rv["MaChiTiet"]);
            string tinhTrang = Convert.ToString(cboTinhTrang.SelectedItem);

            if (string.IsNullOrWhiteSpace(txtNhanVienTra.Text))
            {
                MessageBox.Show("Vui lòng nhập mã nhân viên xử lý trả sách.", "Thông báo");
                return;
            }

            KetQuaXuLy kq = service.TraSach(maChiTiet, tinhTrang, txtNhanVienTra.Text.Trim());
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi",
                MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (kq.ThanhCong) btnTimSachDangMuon_Click(sender, e);
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
