using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Danh mục phòng ban: thêm / sửa / xóa
    public class TabPhongBan : TabBase
    {
        private readonly TextBox txtMa = new TextBox();
        private readonly TextBox txtTen = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabPhongBan()
        {
            Button them = UiHelper.TaoNut("Thêm");
            Button sua = UiHelper.TaoNut("Sửa");
            Button xoa = UiHelper.TaoNut("Xóa");
            them.Click += BtnThem_Click;
            sua.Click += BtnSua_Click;
            xoa.Click += BtnXoa_Click;
            FlowLayoutPanel nut = new FlowLayoutPanel();
            nut.AutoSize = true;
            nut.Controls.Add(them);
            nut.Controls.Add(sua);
            nut.Controls.Add(xoa);

            grid.SelectionChanged += delegate
            {
                if (grid.CurrentRow == null) return;
                txtMa.Text = Convert.ToString(grid.CurrentRow.Cells["Mã PB"].Value);
                txtTen.Text = Convert.ToString(grid.CurrentRow.Cells["Tên phòng ban"].Value);
            };

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Mã phòng ban", txtMa);
            UiHelper.ThemDong(f, "Tên phòng ban", txtTen);
            UiHelper.ThemDong(f, "", nut);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            grid.DataSource = Db.Query("SELECT MaPB AS [Mã PB], TenPB AS [Tên phòng ban] FROM PhongBan ORDER BY MaPB");
        }

        private bool HopLe()
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text) || string.IsNullOrWhiteSpace(txtTen.Text))
            {
                MessageBox.Show("Vui lòng nhập mã và tên phòng ban.");
                return false;
            }
            return true;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (!HopLe()) return;
            try
            {
                Db.Execute("INSERT INTO PhongBan(MaPB,TenPB) VALUES(@Ma,@Ten)",
                    new SqlParameter("@Ma", txtMa.Text.Trim()), new SqlParameter("@Ten", txtTen.Text.Trim()));
                NapLai();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("Mã phòng ban đã tồn tại.");
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (!HopLe()) return;
            try
            {
                int n = Db.Execute("UPDATE PhongBan SET TenPB=@Ten WHERE MaPB=@Ma",
                    new SqlParameter("@Ma", txtMa.Text.Trim()), new SqlParameter("@Ten", txtTen.Text.Trim()));
                if (n == 0) MessageBox.Show("Không tìm thấy phòng ban (không được đổi mã).");
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { MessageBox.Show("Vui lòng chọn phòng ban."); return; }
            if (MessageBox.Show("Xóa phòng ban " + txtMa.Text + "?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                Db.Execute("DELETE FROM PhongBan WHERE MaPB=@Ma", new SqlParameter("@Ma", txtMa.Text.Trim()));
                txtMa.Clear(); txtTen.Clear();
                NapLai();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Không thể xóa: phòng ban đang được dùng trong quyết định.");
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
