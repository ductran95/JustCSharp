using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JustCSharp.Utility.Helpers
{
    public static class CryptoHelper
    {
        // private static int KeySize = 128;
        // private static int DerivationIteration = 1000;

        // 128-bit = 32 character
        public static string PassPhrase;

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

        // public static string EncryptRijndael(string plainText)
        // {
        //     var salStringBytes = GenerateRandomEntropy();
        //     var ivStringBytes = GenerateRandomEntropy();
        //     var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        //     using (var password = new Rfc2898DeriveBytes(PassPhrase, salStringBytes, DerivationIteration))
        //     {
        //         var keyBytes = password.GetBytes(KeySize / 8);
        //         using(var symetricKey = new RijndaelManaged())
        //         {
        //             symetricKey.BlockSize = KeySize;
        //             symetricKey.Mode = CipherMode.CBC;
        //             symetricKey.Padding = PaddingMode.PKCS7;
        //             
        //             using(var encryptor = symetricKey.CreateEncryptor(keyBytes, ivStringBytes))
        //             {
        //                 using (var memoryStream = new MemoryStream())
        //                 {
        //                     using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        //                     {
        //                         cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
        //                         cryptoStream.FlushFinalBlock();
        //             
        //                         var cipherTextBytes = salStringBytes;
        //                         cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
        //                         cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
        //                         memoryStream.Close();
        //                         cryptoStream.Close();
        //                         return Convert.ToBase64String(cipherTextBytes);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
        //
        // public static string DecryptRijndael(string encryptedText)
        // {
        //     var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(encryptedText);
        //     var salStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();
        //     var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();
        //     var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KeySize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray();
        //
        //     using(var password = new Rfc2898DeriveBytes(PassPhrase, salStringBytes, DerivationIteration))
        //     {
        //         var keyBytes = password.GetBytes(KeySize / 8);
        //         using(var symetricKey = new RijndaelManaged())
        //         {
        //             symetricKey.BlockSize = KeySize;
        //             symetricKey.Mode = CipherMode.CBC;
        //             symetricKey.Padding = PaddingMode.PKCS7;
        //
        //             using(var decryptor = symetricKey.CreateDecryptor(keyBytes, ivStringBytes))
        //             {
        //                 using (var memoryStream = new MemoryStream())
        //                 {
        //                     using(var cryptorStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        //                     {
        //                         var plainTextBytes = new byte[cipherTextBytes.Length];
        //                         var decryptByteCount = cryptorStream.Read(plainTextBytes, 0, plainTextBytes.Length);
        //                         memoryStream.Close();
        //                         cryptorStream.Close();
        //                         return Encoding.UTF8.GetString(plainTextBytes, 0, decryptByteCount);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
        //
        // private static byte[] GenerateRandomEntropy()
        // {
        //     var randomBytes = new byte[KeySize / 8];
        //     using (var rngCsp = new RNGCryptoServiceProvider()) 
        //     {
        //         rngCsp.GetBytes(randomBytes);
        //     }
        //     return randomBytes;
        // }
    }
}