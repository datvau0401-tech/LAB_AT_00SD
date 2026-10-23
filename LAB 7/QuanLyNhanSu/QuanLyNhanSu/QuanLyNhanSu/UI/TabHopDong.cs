using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using QuanLyNhanSu.Data;
using QuanLyNhanSu.Services;

namespace QuanLyNhanSu.UI
{
    // UC03/UC04 - Ký hợp đồng chính thức sau thử việc (kèm quyết định lương, hồ sơ BHYT/BHXH nếu dài hạn)
    public class TabHopDong : TabBase
    {
        private readonly ComboBox cboNV = new ComboBox();
        private readonly ComboBox cboQD = new ComboBox();
        private readonly ComboBox cboLoai = new ComboBox();
        private readonly DateTimePicker dtpBatDau = new DateTimePicker();
        private readonly CheckBox chkKetThuc = new CheckBox();
        private readonly DateTimePicker dtpKetThuc = new DateTimePicker();
        private readonly TextBox txtLuongCB = new TextBox();
        private readonly TextBox txtPhuCap = new TextBox();
        private readonly TextBox txtMucBH = new TextBox();
        private readonly DataGridView grid = UiHelper.TaoLuoi();
        private readonly NhanSuService svc = new NhanSuService();
        private bool dangNap;

        public TabHopDong()
        {
            UiHelper.NapComboCoDinh(cboLoai, "DaiHan", "Dài hạn (có BHYT, BHXH)", "NganHan", "Ngắn hạn");
            dtpBatDau.Format = DateTimePickerFormat.Short;
            dtpKetThuc.Format = DateTimePickerFormat.Short;
            dtpKetThuc.Value = DateTime.Today.AddYears(1);
            dtpKetThuc.Enabled = false;
            chkKetThuc.Text = "Có ngày kết thúc";
            txtPhuCap.Text = "0";
            txtMucBH.Text = "0";

            cboNV.SelectedIndexChanged += delegate { if (!dangNap) NapQuyetDinh(); };
            cboLoai.SelectedIndexChanged += delegate { CapNhatTheoLoai(); };
            chkKetThuc.CheckedChanged += delegate { dtpKetThuc.Enabled = chkKetThuc.Checked; };

            Button btn = UiHelper.TaoNut("Ký hợp đồng chính thức");
            btn.Click += BtnKy_Click;

            var f = UiHelper.TaoForm();
            UiHelper.ThemDong(f, "Nhân viên", cboNV);
            UiHelper.ThemDong(f, "QĐ tuyển dụng (đã ký)", cboQD);
            UiHelper.ThemDong(f, "Loại hợp đồng", cboLoai);
            UiHelper.ThemDong(f, "Ngày bắt đầu", dtpBatDau);
            UiHelper.ThemDong(f, "", chkKetThuc);
            UiHelper.ThemDong(f, "Ngày kết thúc", dtpKetThuc);
            UiHelper.ThemDong(f, "Lương cơ bản (đ)", txtLuongCB);
            UiHelper.ThemDong(f, "Phụ cấp độc hại (đ)", txtPhuCap);
            UiHelper.ThemDong(f, "Mức đóng BH (đ) - dài hạn", txtMucBH);
            UiHelper.ThemDong(f, "", btn);

            Controls.Add(grid);
            Controls.Add(f);
        }

        public override void NapLai()
        {
            dangNap = true;
            UiHelper.NapComboNhanVien(cboNV);
            dangNap = false;
            NapQuyetDinh();
            CapNhatTheoLoai();
            grid.DataSource = Db.Query(@"SELECT h.MaHD AS [Mã HĐ], h.MaNV AS [Mã NV], n.HoTen AS [Họ tên],
                    CASE h.LoaiHopDong WHEN 'DaiHan' THEN N'Dài hạn' ELSE N'Ngắn hạn' END AS [Loại],
                    h.NgayKy AS [Ngày ký], h.NgayBatDau AS [Bắt đầu], h.NgayKetThuc AS [Kết thúc],
                    CASE WHEN h.TrangThaiHieuLuc=1 THEN N'Còn hiệu lực' ELSE N'Hết hiệu lực' END AS [Trạng thái],
                    ISNULL(b.MucDongBH,0) AS [Mức đóng BH]
                FROM HopDong h
                JOIN NhanVien n ON n.MaNV=h.MaNV
                LEFT JOIN HoSoBaoHiem b ON b.MaHD=h.MaHD
                ORDER BY h.NgayKy DESC, h.MaHD DESC");
        }

        private void NapQuyetDinh()
        {
            string maNV = UiHelper.GiaTri(cboNV);
            UiHelper.NapCombo(cboQD,
                @"SELECT q.MaQD, q.MaQD + N' - ' + q.SoQD AS Hien
                  FROM QuyetDinh q JOIN QuyetDinhTuyenDung t ON t.MaQD=q.MaQD
                  WHERE q.MaNV=@MaNV AND q.TrangThaiKy=1
                    AND NOT EXISTS (SELECT 1 FROM HopDong h WHERE h.MaQD=q.MaQD)
                  ORDER BY q.NgayQD",
                "MaQD", "Hien", new SqlParameter("@MaNV", (object)maNV ?? DBNull.Value));
        }

        private void CapNhatTheoLoai()
        {
            txtMucBH.Enabled = UiHelper.GiaTri(cboLoai) == "DaiHan";
        }

        private void BtnKy_Click(object sender, EventArgs e)
        {
            string maNV = UiHelper.GiaTri(cboNV);
            string maQD = UiHelper.GiaTri(cboQD);
            if (maNV == null) { MessageBox.Show("Vui lòng chọn nhân viên."); return; }
            if (maQD == null) { MessageBox.Show("Nhân viên này chưa có quyết định tuyển dụng đã ký (hoặc đã dùng hết)."); return; }

            decimal luong, phuCap, mucBH = 0;
            if (!UiHelper.DocSo(txtLuongCB, out luong)) { MessageBox.Show("Lương cơ bản không hợp lệ."); return; }
            if (!UiHelper.DocSo(txtPhuCap, out phuCap)) { MessageBox.Show("Phụ cấp độc hại không hợp lệ."); return; }
            string loai = UiHelper.GiaTri(cboLoai);
            if (loai == "DaiHan" && (!UiHelper.DocSo(txtMucBH, out mucBH) || mucBH <= 0))
            { MessageBox.Show("Hợp đồng dài hạn cần mức đóng bảo hiểm lớn hơn 0."); return; }

            DateTime? ketThuc = chkKetThuc.Checked ? (DateTime?)dtpKetThuc.Value : null;
            if (ketThuc.HasValue && ketThuc.Value.Date < dtpBatDau.Value.Date)
            { MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu."); return; }

            try
            {
                KetQuaXuLy kq = svc.KyHopDongChinhThuc(maNV, maQD, loai, dtpBatDau.Value, ketThuc, luong, phuCap, mucBH);
                UiHelper.ThongBao(kq);
                if (kq.ThanhCong) NapLai();
            }
            catch (Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
