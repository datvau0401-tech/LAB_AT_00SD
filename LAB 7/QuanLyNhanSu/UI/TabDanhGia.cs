using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Đánh giá nhân sự theo tháng (điểm 1-10, tự xếp loại)
    public class TabDanhGia : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly NumericUpDown numThang = new NumericUpDown();
        private readonly NumericUpDown numNam = new NumericUpDown();
        private readonly NumericUpDown numDiem = new NumericUpDown();
        private readonly TextBox txtNhanXet = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabDanhGia()
        {
            numThang.Minimum = 1; numThang.Maximum = 12; numThang.Value = DateTime.Today.Month;
            numNam.Minimum = 2000; numNam.Maximum = 2100; numNam.Value = DateTime.Today.Year;
            numDiem.Minimum = 1; numDiem.Maximum = 10; numDiem.Value = 7;
            Button btn = UiHelper.TaoNut("Lưu đánh giá");
            btn.Click += BtnLuu_Click;

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "Tháng", numThang);
            UiHelper.ThemDong(f, "Năm", numNam);
            UiHelper.ThemDong(f, "Điểm (1-10)", numDiem);
            UiHelper.ThemDong(f, "Nhận xét", txtNhanXet);
            UiHelper.ThemDong(f, "", btn);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            grid.DataSource = Db.Query(@"SELECT d.MaNV AS [Mã NV], n.HoTen AS [Họ tên], d.Thang AS [Tháng], d.Nam AS [Năm],
                    d.Diem AS [Điểm], d.XepLoai AS [Xếp loại], d.NhanXet AS [Nhận xét]
                FROM DanhGiaNhanSu d JOIN NhanVien n ON n.MaNV=d.MaNV
                ORDER BY d.Nam DESC, d.Thang DESC, d.MaNV");
        }

        private static string XepLoai(int diem)
        {
            if (diem >= 9) return "Xuất sắc";
            if (diem >= 7) return "Tốt";
            if (diem >= 5) return "Đạt";
            return "Chưa đạt";
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            string maNV = UiHelper.GiaTri(cboNV);
            if (maNV == null) { MessageBox.Show("Vui lòng chọn nhân viên."); return; }
            int diem = (int)numDiem.Value;
            try
            {
                Db.Execute("DELETE FROM DanhGiaNhanSu WHERE MaNV=@NV AND Thang=@T AND Nam=@N",
                    new SqlParameter("@NV", maNV), new SqlParameter("@T", (int)numThang.Value), new SqlParameter("@N", (int)numNam.Value));
                Db.Execute(@"INSERT INTO DanhGiaNhanSu(MaNV,Thang,Nam,Diem,XepLoai,NhanXet)
                             VALUES(@NV,@T,@N,@D,@XL,@NX)",
                    new SqlParameter("@NV", maNV), new SqlParameter("@T", (int)numThang.Value),
                    new SqlParameter("@N", (int)numNam.Value), new SqlParameter("@D", diem),
                    new SqlParameter("@XL", XepLoai(diem)), new SqlParameter("@NX", txtNhanXet.Text.Trim()));
                txtNhanXet.Clear();
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
