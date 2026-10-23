using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.UI
{
    // UC11 - Lập quyết định khen thưởng cho cá nhân hoặc tập thể phòng ban
    public class TabKhenThuong : TabBase
    {
        private readonly RadioButton rdoCaNhan = new RadioButton();
        private readonly RadioButton rdoTapThe = new RadioButton();
        private readonly ComboBox cboDoiTuong = new ComboBox();
        private readonly TextBox txtHinhThuc = new TextBox();
        private readonly TextBox txtLyDo = new TextBox();
        private readonly TextBox txtSoTien = new TextBox();
        private readonly TextBox txtSoQD = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();
        private readonly KhenThuongKyLuatService svc = new KhenThuongKyLuatService();

        public TabKhenThuong()
        {
            rdoCaNhan.Text = "Cá nhân"; rdoCaNhan.Checked = true; rdoCaNhan.AutoSize = true;
            rdoTapThe.Text = "Tập thể (phòng ban)"; rdoTapThe.AutoSize = true;
            FlowLayoutPanel loai = new FlowLayoutPanel();
            loai.AutoSize = true;
            loai.Controls.Add(rdoCaNhan);
            loai.Controls.Add(rdoTapThe);
            rdoCaNhan.CheckedChanged += delegate { NapDoiTuong(); };
            txtHinhThuc.Text = "Thưởng tiền";

            Button btn = UiHelper.TaoNut("Lập quyết định khen thưởng");
            btn.Click += BtnLap_Click;

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Đối tượng", loai);
            UiHelper.ThemDong(f, "Chọn đối tượng", cboDoiTuong);
            UiHelper.ThemDong(f, "Hình thức", txtHinhThuc);
            UiHelper.ThemDong(f, "Lý do", txtLyDo);
            UiHelper.ThemDong(f, "Số tiền khen thưởng (đ)", txtSoTien);
            UiHelper.ThemDong(f, "Số quyết định", txtSoQD);
            UiHelper.ThemDong(f, "", btn);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            NapDoiTuong();
            grid.DataSource = Db.Query(@"SELECT q.MaQD AS [Mã QĐ], q.SoQD AS [Số QĐ],
                    CASE k.LoaiDoiTuong WHEN 'CaNhan' THEN N'Cá nhân' ELSE N'Tập thể' END AS [Đối tượng],
                    ISNULL(q.MaNV, k.MaPB) AS [Mã đối tượng], k.HinhThuc AS [Hình thức], k.LyDo AS [Lý do],
                    k.SoTienKhenThuong AS [Số tiền],
                    CASE WHEN q.TrangThaiKy=1 THEN N'Đã ký' ELSE N'Chờ ký' END AS [Trạng thái],
                    ISNULL(p.MaPhieuChi, N'') AS [Phiếu chi]
                FROM QuyetDinhKhenThuong k
                JOIN QuyetDinh q ON q.MaQD=k.MaQD
                LEFT JOIN PhieuChi p ON p.MaQD=q.MaQD
                ORDER BY q.NgayQD DESC, q.MaQD DESC");
        }

        private void NapDoiTuong()
        {
            if (rdoCaNhan.Checked) UiHelper.NapComboNhanVien(cboDoiTuong);
            else UiHelper.NapComboPhongBan(cboDoiTuong);
        }

        private void BtnLap_Click(object sender, EventArgs e)
        {
            decimal tien;
            if (!UiHelper.DocSo(txtSoTien, out tien)) { MessageBox.Show("Số tiền không hợp lệ."); return; }
            if (string.IsNullOrWhiteSpace(txtSoQD.Text)) { MessageBox.Show("Vui lòng nhập số quyết định."); return; }
            try
            {
                KetQuaXuLy kq = svc.LapQuyetDinhKhenThuong(rdoCaNhan.Checked ? "CaNhan" : "TapThe",
                    UiHelper.GiaTri(cboDoiTuong), txtHinhThuc.Text.Trim(), txtLyDo.Text.Trim(), tien, txtSoQD.Text.Trim());
                UiHelper.ThongBao(kq);
                if (kq.ThanhCong) { txtLyDo.Clear(); txtSoTien.Clear(); txtSoQD.Clear(); NapLai(); }
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
