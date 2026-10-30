using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLyVanPhong;

/// <summary>Lớp truy cập dữ liệu dùng chung. Sửa ConnStr nếu server của bạn khác.</summary>
public static class Db
{
    // Nếu dùng LocalDB:  Server=(localdb)\MSSQLLocalDB;Database=QuanLyVanPhong;Trusted_Connection=True;
    public static string ConnStr =
        @"Server=TANDAT;Database=QuanLyVanPhong;User Id=sa;Password=123;TrustServerCertificate=True;";

    public static SqlConnection Open()
    {
        var c = new SqlConnection(ConnStr);
        c.Open();
        return c;
    }

    public static SqlParameter P(string name, object value) => new SqlParameter(name, value ?? DBNull.Value);

    static SqlCommand Make(SqlConnection c, SqlTransaction t, string sql, SqlParameter[] ps)
    {
        var cmd = new SqlCommand(sql, c, t);
        cmd.Parameters.AddRange(ps);
        return cmd;
    }

    // ---- Dùng trong transaction (truyền connection + transaction) ----
    public static int Exec(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
    {
        using var cmd = Make(c, t, sql, ps);
        return cmd.ExecuteNonQuery();
    }

    public static object Scalar(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
    {
        using var cmd = Make(c, t, sql, ps);
        var r = cmd.ExecuteScalar();
        return r == DBNull.Value ? null : r;
    }

    public static DataTable Table(SqlConnection c, SqlTransaction t, string sql, params SqlParameter[] ps)
    {
        using var cmd = Make(c, t, sql, ps);
        using var da = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    // ---- Dùng nhanh (tự mở/đóng kết nối) ----
    public static int Exec(string sql, params SqlParameter[] ps)
    {
        try { using var c = Open(); return Exec(c, null, sql, ps); }
        catch (SqlException ex) { throw Translate(ex); }
    }

    public static object Scalar(string sql, params SqlParameter[] ps)
    {
        using var c = Open();
        return Scalar(c, null, sql, ps);
    }

    public static DataTable Table(string sql, params SqlParameter[] ps)
    {
        using var c = Open();
        return Table(c, null, sql, ps);
    }

    /// <summary>Đổi lỗi SQL thành thông báo dễ hiểu.</summary>
    public static Exception Translate(SqlException ex) => ex.Number switch
    {
        2627 or 2601 => new BusinessException("Dữ liệu bị trùng (vi phạm UNIQUE / PRIMARY KEY)."),
        547 => new BusinessException("Vi phạm khóa ngoại hoặc CHECK (ví dụ: phòng không thuộc hợp đồng này)."),
        _ => ex
    };
}
