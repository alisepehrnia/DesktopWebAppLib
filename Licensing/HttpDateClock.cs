using System;
using System.Globalization;
using System.Net;

namespace SG.Core.Licensing
{
    /// <summary>
    /// Takes "now" from the Date header of an HTTP response (the old app used google.com). Unlike the old
    /// loop, it gives up after a few attempts and returns null instead of spinning forever when offline.
    /// </summary>
    public sealed class HttpDateClock : ITrustedClock
    {
        private readonly string _url;
        private readonly int _attempts;

        public HttpDateClock(string url = "http://www.google.com", int attempts = 3)
        {
            _url = url; _attempts = attempts < 1 ? 1 : attempts;
        }

        public DateTime? TryGetNow()
        {
            for (int i = 0; i < _attempts; i++)
            {
                try
                {
                    var req = WebRequest.Create(new Uri(_url));
                    req.Timeout = 8000;
                    using (WebResponse resp = req.GetResponse())
                    {
                        return DateTime.ParseExact(resp.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                            CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
                    }
                }
                catch (Exception ex) { SG.Core.CoreLog.Sink?.Invoke("HttpDateClock attempt failed", ex); }
            }
            return null;
        }
    }
}
