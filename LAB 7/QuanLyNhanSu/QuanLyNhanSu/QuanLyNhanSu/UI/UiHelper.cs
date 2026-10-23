using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using QuanLyNhanSu.Data;

namespace QuanLyNhanSu.UI
{
    // Hàm dùng chung cho các tab (dựng giao diện bằng code, không cần Designer)
    public static class UiHelper
    {
        public static void NapCombo(ComboBox cbo, string sql, string valueMember, string displayMember, params SqlParameter[] ps)
        {
            DataTable dt = Db.Query(sql, ps);
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.DataSource = null;
            cbo.DisplayMember = displayMember;
            cbo.ValueMember = valueMember;
            cbo.DataSource = dt;
            if (dt.Rows.Count == 0) cbo.SelectedIndex = -1;
        }

        public static void NapComboNhanVien(ComboBox cbo)
        {
            NapCombo(cbo, "SELECT MaNV, MaNV + N' - ' + HoTen AS Hien FROM NhanVien ORDER BY MaNV", "MaNV", "Hien");
        }

        public static void NapComboPhongBan(ComboBox cbo)
        {
            NapCombo(cbo, "SELECT MaPB, MaPB + N' - ' + TenPB AS Hien FROM PhongBan ORDER BY MaPB", "MaPB", "Hien");
        }

        public static string GiaTri(ComboBox cbo)
        {
            return cbo.SelectedValue == null ? null : Convert.ToString(cbo.SelectedValue);
        }

        // Đọc số tiền; trả false nếu không hợp lệ
        public static bool DocSo(TextBox txt, out decimal so)
        {
            string s = (txt.Text ?? "").Trim().Replace(",", "").Replace(".", "");
            return decimal.TryParse(s, out so);
        }

        public static DataGridView TaoLuoi()
        {
            DataGridView g = new DataGridView();
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            g.BackgroundColor = SystemColors.Window;
            g.RowHeadersVisible = false;
            return g;
        }

        // Bảng nhập liệu 2 cột (nhãn - ô nhập)
        public static TableLayoutPanel TaoForm()
        {
            TableLayoutPanel t = new TableLayoutPanel();
            t.Dock = DockStyle.Top;
            t.AutoSize = true;
            t.ColumnCount = 2;
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.Padding = new Padding(10);
            return t;
        }

        public static void ThemDong(TableLayoutPanel t, string nhan, Control c)
        {
            Label lb = new Label();
            lb.Text = nhan;
            lb.AutoSize = true;
            lb.Anchor = AnchorStyles.Left;
            lb.Margin = new Padding(3, 8, 3, 8);
            if (c is Button) c.Anchor = AnchorStyles.Left;
            else
            {
                c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                if (c.Width < 260) c.Width = 260;
            }
            int row = t.RowCount;
            t.RowCount = row + 1;
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.Controls.Add(lb, 0, row);
            t.Controls.Add(c, 1, row);
        }

        // Combo cố định từ danh sách (giá trị, hiển thị)
        public static void NapComboCoDinh(ComboBox cbo, params string[] capGiaTriHienThi)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ma", typeof(string));
            dt.Columns.Add("Hien", typeof(string));
            for (int i = 0; i + 1 < capGiaTriHienThi.Length; i += 2)
                dt.Rows.Add(capGiaTriHienThi[i], capGiaTriHienThi[i + 1]);
            cbo.DropDownStyle = ComboBoxStyle.DropDownList;
            cbo.DisplayMember = "Hien";
            cbo.ValueMember = "Ma";
            cbo.DataSource = dt;
        }

        public static Button TaoNut(string text)
        {
            Button b = new Button();
            b.Text = text;
            b.AutoSize = true;
            b.Padding = new Padding(10, 3, 10, 3);
            return b;
        }

        public static void ThongBao(KetQuaXuLy kq)
        {
            MessageBox.Show(kq.ThongBao, kq.ThanhCong ? "Thành công" : "Không thực hiện được",
                MessageBoxButtons.OK, kq.ThanhCong ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        public static void Loi(Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
