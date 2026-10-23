using System.Data;
using System.Data.SqlClient;

namespace QuanLyNhanSu.Data
{
    // Thực thi lệnh SQL trong cùng một SqlTransaction (dùng cho nghiệp vụ nhiều bước)
    public static class DbTx
    {
        public static int Execute(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable Query(SqlConnection cn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                DataTable table = new DataTable();
                da.Fill(table);
                return table;
            }
        }
    }
}
