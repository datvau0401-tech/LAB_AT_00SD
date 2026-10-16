using System.Data;
using Microsoft.Data.SqlClient;

namespace BenhAn.Data;

/// <summary>Lỗi nghiệp vụ do stored procedure / trigger / ràng buộc báo về (mã 50xxx hoặc UNIQUE/CHECK/FK).</summary>
public class NghiepVuException : Exception
{
    public int MaLoi { get; }

    public NghiepVuException(string message, int maLoi, Exception? inner = null) : base(message, inner)
    {
        MaLoi = maLoi;
    }
}

/// <summary>Truy cập SQL Server: mọi nghiệp vụ nhiều bảng đều chạy trong stored procedure có transaction.</summary>
public static class Db
{
    public const string ChuoiKetNoiMacDinh =
        "Server=.;Database=QuanLyBenhAn;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

    public static string ConnectionString { get; set; } = ChuoiKetNoiMacDinh;

    public static SqlConnection Open()
    {
        var cn = new SqlConnection(ConnectionString);
        cn.Open();
        return cn;
    }

    public static SqlParameter P(string name, object? value, SqlDbType? type = null)
    {
        var p = new SqlParameter(name, value ?? DBNull.Value);
        if (type.HasValue) p.SqlDbType = type.Value;
        return p;
    }

    /// <summary>Tham số decimal có khai báo precision/scale (tránh bị làm tròn 37,8 thành 37).</summary>
    public static SqlParameter Dec(string name, decimal? value, byte precision, byte scale) =>
        new SqlParameter(name, SqlDbType.Decimal)
        {
            Precision = precision,
            Scale = scale,
            Value = value.HasValue ? (object)value.Value : DBNull.Value
        };

    public static SqlParameter Out(string name, SqlDbType type, int size = 0)
    {
        var p = new SqlParameter(name, type) { Direction = ParameterDirection.Output };
        if (size > 0) p.Size = size;
        return p;
    }

    private static SqlCommand Cmd(SqlConnection cn, string proc, SqlParameter[] ps)
    {
        var cmd = new SqlCommand(proc, cn) { CommandType = CommandType.StoredProcedure, CommandTimeout = 30 };
        foreach (var p in ps) cmd.Parameters.Add(p);
        return cmd;
    }

    public static DataTable Table(string proc, params SqlParameter[] ps)
    {
        try
        {
            using var cn = Open();
            using var cmd = Cmd(cn, proc, ps);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
        catch (SqlException ex)
        {
            throw Dich(ex);
        }
    }

    public static void Exec(string proc, params SqlParameter[] ps)
    {
        try
        {
            using var cn = Open();
            using var cmd = Cmd(cn, proc, ps);
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw Dich(ex);
        }
    }

    public static object? Scalar(string proc, params SqlParameter[] ps)
    {
        try
        {
            using var cn = Open();
            using var cmd = Cmd(cn, proc, ps);
            var v = cmd.ExecuteScalar();
            return v is DBNull ? null : v;
        }
        catch (SqlException ex)
        {
            throw Dich(ex);
        }
    }

    /// <summary>Đổi SqlException thành thông báo tiếng Việt dễ hiểu.</summary>
    public static Exception Dich(SqlException ex)
    {
        if (ex.Number >= 50000) return new NghiepVuException(ex.Message, ex.Number, ex);
        return ex.Number switch
        {
            2627 or 2601 => new NghiepVuException("Dữ liệu bị trùng (vi phạm ràng buộc duy nhất): " + ex.Message, ex.Number, ex),
            547 => new NghiepVuException("Dữ liệu không hợp lệ (vi phạm ràng buộc khóa ngoại/CHECK): " + ex.Message, ex.Number, ex),
            _ => ex
        };
    }
}
