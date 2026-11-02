using System;
using System.Collections.Generic;
using SG.Core.Models;

namespace SG.Core.Licensing
{
    /// <summary>Two 256-bit keys used to encrypt the local license file. Supplied by the host app, never hardcoded here.</summary>
    public sealed class LicenseKeys
    {
        public byte[] CryptKey { get; private set; }
        public byte[] AuthKey { get; private set; }

        public LicenseKeys(byte[] cryptKey, byte[] authKey)
        {
            if (cryptKey == null || cryptKey.Length != 32) throw new ArgumentException("32-byte key required", "cryptKey");
            if (authKey == null || authKey.Length != 32) throw new ArgumentException("32-byte key required", "authKey");
            CryptKey = cryptKey; AuthKey = authKey;
        }
    }

    /// <summary>Server-side list of subscribers (the old WUsers table).</summary>
    public interface ISubscriberDirectory
    {
        /// <summary>Rows matching the user name and, when given, the hashed password. Null filters are ignored.</summary>
        IList<SGSubscribe> Find(string userName, string xPassword);
    }

    /// <summary>A date source that can't be faked by changing the PC clock.</summary>
    public interface ITrustedClock
    {
        /// <summary>Current date, or null if it can't be determined right now (offline).</summary>
        DateTime? TryGetNow();
    }

    /// <summary>Reads the machine's hardware identifiers.</summary>
    public interface IHardwareProvider
    {
        Hardware Read();
    }
}
