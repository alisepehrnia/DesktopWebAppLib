using System;
using System.Data;

namespace SG.Core.Sources
{
    /// <summary>Null-safe column readers; replace <c>reader["x"].ToString()</c> and <c>int.Parse(reader["id"].ToString())</c>.</summary>
    public static class DbRecordExtensions
    {
        public static string Str(this IDataRecord r, string column)
        {
            object v = r[column];
            return v == null || v is DBNull ? string.Empty : v.ToString();
        }

        public static int Int(this IDataRecord r, string column)
        {
            object v = r[column];
            return v == null || v is DBNull ? 0 : Convert.ToInt32(v);
        }

        public static bool Bool(this IDataRecord r, string column)
        {
            object v = r[column];
            return v != null && !(v is DBNull) && Convert.ToBoolean(v);
        }
    }
}
