using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace SG.Core.Sources
{
    /// <summary>
    /// "SELECT columns FROM table suffix" mapped row-by-row into T. One class instead of ~20 near-identical
    /// Get* methods; the only per-table code left is the row mapper.
    /// Database-agnostic: pass <c>() =&gt; new MySqlConnection(...)</c> (or SQL Server, SQLite...).
    /// </summary>
    public sealed class DbTableSource<T> : ITableSource<T>
    {
        private static readonly Regex Identifier = new Regex(@"^[A-Za-z0-9_\.`]+$", RegexOptions.Compiled);

        private readonly Func<IDbConnection> _connect;
        private readonly Func<IDataRecord, T> _map;
        private readonly string _sql;

        /// <param name="table">Developer-supplied table name (validated, never user input).</param>
        /// <param name="columns">Column list, "*" by default.</param>
        /// <param name="suffix">Trailing clause such as "ORDER BY ID DESC". Must be a constant, not user input.</param>
        public DbTableSource(Func<IDbConnection> connect, string table, Func<IDataRecord, T> map,
                             string columns = "*", string suffix = "")
        {
            if (connect == null) throw new ArgumentNullException("connect");
            if (map == null) throw new ArgumentNullException("map");
            if (string.IsNullOrWhiteSpace(table) || !Identifier.IsMatch(table))
                throw new ArgumentException("Invalid table name: " + table, "table");

            _connect = connect;
            _map = map;
            _sql = string.Format("SELECT {0} FROM {1} {2}", columns, table, suffix);
        }

        public IList<T> Fetch()
        {
            var rows = new List<T>();
            using (IDbConnection conn = _connect())
            {
                conn.Open();
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = _sql;
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) rows.Add(_map(reader));
                    }
                }
            }
            return rows;
        }
    }
}
