using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Ban chấm công: ghi nhận ngày công (mỗi dòng = 1 ngày công của 1 nhân viên)
    public class TabChamCong : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly DateTimePicker dtpNgay = new DateTimePicker();
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabChamCong()
        {
            dtpNgay.Format = DateTimePickerFormat.Short;
            Button ngay = UiHelper.TaoNut("Chấm công ngày đã chọn");
            Button thang = UiHelper.TaoNut("Chấm công cả tháng (trừ Chủ nhật)");
            ngay.Click += delegate { ChamCong(false); };
            thang.Click += delegate { ChamCong(true); };
            FlowLayoutPanel nut = new FlowLayoutPanel();
            nut.AutoSize = true;
            nut.Controls.Add(ngay);
            nut.Controls.Add(thang);

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "Ngày", dtpNgay);
            UiHelper.ThemDong(f, "", nut);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            grid.DataSource = Db.Query(@"SELECT c.MaNV AS [Mã NV], n.HoTen AS [Họ tên], b.Thang AS [Tháng], b.Nam AS [Năm],
                    COUNT(*) AS [Số ngày công]
                FROM ChiTietChamCong c
                JOIN BangChamCong b ON b.MaBCC=c.MaBCC
                JOIN NhanVien n ON n.MaNV=c.MaNV
                GROUP BY c.MaNV, n.HoTen, b.Thang, b.Nam
                ORDER BY b.Nam DESC, b.Thang DESC, c.MaNV");
        }

        private void ChamCong(bool caThang)
        {
            string maNV = UiHelper.GiaTri(cboNV);
            if (maNV == null) { MessageBox.Show("Vui lòng chọn nhân viên."); return; }
            DateTime d = dtpNgay.Value.Date;
            string maBCC = "BCC" + d.ToString("yyyyMM");
            try
            {
                Db.Execute(@"IF NOT EXISTS(SELECT 1 FROM BangChamCong WHERE MaBCC=@B)
                             INSERT INTO BangChamCong(MaBCC,Thang,Nam) VALUES(@B,@T,@N)",
                    new SqlParameter("@B", maBCC), new SqlParameter("@T", d.Month), new SqlParameter("@N", d.Year));

                int soNgay = 0;
                DateTime dau = caThang ? new DateTime(d.Year, d.Month, 1) : d;
                DateTime cuoi = caThang ? dau.AddMonths(1).AddDays(-1) : d;
                for (DateTime x = dau; x <= cuoi; x = x.AddDays(1))
                {
                    if (caThang && x.DayOfWeek == DayOfWeek.Sunday) continue;
                    soNgay += Db.Execute(@"IF NOT EXISTS(SELECT 1 FROM ChiTietChamCong WHERE MaBCC=@B AND MaNV=@NV AND NgayCong=@D)
                                 INSERT INTO ChiTietChamCong(MaBCC,MaNV,NgayCong) VALUES(@B,@NV,@D)",
                        new SqlParameter("@B", maBCC), new SqlParameter("@NV", maNV), new SqlParameter("@D", x));
                }
                MessageBox.Show("Đã ghi nhận " + soNgay + " ngày công.");
                NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
