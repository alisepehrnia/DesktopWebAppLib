using System;

namespace SG.Core
{
    /// <summary>
    /// Logging hook. The library has no logging dependency; the host app plugs its own
    /// logger in once at startup, e.g. <c>CoreLog.Sink = (msg, ex) =&gt; logger.Debug(ex, msg);</c>
    /// </summary>
    public static class CoreLog
    {
        public static Action<string, Exception> Sink;

        internal static void Write(string message, Exception ex = null)
        {
            try { Sink?.Invoke(message, ex); } catch { /* logging must never break the kiosk */ }
        }
    }
}
