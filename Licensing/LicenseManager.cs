using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SG.Core.Models;
using SG.Core.Security;

namespace SG.Core.Licensing
{
    public sealed class LicenseCheck
    {
        public AuthStatus Status { get; internal set; }
        /// <summary>Subscriber display name when known.</summary>
        public string Name { get; internal set; }
        public bool IsAuthorized { get { return Status == AuthStatus.Authorized; } }
    }

    /// <summary>
    /// Replaces Subscribe.Auth / Subscribe.IO: sign in, "forgot", offline check at startup, online re-check.
    /// All I/O (server, clock, hardware, keys, file path) is injected, so it is testable and has no UI.
    /// </summary>
    public sealed class LicenseManager
    {
        private readonly ISubscriberDirectory _directory;
        private readonly ITrustedClock _clock;
        private readonly IHardwareProvider _hardware;
        private readonly LicenseKeys _keys;
        private readonly string _path;

        public LicenseManager(string licenseFilePath, LicenseKeys keys, ISubscriberDirectory directory,
                              ITrustedClock clock = null, IHardwareProvider hardware = null)
        {
            if (string.IsNullOrWhiteSpace(licenseFilePath)) throw new ArgumentException("path required", "licenseFilePath");
            if (keys == null) throw new ArgumentNullException("keys");
            if (directory == null) throw new ArgumentNullException("directory");
            _path = licenseFilePath; _keys = keys; _directory = directory;
            _clock = clock ?? new HttpDateClock();
            _hardware = hardware ?? new WmiHardwareProvider();
        }

        public bool HasLicenseFile { get { return File.Exists(_path); } }

        /// <summary>Checks the password, hardware and expiry on the server. Authorized does NOT write the file; call Save.</summary>
        public LicenseCheck SignIn(string userName, string xPassword)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(xPassword))
                return Result(AuthStatus.WrongUserPass);
            return Verify(userName, xPassword, AuthStatus.UserNotFound);
        }

        /// <summary>Same as SignIn without a password (re-issuing a lost license file on the same machine).</summary>
        public LicenseCheck Forget(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName)) return Result(AuthStatus.WrongUserPass);
            return Verify(userName, null, AuthStatus.UserNotFound);
        }

        /// <summary>Startup check with no network: does the file belong to this machine?</summary>
        public LicenseCheck CheckOffline()
        {
            SubscribeFile file = Read();
            if (file == null) return Result(AuthStatus.FileNotFound);
            bool same = file.Fingerprint == WmiHardwareProvider.Fingerprint(_hardware.Read());
            return new LicenseCheck { Status = same ? AuthStatus.Authorized : AuthStatus.FingerprintNotMatch, Name = same ? file.Name : null };
        }

        /// <summary>Re-validates the saved license against the server. Deletes the file when expired (as before).</summary>
        public LicenseCheck CheckOnline()
        {
            SubscribeFile file = Read();
            if (file == null) return Result(AuthStatus.FileNotFound);
            if (file.UserName == null) { Delete(); return Result(AuthStatus.WrongUserPass); }

            var rows = _directory.Find(file.UserName, null);
            if (rows == null || rows.Count == 0) return Result(AuthStatus.WrongUserPass);

            SGSubscribe match = rows.FirstOrDefault(x => x.Hardware == file.Fingerprint);
            if (match == null) return Result(AuthStatus.FingerprintNotMatch);

            DateTime? now = _clock.TryGetNow();
            if (now == null) return new LicenseCheck { Status = AuthStatus.Authorized, Name = match.Name }; // can't tell: don't lock the user out
            if (PersianDate.IsExpired(match.ExpireDate, now.Value)) { Delete(); return Result(AuthStatus.TimeExpired); }
            return new LicenseCheck { Status = AuthStatus.Authorized, Name = match.Name };
        }

        /// <summary>Writes the encrypted license file for this machine (old IO.WriteSubscribeFile).</summary>
        public void Save(string userName, string name, bool expired = false)
        {
            var file = new SubscribeFile
            {
                UserName = userName, Name = name, Expired = expired,
                Fingerprint = WmiHardwareProvider.Fingerprint(_hardware.Read())
            };
            File.WriteAllText(_path, Cryptography.ENC(JsonConvert.SerializeObject(file, Formatting.None), _keys.CryptKey, _keys.AuthKey));
        }

        /// <summary>Overwrites the file with a blank, expired license (old behaviour when the server rejects it).</summary>
        public void Revoke(string name) { Save(null, name, true); }

        public void Delete() { try { if (File.Exists(_path)) File.Delete(_path); } catch (Exception) { } }

        private LicenseCheck Verify(string userName, string xPassword, AuthStatus whenNoRows)
        {
            string fp = WmiHardwareProvider.Fingerprint(_hardware.Read());
            var rows = _directory.Find(userName, xPassword);
            if (rows == null || rows.Count == 0) return Result(whenNoRows);

            SGSubscribe match = rows.FirstOrDefault(x => x.Hardware == fp);
            if (match == null) return Result(AuthStatus.FingerprintNotMatch);

            DateTime? now = _clock.TryGetNow();
            if (now == null) return Result(AuthStatus.TimeExpired); // sign-in needs a trustworthy date
            if (PersianDate.IsExpired(match.ExpireDate, now.Value)) return Result(AuthStatus.TimeExpired);
            return new LicenseCheck { Status = AuthStatus.Authorized, Name = match.Name };
        }

        private SubscribeFile Read()
        {
            if (!File.Exists(_path)) return null;
            try
            {
                string json = Cryptography.DEC(File.ReadAllText(_path), _keys.CryptKey, _keys.AuthKey);
                return json == null ? null : JsonConvert.DeserializeObject<SubscribeFile>(json);
            }
            catch (Exception ex)
            {
                SG.Core.CoreLog.Sink?.Invoke("License file unreadable", ex);
                return null;
            }
        }

        private static LicenseCheck Result(AuthStatus s) { return new LicenseCheck { Status = s }; }
    }
}
