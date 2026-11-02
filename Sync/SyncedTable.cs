using System;
using System.Collections.Generic;
using System.Threading;
using SG.Core.Sources;
using SG.Core.Storage;

namespace SG.Core.Sync
{
    public sealed class RefreshResult
    {
        public bool Success { get; internal set; }
        public Exception Error { get; internal set; }
    }

    public sealed class RefreshFailedEventArgs : EventArgs
    {
        public Exception Error { get; internal set; }
    }

    /// <summary>
    /// "Show the offline copy now, refresh from the server in the background, save the new copy."
    /// This is the pattern every page repeated by hand with a BackgroundWorker, a List&lt;T&gt; passed by ref,
    /// and a Load/Update_Click/RunWorkerCompleted trio.
    ///
    /// Create it on the UI thread: events and callbacks are marshalled back to the creating
    /// SynchronizationContext, so handlers can touch controls directly.
    /// </summary>
    public sealed class SyncedTable<T>
    {
        private readonly ITableSource<T> _source;
        private readonly LocalTable<T> _cache;
        private readonly Func<IList<T>, IList<T>> _postProcess;
        private readonly SynchronizationContext _ui;
        private int _busy;
        private volatile IList<T> _items = new List<T>();

        /// <summary>Raised (on the UI context) after a successful refresh.</summary>
        public event EventHandler Refreshed;
        /// <summary>Raised (on the UI context) when a refresh fails. Existing items are kept.</summary>
        public event EventHandler<RefreshFailedEventArgs> RefreshFailed;

        /// <param name="postProcess">Optional step run on the worker thread before saving, e.g. flag expired slides.</param>
        public SyncedTable(ITableSource<T> source, LocalTable<T> cache, Func<IList<T>, IList<T>> postProcess = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            _source = source;
            _cache = cache;
            _postProcess = postProcess;
            _ui = SynchronizationContext.Current;
        }

        /// <summary>Latest rows. The list is replaced as a whole after a refresh, never mutated in place.</summary>
        public IList<T> Items { get { return _items; } }

        public bool IsRefreshing { get { return Thread.VolatileRead(ref _busy) == 1; } }

        /// <summary>Loads the offline copy synchronously. Returns how it went.</summary>
        public CacheLoadResult LoadCache()
        {
            if (_cache == null) return CacheLoadResult.Missing;
            List<T> loaded;
            CacheLoadResult result = _cache.Load(out loaded);
            if (result == CacheLoadResult.Loaded) _items = loaded;
            return result;
        }

        /// <summary>
        /// Starts a background refresh. Returns false (and does nothing) if one is already running.
        /// <paramref name="completed"/> is a per-call callback, so repeated clicks never stack handlers.
        /// </summary>
        public bool BeginRefresh(Action<RefreshResult> completed = null)
        {
            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0) return false;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var result = new RefreshResult();
                try
                {
                    IList<T> fresh = _source.Fetch() ?? new List<T>();
                    if (_postProcess != null) fresh = _postProcess(fresh) ?? fresh;
                    if (_cache != null) _cache.Save(fresh);
                    _items = fresh;
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    CoreLog.Write("SyncedTable refresh failed", ex);
                    result.Error = ex;
                }
                finally
                {
                    Interlocked.Exchange(ref _busy, 0);
                }

                Post(() =>
                {
                    if (result.Success) Raise(Refreshed);
                    else { var h = RefreshFailed; if (h != null) h(this, new RefreshFailedEventArgs { Error = result.Error }); }
                    if (completed != null) completed(result);
                });
            });
            return true;
        }

        private void Raise(EventHandler h) { if (h != null) h(this, EventArgs.Empty); }

        private void Post(Action a)
        {
            if (_ui != null) _ui.Post(_ => a(), null);
            else a();
        }
    }
}
