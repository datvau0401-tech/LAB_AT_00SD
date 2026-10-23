using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.UI
{
    // UC02 - Lập quyết định tuyển dụng (thử việc)
    public class TabTuyenDung : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly ComboBox cboPB = new ComboBox();
        private readonly NumericUpDown numThang = new NumericUpDown();
        private readonly TextBox txtLuong = new TextBox();
        private readonly TextBox txtSoQD = new TextBox();
        private readonly TextBox txtDieuKhoan = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();
        private readonly NhanSuService svc = new NhanSuService();

        public TabTuyenDung()
        {
            numThang.Minimum = 1; numThang.Maximum = 24; numThang.Value = 2;
            txtDieuKhoan.Multiline = true; txtDieuKhoan.Height = 60;
            txtDieuKhoan.Text = "Hai bên thực hiện đúng nội quy công ty trong thời gian thử việc.";

            Button btn = UiHelper.TaoNut("Lập quyết định tuyển dụng");
            btn.Click += BtnLap_Click;

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "Phòng ban thử việc", cboPB);
            UiHelper.ThemDong(f, "Thời gian thử việc (tháng)", numThang);
            UiHelper.ThemDong(f, "Mức lương thử việc (đ)", txtLuong);
            UiHelper.ThemDong(f, "Số quyết định", txtSoQD);
            UiHelper.ThemDong(f, "Điều khoản hai bên", txtDieuKhoan);
            UiHelper.ThemDong(f, "", btn);

            Controls.Add(grid);   // Fill thêm trước, Top thêm sau
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            UiHelper.NapComboPhongBan(cboPB);
            grid.DataSource = Db.Query(@"SELECT q.MaQD AS [Mã QĐ], q.SoQD AS [Số QĐ], q.MaNV AS [Mã NV], n.HoTen AS [Họ tên],
                    t.MaPB AS [Phòng ban], t.ThoiGianThuViecThang AS [Thử việc (tháng)],
                    t.MucLuongThuViec AS [Lương thử việc],
                    CASE WHEN q.TrangThaiKy=1 THEN N'Đã ký' ELSE N'Chờ ký' END AS [Trạng thái]
                FROM QuyetDinh q
                JOIN QuyetDinhTuyenDung t ON t.MaQD=q.MaQD
                JOIN NhanVien n ON n.MaNV=q.MaNV
                ORDER BY q.NgayQD DESC, q.MaQD DESC");
        }

        private void BtnLap_Click(object sender, EventArgs e)
        {
            decimal luong;
            if (!UiHelper.DocSo(txtLuong, out luong)) { MessageBox.Show("Mức lương thử việc không hợp lệ."); return; }
            if (string.IsNullOrWhiteSpace(txtSoQD.Text)) { MessageBox.Show("Vui lòng nhập số quyết định."); return; }
            try
            {
                KetQuaXuLy kq = svc.LapQuyetDinhTuyenDung(UiHelper.GiaTri(cboNV), UiHelper.GiaTri(cboPB),
                    (int)numThang.Value, luong, txtDieuKhoan.Text, txtSoQD.Text.Trim());
                UiHelper.ThongBao(kq);
                if (kq.ThanhCong) { txtSoQD.Clear(); NapLai(); }
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
