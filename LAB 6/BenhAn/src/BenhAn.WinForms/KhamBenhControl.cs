using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Module 4: lập phiếu khám (điều trị / định kỳ), liên kết xét nghiệm và toa thuốc với phiếu khám.</summary>
public sealed class KhamBenhControl : UserControl, ILamMoi
{
    // phiếu khám
    private readonly TextBox txtMaHoSo = Ui.Txt(140);
    private readonly Label lblHoSo = new() { AutoSize = true };
    private readonly ComboBox cboBacSi = new() { Width = 280 };
    private readonly RadioButton rdoDieuTri = new() { Text = "Khám điều trị", Checked = true, AutoSize = true };
    private readonly RadioButton rdoDinhKy = new() { Text = "Khám định kỳ theo quý (miễn phí)", AutoSize = true };
    private readonly ComboBox cboLichHen = new() { Width = 280, Enabled = false };
    private readonly TextBox txtTrieuChung = Ui.Txt(280);
    private readonly NumericUpDown numNhiet = new() { DecimalPlaces = 1, Minimum = 30, Maximum = 45, Increment = 0.1m, Value = 36.5m, Width = 70 };
    private readonly TextBox txtHuyetAp = Ui.Txt(80);
    private readonly Label lblPhieu = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };

    // xét nghiệm
    private readonly ComboBox cboLoaiXN = new() { Width = 220 };
    private readonly TextBox txtKetQuaXN = Ui.Txt(220);
    private readonly DataGridView dgvXN = Ui.Grid();

    // chẩn đoán + toa
    private readonly TextBox txtChanDoan = Ui.Txt(300);
    private readonly ComboBox cboThuoc = new() { Width = 220 };
    private readonly NumericUpDown numSoLuong = new() { Minimum = 1, Maximum = 1000, Value = 1, Width = 70 };
    private readonly TextBox txtLieuDung = Ui.Txt(220);
    private readonly TextBox txtGhiChuToa = Ui.Txt(300);
    private readonly DataGridView dgvToa = Ui.Grid();
    private readonly DataTable toaNhap = TaoBangToa();
    private readonly Button btnThemDong, btnXoaDong, btnLapToa;

    private int? maHS;
    private int? maPK;

    public KhamBenhControl()
    {
        Dock = DockStyle.Fill;
        btnThemDong = Ui.Btn("Thêm thuốc vào toa", (_, _) => Ui.Safe(this, ThemDongToa));
        btnXoaDong = Ui.Btn("Bỏ dòng đang chọn", (_, _) => Ui.Safe(this, XoaDongToa));
        btnLapToa = Ui.Btn("Lập toa thuốc", (_, _) => Ui.Safe(this, LapToa));

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 270));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(Ui.Nhom("1. Tìm hồ sơ và lập phiếu khám", DungPhieu()), 0, 0);
        root.SetColumnSpan(root.GetControlFromPosition(0, 0)!, 2);
        root.Controls.Add(Ui.Nhom("2. Xét nghiệm của phiếu khám", DungXetNghiem()), 0, 1);
        root.Controls.Add(Ui.Nhom("3. Chẩn đoán và toa thuốc của phiếu khám", DungToa()), 1, 1);
        Controls.Add(root);

        rdoDinhKy.CheckedChanged += (_, _) => Ui.Safe(this, DoiLoaiKham);
        dgvToa.DataSource = toaNhap;
        Load += (_, _) => LamMoi();
    }

    private static DataTable TaoBangToa()
    {
        var dt = new DataTable();
        dt.Columns.Add("MaThuoc", typeof(int));
        dt.Columns.Add("TenThuoc", typeof(string));
        dt.Columns.Add("SoLuong", typeof(int));
        dt.Columns.Add("LieuDung", typeof(string));
        return dt;
    }

    private Control DungPhieu()
    {
        var t = Ui.Luoi2Cot();
        var tim = new FlowLayoutPanel { AutoSize = true };
        tim.Controls.Add(txtMaHoSo);
        tim.Controls.Add(Ui.Btn("Tìm hồ sơ", (_, _) => Ui.Safe(this, TimHoSo)));
        Ui.ThemDong(t, "Mã hồ sơ / SĐT", tim);
        Ui.ThemDong(t, "Bệnh nhân", lblHoSo);
        Ui.ThemDong(t, "Bác sĩ khám", cboBacSi);
        var loai = new FlowLayoutPanel { AutoSize = true };
        loai.Controls.Add(rdoDieuTri);
        loai.Controls.Add(rdoDinhKy);
        Ui.ThemDong(t, "Loại khám", loai);
        Ui.ThemDong(t, "Lịch hẹn định kỳ", cboLichHen);
        Ui.ThemDong(t, "Triệu chứng", txtTrieuChung);
        var sinhHieu = new FlowLayoutPanel { AutoSize = true };
        sinhHieu.Controls.Add(Ui.Lbl("Thân nhiệt (°C)"));
        sinhHieu.Controls.Add(numNhiet);
        sinhHieu.Controls.Add(Ui.Lbl("Huyết áp"));
        sinhHieu.Controls.Add(txtHuyetAp);
        Ui.ThemDong(t, "Sinh hiệu", sinhHieu);
        var nut = new FlowLayoutPanel { AutoSize = true };
        nut.Controls.Add(Ui.Btn("Tạo phiếu khám", (_, _) => Ui.Safe(this, TaoPhieu)));
        nut.Controls.Add(lblPhieu);
        Ui.ThemDong(t, "", nut);
        return t;
    }

    private Control DungXetNghiem()
    {
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var t = Ui.Luoi2Cot();
        Ui.ThemDong(t, "Loại xét nghiệm", cboLoaiXN);
        Ui.ThemDong(t, "Kết quả", txtKetQuaXN);
        var nut = new FlowLayoutPanel { AutoSize = true };
        nut.Controls.Add(Ui.Btn("Thêm xét nghiệm", (_, _) => Ui.Safe(this, ThemXetNghiem)));
        nut.Controls.Add(Ui.Btn("Cập nhật kết quả dòng đang chọn", (_, _) => Ui.Safe(this, CapNhatKetQua)));
        Ui.ThemDong(t, "", nut);
        host.Controls.Add(t, 0, 0);
        host.Controls.Add(dgvXN, 0, 1);
        return host;
    }

    private Control DungToa()
    {
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var t = Ui.Luoi2Cot();
        var cd = new FlowLayoutPanel { AutoSize = true };
        cd.Controls.Add(txtChanDoan);
        cd.Controls.Add(Ui.Btn("Lưu chẩn đoán", (_, _) => Ui.Safe(this, LuuChanDoan)));
        Ui.ThemDong(t, "Chẩn đoán", cd);
        Ui.ThemDong(t, "Thuốc", cboThuoc);
        var sl = new FlowLayoutPanel { AutoSize = true };
        sl.Controls.Add(numSoLuong);
        sl.Controls.Add(Ui.Lbl("Liều dùng"));
        sl.Controls.Add(txtLieuDung);
        Ui.ThemDong(t, "Số lượng", sl);
        var nut = new FlowLayoutPanel { AutoSize = true };
        nut.Controls.Add(btnThemDong);
        nut.Controls.Add(btnXoaDong);
        Ui.ThemDong(t, "", nut);
        Ui.ThemDong(t, "Ghi chú toa", txtGhiChuToa);
        Ui.ThemDong(t, "", btnLapToa);
        host.Controls.Add(t, 0, 0);
        host.Controls.Add(dgvToa, 0, 1);
        return host;
    }

    public void LamMoi() => Ui.Safe(this, () =>
    {
        Ui.GanCombo(cboBacSi, DanhMucService.BacSi(), "Hien", "MaBS");
        Ui.GanCombo(cboLoaiXN, DanhMucService.LoaiXetNghiem(), "TenXN", "MaLoaiXN");
        Ui.GanCombo(cboThuoc, DanhMucService.Thuoc(), "TenThuoc", "MaThuoc");
        if (maHS is not null) TaiPhieuDangKham();
    });

    private void TimHoSo()
    {
        var row = TraCuuService.TimHoSo(txtMaHoSo.Text);
        maPK = null;
        if (row is null)
        {
            maHS = null;
            lblHoSo.Text = "Không tìm thấy hồ sơ.";
            lblPhieu.Text = "";
            return;
        }
        maHS = Convert.ToInt32(row["MaHS"]);
        var conHan = (row["ConHan"] is bool daDong && daDong) ? "đã đóng lệ phí năm nay" : "CHƯA đóng lệ phí năm nay";
        lblHoSo.Text = $"{row["MaHoSo"]} - {row["HoTen"]} ({conHan})";
        NapLichHen();
        TaiPhieuDangKham();
    }

    private void NapLichHen()
    {
        cboLichHen.DataSource = null;
        if (maHS is null) return;
        var ds = LichHenService.TheoHoSo(maHS.Value, chiConHieuLuc: true);
        var dt = new DataTable();
        dt.Columns.Add("MaHen", typeof(int));
        dt.Columns.Add("Hien", typeof(string));
        foreach (DataRow r in ds.Rows)
            dt.Rows.Add(r["MaHen"], $"Quý {r["Quy"]}/{r["Nam"]} - {Convert.ToDateTime(r["NgayHen"]):dd/MM/yyyy HH:mm}");
        Ui.GanCombo(cboLichHen, dt, "Hien", "MaHen");
    }

    private void DoiLoaiKham()
    {
        cboLichHen.Enabled = rdoDinhKy.Checked;
        if (rdoDinhKy.Checked) NapLichHen();
    }

    /// <summary>Mỗi hồ sơ tối đa 1 phiếu đang khám: nạp lại phiếu đó (nếu có) cùng xét nghiệm và toa.</summary>
    private void TaiPhieuDangKham()
    {
        maPK = null;
        toaNhap.Rows.Clear();
        dgvXN.DataSource = null;
        txtChanDoan.Clear();
        if (maHS is null) return;

        var ds = KhamBenhService.PhieuDangKham(maHS.Value);
        if (ds.Rows.Count == 0)
        {
            lblPhieu.Text = "Chưa có phiếu đang khám.";
            DatTrangThaiToa(true);
            dgvToa.DataSource = toaNhap;
            return;
        }
        var r = ds.Rows[0];
        maPK = Convert.ToInt32(r["MaPK"]);
        lblPhieu.Text = $"Phiếu đang khám: #{maPK}";
        txtChanDoan.Text = r["ChanDoan"] as string ?? "";
        TaiXetNghiem();

        var toa = TraCuuService.Toa(maPK.Value);
        if (toa.Rows.Count > 0)
        {
            dgvToa.DataSource = toa;           // toa đã lập: chỉ xem
            DatTrangThaiToa(false);
        }
        else
        {
            dgvToa.DataSource = toaNhap;
            DatTrangThaiToa(true);
        }
    }

    private void DatTrangThaiToa(bool choPhepSua)
    {
        btnThemDong.Enabled = choPhepSua;
        btnXoaDong.Enabled = choPhepSua;
        btnLapToa.Enabled = choPhepSua;
    }

    private void TaiXetNghiem()
    {
        if (maPK is null) { dgvXN.DataSource = null; return; }
        dgvXN.DataSource = TraCuuService.XetNghiem(maPK.Value);
    }

    private void TaoPhieu()
    {
        if (maHS is null) { TimHoSo(); if (maHS is null) return; }
        var bacSi = Ui.ChonInt(cboBacSi);
        if (bacSi is null)
        {
            MessageBox.Show(FindForm(), "Hãy chọn bác sĩ khám.", "Phiếu khám", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dinhKy = rdoDinhKy.Checked;
        maPK = KhamBenhService.TaoPhieu(maHS.Value, bacSi.Value, dinhKy ? "DinhKy" : "DieuTri",
            txtTrieuChung.Text.Trim(), numNhiet.Value, txtHuyetAp.Text.Trim(),
            dinhKy ? Ui.ChonInt(cboLichHen) : null);
        TaiPhieuDangKham();
        NapLichHen();
    }

    private bool CoPhieu()
    {
        if (maPK is not null) return true;
        MessageBox.Show(FindForm(), "Hãy tạo phiếu khám (hoặc tìm hồ sơ đang có phiếu khám) trước.", "Chưa có phiếu khám",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private void ThemXetNghiem()
    {
        if (!CoPhieu()) return;
        var loai = Ui.ChonInt(cboLoaiXN);
        if (loai is null) return;
        KhamBenhService.ThemXetNghiem(maPK!.Value, loai.Value, string.IsNullOrWhiteSpace(txtKetQuaXN.Text) ? null : txtKetQuaXN.Text.Trim());
        txtKetQuaXN.Clear();
        TaiXetNghiem();
    }

    private void CapNhatKetQua()
    {
        if (!CoPhieu()) return;
        var maXN = Ui.IdDong(dgvXN, "MaXN");
        if (maXN is null) return;
        KhamBenhService.CapNhatKetQuaXN(maXN.Value, txtKetQuaXN.Text.Trim());
        TaiXetNghiem();
    }

    private void LuuChanDoan()
    {
        if (!CoPhieu()) return;
        KhamBenhService.CapNhatChanDoan(maPK!.Value, txtChanDoan.Text.Trim());
        MessageBox.Show(FindForm(), "Đã lưu chẩn đoán.", "Chẩn đoán", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ThemDongToa()
    {
        var maThuoc = Ui.ChonInt(cboThuoc);
        if (maThuoc is null) return;
        foreach (DataRow r in toaNhap.Rows)
        {
            if (Convert.ToInt32(r["MaThuoc"]) == maThuoc.Value)
            {
                MessageBox.Show(FindForm(), "Thuốc này đã có trong toa.", "Toa thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        toaNhap.Rows.Add(maThuoc.Value, cboThuoc.Text, (int)numSoLuong.Value,
            string.IsNullOrWhiteSpace(txtLieuDung.Text) ? (object)DBNull.Value : txtLieuDung.Text.Trim());
    }

    private void XoaDongToa()
    {
        if (dgvToa.CurrentRow?.DataBoundItem is DataRowView v && ReferenceEquals(v.Row.Table, toaNhap))
            toaNhap.Rows.Remove(v.Row);
    }

    private void LapToa()
    {
        if (!CoPhieu()) return;
        var dong = toaNhap.Rows.Cast<DataRow>()
            .Select(r => new DongToa(Convert.ToInt32(r["MaThuoc"]), Convert.ToInt32(r["SoLuong"]), r["LieuDung"] as string))
            .ToList();
        if (dong.Count == 0)
        {
            MessageBox.Show(FindForm(), "Toa thuốc cần ít nhất một loại thuốc.", "Toa thuốc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var maToa = KhamBenhService.LapToa(maPK!.Value, string.IsNullOrWhiteSpace(txtGhiChuToa.Text) ? null : txtGhiChuToa.Text.Trim(), dong);
        MessageBox.Show(FindForm(), $"Đã lập toa thuốc #{maToa} cho phiếu khám #{maPK}.", "Toa thuốc", MessageBoxButtons.OK, MessageBoxIcon.Information);
        toaNhap.Rows.Clear();
        TaiPhieuDangKham();
    }
}
