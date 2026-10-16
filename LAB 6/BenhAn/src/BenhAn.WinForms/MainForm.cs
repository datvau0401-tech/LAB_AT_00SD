using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Cửa sổ chính: mỗi module là một tab.</summary>
public sealed class MainForm : Form
{
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel lblTrangThai = new() { Text = "Đang kiểm tra kết nối..." };

    public MainForm()
    {
        Text = "Quản lý hồ sơ bệnh án - Bệnh viện đa khoa quốc tế";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 800);
        MinimumSize = new Size(1000, 650);

        ThemTab("1. Đăng ký & lệ phí", new DangKyControl());
        ThemTab("2. Tra cứu hồ sơ", new TraCuuControl());
        ThemTab("3. Lịch khám định kỳ", new LichDinhKyControl());
        ThemTab("4. Khám bệnh", new KhamBenhControl());
        ThemTab("5. Thanh toán", new ThanhToanControl());

        var status = new StatusStrip();
        status.Items.Add(lblTrangThai);
        Controls.Add(tabs);
        Controls.Add(status);

        tabs.SelectedIndexChanged += (_, _) =>
        {
            if (tabs.SelectedTab?.Controls.Count > 0 && tabs.SelectedTab.Controls[0] is ILamMoi m) m.LamMoi();
        };
        Shown += (_, _) => KiemTraKetNoi();
    }

    private void ThemTab(string tieuDe, Control noiDung)
    {
        var page = new TabPage(tieuDe) { Padding = new Padding(0) };
        page.Controls.Add(noiDung);
        tabs.TabPages.Add(page);
    }

    private void KiemTraKetNoi()
    {
        try
        {
            using var cn = Db.Open();
            lblTrangThai.Text = $"Đã kết nối: {cn.DataSource} / {cn.Database}";
        }
        catch (Exception ex)
        {
            lblTrangThai.Text = "Chưa kết nối được SQL Server";
            MessageBox.Show(this,
                "Không kết nối được CSDL QuanLyBenhAn.\n\n" +
                "1) Chạy lần lượt sql/01_schema.sql, 02_procs.sql, 03_seed.sql.\n" +
                "2) Sửa chuỗi kết nối trong appsettings.json (cạnh file .exe) cho đúng máy chủ.\n\n" + ex.Message,
                "Kết nối CSDL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
