using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Module 1: đăng ký lần đầu + đóng lệ phí năm + cấp hồ sơ; gia hạn lệ phí hằng năm.</summary>
public sealed class DangKyControl : UserControl, ILamMoi
{
    // đăng ký lần đầu
    private readonly TextBox txtHoTen = Ui.Txt(), txtDiaChi = Ui.Txt(), txtDienThoai = Ui.Txt();
    private readonly DateTimePicker dtpNgaySinh = new()
    {
        Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today, Value = new DateTime(2000, 1, 1)
    };
    private readonly NumericUpDown numLePhi = Ui.Money();
    private readonly PictureBox pic = new()
    {
        Width = 90, Height = 120, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle
    };
    private readonly Label lblKetQua = new() { AutoSize = true, ForeColor = Color.DarkGreen, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
    private byte[]? anh;

    // gia hạn lệ phí
    private readonly TextBox txtMaHoSo = Ui.Txt(140);
    private readonly Label lblHoSo = new() { AutoSize = true };
    private readonly NumericUpDown numLePhiGH = Ui.Money();
    private readonly NumericUpDown numNam = new() { Minimum = 2000, Maximum = 2100, Value = DateTime.Today.Year, Width = 80 };
    private readonly DataGridView dgvLePhi = Ui.Grid();
    private int? maHSGiaHan;

    public DangKyControl()
    {
        Dock = DockStyle.Fill;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.Controls.Add(Ui.Nhom("Đăng ký khám lần đầu - đóng lệ phí năm - cấp hồ sơ bệnh án", DungFormDangKy()), 0, 0);
        root.Controls.Add(Ui.Nhom("Gia hạn lệ phí hằng năm", DungFormGiaHan()), 1, 0);
        Controls.Add(root);

        Load += (_, _) => LamMoi();
    }

    private Control DungFormDangKy()
    {
        var t = Ui.Luoi2Cot();
        Ui.ThemDong(t, "Họ tên (*)", txtHoTen);
        Ui.ThemDong(t, "Địa chỉ (*)", txtDiaChi);
        Ui.ThemDong(t, "Ngày sinh (*)", dtpNgaySinh);
        Ui.ThemDong(t, "Điện thoại (*)", txtDienThoai);

        var anhPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        anhPanel.Controls.Add(pic);
        anhPanel.Controls.Add(Ui.Btn("Chọn ảnh 3x4...", (_, _) => ChonAnh()));
        Ui.ThemDong(t, "Ảnh 3x4", anhPanel);

        Ui.ThemDong(t, "Lệ phí năm (VNĐ)", numLePhi);
        var btn = Ui.Btn("Đăng ký + đóng lệ phí + cấp hồ sơ", (_, _) => Ui.Safe(this, DangKy));
        Ui.ThemDong(t, "", btn);
        Ui.ThemDong(t, "", lblKetQua);
        return t;
    }

    private Control DungFormGiaHan()
    {
        var host = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var t = Ui.Luoi2Cot();
        var timPanel = new FlowLayoutPanel { AutoSize = true };
        timPanel.Controls.Add(txtMaHoSo);
        timPanel.Controls.Add(Ui.Btn("Tìm", (_, _) => Ui.Safe(this, TimHoSoGiaHan)));
        Ui.ThemDong(t, "Mã hồ sơ / SĐT", timPanel);
        Ui.ThemDong(t, "Bệnh nhân", lblHoSo);
        Ui.ThemDong(t, "Năm", numNam);
        Ui.ThemDong(t, "Lệ phí (VNĐ)", numLePhiGH);
        Ui.ThemDong(t, "", Ui.Btn("Đóng lệ phí", (_, _) => Ui.Safe(this, GiaHan)));

        host.Controls.Add(t, 0, 0);
        host.Controls.Add(dgvLePhi, 0, 1);
        return host;
    }

    public void LamMoi() => Ui.Safe(this, () =>
    {
        var lePhi = DanhMucService.ThamSo("LePhiNam");
        numLePhi.Value = Math.Min(lePhi, numLePhi.Maximum);
        numLePhiGH.Value = Math.Min(lePhi, numLePhiGH.Maximum);
    });

    private void ChonAnh()
    {
        using var dlg = new OpenFileDialog { Filter = "Ảnh (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", Title = "Chọn ảnh 3x4" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var f = new FileInfo(dlg.FileName);
        if (f.Length > 2 * 1024 * 1024)
        {
            MessageBox.Show(FindForm(), "Ảnh tối đa 2 MB.", "Ảnh 3x4", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        anh = File.ReadAllBytes(f.FullName);
        pic.ImageLocation = f.FullName;
    }

    private void DangKy()
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text) || string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
            string.IsNullOrWhiteSpace(txtDienThoai.Text))
        {
            MessageBox.Show(FindForm(), "Vui lòng nhập đủ họ tên, địa chỉ và điện thoại.", "Thiếu thông tin",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var kq = DangKyService.DangKyLanDau(new DangKyRequest(
            txtHoTen.Text.Trim(), txtDiaChi.Text.Trim(), dtpNgaySinh.Value, txtDienThoai.Text.Trim(), anh, numLePhi.Value));

        lblKetQua.Text = $"Đã cấp hồ sơ: {kq.MaHoSo}";
        MessageBox.Show(FindForm(), $"Đăng ký thành công.\nMã hồ sơ: {kq.MaHoSo}\nLệ phí năm {DateTime.Today.Year}: {Ui.Tien(numLePhi.Value)}",
            "Cấp hồ sơ", MessageBoxButtons.OK, MessageBoxIcon.Information);

        txtHoTen.Clear(); txtDiaChi.Clear(); txtDienThoai.Clear();
        anh = null; pic.ImageLocation = null; pic.Image = null;
    }

    private void TimHoSoGiaHan()
    {
        var row = TraCuuService.TimHoSo(txtMaHoSo.Text);
        if (row is null)
        {
            maHSGiaHan = null;
            lblHoSo.Text = "Không tìm thấy hồ sơ.";
            dgvLePhi.DataSource = null;
            return;
        }
        maHSGiaHan = Convert.ToInt32(row["MaHS"]);
        lblHoSo.Text = $"{row["MaHoSo"]} - {row["HoTen"]}";
        dgvLePhi.DataSource = DangKyService.LePhiTheoHoSo(maHSGiaHan.Value);
    }

    private void GiaHan()
    {
        if (maHSGiaHan is null) TimHoSoGiaHan();
        if (maHSGiaHan is null) return;
        DangKyService.GiaHanLePhi(maHSGiaHan.Value, numLePhiGH.Value, (short)numNam.Value);
        dgvLePhi.DataSource = DangKyService.LePhiTheoHoSo(maHSGiaHan.Value);
        MessageBox.Show(FindForm(), $"Đã ghi nhận lệ phí năm {numNam.Value:0}.", "Gia hạn", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
