using System;

namespace SG.Core.Sync
{
    /// <summary>How the host app shows messages (MessageBox, toast, label...). Keeps the library UI-free.</summary>
    public interface IUserNotifier
    {
        void Info(string message);
        void Error(string message);
    }

    public sealed class RefreshMessages
    {
        public string NoInternet = "No internet connection.";
        public string Busy = "An update is already running. Try again in a few seconds.";
        public string Done = "Updated.";
        public string Failed = "Update failed.";
    }

    /// <summary>
    /// The "Update" button policy that was pasted into Education, Laws, FAQ, Import, Export:
    /// check connection, check busy, show spinner, refresh, hide spinner, tell the user.
    /// </summary>
    public static class UserRefresh
    {
        /// <param name="started">Show the "updating..." indicator.</param>
        /// <param name="finished">Hide the indicator and re-bind the grid (called on success and failure).</param>
        /// <returns>true if a refresh was started.</returns>
        public static bool Run<T>(SyncedTable<T> table, IUserNotifier notifier, RefreshMessages messages,
                                  Action started = null, Action<RefreshResult> finished = null)
        {
            if (messages == null) messages = new RefreshMessages();

            if (!Connectivity.IsOnline) { notifier.Error(messages.NoInternet); return false; }

            if (started != null) started();
            bool ok = table.BeginRefresh(result =>
            {
                if (finished != null) finished(result);
                if (result.Success) notifier.Info(messages.Done);
                else notifier.Error(messages.Failed);
            });

            if (!ok)
            {
                if (finished != null) finished(new RefreshResult { Success = false });
                notifier.Error(messages.Busy);
            }
            return ok;
        }
    }
}
