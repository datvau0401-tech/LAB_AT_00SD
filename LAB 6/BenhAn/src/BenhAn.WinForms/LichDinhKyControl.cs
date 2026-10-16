using System.Data;
using BenhAn.Data;

namespace BenhAn.WinForms;

/// <summary>Module 3: đặt/hủy lịch khám sức khỏe định kỳ theo quý (miễn phí, mỗi quý 1 lần).</summary>
public sealed class LichDinhKyControl : UserControl, ILamMoi
{
    private readonly TextBox txtMaHoSo = Ui.Txt(140);
    private readonly Label lblHoSo = new() { AutoSize = true };
    private readonly ComboBox cboBacSi = new() { Width = 260 };
    private readonly DateTimePicker dtpNgayHen = new()
    {
        Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Width = 160
    };
    private readonly Label lblQuy = new() { AutoSize = true };
    private readonly DataGridView dgvLich = Ui.Grid();
    private int? maHS;

    public LichDinhKyControl()
    {
        Dock = DockStyle.Fill;

        var t = Ui.Luoi2Cot();
        var tim = new FlowLayoutPanel { AutoSize = true };
        tim.Controls.Add(txtMaHoSo);
        tim.Controls.Add(Ui.Btn("Tìm hồ sơ", (_, _) => Ui.Safe(this, TimHoSo)));
        Ui.ThemDong(t, "Mã hồ sơ / SĐT", tim);
        Ui.ThemDong(t, "Bệnh nhân", lblHoSo);
        Ui.ThemDong(t, "Bác sĩ (tùy chọn)", cboBacSi);
        Ui.ThemDong(t, "Ngày giờ hẹn", dtpNgayHen);
        Ui.ThemDong(t, "Thuộc quý", lblQuy);
        var nut = new FlowLayoutPanel { AutoSize = true };
        nut.Controls.Add(Ui.Btn("Đặt lịch định kỳ", (_, _) => Ui.Safe(this, DatLich)));
        nut.Controls.Add(Ui.Btn("Hủy lịch đang chọn", (_, _) => Ui.Safe(this, HuyLich)));
        Ui.ThemDong(t, "", nut);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
        body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(Ui.Nhom("Đặt lịch khám sức khỏe định kỳ", t), 0, 0);
        body.Controls.Add(Ui.Nhom("Các lịch hẹn của hồ sơ", dgvLich), 0, 1);
        ((GroupBox)body.GetControlFromPosition(0, 0)!).AutoSize = true;
        Controls.Add(body);

        dtpNgayHen.ValueChanged += (_, _) => CapNhatQuy();
        Load += (_, _) => LamMoi();
    }

    public void LamMoi() => Ui.Safe(this, () =>
    {
        var dt = DanhMucService.BacSi();
        var r = dt.NewRow();
        r["MaBS"] = DBNull.Value;
        r["Hien"] = "(Chưa chỉ định bác sĩ)";
        dt.Rows.InsertAt(r, 0);
        Ui.GanCombo(cboBacSi, dt, "Hien", "MaBS");

        var sang = DateTime.Today.AddDays(1).AddHours(8);
        dtpNgayHen.MinDate = DateTime.Now.AddMinutes(-1);
        dtpNgayHen.Value = sang;
        CapNhatQuy();
        if (maHS is not null) TaiLich();
    });

    private void CapNhatQuy()
    {
        var d = dtpNgayHen.Value;
        lblQuy.Text = $"Quý {(d.Month + 2) / 3}/{d.Year}";
    }

    private void TimHoSo()
    {
        var row = TraCuuService.TimHoSo(txtMaHoSo.Text);
        if (row is null)
        {
            maHS = null;
            lblHoSo.Text = "Không tìm thấy hồ sơ.";
            dgvLich.DataSource = null;
            return;
        }
        maHS = Convert.ToInt32(row["MaHS"]);
        var conHan = (row["ConHan"] is bool daDong && daDong) ? "đã đóng lệ phí năm nay" : "CHƯA đóng lệ phí năm nay";
        lblHoSo.Text = $"{row["MaHoSo"]} - {row["HoTen"]} ({conHan})";
        TaiLich();
    }

    private void TaiLich()
    {
        if (maHS is null) return;
        dgvLich.DataSource = LichHenService.TheoHoSo(maHS.Value);
    }

    private void DatLich()
    {
        if (maHS is null) { TimHoSo(); if (maHS is null) return; }
        var ma = LichHenService.Tao(maHS.Value, dtpNgayHen.Value, Ui.ChonInt(cboBacSi));
        TaiLich();
        MessageBox.Show(FindForm(), $"Đã đặt lịch khám định kỳ ({lblQuy.Text}). Mã lịch hẹn: {ma}.", "Lịch định kỳ",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void HuyLich()
    {
        var maHen = Ui.IdDong(dgvLich, "MaHen");
        if (maHen is null)
        {
            MessageBox.Show(FindForm(), "Hãy chọn một lịch hẹn trong danh sách.", "Hủy lịch", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (MessageBox.Show(FindForm(), "Hủy lịch hẹn đang chọn?", "Hủy lịch", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        LichHenService.Huy(maHen.Value);
        TaiLich();
    }
}
