using System;

namespace QuanLyKhachSan.Services
{
    public class KetQuaXuLy
    {
        public bool ThanhCong { get; set; }
        public string ThongBao { get; set; }

        public static KetQuaXuLy Ok(string thongBao)
        {
            return new KetQuaXuLy
            {
                ThanhCong = true,
                ThongBao = thongBao
            };
        }

        public static KetQuaXuLy Fail(string thongBao)
        {
            return new KetQuaXuLy
            {
                ThanhCong = false,
                ThongBao = thongBao
            };
        }
    }
}