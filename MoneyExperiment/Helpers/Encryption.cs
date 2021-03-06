﻿/*
    Money Experiment Experimental console budgeting app.
    Built on .net core. Use it to sync between PCs.
    Copyright (C) 2019  Krasen Ivanov

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
            // Encrypt budget costs file.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.CostsFilePath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedUserCostsString = EncryptString(selectedAccount.Budget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedUserCostsString);
                }
            }

            // Encrypt budget items file.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.ItemsFilePath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedUserItemsString = EncryptString(selectedAccount.Budget.UserInputItem[i]);
                    outputFile.WriteLine(encryptedUserItemsString);
                }
            }

            // Encrypt budget all transactions file.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.AllTransactionsFilePath))
            {
                for (int i = 0; i < allTransactionsLineCount; i++)
                {
                    var encryptedAllUserTransactionsString = EncryptString(selectedAccount.Budget.AllUserTransactionFile[i]);
                    outputFile.WriteLine(encryptedAllUserTransactionsString);
                }
            }

            // Encrypt budget file path file.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.BudgetFilePath))
            {
                var encryptedBudgetAmount = EncryptString(selectedAccount.Budget.Amount.ToString());
                var encryptedBudgetName = EncryptString(selectedAccount.Budget.Name);

                outputFile.WriteLine(encryptedBudgetAmount);
                outputFile.WriteLine(encryptedBudgetName);
            }

            // Encrypt wallet amount and name file path file.
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Wallet[0].AmountAndNameFilePath))
            {
                foreach (var wallet in selectedAccount.Wallet)
                {
                    var encryptedWalletAmount = EncryptString(wallet.WalletAmount.ToString());
                    var encryptedWalletName = EncryptString(wallet.WalletName);

                    outputFile.WriteLine(encryptedWalletAmount);
                    outputFile.WriteLine(encryptedWalletName);
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