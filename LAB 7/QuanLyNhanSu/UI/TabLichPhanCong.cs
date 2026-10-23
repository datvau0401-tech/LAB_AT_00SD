using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Kế hoạch phân công nhân viên theo ca làm việc
    public class TabLichPhanCong : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly DateTimePicker dtpNgay = new DateTimePicker();
        private readonly ComboBox cboCa = new ComboBox();
        private readonly TextBox txtGhiChu = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabLichPhanCong()
        {
            dtpNgay.Format = DateTimePickerFormat.Short;
            UiHelper.NapComboCoDinh(cboCa, "Ca sáng", "Ca sáng (6h-14h)", "Ca chiều", "Ca chiều (14h-22h)", "Ca đêm", "Ca đêm (22h-6h)");
            Button them = UiHelper.TaoNut("Thêm phân công");
            Button xoa = UiHelper.TaoNut("Xóa dòng đang chọn");
            them.Click += BtnThem_Click;
            xoa.Click += BtnXoa_Click;
            FlowLayoutPanel nut = new FlowLayoutPanel();
            nut.AutoSize = true;
            nut.Controls.Add(them);
            nut.Controls.Add(xoa);

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "Ngày làm", dtpNgay);
            UiHelper.ThemDong(f, "Ca", cboCa);
            UiHelper.ThemDong(f, "Ghi chú", txtGhiChu);
            UiHelper.ThemDong(f, "", nut);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            grid.DataSource = Db.Query(@"SELECT l.Id AS [Id], l.MaNV AS [Mã NV], n.HoTen AS [Họ tên],
                    l.NgayLam AS [Ngày làm], l.Ca AS [Ca], l.GhiChu AS [Ghi chú]
                FROM LichPhanCong l JOIN NhanVien n ON n.MaNV=l.MaNV
                ORDER BY l.NgayLam DESC, l.Id DESC");
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            string maNV = UiHelper.GiaTri(cboNV);
            if (maNV == null) { MessageBox.Show("Vui lòng chọn nhân viên."); return; }
            try
            {
                Db.Execute("INSERT INTO LichPhanCong(MaNV,NgayLam,Ca,GhiChu) VALUES(@NV,@Ngay,@Ca,@GC)",
                    new SqlParameter("@NV", maNV), new SqlParameter("@Ngay", dtpNgay.Value.Date),
                    new SqlParameter("@Ca", UiHelper.GiaTri(cboCa)), new SqlParameter("@GC", txtGhiChu.Text.Trim()));
                txtGhiChu.Clear();
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Vui lòng chọn một dòng."); return; }
            try
            {
                Db.Execute("DELETE FROM LichPhanCong WHERE Id=@Id",
                    new SqlParameter("@Id", Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value)));
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
