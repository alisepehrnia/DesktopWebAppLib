using System;

namespace SG.Core.Storage
{
    public interface ICipher
    {
        string Encrypt(string plainText);
        /// <summary>Returns the plain text, or throws / returns null if the data is corrupt or tampered.</summary>
        string Decrypt(string cipherText);
    }

    /// <summary>No encryption (tests, non-sensitive data).</summary>
    public sealed class PlainTextCipher : ICipher
    {
        public string Encrypt(string plainText) { return plainText; }
        public string Decrypt(string cipherText) { return cipherText; }
    }

    /// <summary>Adapts any existing encrypt/decrypt pair, e.g. the SG library's Cryptography.ENC/DEC.</summary>
    public sealed class DelegateCipher : ICipher
    {
        private readonly Func<string, string> _enc, _dec;

        public DelegateCipher(Func<string, string> encrypt, Func<string, string> decrypt)
        {
            if (encrypt == null) throw new ArgumentNullException("encrypt");
            if (decrypt == null) throw new ArgumentNullException("decrypt");
            _enc = encrypt; _dec = decrypt;
        }

        public string Encrypt(string plainText) { return _enc(plainText); }
        public string Decrypt(string cipherText) { return _dec(cipherText); }
    }
}
