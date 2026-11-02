using System.Net.NetworkInformation;

namespace SG.Core
{
    public static class Connectivity
    {
        /// <summary>True when any network interface is up. (Does not prove the internet is reachable.)</summary>
        public static bool IsOnline
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }
    }
}
