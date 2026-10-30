namespace QuanLyVanPhong;

public class FrmKiemThu : Form
{
    TextBox txt = new TextBox
    {
        Left = 10, Top = 50, Width = 780, Height = 400,
        Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 10)
    };

    public FrmKiemThu()
    {
        Ui.Setup(this, "Kiểm thử các quy tắc nghiệp vụ", 800, 460);
        Controls.AddRange(new Control[]
        {
            Ui.B("Chạy kiểm thử", 10, 10, 150, Chay),
            Ui.L("Tự tạo dữ liệu tạm (phòng TST*, khách TEST) rồi xóa sạch sau khi chạy.", 175, 16),
            txt
        });
    }

    void Chay(object sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;
        try { txt.Text = string.Join("\r\n", Tester.Run()); }
        catch (Exception ex) { Ui.Err(ex); }
        finally { Cursor = Cursors.Default; }
    }
}
