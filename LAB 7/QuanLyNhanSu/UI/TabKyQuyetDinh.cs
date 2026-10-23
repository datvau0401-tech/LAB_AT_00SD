using System;
using System.Windows.Forms;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.UI
{
    // UC03/UC05/UC06/UC11/UC13 - Giám đốc ký quyết định
    public class TabKyQuyetDinh : TabBase
    {
        private readonly DataGridView grid = UiHelper.TaoLuoi();
        private readonly DateTimePicker dtpHieuLuc = new DateTimePicker();
        private readonly ComboBox cboLoaiChi = new ComboBox();
        private readonly NhanSuService nhanSu = new NhanSuService();
        private readonly KhenThuongKyLuatService khenThuong = new KhenThuongKyLuatService();

        public TabKyQuyetDinh()
        {
            dtpHieuLuc.Format = DateTimePickerFormat.Short;
            UiHelper.NapComboCoDinh(cboLoaiChi,
                "Khen thưởng", "Khen thưởng",
                "Ngày lễ", "Chi ngày lễ",
                "Thưởng cuối năm", "Thưởng cuối năm",
                "Lương tháng 13", "Lương tháng 13");

            Button btn = UiHelper.TaoNut("Giám đốc ký quyết định đang chọn");
            btn.Click += BtnKy_Click;

            Label gc = new Label();
            gc.AutoSize = true;
            gc.Text = "Quyết định tuyển dụng: ký theo ngày hiệu lực. Quyết định khen thưởng: ký và tự lập phiếu chi theo loại chi.";

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Ngày hiệu lực (QĐ tuyển dụng)", dtpHieuLuc);
            UiHelper.ThemDong(f, "Loại chi (QĐ khen thưởng)", cboLoaiChi);
            UiHelper.ThemDong(f, "", btn);
            UiHelper.ThemDong(f, "", gc);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            grid.DataSource = Db.Query(@"SELECT q.MaQD AS [Mã QĐ], q.LoaiQD AS [Loại], q.SoQD AS [Số QĐ],
                    q.NgayQD AS [Ngày lập], q.MaNV AS [Mã NV]
                FROM QuyetDinh q
                WHERE q.TrangThaiKy=0 AND q.LoaiQD IN ('TuyenDung','KhenThuong')
                ORDER BY q.NgayQD, q.MaQD");
        }

        private void BtnKy_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) { MessageBox.Show("Vui lòng chọn một quyết định cần ký."); return; }
            string maQD = Convert.ToString(grid.CurrentRow.Cells["Mã QĐ"].Value);
            string loai = Convert.ToString(grid.CurrentRow.Cells["Loại"].Value);
            try
            {
                KetQuaXuLy kq = loai == "KhenThuong"
                    ? khenThuong.KyVaLapPhieuChi(maQD, UiHelper.GiaTri(cboLoaiChi))
                    : nhanSu.KyQuyetDinh(maQD, dtpHieuLuc.Value);
                UiHelper.ThongBao(kq);
                if (kq.ThanhCong) NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
