using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JustCSharp.Data.Enums;

namespace JustCSharp.Utility.Helpers
{
    public static class CryptoHelper
    {
        // private static int KeySize = 128;
        // private static int DerivationIteration = 1000;

        // 128-bit = 32 character
        public static string PassPhrase;
        
        public delegate HashAlgorithm HashAlgorithmCreator();

        public static string Encrypt(string text)
        {
            var key = Encoding.UTF8.GetBytes(PassPhrase);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);
            var key = Encoding.UTF8.GetBytes(PassPhrase);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Hash string
        /// </summary>
        /// <param name="rawText"></param>
        /// <param name="hashAlgorithm"></param>
        /// <param name="encoding"></param>
        /// <returns>Hashed string in UPPERCASE</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static string Hash(string rawText, HashAlgorithmEnum hashAlgorithm, Encoding encoding = null)
        {
            HashAlgorithmCreator hashCreator = SHA256Managed.Create;
            switch (hashAlgorithm)
            {
                case HashAlgorithmEnum.MD5:
                    hashCreator = MD5.Create;
                    break;
                
                case HashAlgorithmEnum.SHA1:
                    hashCreator = SHA1.Create;
                    break;
                
                case HashAlgorithmEnum.SHA256:
                    hashCreator = SHA256.Create;
                    break;
                
                case HashAlgorithmEnum.SHA384:
                    hashCreator = SHA384.Create;
                    break;
                
                case HashAlgorithmEnum.SHA512:
                    hashCreator = SHA512.Create;
                    break;
                
                default:
                    throw new NotSupportedException();
            }
            
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            
            using (var hashAlgorithmInstance = hashCreator())
            {
                byte[] inputBytes = encoding.GetBytes(rawText);
                byte[] hashBytes = hashAlgorithmInstance.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}