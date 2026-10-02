using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopping
{
    // ===== Dữ liệu lấy từ Hệ thống quản lý sản phẩm (ngoài) =====
    public class NhomSanPham
    {
        public string MaNhom { get; set; }
        public string TenNhom { get; set; }
    }

    public class SanPham
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string TenNhaSanXuat { get; set; }
        public string MoTa { get; set; }
        public string ThongSoKyThuat { get; set; }
        public decimal GiaBan { get; set; }
        public bool ConHang { get; set; }
        public string MaNhom { get; set; }
        public List<string> HinhAnh { get; set; } = new List<string>();
    }

    // ===== Thực thể của e-Shopping =====
    public class KhachHang
    {
        public string MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string SoGiayTo { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string TenDangNhap { get; set; }
        public string Email { get; set; }
    }

    public class NguoiNhan
    {
        public string HoTen { get; set; }
        public string DiaChi { get; set; }
        public string DienThoai { get; set; }
        public string MaKhuVuc { get; set; }
    }

    // Dữ liệu thẻ chỉ tồn tại trong bộ nhớ khi đặt hàng, không lưu đầy đủ xuống CSDL.
    public class TheTinDung
    {
        public string MaLoaiThe { get; set; }
        public string SoThe { get; set; }
        public string CSV { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string TenChuThe { get; set; }
    }

    // ===== Giỏ hàng hiện tại: lưu trong phiên làm việc (bộ nhớ) =====
    public class MucGioHang
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get { return DonGia * SoLuong; } }
    }

    public class GioHang
    {
        public List<MucGioHang> Muc { get; } = new List<MucGioHang>();
        public decimal TongTienHang { get { return Muc.Sum(m => m.ThanhTien); } }
        public bool Rong { get { return Muc.Count == 0; } }
    }

    public static class PhienLamViec
    {
        public static KhachHang KhachHangHienTai;
        public static GioHang GioHang = new GioHang();
    }

    // ===== Kết quả xử lý =====
    public class KetQuaTinhTien
    {
        public decimal TongTienHang { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal LePhiThe { get; set; }
        public bool MienPhiGiaoHang { get; set; }
        public bool ThieuBangGia { get; set; }
        public decimal TongTriGia { get { return TongTienHang + PhiGiaoHang + LePhiThe; } }
    }

    public class KetQuaThanhToan
    {
        public bool HopLe { get; set; }
        public string MaXacNhan { get; set; }
        public string ThongBao { get; set; }
    }

    public class KetQuaXuLy
    {
        public bool ThanhCong { get; private set; }
        public string ThongBao { get; private set; }
        public object DuLieu { get; private set; }

        private KetQuaXuLy(bool thanhCong, string thongBao, object duLieu)
        {
            ThanhCong = thanhCong;
            ThongBao = thongBao;
            DuLieu = duLieu;
        }

        public static KetQuaXuLy Ok(string thongBao, object duLieu = null) { return new KetQuaXuLy(true, thongBao, duLieu); }
        public static KetQuaXuLy Loi(string thongBao) { return new KetQuaXuLy(false, thongBao, null); }
    }
}
