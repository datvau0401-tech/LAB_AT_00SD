using System;
using System.Windows.Forms;
using QuanLyNhanSu.UI;

namespace QuanLyNhanSu
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Kiểm tra kết nối CSDL trước khi mở form chính
            try
            {
                Data.Db.OpenConnection().Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không kết nối được CSDL QuanLyNhanSuDB.\n\n" +
                    "Hãy chạy file Database\\QuanLyNhanSuDB.sql trước và kiểm tra chuỗi kết nối trong App.config.\n\n" +
                    "Chi tiết: " + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Data.Schema.DamBao();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tạo được bảng bổ sung (Lịch phân công, Đánh giá): " + ex.Message,
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Application.Run(new FrmMenu());
        }
    }
}
