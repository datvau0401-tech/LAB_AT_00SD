using System.Globalization;
using System.Windows.Forms;

namespace eShopping.Forms
{
    // Hàm dùng chung để dựng control bằng code (không cần Form Designer).
    internal static class Ui
    {
        private static readonly CultureInfo VN = new CultureInfo("vi-VN");

        public static string Tien(decimal d) { return d.ToString("N0", VN) + " đ"; }

        public static Label Nhan(Control cha, string text, int x, int y, int w)
        {
            Label l = new Label { Text = text, Left = x, Top = y + 4, Width = w, AutoSize = false };
            cha.Controls.Add(l);
            return l;
        }

        public static TextBox O(Control cha, string name, int x, int y, int w)
        {
            TextBox t = new TextBox { Name = name, Left = x, Top = y, Width = w };
            cha.Controls.Add(t);
            return t;
        }

        public static Button Nut(Control cha, string name, string text, int x, int y, int w)
        {
            Button b = new Button { Name = name, Text = text, Left = x, Top = y, Width = w, Height = 32 };
            cha.Controls.Add(b);
            return b;
        }

        public static ComboBox Combo(Control cha, string name, int x, int y, int w)
        {
            ComboBox c = new ComboBox { Name = name, Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            cha.Controls.Add(c);
            return c;
        }

        public static DataGridView Luoi(Control cha, string name, int x, int y, int w, int h)
        {
            DataGridView g = new DataGridView
            {
                Name = name, Left = x, Top = y, Width = w, Height = h,
                ReadOnly = true, MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            cha.Controls.Add(g);
            return g;
        }

        public static void HienKetQua(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thông báo" : "Lỗi", MessageBoxButtons.OK,
                kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
    }
}
