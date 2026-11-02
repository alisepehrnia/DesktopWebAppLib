using Newtonsoft.Json;
using SG.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SG.Core.Security
{
    /// <summary>
    /// Provide SG Hash, Encrypt and Decrypt Functions
    /// </summary>
    public static class Cryptography
        {
            /// <summary>
            /// Make Hash String Of Provided Object JSON Serialization
            /// </summary>
            /// <param name="obj">Target Object</param>
            /// <returns></returns>
            public static string Fingerprint(object obj)
            {
                string json = JsonConvert.SerializeObject(obj, Formatting.None);
                string hash = XHash(json);
                json = null;
                return hash;
            }
            /// <summary>
            /// Inter-Class Private Function To Compute SHA-256 Hashing With UTF8 Encoding And Hexadecimal Output
            /// </summary>
            /// <param name="input">String To Compute Its Hash</param>
            /// <returns>SHA-256 Hexadecimal String</returns>
            public static string Hash(string input)
            {
                byte[] data = null;

                using (SHA256 sha = new SHA256CryptoServiceProvider())
                {
                    // Convert the input string to a byte array and compute the hash.
                    data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                }

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            /// <summary>
            /// Inter-Class Private Function To Compute SHA-256 Hashing With UTF8 Encoding And Hexadecimal Output
            /// </summary>
            /// <param name="input">String To Compute Its Hash</param>
            /// <returns>SHA-256 Hexadecimal String</returns>
            public static string XHash(string input)
            {
                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                    return null;
                else
                {
                    byte[] data = null;

                    using (SHA256 sha = new SHA256CryptoServiceProvider())
                    {
                        // Convert the input string to a byte array and compute the hash.
                        data = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                    }

                    // Create a new Stringbuilder to collect the bytes
                    // and create a string.
                    StringBuilder sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data 
                    // and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string.
                    return sBuilder.ToString();
                }
            }

            /// <summary>
            /// Pre-configured Crypthographic Parameters (Block Size In Bits) For Inter-class Use
            /// </summary>
            public static readonly int BlockBitSize = 128;
            /// <summary>
            /// Pre-configured Crypthographic Parameters (Key Size In Bits) For Inter-class Use
            /// </summary>
            public static readonly int KeyBitSize = 256;

            /// <summary>
            /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <exception cref="ArgumentException">Secret Message Required!;secretMessage</exception>
            /// <remarks>
            /// Adds overhead of (Optional-Payload + BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64
            /// </remarks>
            public static string ENC(string secretMessage, byte[] cryptKey, byte[] authKey,
                               byte[] nonSecretPayload = null)
            {
                if (string.IsNullOrEmpty(secretMessage))
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                var plainText = Encoding.UTF8.GetBytes(secretMessage);
                var cipherText = ENC(plainText, cryptKey, authKey, nonSecretPayload);
                return Convert.ToBase64String(cipherText);
            }

            /// <summary>
            /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            /// <exception cref="ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
            public static string DEC(string encryptedMessage, byte[] cryptKey, byte[] authKey,
                               int nonSecretPayloadLength = 0)
            {
                if (string.IsNullOrWhiteSpace(encryptedMessage))
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                var cipherText = Convert.FromBase64String(encryptedMessage);
                var plainText = DEC(cipherText, cryptKey, authKey, nonSecretPayloadLength);
                return plainText == null ? null : Encoding.UTF8.GetString(plainText);
            }
            /// <summary>
            /// Simple Encryption (AES) then Authentication (HMAC) for a UTF8 Message.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayload">(Optional) Non-Secret Payload.</param>
            /// <returns>Encrypted Message</returns>
            private static byte[] ENC(byte[] secretMessage, byte[] cryptKey, byte[] authKey, byte[] nonSecretPayload = null)
            {
                //User Error Checks
                if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "cryptKey");

                if (authKey == null || authKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "authKey");

                if (secretMessage == null || secretMessage.Length < 1)
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                //non-secret payload optional
                nonSecretPayload = nonSecretPayload ?? new byte[] { };

                byte[] cipherText;
                byte[] iv;

                using (var aes = new AesManaged()
                {
                    KeySize = KeyBitSize,
                    BlockSize = BlockBitSize,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                })
                {

                    //Use random IV
                    aes.GenerateIV();
                    iv = aes.IV;

                    using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
                    {
                        aes.Dispose();

                        using (var cipherStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                            {
                                using (var binaryWriter = new BinaryWriter(cryptoStream))
                                {
                                    //Encrypt Data
                                    binaryWriter.Write(secretMessage);
                                }
                            }
                            cipherText = cipherStream.ToArray();
                        }
                    }
                }

                //Assemble encrypted message and add authentication
                using (var hmac = new HMACSHA256(authKey))
                using (var encryptedStream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(encryptedStream))
                    {
                        //Prepend non-secret payload if any
                        binaryWriter.Write(nonSecretPayload);
                        //Prepend IV
                        binaryWriter.Write(iv);
                        //Write Ciphertext
                        binaryWriter.Write(cipherText);
                        binaryWriter.Flush();

                        //Authenticate all data
                        var tag = hmac.ComputeHash(encryptedStream.ToArray());
                        //Postpend tag
                        binaryWriter.Write(tag);
                    }
                    return encryptedStream.ToArray();
                }

            }
            /// <summary>
            /// Simple Authentication (HMAC) then Decryption (AES) for a secrets UTF8 Message.
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="cryptKey">The crypt key.</param>
            /// <param name="authKey">The auth key.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            private static byte[] DEC(byte[] encryptedMessage, byte[] cryptKey, byte[] authKey, int nonSecretPayloadLength = 0)
            {

                //Basic Usage Error Checks
                if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("CryptKey needs to be {0} bit!", KeyBitSize), "cryptKey");

                if (authKey == null || authKey.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("AuthKey needs to be {0} bit!", KeyBitSize), "authKey");

                if (encryptedMessage == null || encryptedMessage.Length == 0)
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                using (var hmac = new HMACSHA256(authKey))
                {
                    var sentTag = new byte[hmac.HashSize / 8];
                    //Calculate Tag
                    var calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
                    var ivLength = (BlockBitSize / 8);

                    //if message length is to small just return null
                    if (encryptedMessage.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                        return null;

                    //Grab Sent Tag
                    Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                    //Compare Tag with constant time comparison
                    var compare = 0;
                    for (var i = 0; i < sentTag.Length; i++)
                        compare |= sentTag[i] ^ calcTag[i];

                    //if message doesn't authenticate return null
                    if (compare != 0)
                        return null;

                    using (var aes = new AesManaged()
                    {
                        KeySize = KeyBitSize,
                        BlockSize = BlockBitSize,
                        Mode = CipherMode.CBC,
                        Padding = PaddingMode.PKCS7
                    })
                    {

                        //Grab IV from message
                        var iv = new byte[ivLength];
                        Array.Copy(encryptedMessage, nonSecretPayloadLength, iv, 0, iv.Length);

                        using (var decrypter = aes.CreateDecryptor(cryptKey, iv))
                        {
                            aes.Dispose();

                            using (var plainTextStream = new MemoryStream())
                            {
                                using (var decrypterStream = new CryptoStream(plainTextStream, decrypter, CryptoStreamMode.Write))
                                {
                                    using (var binaryWriter = new BinaryWriter(decrypterStream))
                                    {
                                        //Decrypt Cipher Text from Message
                                        binaryWriter.Write(
                                          encryptedMessage,
                                          nonSecretPayloadLength + iv.Length,
                                          encryptedMessage.Length - nonSecretPayloadLength - iv.Length - sentTag.Length
                                        );
                                    }
                                }
                                //Return Plain Text
                                return plainTextStream.ToArray();
                            }
                        }
                    }
                }
            }
        }
}
