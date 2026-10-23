using System.Windows.Forms;

namespace QuanLyNhanSu.UI
{
    // Lớp cơ sở cho các tab: NapLai() được gọi mỗi khi người dùng mở tab
    public abstract class TabBase : UserControl
    {
        public abstract void NapLai();
    }
}
