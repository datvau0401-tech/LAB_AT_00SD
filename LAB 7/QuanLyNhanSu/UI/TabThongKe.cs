using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Thống kê lương theo tháng
    public class TabThongKe : TabBase
    {
        private readonly DataGridView grid = UiHelper.TaoLuoi();

        public TabThongKe()
        {
            Controls.Add(grid);
        }

        public override void NapLai()
        {
            grid.DataSource = Db.Query(@"SELECT Nam AS [Năm], Thang AS [Tháng], COUNT(*) AS [Số NV],
                    SUM(LuongChinh) AS [Tổng lương chính], SUM(TongPhuCap) AS [Tổng phụ cấp],
                    SUM(TienPhuTrong) AS [Tổng phụ trội], SUM(TongKhauTru) AS [Tổng khấu trừ BH],
                    SUM(TienPhatTruLuong) AS [Tổng phạt], SUM(LuongThucNhan) AS [Tổng thực nhận]
                FROM BangLuong
                GROUP BY Nam, Thang
                ORDER BY Nam DESC, Thang DESC");
        }
    }
}
