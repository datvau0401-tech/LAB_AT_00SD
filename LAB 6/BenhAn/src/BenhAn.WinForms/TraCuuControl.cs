using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Module 2: tra cứu hồ sơ (mã hồ sơ / SĐT / họ tên) và xem lịch sử khám, xét nghiệm, toa thuốc.</summary>
public sealed class TraCuuControl : UserControl, ILamMoi
{
    private readonly TextBox txtTuKhoa = Ui.Txt(260);
    private readonly DataGridView dgvHoSo = Ui.Grid(), dgvLichSu = Ui.Grid(), dgvXN = Ui.Grid(), dgvToa = Ui.Grid();

    public TraCuuControl()
    {
        Dock = DockStyle.Fill;

        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8, 6, 8, 0) };
        top.Controls.Add(Ui.Lbl("Mã hồ sơ / SĐT / họ tên:"));
        top.Controls.Add(txtTuKhoa);
        var btnTim = Ui.Btn("Tìm", (_, _) => Tim());
        top.Controls.Add(btnTim);
        txtTuKhoa.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Tim(); }
        };

        var chiTiet = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        chiTiet.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chiTiet.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        chiTiet.Controls.Add(Ui.Nhom("Xét nghiệm của phiếu khám đang chọn", dgvXN), 0, 0);
        chiTiet.Controls.Add(Ui.Nhom("Toa thuốc của phiếu khám đang chọn", dgvToa), 1, 0);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
        body.Controls.Add(Ui.Nhom("Kết quả tra cứu hồ sơ", dgvHoSo), 0, 0);
        body.Controls.Add(Ui.Nhom("Lịch sử khám của hồ sơ đang chọn", dgvLichSu), 0, 1);
        body.Controls.Add(chiTiet, 0, 2);

        Controls.Add(body);
        Controls.Add(top);

        dgvHoSo.SelectionChanged += (_, _) => Ui.Safe(this, TaiLichSu);
        dgvLichSu.SelectionChanged += (_, _) => Ui.Safe(this, TaiChiTietPhieu);
        Load += (_, _) => Tim();
    }

    public void LamMoi() => Tim();

    private void Tim() => Ui.Safe(this, () => dgvHoSo.DataSource = TraCuuService.TraCuuHoSo(txtTuKhoa.Text));

    private void TaiLichSu()
    {
        var maHS = Ui.IdDong(dgvHoSo, "MaHS");
        dgvLichSu.DataSource = maHS is null ? null : TraCuuService.LichSuKham(maHS.Value);
        if (maHS is null) { dgvXN.DataSource = null; dgvToa.DataSource = null; }
    }

    private void TaiChiTietPhieu()
    {
        var maPK = Ui.IdDong(dgvLichSu, "MaPK");
        dgvXN.DataSource = maPK is null ? null : TraCuuService.XetNghiem(maPK.Value);
        dgvToa.DataSource = maPK is null ? null : TraCuuService.Toa(maPK.Value);
    }
}
