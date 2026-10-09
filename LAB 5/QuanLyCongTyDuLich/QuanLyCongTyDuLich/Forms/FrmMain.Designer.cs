using System.Drawing;
using System.Windows.Forms;

namespace QuanLyCongTyDuLich.Forms
{
    partial class FrmMain
    {
        private Button btnDanhMuc, btnTour, btnChuyenLe, btnDangKyLe, btnDangKyDoan, btnPhanCong, btnKetThuc, btnThongKe, btnThoat;

        private void InitializeComponent()
        {
            SuspendLayout();
            FormHelper.Cau(this, "Quản lý công ty du lịch Văn Hóa Việt", 640, 400);
            StartPosition = FormStartPosition.CenterScreen;

            var lbl = new Label
            {
                Text = "CÔNG TY DU LỊCH VĂN HÓA VIỆT", AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
                Left = 0, Top = 20, Width = 640, Height = 40,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 51, 102)
            };
            Controls.Add(lbl);

            btnDanhMuc = FormHelper.Bt(this, "Danh mục", 60, 90, 240);
            btnTour = FormHelper.Bt(this, "Tour - hành trình", 340, 90, 240);
            btnChuyenLe = FormHelper.Bt(this, "Lịch chuyến khách lẻ", 60, 140, 240);
            btnDangKyLe = FormHelper.Bt(this, "Đăng ký khách lẻ", 340, 140, 240);
            btnDangKyDoan = FormHelper.Bt(this, "Đăng ký theo đoàn", 60, 190, 240);
            btnPhanCong = FormHelper.Bt(this, "Phân công hướng dẫn viên", 340, 190, 240);
            btnKetThuc = FormHelper.Bt(this, "Kết thúc tour - khảo sát", 60, 240, 240);
            btnThongKe = FormHelper.Bt(this, "Lương - thống kê", 340, 240, 240);
            btnThoat = FormHelper.Bt(this, "Thoát", 200, 300, 240);
            foreach (var b in new[] { btnDanhMuc, btnTour, btnChuyenLe, btnDangKyLe, btnDangKyDoan, btnPhanCong, btnKetThuc, btnThongKe, btnThoat }) b.Height = 38;

            btnDanhMuc.Click += btnDanhMuc_Click;
            btnTour.Click += btnTour_Click;
            btnChuyenLe.Click += btnChuyenLe_Click;
            btnDangKyLe.Click += btnDangKyLe_Click;
            btnDangKyDoan.Click += btnDangKyDoan_Click;
            btnPhanCong.Click += btnPhanCong_Click;
            btnKetThuc.Click += btnKetThuc_Click;
            btnThongKe.Click += btnThongKe_Click;
            btnThoat.Click += btnThoat_Click;
            ResumeLayout(false);
        }
    }
}
