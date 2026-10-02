using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using eShopping.Adapters;
using eShopping.Data;

namespace eShopping.Services
{
    public class DatHangService
    {
        private readonly IProductSystemAdapter sanPham;
        private readonly IPaymentGateway thanhToan;
        private readonly IEmailService email;

        public DatHangService()
            : this(new ProductSystemAdapter(), new MockPaymentGateway(), new MockEmailService()) { }

        public DatHangService(IProductSystemAdapter sanPham, IPaymentGateway thanhToan, IEmailService email)
        {
            this.sanPham = sanPham;
            this.thanhToan = thanhToan;
            this.email = email;
        }

        // ---------- Danh mục cho giao diện ----------
        public DataTable LayKhuVuc()
        {
            return Db.Query("SELECT MaKhuVuc, TenKhuVuc FROM KhuVucGiaoHang ORDER BY TenKhuVuc");
        }

        public DataTable LayLoaiGiaoHang()
        {
            return Db.Query(@"SELECT MaLoaiGiaoHang, TenLoai + N' (' + MoTaThoiGianXuLy + N')' AS HienThi
                              FROM LoaiGiaoHang ORDER BY ThuTu");
        }

        public DataTable LayLoaiThe()
        {
            return Db.Query("SELECT MaLoaiThe, TenLoaiThe FROM LoaiThe ORDER BY TenLoaiThe");
        }

        // ---------- Tính tiền ----------
        // Miễn phí giao khi TỔNG TIỀN HÀNG >= ngưỡng của loại giao hàng đã chọn.
        public KetQuaTinhTien TinhTien(decimal tongTienHang, string maKhuVuc, string maLoaiGiaoHang, string maLoaiThe)
        {
            KetQuaTinhTien kq = new KetQuaTinhTien { TongTienHang = tongTienHang };

            if (!string.IsNullOrEmpty(maLoaiGiaoHang))
            {
                DataTable lg = Db.Query("SELECT NguongMienPhi FROM LoaiGiaoHang WHERE MaLoaiGiaoHang=@Ma",
                    new SqlParameter("@Ma", maLoaiGiaoHang));
                if (lg.Rows.Count > 0)
                {
                    object nguong = lg.Rows[0]["NguongMienPhi"];
                    if (nguong != DBNull.Value && tongTienHang >= Convert.ToDecimal(nguong))
                    {
                        kq.MienPhiGiaoHang = true;
                    }
                    else if (!string.IsNullOrEmpty(maKhuVuc))
                    {
                        object phi = Db.Scalar("SELECT PhiGiaoHang FROM BangGiaGiaoHang WHERE MaKhuVuc=@KV AND MaLoaiGiaoHang=@LG",
                            new SqlParameter("@KV", maKhuVuc), new SqlParameter("@LG", maLoaiGiaoHang));
                        if (phi == null) kq.ThieuBangGia = true;
                        else kq.PhiGiaoHang = Convert.ToDecimal(phi);
                    }
                }
            }

            if (!string.IsNullOrEmpty(maLoaiThe))
            {
                object lp = Db.Scalar("SELECT LePhiGiaoDich FROM LoaiThe WHERE MaLoaiThe=@Ma", new SqlParameter("@Ma", maLoaiThe));
                kq.LePhiThe = lp == null ? 0 : Convert.ToDecimal(lp);
            }
            return kq;
        }

        // ---------- Đặt hàng ----------
        public KetQuaXuLy DatHang(KhachHang kh, GioHang gh, NguoiNhan nn, string maLoaiGiaoHang, TheTinDung the)
        {
            if (kh == null) return KetQuaXuLy.Loi("Vui lòng đăng nhập trước khi đặt hàng.");
            if (gh == null || gh.Rong) return KetQuaXuLy.Loi("Giỏ hàng đang trống.");
            if (nn == null || string.IsNullOrWhiteSpace(nn.HoTen) || string.IsNullOrWhiteSpace(nn.DiaChi) ||
                string.IsNullOrWhiteSpace(nn.DienThoai) || string.IsNullOrWhiteSpace(nn.MaKhuVuc))
                return KetQuaXuLy.Loi("Vui lòng nhập đầy đủ họ tên, địa chỉ, điện thoại và khu vực của người nhận.");
            if (string.IsNullOrWhiteSpace(maLoaiGiaoHang)) return KetQuaXuLy.Loi("Vui lòng chọn loại phiếu đặt hàng.");

            // 1) Đối chiếu lại với Hệ thống quản lý sản phẩm: giá bán có thể đổi, hàng có thể hết.
            List<string> thayDoi = new List<string>();
            foreach (MucGioHang m in gh.Muc.ToList())
            {
                SanPham sp = sanPham.LayChiTiet(m.MaSanPham);
                if (sp == null || !sp.ConHang)
                {
                    thayDoi.Add("\"" + m.TenSanPham + "\" đã hết hàng nên bị bỏ khỏi giỏ.");
                    gh.Muc.Remove(m);
                }
                else if (sp.GiaBan != m.DonGia)
                {
                    thayDoi.Add("\"" + m.TenSanPham + "\" đổi giá từ " + m.DonGia.ToString("N0") + " thành " + sp.GiaBan.ToString("N0") + ".");
                    m.DonGia = sp.GiaBan;
                }
            }
            if (thayDoi.Count > 0)
                return KetQuaXuLy.Loi("Giỏ hàng có thay đổi:\n- " + string.Join("\n- ", thayDoi) + "\nVui lòng kiểm tra lại rồi đặt hàng.");

            // 2) Kiểm tra định dạng thẻ theo loại thẻ
            KetQuaXuLy kqThe = KiemTraDinhDangThe(the);
            if (!kqThe.ThanhCong) return kqThe;

            // 3) Tính tiền
            KetQuaTinhTien tinh = TinhTien(gh.TongTienHang, nn.MaKhuVuc, maLoaiGiaoHang, the.MaLoaiThe);
            if (tinh.ThieuBangGia) return KetQuaXuLy.Loi("Chưa có bảng giá giao hàng cho khu vực và loại giao hàng đã chọn.");

            // 4) Dịch vụ thanh toán trực tuyến (ngoài): thẻ hợp lệ + đủ khả năng thanh toán
            KetQuaThanhToan tt = thanhToan.XacThuc(the, tinh.TongTriGia);
            if (tt == null || !tt.HopLe)
                return KetQuaXuLy.Loi(tt == null ? "Không nhận được phản hồi từ dịch vụ thanh toán." : tt.ThongBao);

            // 5) Ghi nhận đơn hàng trong một transaction
            string hau = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string maDH = "DH" + hau, maNN = "NN" + hau, maTT = "TT" + hau;
            DateTime thoiDiemDat = DateTime.Now;
            string soThe = the.SoThe;
            string soTheCuoi = soThe.Substring(soThe.Length - 4);

            using (SqlConnection cn = Db.OpenConnection())
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    Exec(cn, tx, @"INSERT INTO NguoiNhan(MaNguoiNhan,HoTen,DiaChi,DienThoai,MaKhuVuc) VALUES(@Ma,@Ten,@DC,@DT,@KV)",
                        new SqlParameter("@Ma", maNN), new SqlParameter("@Ten", nn.HoTen.Trim()),
                        new SqlParameter("@DC", nn.DiaChi.Trim()), new SqlParameter("@DT", nn.DienThoai.Trim()),
                        new SqlParameter("@KV", nn.MaKhuVuc));

                    Exec(cn, tx, @"INSERT INTO DonHang(MaDonHang,MaKhachHang,MaNguoiNhan,MaLoaiGiaoHang,ThoiDiemDat,TongTienHang,PhiGiaoHang,LePhiThe,TongTriGia)
                                   VALUES(@Ma,@KH,@NN,@LG,@Gio,@TH,@PG,@LP,@TG)",
                        new SqlParameter("@Ma", maDH), new SqlParameter("@KH", kh.MaKhachHang),
                        new SqlParameter("@NN", maNN), new SqlParameter("@LG", maLoaiGiaoHang),
                        new SqlParameter("@Gio", thoiDiemDat), new SqlParameter("@TH", tinh.TongTienHang),
                        new SqlParameter("@PG", tinh.PhiGiaoHang), new SqlParameter("@LP", tinh.LePhiThe),
                        new SqlParameter("@TG", tinh.TongTriGia));

                    int i = 1;
                    foreach (MucGioHang m in gh.Muc)
                    {
                        Exec(cn, tx, @"INSERT INTO ChiTietDonHang(MaChiTiet,MaDonHang,MaSanPham,TenSanPham,SoLuong,DonGia)
                                       VALUES(@Ma,@DH,@SP,@Ten,@SL,@Gia)",
                            new SqlParameter("@Ma", maDH + "_" + i.ToString("00")), new SqlParameter("@DH", maDH),
                            new SqlParameter("@SP", m.MaSanPham), new SqlParameter("@Ten", m.TenSanPham),
                            new SqlParameter("@SL", m.SoLuong), new SqlParameter("@Gia", m.DonGia));
                        i++;
                    }

                    // Chỉ lưu 4 số cuối; số thẻ đầy đủ và CSV không bao giờ được ghi xuống CSDL.
                    Exec(cn, tx, @"INSERT INTO TheThanhToan(MaThanhToan,MaDonHang,MaLoaiThe,SoTheCuoi,TenChuThe,NgayHetHan,MaXacNhanNgoai,SoTien)
                                   VALUES(@Ma,@DH,@Loai,@Cuoi,@Chu,@Han,@XN,@Tien)",
                        new SqlParameter("@Ma", maTT), new SqlParameter("@DH", maDH),
                        new SqlParameter("@Loai", the.MaLoaiThe), new SqlParameter("@Cuoi", soTheCuoi),
                        new SqlParameter("@Chu", the.TenChuThe.Trim()), new SqlParameter("@Han", the.NgayHetHan.Date),
                        new SqlParameter("@XN", tt.MaXacNhan), new SqlParameter("@Tien", tinh.TongTriGia));

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    try { tx.Rollback(); } catch { }
                    // Thực tế cần gọi dịch vụ thanh toán để hoàn/hủy giao dịch (bù trừ) ở bước này.
                    return KetQuaXuLy.Loi("Không thể ghi nhận đơn hàng: " + ex.Message);
                }
            }

            // 6) Email xác nhận (sau khi đã commit; lỗi email KHÔNG làm mất đơn hàng)
            string ghiChuMail = "";
            if (!string.IsNullOrWhiteSpace(kh.Email))
            {
                string tenLoaiGiao = Convert.ToString(Db.Scalar("SELECT TenLoai FROM LoaiGiaoHang WHERE MaLoaiGiaoHang=@Ma",
                    new SqlParameter("@Ma", maLoaiGiaoHang)));
                string noiDung = XayDungNoiDungEmail(maDH, thoiDiemDat, kh, nn, tenLoaiGiao, gh, tinh);
                KetQuaXuLy mail = email.Gui(maDH, kh.Email, "Xác nhận đơn hàng " + maDH, noiDung);
                if (mail.ThanhCong)
                    Db.Execute("UPDATE DonHang SET EmailDaGui=1 WHERE MaDonHang=@Ma", new SqlParameter("@Ma", maDH));
                else
                    ghiChuMail = "\n(Chưa gửi được email xác nhận: " + mail.ThongBao + ")";
            }

            return KetQuaXuLy.Ok("Đặt hàng thành công. Mã đơn hàng: " + maDH + "\nTổng thanh toán: " + tinh.TongTriGia.ToString("N0") + " đ" + ghiChuMail, maDH);
        }

        // ---------- Hỗ trợ ----------
        private KetQuaXuLy KiemTraDinhDangThe(TheTinDung the)
        {
            if (the == null || string.IsNullOrWhiteSpace(the.MaLoaiThe)) return KetQuaXuLy.Loi("Vui lòng chọn loại thẻ.");

            DataTable lt = Db.Query("SELECT TenLoaiThe, SoChuSoThe, SoChuSoCSV FROM LoaiThe WHERE MaLoaiThe=@Ma",
                new SqlParameter("@Ma", the.MaLoaiThe));
            if (lt.Rows.Count == 0) return KetQuaXuLy.Loi("Loại thẻ không hợp lệ.");

            string ten = Convert.ToString(lt.Rows[0]["TenLoaiThe"]);
            int soChuSo = Convert.ToInt32(lt.Rows[0]["SoChuSoThe"]);
            int soCsv = Convert.ToInt32(lt.Rows[0]["SoChuSoCSV"]);

            string so = (the.SoThe ?? "").Replace(" ", "").Replace("-", "");
            if (!ToanChuSo(so) || so.Length != soChuSo)
                return KetQuaXuLy.Loi("Số thẻ " + ten + " phải gồm đúng " + soChuSo + " chữ số.");

            string csv = (the.CSV ?? "").Trim();
            if (!ToanChuSo(csv) || csv.Length != soCsv)
                return KetQuaXuLy.Loi("Mã an ninh (CSV) của thẻ " + ten + " phải gồm đúng " + soCsv + " chữ số.");

            if (string.IsNullOrWhiteSpace(the.TenChuThe)) return KetQuaXuLy.Loi("Vui lòng nhập họ tên chủ thẻ.");
            if (the.NgayHetHan.Date < DateTime.Today) return KetQuaXuLy.Loi("Thẻ đã hết hạn sử dụng.");

            the.SoThe = so;
            the.CSV = csv;
            return KetQuaXuLy.Ok("Thông tin thẻ hợp lệ về định dạng.");
        }

        private static bool ToanChuSo(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s) if (c < '0' || c > '9') return false;
            return true;
        }

        private static void Exec(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] ps)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                cmd.Parameters.AddRange(ps);
                cmd.ExecuteNonQuery();
            }
        }

        // Nội dung email KHÔNG chứa bất kỳ thông tin thẻ tín dụng nào.
        private static string XayDungNoiDungEmail(string maDH, DateTime thoiDiem, KhachHang kh, NguoiNhan nn,
                                                  string tenLoaiGiao, GioHang gh, KetQuaTinhTien tinh)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Kính gửi " + kh.HoTen + ",");
            sb.AppendLine("Cửa hàng ABC xác nhận đã nhận đơn đặt hàng của quý khách.");
            sb.AppendLine();
            sb.AppendLine("Mã đơn hàng : " + maDH);
            sb.AppendLine("Thời điểm đặt: " + thoiDiem.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine("Hình thức giao: " + tenLoaiGiao);
            sb.AppendLine("Người nhận   : " + nn.HoTen + " - " + nn.DienThoai);
            sb.AppendLine("Địa chỉ nhận : " + nn.DiaChi);
            sb.AppendLine();
            sb.AppendLine("Sản phẩm:");
            foreach (MucGioHang m in gh.Muc)
                sb.AppendLine("- " + m.TenSanPham + " x" + m.SoLuong + " @ " + m.DonGia.ToString("N0") + " = " + m.ThanhTien.ToString("N0") + " đ");
            sb.AppendLine();
            sb.AppendLine("Tiền hàng      : " + tinh.TongTienHang.ToString("N0") + " đ");
            sb.AppendLine("Phí giao hàng  : " + (tinh.MienPhiGiaoHang ? "Miễn phí" : tinh.PhiGiaoHang.ToString("N0") + " đ"));
            sb.AppendLine("Lệ phí thanh toán: " + tinh.LePhiThe.ToString("N0") + " đ");
            sb.AppendLine("TỔNG TRỊ GIÁ   : " + tinh.TongTriGia.ToString("N0") + " đ");
            return sb.ToString();
        }
    }
}
