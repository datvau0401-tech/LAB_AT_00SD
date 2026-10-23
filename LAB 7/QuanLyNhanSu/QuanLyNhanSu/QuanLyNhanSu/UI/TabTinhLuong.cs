using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.UI
{
    // UC10 - Tính lương tháng
    public class TabTinhLuong : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly NumericUpDown numThang = new NumericUpDown();
        private readonly NumericUpDown numNam = new NumericUpDown();
        private readonly DataGridView grid = UiHelper.TaoLuoi();
        private readonly LuongService svc = new LuongService();

        public TabTinhLuong()
        {
            numThang.Minimum = 1; numThang.Maximum = 12; numThang.Value = DateTime.Today.Month;
            numNam.Minimum = 2000; numNam.Maximum = 2100; numNam.Value = DateTime.Today.Year;

            Button btn = UiHelper.TaoNut("Tính lương nhân viên đang chọn");
            btn.Click += BtnTinh_Click;
            Button btnAll = UiHelper.TaoNut("Tính lương tất cả nhân viên");
            btnAll.Click += BtnTinhTatCa_Click;
            FlowLayoutPanel nut = new FlowLayoutPanel();
            nut.AutoSize = true;
            nut.Controls.Add(btn);
            nut.Controls.Add(btnAll);

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "Tháng", numThang);
            UiHelper.ThemDong(f, "Năm", numNam);
            UiHelper.ThemDong(f, "", nut);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            UiHelper.NapComboNhanVien(cboNV);
            NapBangLuong();
        }

        private void NapBangLuong()
        {
            grid.DataSource = Db.Query(@"SELECT b.MaBL AS [Mã bảng lương], b.MaNV AS [Mã NV], n.HoTen AS [Họ tên],
                    b.Thang AS [Tháng], b.Nam AS [Năm], b.LuongChinh AS [Lương chính],
                    b.TongPhuCap AS [Phụ cấp], b.TienPhuTrong AS [Phụ trội],
                    b.TongKhauTru AS [Khấu trừ BH], b.TienPhatTruLuong AS [Phạt trừ lương],
                    b.LuongThucNhan AS [Thực nhận]
                FROM BangLuong b JOIN NhanVien n ON n.MaNV=b.MaNV
                ORDER BY b.Nam DESC, b.Thang DESC, b.MaNV");
        }

        private void BtnTinh_Click(object sender, EventArgs e)
        {
            try
            {
                KetQuaXuLy kq = svc.TinhLuongThang(UiHelper.GiaTri(cboNV), (int)numThang.Value, (int)numNam.Value);
                UiHelper.ThongBao(kq);
                if (kq.ThanhCong) NapBangLuong();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }

        private void BtnTinhTatCa_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (DataRow r in Db.Query("SELECT MaNV FROM NhanVien ORDER BY MaNV").Rows)
                {
                    string ma = Convert.ToString(r["MaNV"]);
                    KetQuaXuLy kq = svc.TinhLuongThang(ma, (int)numThang.Value, (int)numNam.Value);
                    sb.AppendLine(ma + ": " + (kq.ThanhCong ? "OK - " : "Bỏ qua - ") + kq.ThongBao);
                }
                NapBangLuong();
                MessageBox.Show(sb.ToString(), "Kết quả tính lương tháng " + numThang.Value + "/" + numNam.Value);
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
