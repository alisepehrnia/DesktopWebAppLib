using System;
using System.Collections.Generic;

namespace SG.Core.Sources
{
    /// <summary>Anything that can produce a fresh list of rows (database, RSS, REST...).</summary>
    public interface ITableSource<T>
    {
        /// <summary>Blocking. Called on a background thread by SyncedTable. May throw.</summary>
        IList<T> Fetch();
    }

    /// <summary>Wraps an arbitrary function; use for one-off sources such as a two-query join.</summary>
    public sealed class DelegateSource<T> : ITableSource<T>
    {
        private readonly Func<IList<T>> _fetch;

        public DelegateSource(Func<IList<T>> fetch)
        {
            if (fetch == null) throw new ArgumentNullException("fetch");
            _fetch = fetch;
        }

        public IList<T> Fetch() { return _fetch(); }
    }
}
