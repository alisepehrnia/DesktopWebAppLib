using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using SG.Core.Models;
using SG.Core.Security;
using SG.Core.Sources;

namespace SG.Core.Licensing
{
    /// <summary>
    /// Subscriber table access. Uses query parameters; the old Get.GetSubscribes built the SQL by string
    /// concatenation from the typed user name, which is injectable.
    /// </summary>
    public sealed class DbSubscriberDirectory : ISubscriberDirectory
    {
        private static readonly Regex Ident = new Regex(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);
        private readonly Func<IDbConnection> _connect;
        private readonly string _table;

        public DbSubscriberDirectory(Func<IDbConnection> connect, string table = "WUsers")
        {
            if (connect == null) throw new ArgumentNullException("connect");
            if (!Ident.IsMatch(table ?? "")) throw new ArgumentException("bad table name", "table");
            _connect = connect; _table = table;
        }

        public IList<SGSubscribe> Find(string userName, string xPassword)
        {
            var rows = new List<SGSubscribe>();
            using (IDbConnection c = _connect())
            {
                c.Open();
                using (IDbCommand cmd = c.CreateCommand())
                {
                    string sql = "SELECT * FROM " + _table;
                    var where = new List<string>();
                    if (!string.IsNullOrEmpty(userName)) { where.Add("Username=@u"); Add(cmd, "@u", userName); }
                    if (!string.IsNullOrEmpty(xPassword)) { where.Add("Password=@p"); Add(cmd, "@p", xPassword); }
                    if (where.Count > 0) sql += " WHERE " + string.Join(" AND ", where.ToArray());
                    cmd.CommandText = sql;

                    using (IDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            rows.Add(new SGSubscribe
                            {
                                ID = r.Int("ID"), Name = r.Str("Name"), UserName = r.Str("Username"),
                                Password = r.Str("Password"), Hardware = r.Str("Hardware"), ExpireDate = r.Str("ExpireDate")
                            });
                }
            }
            return rows;
        }

        /// <summary>
        /// Records a new subscription (old Online.RecordNewSubscribe). The password is stored hashed (XHash);
        /// the fail-log variant of the old code stored it in clear text, so don't do that - pass a failed
        /// attempt to <paramref name="table"/> "WUsersFails" with <paramref name="hashPassword"/> true as well.
        /// </summary>
        public void Record(string table, User user, int status, long refId, string auth, SGRegDate dates, bool hashPassword = true)
        {
            if (!Ident.IsMatch(table ?? "")) throw new ArgumentException("bad table name", "table");
            var cols = new[] { "Name", "Phone", "Username", "Password", "CPU", "BIOS", "Motherboard", "Hardware", "RegisterDate", "ExpireDate", "Status", "RefID", "Auth" };
            var vals = new object[]
            {
                user.Name, user.Phone, user.UserName, hashPassword ? Cryptography.XHash(user.Password) : user.Password,
                Cryptography.Fingerprint(user.Hardware.CPU), Cryptography.Fingerprint(user.Hardware.BIOS),
                Cryptography.Fingerprint(user.Hardware.Motherboard), Cryptography.Fingerprint(user.Hardware),
                dates.RegisterDate, dates.ExpireDate, status, refId.ToString(), auth
            };

            using (IDbConnection c = _connect())
            {
                c.Open();
                using (IDbCommand cmd = c.CreateCommand())
                {
                    var names = new string[cols.Length];
                    for (int i = 0; i < cols.Length; i++) { names[i] = "@v" + i; Add(cmd, names[i], vals[i]); }
                    cmd.CommandText = "INSERT INTO " + table + " (" + string.Join(",", cols) + ") VALUES (" + string.Join(",", names) + ")";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void Add(IDbCommand cmd, string name, object value)
        {
            IDbDataParameter p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
