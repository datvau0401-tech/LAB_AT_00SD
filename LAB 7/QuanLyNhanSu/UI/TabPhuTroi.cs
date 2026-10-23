using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Phiếu ghi nhận giờ phụ trội (hệ số 1,5) + chi tiết từng nhân viên
    public class TabPhuTroi : TabBase
    {
        private readonly TextBox txtSoPhieu = new TextBox();
        private readonly DateTimePicker dtpNgay = new DateTimePicker();
        private readonly ComboBox cboHinhThuc = new ComboBox();
        private readonly TextBox txtSoGio = new TextBox();
        private readonly ComboBox cboNV = new ComboBox();
        private readonly TextBox txtSoGioNV = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabPhuTroi()
        {
            dtpNgay.Format = DateTimePickerFormat.Short;
            UiHelper.NapComboCoDinh(cboHinhThuc, "Tăng ca", "Tăng ca", "Ngày lễ", "Ngày lễ", "Ngày nghỉ", "Ngày nghỉ");
            Button btn = UiHelper.TaoNut("Lưu phiếu và chi tiết nhân viên");
            btn.Click += BtnLuu_Click;

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Số phiếu", txtSoPhieu);
            UiHelper.ThemDong(f, "Ngày phụ trội", dtpNgay);
            UiHelper.ThemDong(f, "Hình thức", cboHinhThuc);
            UiHelper.ThemDong(f, "Số giờ phụ trội (phiếu)", txtSoGio);
            UiHelper.ThemDong(f, "Nhân viên tham gia", cboNV);
            UiHelper.ThemDong(f, "Số giờ của nhân viên", txtSoGioNV);
            UiHelper.ThemDong(f, "", btn);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            grid.DataSource = Db.Query(@"SELECT p.SoPhieu AS [Số phiếu], p.NgayPhuTrong AS [Ngày], p.HinhThuc AS [Hình thức],
                    p.HeSo AS [Hệ số], c.MaNV AS [Mã NV], n.HoTen AS [Họ tên], c.SoGioNV AS [Số giờ NV]
                FROM PhieuGhiNhanPhuTrong p
                JOIN ChiTietPhuTrong c ON c.SoPhieu=p.SoPhieu
                JOIN NhanVien n ON n.MaNV=c.MaNV
                ORDER BY p.NgayPhuTrong DESC, p.SoPhieu");
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            string soPhieu = txtSoPhieu.Text.Trim();
            string maNV = UiHelper.GiaTri(cboNV);
            decimal gioPhieu, gioNV;
            if (soPhieu == "") { MessageBox.Show("Vui lòng nhập số phiếu."); return; }
            if (maNV == null) { MessageBox.Show("Vui lòng chọn nhân viên."); return; }
            if (!decimal.TryParse(txtSoGio.Text.Trim(), out gioPhieu) || gioPhieu <= 0) { MessageBox.Show("Số giờ phiếu không hợp lệ."); return; }
            if (!decimal.TryParse(txtSoGioNV.Text.Trim(), out gioNV) || gioNV <= 0) { MessageBox.Show("Số giờ nhân viên không hợp lệ."); return; }
            try
            {
                Db.Execute(@"IF NOT EXISTS(SELECT 1 FROM PhieuGhiNhanPhuTrong WHERE SoPhieu=@P)
                             INSERT INTO PhieuGhiNhanPhuTrong(SoPhieu,NgayPhuTrong,SoGio,HinhThuc,HeSo)
                             VALUES(@P,@Ngay,@Gio,@HT,1.5)",
                    new SqlParameter("@P", soPhieu), new SqlParameter("@Ngay", dtpNgay.Value.Date),
                    new SqlParameter("@Gio", gioPhieu), new SqlParameter("@HT", UiHelper.GiaTri(cboHinhThuc)));
                Db.Execute(@"IF EXISTS(SELECT 1 FROM ChiTietPhuTrong WHERE SoPhieu=@P AND MaNV=@NV)
                                 UPDATE ChiTietPhuTrong SET SoGioNV=@G WHERE SoPhieu=@P AND MaNV=@NV
                             ELSE INSERT INTO ChiTietPhuTrong(SoPhieu,MaNV,SoGioNV) VALUES(@P,@NV,@G)",
                    new SqlParameter("@P", soPhieu), new SqlParameter("@NV", maNV), new SqlParameter("@G", gioNV));
                txtSoGioNV.Clear();
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
