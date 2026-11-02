using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SG.Core.Storage
{
    /// <summary>Outcome of reading an offline copy. Numeric values match the old Functions.LoadTable codes.</summary>
    public enum CacheLoadResult
    {
        Loaded = 1,
        Empty = 0,
        Corrupt = -1,
        Missing = -2
    }

    /// <summary>
    /// One offline table on disk: JSON, optionally encrypted. Replaces Functions.LoadTable and the
    /// "serialize + encrypt + WriteAllText" tail that was copy-pasted into every Get* method.
    /// </summary>
    public sealed class LocalTable<T>
    {
        private readonly ICipher _cipher;
        private readonly object _gate = new object();

        public string Path { get; private set; }

        public LocalTable(string path, ICipher cipher = null)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path required", "path");
            Path = path;
            _cipher = cipher ?? new PlainTextCipher();
        }

        /// <summary>Never throws and never returns a null list.</summary>
        public CacheLoadResult Load(out List<T> items)
        {
            items = new List<T>();
            if (!File.Exists(Path))
            {
                CoreLog.Write("LocalTable missing: " + Path);
                return CacheLoadResult.Missing;
            }

            try
            {
                string raw;
                lock (_gate) raw = File.ReadAllText(Path);
                string json = _cipher.Decrypt(raw);
                var parsed = json == null ? null : JsonConvert.DeserializeObject<List<T>>(json);
                if (parsed == null) return CacheLoadResult.Corrupt;
                items = parsed;
            }
            catch (Exception ex)
            {
                CoreLog.Write("LocalTable corrupt: " + Path, ex);
                return CacheLoadResult.Corrupt;
            }

            return items.Count > 0 ? CacheLoadResult.Loaded : CacheLoadResult.Empty;
        }

        /// <summary>Writes via a temp file so a power cut mid-save can't destroy the previous good copy.</summary>
        public void Save(IEnumerable<T> items)
        {
            string json = JsonConvert.SerializeObject(items, Formatting.None);
            string data = _cipher.Encrypt(json);
            string tmp = Path + ".tmp";

            lock (_gate)
            {
                File.WriteAllText(tmp, data);
                if (File.Exists(Path)) File.Replace(tmp, Path, null);
                else File.Move(tmp, Path);
            }
        }
    }
}
