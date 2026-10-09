using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyCongTyDuLich.Services;

namespace QuanLyCongTyDuLich.Forms
{
    /// <summary>Tiện ích dùng chung cho các Form (nạp dữ liệu, thông báo, dựng control bằng code).</summary>
    internal static class FormHelper
    {
        public static void Nap(ComboBox cbo, DataTable dt, string display, string value)
        {
            cbo.DisplayMember = display;
            cbo.ValueMember = value;
            cbo.DataSource = dt;
        }

        public static string Gia(ComboBox cbo)
        {
            return cbo.SelectedValue == null ? "" : cbo.SelectedValue.ToString();
        }

        public static string O(DataGridView dgv, string cot)
        {
            return dgv.CurrentRow == null ? "" : System.Convert.ToString(dgv.CurrentRow.Cells[cot].Value);
        }

        /// <summary>Hiển thị kết quả; trả về true nếu thành công.</summary>
        public static bool Bao(KetQuaXuLy k)
        {
            MessageBox.Show(k.ThongBao, k.ThanhCong ? "Thông báo" : "Không thực hiện được",
                MessageBoxButtons.OK, k.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            return k.ThanhCong;
        }

        // ---------- Dựng giao diện bằng code (dùng trong *.Designer.cs) ----------
        public static void Cau(Form f, string title, int w, int h)
        {
            f.Text = title;
            f.ClientSize = new Size(w, h);
            f.StartPosition = FormStartPosition.CenterParent;
            f.Font = new Font("Segoe UI", 9F);
        }

        public static Label Lb(Control p, string text, int x, int y)
        {
            var c = new Label { Text = text, Left = x, Top = y + 4, AutoSize = true };
            p.Controls.Add(c); return c;
        }

        public static TextBox Tx(Control p, int x, int y, int w)
        {
            var c = new TextBox { Left = x, Top = y, Width = w };
            p.Controls.Add(c); return c;
        }

        public static NumericUpDown Nu(Control p, int x, int y, int w, decimal min, decimal max, decimal val)
        {
            var c = new NumericUpDown { Left = x, Top = y, Width = w, Minimum = min, Maximum = max, ThousandsSeparator = true };
            c.Value = val;
            p.Controls.Add(c); return c;
        }

        public static ComboBox Cb(Control p, int x, int y, int w)
        {
            var c = new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            p.Controls.Add(c); return c;
        }

        public static DateTimePicker Dt(Control p, int x, int y, int w)
        {
            var c = new DateTimePicker { Left = x, Top = y, Width = w, Format = DateTimePickerFormat.Short };
            p.Controls.Add(c); return c;
        }

        public static CheckBox Ck(Control p, string text, int x, int y)
        {
            var c = new CheckBox { Text = text, Left = x, Top = y, AutoSize = true };
            p.Controls.Add(c); return c;
        }

        public static Button Bt(Control p, string text, int x, int y, int w)
        {
            var c = new Button { Text = text, Left = x, Top = y, Width = w, Height = 28 };
            p.Controls.Add(c); return c;
        }

        public static DataGridView Dg(Control p, int x, int y, int w, int h, bool readOnly = true)
        {
            var c = new DataGridView
            {
                Left = x, Top = y, Width = w, Height = h,
                ReadOnly = readOnly,
                AllowUserToAddRows = !readOnly,
                AllowUserToDeleteRows = !readOnly,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                RowHeadersVisible = true
            };
            p.Controls.Add(c); return c;
        }

        public static TabPage Tp(TabControl t, string title)
        {
            var p = new TabPage(title) { UseVisualStyleBackColor = true };
            t.TabPages.Add(p); return p;
        }
    }
}
