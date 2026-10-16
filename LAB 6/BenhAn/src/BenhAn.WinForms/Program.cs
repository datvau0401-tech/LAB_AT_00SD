using System.Text.Json;
using BenhAn.Data;

namespace BenhAn.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DocCauHinh();
        Application.Run(new MainForm());
    }

    /// <summary>Đọc chuỗi kết nối từ appsettings.json (cạnh file .exe). Không có thì dùng mặc định (localhost, Windows Auth).</summary>
    private static void DocCauHinh()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path)) return;
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                cs.TryGetProperty("QuanLyBenhAn", out var v) &&
                !string.IsNullOrWhiteSpace(v.GetString()))
            {
                Db.ConnectionString = v.GetString()!;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không đọc được appsettings.json: " + ex.Message, "Cấu hình",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
