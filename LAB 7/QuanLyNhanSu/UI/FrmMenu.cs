using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyNhanSu.UI
{
    // Form menu chính (hiện đầu tiên khi chạy chương trình)
    public class FrmMenu : Form
    {
        public FrmMenu()
        {
            Text = "Quản lý nhân sự";
            ClientSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            Label tieuDe = new Label();
            tieuDe.Text = "CÔNG TY SƠN ĐÔNG Á - QUẢN LÝ NHÂN SỰ";
            tieuDe.Dock = DockStyle.Top;
            tieuDe.Height = 90;
            tieuDe.TextAlign = ContentAlignment.MiddleCenter;
            tieuDe.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            tieuDe.ForeColor = Color.FromArgb(18, 69, 107);

            TableLayoutPanel luoi = new TableLayoutPanel();
            luoi.Dock = DockStyle.Fill;
            luoi.Padding = new Padding(20, 0, 20, 20);
            luoi.ColumnCount = 2;
            luoi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            luoi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            luoi.RowCount = 5;
            for (int i = 0; i < 5; i++) luoi.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            luoi.Controls.Add(TaoNut("Danh mục phòng ban", MoPhongBan), 0, 0);
            luoi.Controls.Add(TaoNut("Tuyển dụng - Thử việc", MoTuyenDung), 1, 0);
            luoi.Controls.Add(TaoNut("Khen thưởng - Kỷ luật", MoKhenThuong), 0, 1);
            luoi.Controls.Add(TaoNut("Lịch phân công ca", MoLichPhanCong), 1, 1);
            luoi.Controls.Add(TaoNut("Chấm công & Phụ trội", MoChamCong), 0, 2);
            luoi.Controls.Add(TaoNut("Hợp đồng & Bảo hiểm", MoHopDong), 1, 2);
            luoi.Controls.Add(TaoNut("Lương - Thống kê", MoLuong), 0, 3);
            luoi.Controls.Add(TaoNut("Đánh giá nhân sự", MoDanhGia), 1, 3);

            Button thoat = TaoNut("Thoát", delegate { Close(); });
            thoat.Dock = DockStyle.None;
            thoat.Anchor = AnchorStyles.None;
            thoat.Size = new Size(225, 50);
            luoi.Controls.Add(thoat, 0, 4);
            luoi.SetColumnSpan(thoat, 2);

            Controls.Add(luoi);
            Controls.Add(tieuDe);
        }

        private static Button TaoNut(string text, EventHandler click)
        {
            Button b = new Button();
            b.Text = text;
            b.Dock = DockStyle.Fill;
            b.Margin = new Padding(6);
            b.BackColor = Color.FromArgb(228, 228, 228);
            b.Click += click;
            return b;
        }

        private void Mo(FrmHost f)
        {
            using (f) f.ShowDialog(this);
        }

        private void MoPhongBan(object s, EventArgs e)
        {
            Mo(new FrmHost("Danh mục phòng ban").ThemTab("Phòng ban", new TabPhongBan()));
        }

        private void MoTuyenDung(object s, EventArgs e)
        {
            Mo(new FrmHost("Tuyển dụng - Thử việc")
                .ThemTab("Lập quyết định tuyển dụng", new TabTuyenDung())
                .ThemTab("Ký quyết định", new TabKyQuyetDinh()));
        }

        private void MoKhenThuong(object s, EventArgs e)
        {
            Mo(new FrmHost("Khen thưởng - Kỷ luật")
                .ThemTab("Lập quyết định khen thưởng", new TabKhenThuong())
                .ThemTab("Ký quyết định", new TabKyQuyetDinh()));
        }

        private void MoLichPhanCong(object s, EventArgs e)
        {
            Mo(new FrmHost("Lịch phân công ca").ThemTab("Phân công ca", new TabLichPhanCong()));
        }

        private void MoChamCong(object s, EventArgs e)
        {
            Mo(new FrmHost("Chấm công & Phụ trội")
                .ThemTab("Chấm công", new TabChamCong())
                .ThemTab("Phiếu phụ trội", new TabPhuTroi()));
        }

        private void MoHopDong(object s, EventArgs e)
        {
            Mo(new FrmHost("Hợp đồng & Bảo hiểm").ThemTab("Ký hợp đồng chính thức", new TabHopDong()));
        }

        private void MoLuong(object s, EventArgs e)
        {
            Mo(new FrmHost("Lương - Thống kê")
                .ThemTab("Tính lương", new TabTinhLuong())
                .ThemTab("Thống kê theo tháng", new TabThongKe()));
        }

        private void MoDanhGia(object s, EventArgs e)
        {
            Mo(new FrmHost("Đánh giá nhân sự").ThemTab("Đánh giá", new TabDanhGia()));
        }
    }
}
