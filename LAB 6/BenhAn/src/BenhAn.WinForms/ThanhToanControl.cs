using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Module 5: thanh toán viện phí, sau đó phiếu khám mới được hoàn tất (ghi nhận vào hồ sơ và khóa chỉnh sửa).</summary>
public sealed class ThanhToanControl : UserControl, ILamMoi
{
    private readonly DataGridView dgvPhieu = Ui.Grid(), dgvXN = Ui.Grid(), dgvToa = Ui.Grid();
    private readonly Label lblKham = new() { AutoSize = true }, lblXN = new() { AutoSize = true },
                           lblThuoc = new() { AutoSize = true },
                           lblTong = new() { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
    private readonly ComboBox cboPhuongThuc = new() { Width = 160 };

    public ThanhToanControl()
    {
        Dock = DockStyle.Fill;

        var t = Ui.Luoi2Cot();
        Ui.ThemDong(t, "Tiền khám", lblKham);
        Ui.ThemDong(t, "Tiền xét nghiệm", lblXN);
        Ui.ThemDong(t, "Tiền thuốc", lblThuoc);
        Ui.ThemDong(t, "TỔNG viện phí", lblTong);
        cboPhuongThuc.DropDownStyle = ComboBoxStyle.DropDownList;
        cboPhuongThuc.DataSource = new List<LuaChon> { new("Tiền mặt", "TienMat"), new("Thẻ", "The"), new("Chuyển khoản", "ChuyenKhoan") };
        cboPhuongThuc.DisplayMember = nameof(LuaChon.Text);
        cboPhuongThuc.ValueMember = nameof(LuaChon.Value);
        Ui.ThemDong(t, "Phương thức", cboPhuongThuc);
        var nut = new FlowLayoutPanel { AutoSize = true };
        nut.Controls.Add(Ui.Btn("Thanh toán và hoàn tất phiếu khám", (_, _) => Ui.Safe(this, ThanhToan)));
        nut.Controls.Add(Ui.Btn("Làm mới", (_, _) => LamMoi()));
        Ui.ThemDong(t, "", nut);

        var phai = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        phai.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        phai.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        phai.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        phai.Controls.Add(Ui.Nhom("Viện phí", t), 0, 0);
        ((GroupBox)phai.GetControlFromPosition(0, 0)!).AutoSize = true;
        phai.Controls.Add(Ui.Nhom("Xét nghiệm", dgvXN), 0, 1);
        phai.Controls.Add(Ui.Nhom("Toa thuốc", dgvToa), 0, 2);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(8) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        root.Controls.Add(Ui.Nhom("Phiếu khám đang chờ thanh toán", dgvPhieu), 0, 0);
        root.Controls.Add(phai, 1, 0);
        Controls.Add(root);

        dgvPhieu.SelectionChanged += (_, _) => Ui.Safe(this, TaiChiTiet);
        Load += (_, _) => LamMoi();
    }

    public void LamMoi() => Ui.Safe(this, () => dgvPhieu.DataSource = ThanhToanService.PhieuChoThanhToan());

    private void TaiChiTiet()
    {
        var maPK = Ui.IdDong(dgvPhieu, "MaPK");
        if (maPK is null)
        {
            lblKham.Text = lblXN.Text = lblThuoc.Text = lblTong.Text = "";
            dgvXN.DataSource = null;
            dgvToa.DataSource = null;
            return;
        }
        var vp = ThanhToanService.XemVienPhi(maPK.Value);
        if (vp.Rows.Count > 0)
        {
            var r = vp.Rows[0];
            lblKham.Text = Ui.Tien(r["TienKham"]);
            lblXN.Text = Ui.Tien(r["TienXetNghiem"]);
            lblThuoc.Text = Ui.Tien(r["TienThuoc"]);
            lblTong.Text = Ui.Tien(r["TongTien"]);
        }
        dgvXN.DataSource = TraCuuService.XetNghiem(maPK.Value);
        dgvToa.DataSource = TraCuuService.Toa(maPK.Value);
    }

    private void ThanhToan()
    {
        var maPK = Ui.IdDong(dgvPhieu, "MaPK");
        if (maPK is null)
        {
            MessageBox.Show(FindForm(), "Hãy chọn một phiếu khám.", "Thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(FindForm(), $"Thanh toán {lblTong.Text} cho phiếu #{maPK} và hoàn tất phiếu khám?\nSau đó không thể sửa xét nghiệm/toa thuốc.",
                "Xác nhận thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var maHD = ThanhToanService.ThanhToan(maPK.Value, (string)cboPhuongThuc.SelectedValue!);
        MessageBox.Show(FindForm(), $"Đã thanh toán. Hóa đơn #{maHD}. Phiếu khám #{maPK} đã hoàn tất và ghi vào hồ sơ bệnh án.",
            "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LamMoi();
    }
}
