using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Các module (tab) có thể nạp lại dữ liệu khi được chọn.</summary>
public interface ILamMoi
{
    void LamMoi();
}

public record LuaChon(string Text, string Value);

/// <summary>Hàm tiện ích dựng giao diện bằng code (không cần Designer) và bắt lỗi nghiệp vụ.</summary>
internal static class Ui
{
    private static readonly Dictionary<string, string> TieuDe = new()
    {
        ["MaHoSo"] = "Mã hồ sơ", ["HoTen"] = "Họ tên", ["NgaySinh"] = "Ngày sinh", ["DienThoai"] = "Điện thoại",
        ["DiaChi"] = "Địa chỉ", ["NgayCap"] = "Ngày cấp", ["ConHan"] = "Còn hạn năm nay",
        ["NgayGioKham"] = "Ngày giờ khám", ["LoaiKham"] = "Loại khám", ["BacSi"] = "Bác sĩ", ["Khoa"] = "Khoa",
        ["TrieuChung"] = "Triệu chứng", ["NhietDo"] = "Nhiệt độ", ["HuyetAp"] = "Huyết áp", ["ChanDoan"] = "Chẩn đoán",
        ["TrangThai"] = "Trạng thái", ["TongTien"] = "Tổng tiền", ["TenXN"] = "Xét nghiệm", ["DonGia"] = "Đơn giá",
        ["NgayLam"] = "Ngày làm", ["KetQua"] = "Kết quả", ["TenThuoc"] = "Thuốc", ["DonVi"] = "ĐVT",
        ["SoLuong"] = "Số lượng", ["ThanhTien"] = "Thành tiền", ["LieuDung"] = "Liều dùng", ["NgayLap"] = "Ngày lập",
        ["Nam"] = "Năm", ["Quy"] = "Quý", ["NgayHen"] = "Ngày hẹn", ["SoTien"] = "Số tiền", ["NgayDong"] = "Ngày đóng",
        ["MaPK"] = "Mã phiếu", ["BenhNhan"] = "Bệnh nhân", ["TienKham"] = "Tiền khám",
        ["TienXetNghiem"] = "Tiền xét nghiệm", ["TienThuoc"] = "Tiền thuốc"
    };

    private static readonly HashSet<string> CotAn = new() { "MaHS", "MaToa", "MaXN", "MaHen", "MaThuoc", "MaBN" };
    private static readonly HashSet<string> CotTien = new() { "DonGia", "ThanhTien", "TongTien", "SoTien", "TienKham", "TienXetNghiem", "TienThuoc" };

    public static void Safe(Control owner, Action action)
    {
        try
        {
            action();
        }
        catch (NghiepVuException ex)
        {
            MessageBox.Show(owner.FindForm(), ex.Message, "Không thực hiện được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner.FindForm(),
                "Lỗi hệ thống (kiểm tra SQL Server và chuỗi kết nối trong appsettings.json):\n" + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static Label Lbl(string text) =>
        new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 6, 3) };

    public static Button Btn(string text, EventHandler onClick)
    {
        var b = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 2, 8, 2), Margin = new Padding(3) };
        b.Click += onClick;
        return b;
    }

    public static NumericUpDown Money() => new()
    {
        Minimum = 0, Maximum = 1_000_000_000, Increment = 10_000, DecimalPlaces = 0, ThousandsSeparator = true,
        Width = 140
    };

    public static TextBox Txt(int width = 200) => new() { Width = width, Anchor = AnchorStyles.Left | AnchorStyles.Right };

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
            AutoGenerateColumns = true,
            RowHeadersVisible = false,
            BackgroundColor = SystemColors.Window
        };
        g.DataBindingComplete += (_, _) => DinhDangCot(g);
        return g;
    }

    private static void DinhDangCot(DataGridView g)
    {
        foreach (DataGridViewColumn c in g.Columns)
        {
            if (TieuDe.TryGetValue(c.DataPropertyName, out var t)) c.HeaderText = t;
            if (CotAn.Contains(c.DataPropertyName)) c.Visible = false;
            if (CotTien.Contains(c.DataPropertyName))
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else if (c.ValueType == typeof(DateTime))
            {
                c.DefaultCellStyle.Format = c.DataPropertyName is "NgaySinh" or "NgayCap" ? "dd/MM/yyyy" : "dd/MM/yyyy HH:mm";
            }
        }
    }

    public static GroupBox Nhom(string tieuDe, Control noiDung)
    {
        var gb = new GroupBox { Text = tieuDe, Dock = DockStyle.Fill, Padding = new Padding(8) };
        noiDung.Dock = DockStyle.Fill;
        gb.Controls.Add(noiDung);
        return gb;
    }

    public static void ThemDong(TableLayoutPanel t, string nhan, Control c)
    {
        int r = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(Lbl(nhan), 0, r);
        c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        t.Controls.Add(c, 1, r);
    }

    public static TableLayoutPanel Luoi2Cot()
    {
        var t = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Dock = DockStyle.Top };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return t;
    }

    public static void GanCombo(ComboBox cbo, DataTable dt, string hienThi, string giaTri)
    {
        cbo.DropDownStyle = ComboBoxStyle.DropDownList;
        cbo.DataSource = dt;
        cbo.DisplayMember = hienThi;
        cbo.ValueMember = giaTri;
    }

    public static int? IdDong(DataGridView g, string cot)
    {
        if (g.CurrentRow?.DataBoundItem is DataRowView v && v.Row.Table.Columns.Contains(cot) && v[cot] is not DBNull)
            return Convert.ToInt32(v[cot]);
        return null;
    }

    public static int? ChonInt(ComboBox cbo)
    {
        var v = cbo.SelectedValue;
        if (v is null || v is DBNull) return null;
        return Convert.ToInt32(v);
    }

    public static string Tien(object? v)
    {
        if (v is null || v is DBNull) return "0 đ";
        return string.Format("{0:N0} đ", Convert.ToDecimal(v));
    }
}
