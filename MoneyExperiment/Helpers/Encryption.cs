// Krasen Ivanov 2019

namespace MoneyExperiment.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class Encryption
    {
        public static bool IsPasswordWrong { get; set; }

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
            byte[] iv;
            byte[] buffer;

            IsPasswordWrong = false;
            try
            {
                iv = new byte[16];
                buffer = Convert.FromBase64String(cipherText);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (FormatException)
            {
                IsPasswordWrong = true;
                return "";
            }
#pragma warning restore CA1031 // Do not catch general exception types

            using AesManaged aes = new AesManaged();

            try
            {
                aes.Key = Encoding.UTF8.GetBytes(password);
                aes.IV = iv;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (CryptographicException)
            {
                IsPasswordWrong = true;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);

            IsPasswordWrong = false;
            try
            {
                return streamReader.ReadToEnd();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (CryptographicException)
            {
                IsPasswordWrong = true;
                return "";
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                if (IsPasswordWrong)
                {
                    memoryStream.Dispose();
                    cryptoStream.Dispose();
                    streamReader.Dispose();
                }
            }
        }
    }
}