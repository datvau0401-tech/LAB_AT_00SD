namespace QuanLyVanPhong;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Kiểm tra kết nối CSDL ngay khi mở ứng dụng
        try { Db.Scalar("SELECT 1"); }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Không kết nối được SQL Server.\n\n" +
                "1) Chạy file Database\\Database.sql trong SSMS trước.\n" +
                "2) Sửa chuỗi kết nối Db.ConnStr trong Db.cs cho đúng tên server.\n\n" + ex.Message,
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        Application.Run(new FrmMain());
    }
}
