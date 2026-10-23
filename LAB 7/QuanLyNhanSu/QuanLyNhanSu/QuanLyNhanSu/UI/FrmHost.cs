using System.Drawing;
using System.Windows.Forms;

namespace QuanLyNhanSu.UI
{
    // Form chứa một hoặc nhiều tab; mở từ menu chính
    public class FrmHost : Form
    {
        private readonly TabControl tabs = new TabControl();

        public FrmHost(string tieuDe)
        {
            Text = tieuDe;
            Size = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10F);
            tabs.Dock = DockStyle.Fill;
            tabs.SelectedIndexChanged += delegate { NapTabHienTai(); };
            Controls.Add(tabs);
            Shown += delegate { NapTabHienTai(); };
        }

        public FrmHost ThemTab(string tieuDe, TabBase noiDung)
        {
            TabPage page = new TabPage(tieuDe);
            noiDung.Dock = DockStyle.Fill;
            page.Controls.Add(noiDung);
            tabs.TabPages.Add(page);
            return this;
        }

        private void NapTabHienTai()
        {
            if (tabs.SelectedTab == null) return;
            try { ((TabBase)tabs.SelectedTab.Controls[0]).NapLai(); }
            catch (System.Exception ex) { UiHelper.Loi(ex); }
        }
    }
}
