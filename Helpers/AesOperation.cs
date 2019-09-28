// Krasen Ivanov 2019

namespace MoneyExperiment.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class AesOperation
    {
        public static bool IsWrongPassword { get; set; }

        public static string EncryptString(string password, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] encrypted;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes(password);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new MemoryStream();
                using CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                encrypted = memoryStream.ToArray();
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptString(string password, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using AesManaged aes = new AesManaged
            {
                Key = Encoding.UTF8.GetBytes(password),
                IV = iv
            };

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);
            {
                IsWrongPassword = false;
                try
                {
                    return streamReader.ReadToEnd();
                }
                catch (CryptographicException)
                {
                    IsWrongPassword = true;
                    return "";
                }
                finally
                {
                    if (IsWrongPassword)
                    {
                        memoryStream.Dispose();
                        cryptoStream.Dispose();
                        streamReader.Dispose();
                    }
                }
            }
        }
    }
}
