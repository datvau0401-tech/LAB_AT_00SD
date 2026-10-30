using System.Data;

namespace QuanLyVanPhong;

/// <summary>Vài hàm nhỏ giúp tạo control nhanh bằng code (không cần Designer).</summary>
public static class Ui
{
    public static void Setup(Form f, string title, int w, int h)
    {
        f.Text = title;
        f.ClientSize = new Size(w, h);
        f.FormBorderStyle = FormBorderStyle.FixedDialog;
        f.MaximizeBox = false;
        f.MinimizeBox = false;
        f.StartPosition = FormStartPosition.CenterParent;
    }

    public static Label L(string text, int x, int y) => new Label { Text = text, Left = x, Top = y, AutoSize = true };

    public static TextBox T(int x, int y, int w) => new TextBox { Left = x, Top = y, Width = w };

    public static ComboBox Combo(int x, int y, int w) =>
        new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };

    public static Button B(string text, int x, int y, int w, EventHandler click)
    {
        var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 28 };
        b.Click += click;
        return b;
    }

    public static DataGridView Grid(int x, int y, int w, int h) => new DataGridView
    {
        Left = x, Top = y, Width = w, Height = h,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        MultiSelect = false,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };

    public static void Bind(ComboBox cb, DataTable dt, string valueMember, string displayMember)
    {
        cb.DisplayMember = displayMember;
        cb.ValueMember = valueMember;
        cb.DataSource = dt;
    }

    /// <summary>Nạp combobox hợp đồng (bỏ qua dữ liệu kiểm thử).</summary>
    public static void NapHopDong(ComboBox cb) => Bind(cb, Db.Table(
        "SELECT hd.MaHD, N'HĐ #' + CAST(hd.MaHD AS NVARCHAR(10)) + N' - ' + kh.TenKH AS Hien " +
        "FROM HopDong hd JOIN KhachHang kh ON kh.MaKH = hd.MaKH " +
        "WHERE kh.DienThoai NOT LIKE 'TEST%' ORDER BY hd.MaHD DESC"), "MaHD", "Hien");

    public static int Sel(ComboBox cb) => cb.SelectedValue is int i ? i : -1;

    public static int SelId(DataGridView g, string col) =>
        g.CurrentRow != null && g.CurrentRow.Cells[col].Value is int i ? i : -1;

    public static void Info(string msg) =>
        MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Err(Exception ex)
    {
        bool nghiepVu = ex is BusinessException;
        MessageBox.Show(nghiepVu ? ex.Message : "Lỗi hệ thống: " + ex.Message,
            nghiepVu ? "Không hợp lệ" : "Lỗi",
            MessageBoxButtons.OK, nghiepVu ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
    }
}
