using System;
using System.Diagnostics;

namespace SG.Core
{
    public static class Shell
    {
        /// <summary>
        /// Opens a link in the default browser. Only http/https links are allowed: links come from
        /// remote tables/feeds, and Process.Start would happily launch a local executable otherwise.
        /// </summary>
        /// <returns>true if the browser was launched.</returns>
        public static bool OpenUrl(string url)
        {
            Uri uri;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                CoreLog.Write("Shell.OpenUrl rejected: " + url);
                return false;
            }
            try
            {
                Process.Start(uri.AbsoluteUri);
                return true;
            }
            catch (Exception ex)
            {
                CoreLog.Write("Shell.OpenUrl failed: " + url, ex);
                return false;
            }
        }
    }
}
