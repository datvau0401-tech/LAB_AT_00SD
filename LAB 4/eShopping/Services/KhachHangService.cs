using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using eShopping.Data;

namespace eShopping.Services
{
    public class KhachHangService
    {
        private const int SoVongLap = 100000;

        public KetQuaXuLy DangKy(KhachHang kh, string matKhau)
        {
            if (kh == null || string.IsNullOrWhiteSpace(kh.HoTen) || string.IsNullOrWhiteSpace(kh.SoGiayTo) ||
                string.IsNullOrWhiteSpace(kh.DiaChi) || string.IsNullOrWhiteSpace(kh.DienThoai) ||
                string.IsNullOrWhiteSpace(kh.TenDangNhap))
                return KetQuaXuLy.Loi("Vui lòng nhập đầy đủ họ tên, số CMND/Passport, địa chỉ, điện thoại và tên đăng nhập.");

            if (kh.NgaySinh.Date >= DateTime.Today)
                return KetQuaXuLy.Loi("Ngày sinh không hợp lệ.");

            if (string.IsNullOrEmpty(matKhau) || matKhau.Length < 6)
                return KetQuaXuLy.Loi("Mật khẩu phải có ít nhất 6 ký tự.");

            if (!string.IsNullOrWhiteSpace(kh.Email) && (!kh.Email.Contains("@") || !kh.Email.Contains(".")))
                return KetQuaXuLy.Loi("Email không đúng định dạng cơ bản.");

            string ma = "KH" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            try
            {
                Db.Execute(@"INSERT INTO KhachHang(MaKhachHang,HoTen,NgaySinh,SoGiayTo,DiaChi,DienThoai,TenDangNhap,MatKhauHash,Email)
                             VALUES(@Ma,@Ten,@NgaySinh,@GiayTo,@DiaChi,@DT,@User,@Hash,@Email)",
                    new SqlParameter("@Ma", ma),
                    new SqlParameter("@Ten", kh.HoTen.Trim()),
                    new SqlParameter("@NgaySinh", kh.NgaySinh.Date),
                    new SqlParameter("@GiayTo", kh.SoGiayTo.Trim()),
                    new SqlParameter("@DiaChi", kh.DiaChi.Trim()),
                    new SqlParameter("@DT", kh.DienThoai.Trim()),
                    new SqlParameter("@User", kh.TenDangNhap.Trim()),
                    new SqlParameter("@Hash", BamMatKhau(matKhau)),
                    new SqlParameter("@Email", string.IsNullOrWhiteSpace(kh.Email) ? (object)DBNull.Value : kh.Email.Trim()));
                return KetQuaXuLy.Ok("Đăng ký tài khoản thành công.", kh.TenDangNhap.Trim());
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    if (ex.Message.Contains("UQ_KhachHang_TenDangNhap")) return KetQuaXuLy.Loi("Tên đăng nhập đã tồn tại.");
                    if (ex.Message.Contains("UQ_KhachHang_SoGiayTo")) return KetQuaXuLy.Loi("Số CMND/Passport đã được đăng ký.");
                    return KetQuaXuLy.Loi("Thông tin khách hàng bị trùng.");
                }
                return KetQuaXuLy.Loi("Lỗi cơ sở dữ liệu: " + ex.Message);
            }
        }

        public KetQuaXuLy DangNhap(string tenDangNhap, string matKhau)
        {
            const string sai = "Tên đăng nhập hoặc mật khẩu không đúng.";
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
                return KetQuaXuLy.Loi("Vui lòng nhập tên đăng nhập và mật khẩu.");

            DataTable t = Db.Query(@"SELECT MaKhachHang,HoTen,NgaySinh,SoGiayTo,DiaChi,DienThoai,TenDangNhap,MatKhauHash,Email
                                     FROM KhachHang WHERE TenDangNhap=@User",
                new SqlParameter("@User", tenDangNhap.Trim()));
            if (t.Rows.Count == 0) return KetQuaXuLy.Loi(sai);

            DataRow r = t.Rows[0];
            if (!KiemTraMatKhau(matKhau, Convert.ToString(r["MatKhauHash"]))) return KetQuaXuLy.Loi(sai);

            KhachHang kh = new KhachHang
            {
                MaKhachHang = Convert.ToString(r["MaKhachHang"]),
                HoTen = Convert.ToString(r["HoTen"]),
                NgaySinh = Convert.ToDateTime(r["NgaySinh"]),
                SoGiayTo = Convert.ToString(r["SoGiayTo"]),
                DiaChi = Convert.ToString(r["DiaChi"]),
                DienThoai = Convert.ToString(r["DienThoai"]),
                TenDangNhap = Convert.ToString(r["TenDangNhap"]),
                Email = r["Email"] == DBNull.Value ? null : Convert.ToString(r["Email"])
            };
            return KetQuaXuLy.Ok("Đăng nhập thành công.", kh);
        }

        // ---- Băm mật khẩu PBKDF2: "vòng lặp.salt.hash" ----
        private static string BamMatKhau(string matKhau)
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash = Pbkdf2(matKhau, salt, SoVongLap);
            return SoVongLap + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        private static bool KiemTraMatKhau(string matKhau, string luu)
        {
            try
            {
                string[] p = luu.Split('.');
                if (p.Length != 3) return false;
                int vong = int.Parse(p[0]);
                byte[] salt = Convert.FromBase64String(p[1]);
                byte[] goc = Convert.FromBase64String(p[2]);
                byte[] thu = Pbkdf2(matKhau, salt, vong);
                if (thu.Length != goc.Length) return false;
                int khac = 0;
                for (int i = 0; i < thu.Length; i++) khac |= thu[i] ^ goc[i];
                return khac == 0;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] Pbkdf2(string matKhau, byte[] salt, int vong)
        {
            using (Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(matKhau, salt, vong, HashAlgorithmName.SHA256))
                return kdf.GetBytes(32);
        }
    }
}
