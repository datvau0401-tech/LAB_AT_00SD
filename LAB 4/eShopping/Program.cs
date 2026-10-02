using System;
using System.Windows.Forms;
using eShopping.Data;
using eShopping.Forms;

namespace eShopping
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (var cn = Db.OpenConnection()) { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không kết nối được CSDL eShoppingDB.\n" +
                    "Hãy chạy Database/eShopping.sql và kiểm tra connectionString trong App.config.\n\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmMain());
        }
    }
}
