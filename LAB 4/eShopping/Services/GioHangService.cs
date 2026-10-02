using System.Linq;

namespace eShopping.Services
{
    // Giỏ hàng hiện tại nằm trong PhienLamViec (bộ nhớ), chỉ được ghi xuống CSDL khi đặt hàng thành công.
    public class GioHangService
    {
        public const int ToiDaMoiSanPham = 99;

        private GioHang Gio { get { return PhienLamViec.GioHang; } }

        public KetQuaXuLy Them(SanPham sp, int soLuong)
        {
            if (sp == null) return KetQuaXuLy.Loi("Vui lòng chọn sản phẩm.");
            if (!sp.ConHang) return KetQuaXuLy.Loi("Sản phẩm \"" + sp.TenSanPham + "\" hiện đã hết hàng.");
            if (soLuong < 1) return KetQuaXuLy.Loi("Số lượng phải từ 1 trở lên.");

            MucGioHang muc = Gio.Muc.FirstOrDefault(m => m.MaSanPham == sp.MaSanPham);
            int moi = (muc == null ? 0 : muc.SoLuong) + soLuong;
            if (moi > ToiDaMoiSanPham)
                return KetQuaXuLy.Loi("Mỗi sản phẩm chỉ được mua tối đa " + ToiDaMoiSanPham + " cái trong một đơn.");

            if (muc == null)
                Gio.Muc.Add(new MucGioHang { MaSanPham = sp.MaSanPham, TenSanPham = sp.TenSanPham, DonGia = sp.GiaBan, SoLuong = soLuong });
            else
            {
                muc.SoLuong = moi;
                muc.DonGia = sp.GiaBan;
            }
            return KetQuaXuLy.Ok("Đã thêm \"" + sp.TenSanPham + "\" vào giỏ hàng.");
        }

        public KetQuaXuLy CapNhatSoLuong(string maSanPham, int soLuong)
        {
            MucGioHang muc = Gio.Muc.FirstOrDefault(m => m.MaSanPham == maSanPham);
            if (muc == null) return KetQuaXuLy.Loi("Sản phẩm không có trong giỏ hàng.");
            if (soLuong < 1 || soLuong > ToiDaMoiSanPham)
                return KetQuaXuLy.Loi("Số lượng phải từ 1 đến " + ToiDaMoiSanPham + ". Muốn bỏ sản phẩm hãy chọn Xóa.");
            muc.SoLuong = soLuong;
            return KetQuaXuLy.Ok("Đã cập nhật số lượng.");
        }

        public KetQuaXuLy Xoa(string maSanPham)
        {
            MucGioHang muc = Gio.Muc.FirstOrDefault(m => m.MaSanPham == maSanPham);
            if (muc == null) return KetQuaXuLy.Loi("Sản phẩm không có trong giỏ hàng.");
            Gio.Muc.Remove(muc);
            return KetQuaXuLy.Ok("Đã bỏ sản phẩm khỏi giỏ hàng.");
        }

        public void LamRong() { Gio.Muc.Clear(); }
    }
}
