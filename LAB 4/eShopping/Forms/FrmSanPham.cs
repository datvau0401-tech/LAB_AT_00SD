using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using eShopping.Adapters;
using eShopping.Services;

namespace eShopping.Forms
{
    public class FrmSanPham : Form
    {
        private readonly IProductSystemAdapter sp = new ProductSystemAdapter();
        private readonly GioHangService gio = new GioHangService();

        private ComboBox cboNhom;
        private DataGridView dgvSanPham;
        private TextBox txtChiTiet;
        private NumericUpDown numSoLuong;
        private Button btnThemGio, btnDong;
        private bool daTai;

        public FrmSanPham()
        {
            KhoiTao();
            Load += FrmSanPham_Load;
        }

        private void KhoiTao()
        {
            Text = "Danh sách sản phẩm";
            ClientSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9.5F);

            Ui.Nhan(this, "Nhóm sản phẩm:", 15, 15, 110);
            cboNhom = Ui.Combo(this, "cboNhom", 130, 15, 300);
            dgvSanPham = Ui.Luoi(this, "dgvSanPham", 15, 55, 600, 480);

            txtChiTiet = new TextBox
            {
                Name = "txtChiTiet", Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Left = 630, Top = 55, Width = 355, Height = 380
            };
            Controls.Add(txtChiTiet);

            Ui.Nhan(this, "Số lượng:", 630, 450, 70);
            numSoLuong = new NumericUpDown { Name = "numSoLuong", Left = 705, Top = 450, Width = 70, Minimum = 1, Maximum = 99, Value = 1 };
            Controls.Add(numSoLuong);
            btnThemGio = Ui.Nut(this, "btnThemGio", "Thêm vào giỏ", 790, 446, 195);
            btnDong = Ui.Nut(this, "btnDong", "Đóng", 885, 550, 100);

            cboNhom.SelectedIndexChanged += cboNhom_SelectedIndexChanged;
            dgvSanPham.SelectionChanged += dgvSanPham_SelectionChanged;
            btnThemGio.Click += btnThemGio_Click;
            btnDong.Click += btnDong_Click;
        }

        private void FrmSanPham_Load(object sender, EventArgs e)
        {
            cboNhom.DisplayMember = "TenNhom";
            cboNhom.ValueMember = "MaNhom";
            cboNhom.DataSource = new List<NhomSanPham>(sp.LayNhomSanPham());
            daTai = true;
            TaiSanPham();
        }

        private void cboNhom_SelectedIndexChanged(object sender, EventArgs e) { TaiSanPham(); }

        private void TaiSanPham()
        {
            if (!daTai || cboNhom.SelectedValue == null) return;

            DataTable t = new DataTable();
            t.Columns.Add("MaSanPham");
            t.Columns.Add("TenSanPham");
            t.Columns.Add("NhaSanXuat");
            t.Columns.Add("GiaBan");
            t.Columns.Add("TinhTrang");
            foreach (SanPham s in sp.LaySanPhamTheoNhom(Convert.ToString(cboNhom.SelectedValue)))
                t.Rows.Add(s.MaSanPham, s.TenSanPham, s.TenNhaSanXuat, Ui.Tien(s.GiaBan), s.ConHang ? "Còn hàng" : "Hết hàng");

            dgvSanPham.DataSource = t;
            dgvSanPham.Columns["MaSanPham"].HeaderText = "Mã";
            dgvSanPham.Columns["TenSanPham"].HeaderText = "Tên sản phẩm";
            dgvSanPham.Columns["NhaSanXuat"].HeaderText = "Nhà sản xuất";
            dgvSanPham.Columns["GiaBan"].HeaderText = "Giá bán";
            dgvSanPham.Columns["TinhTrang"].HeaderText = "Tình trạng";
            HienChiTiet();
        }

        private string MaDangChon()
        {
            if (dgvSanPham.CurrentRow == null) return null;
            return Convert.ToString(dgvSanPham.CurrentRow.Cells["MaSanPham"].Value);
        }

        private void dgvSanPham_SelectionChanged(object sender, EventArgs e) { HienChiTiet(); }

        private void HienChiTiet()
        {
            string ma = MaDangChon();
            if (string.IsNullOrEmpty(ma)) { txtChiTiet.Text = ""; return; }
            SanPham s = sp.LayChiTiet(ma);
            if (s == null) { txtChiTiet.Text = "Không tìm thấy sản phẩm."; return; }

            txtChiTiet.Text = string.Join(Environment.NewLine, new[]
            {
                "Mã: " + s.MaSanPham,
                "Tên: " + s.TenSanPham,
                "Nhà sản xuất: " + s.TenNhaSanXuat,
                "Giá bán: " + Ui.Tien(s.GiaBan),
                "Tình trạng: " + (s.ConHang ? "Còn hàng" : "Hết hàng"),
                "",
                "Mô tả:",
                s.MoTa,
                "",
                "Thông số kỹ thuật:",
                s.ThongSoKyThuat,
                "",
                "Hình ảnh: " + (s.HinhAnh.Count == 0 ? "(chưa có)" : string.Join("; ", s.HinhAnh))
            });
        }

        // Dùng chung cho cả "thêm từ danh sách" và "thêm khi đang xem chi tiết".
        private void btnThemGio_Click(object sender, EventArgs e)
        {
            string ma = MaDangChon();
            if (string.IsNullOrEmpty(ma)) { MessageBox.Show("Vui lòng chọn một sản phẩm."); return; }
            Ui.HienKetQua(gio.Them(sp.LayChiTiet(ma), (int)numSoLuong.Value));
        }

        private void btnDong_Click(object sender, EventArgs e) { Close(); }
    }
}
