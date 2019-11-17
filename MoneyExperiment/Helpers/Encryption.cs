// Krasen Ivanov 2019

namespace MoneyExperiment.Helpers
{
    using MoneyExperiment.Model;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class Encryption
    {
        private static string userPassword = string.Empty;
        public static bool IsPasswordWrong { get; set; }

        public static string EncryptString(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] encrypted;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = Encoding.UTF8.GetBytes(userPassword);
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

        public static string DecryptString(string cipherText)
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
                aes.Key = Encoding.UTF8.GetBytes(userPassword);
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

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        public static void SaveDatabase(Account selectedAccount, int fileLineCount, int allTransactionsLineCount)
        {
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.CostsFilePath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(selectedAccount.Budget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.ItemsFilePath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(selectedAccount.Budget.UserInputItem[i]);
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.AllTransactionsFilePath))
            {
                for (int i = 0; i < allTransactionsLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(selectedAccount.Budget.AllUserTransactionFile[i]);
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.BudgetFilePath))
            {
                outputFile.WriteLine(selectedAccount.Budget.Amount);
                outputFile.WriteLine(selectedAccount.Budget.Name);
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Wallet[0].AmountFilePath))
            {
                foreach (var wallet in selectedAccount.Wallet)
                {
                    outputFile.WriteLine(wallet.WalletAmount);
                    outputFile.WriteLine(wallet.WalletName);
                }
            }
        }

        /// <summary>
        /// Ask for user to set password.
        /// </summary>
        /// <returns>string of User input</returns>
        public static void AskForPassword()
        {
            Console.Write("Please enter your password: ");

            StringBuilder passwordInput = new StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    passwordInput.Append(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && passwordInput.Length > 0)
                    {
                        passwordInput.Remove(passwordInput.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.Clear();

            // Save password
            if (passwordInput.ToString().Length <= Constants.PasswordLength)
            {
                StringBuilder builder = new StringBuilder(passwordInput.ToString());

                for (int i = Constants.PasswordLength; i >= passwordInput.ToString().Length; i--)
                {
                    builder.Append("-");
                }

                userPassword = builder.ToString();
            }
            else if (passwordInput.ToString().Length >= Constants.PasswordLength + 2)
            {
                Console.WriteLine("Your password is too long.");
                AskForPassword();
            }
            else
            {
                userPassword = passwordInput.ToString();
            }
        }
    }
}