/*
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

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using MoneyExperiment.Model;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class Begin
    {
        private int itemsFileLineCount;
        private int allTransactionsLineCount;

        public Begin()
        {
            Console.WriteLine("*********** Welcome **********");
            Encryption.AskForPassword();
            PullDataBase();
            this.Start(LoadAccount(null));
        }

        /// <summary>
        /// Gets the updated database from remote.
        /// </summary>
        private static void PullDataBase()
        {
            // Checks if directory contains any files or directories, pulls updated db.
            if (Directory.Exists(Constants.DatabaseFolderPath))
            {
                try
                {
                    const string PullDB = @"Scripts\PullDB.bat";

                    var process = Process.Start(PullDB);
                    process.WaitForExit();
                    Constants.PressEnterToContinue();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Performs a setup of a budget file.
        /// </summary>
        /// <param name="budgetName">Selected budget to load.</param>
        /// <returns>Budget item with set fields.</returns>
        private static Account LoadAccount(string? budgetName)
        {
            Account accountToLoad = new Account();

            if (budgetName == null)
            {
                accountToLoad.Wallet.Add(new Wallet());
                accountToLoad.Wallet[0].WalletName = Constants.DefaultWalletName;
                accountToLoad.Budget = new Budget
                {
                    Name = Constants.DefaultBudgetName
                };
            }
            else
            {
                accountToLoad.Wallet.Add(new Wallet());
                accountToLoad.Budget = new Budget
                {
                    Name = budgetName
                };
            }

            accountToLoad.Wallet[0].AmountAndNameFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\Amount" + accountToLoad.Wallet[0].WalletName + ".krs";
            accountToLoad.Budget.BudgetFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\" + accountToLoad.Budget.Name + "\\Budget" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.ItemsFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\" + accountToLoad.Budget.Name + "\\Items" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.CostsFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\" + accountToLoad.Budget.Name + "\\Costs" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.AllTransactionsFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\" + accountToLoad.Budget.Name + "\\AllTransactions" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.SummaryFilePath = Constants.DatabaseFolderPath + accountToLoad.Wallet[0].WalletName + "\\" + accountToLoad.Budget.Name + "\\Summary" + accountToLoad.Budget.Name + ".txt";

            return accountToLoad;
        }

        #region Start

        /// <summary>
        /// Main logic of the program.
        /// </summary>
        /// <param name="selectedAccount">The Budget to operate on.</param>
        private void Start(Account selectedAccount)
        {
            var tempWallet = new List<Wallet>
            {
                // Add default wallet
                selectedAccount.Wallet[0]
            };

            foreach (var wallet in selectedAccount.Wallet)
            {
                if (wallet.WalletName == Constants.DefaultWalletName)
                {
                    // Do not add repeatables...
                }
                else
                {
                    // This contains only non default wallets
                    tempWallet.Add(wallet);
                }
            }

            selectedAccount.Wallet = tempWallet;

            if (this.DecryptDatabaseFiles(selectedAccount, out Account decryptedAccount))
            {
                Encryption.SaveDatabase(decryptedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);

                // Start UI

                // Update console size.
                Console.SetWindowSize(Console.WindowWidth, Console.WindowHeight + this.itemsFileLineCount + 7); // Increases the console height to fit the items + offset

                this.ListDataBaseSummary(decryptedAccount);
                this.ShowMainMenu(decryptedAccount);
            }
            else
            {
                // Try again.
                Encryption.IsPasswordWrong = false;
                Encryption.AskForPassword();
                this.Start(selectedAccount);
            }
        }

        /// <summary>
        /// Decrypts the user database with the provided password.
        /// </summary>
        /// <returns>Return true on succesful decrypt.</returns>
        private bool DecryptDatabaseFiles(Account selectedAccount, out Account decryptedAccount)
        {
            decryptedAccount = selectedAccount;

            // Database folder
            if (!Directory.Exists(Constants.DatabaseFolderPath))
            {
                Console.WriteLine("Database folder was missing so we created one for you.");
                Directory.CreateDirectory(Constants.DatabaseFolderPath);
            }

            const string backslash = "\\";

            // Default account folder
            if (selectedAccount.Wallet[0].WalletName == Constants.DefaultWalletName)
            {
                if (!Directory.Exists(Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName))
                {
                    Directory.CreateDirectory(Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName);
                }
                else
                {
                    // implement logic...
                }
            }

            // Acc Amount file
            if (!File.Exists(selectedAccount.Wallet[0].AmountAndNameFilePath))
            {
                File.Create(selectedAccount.Wallet[0].AmountAndNameFilePath).Dispose();
            }
            else
            {
                try
                {
                    var walletsNumber = File.ReadLines(selectedAccount.Wallet[0].AmountAndNameFilePath).Count();
                    walletsNumber /= 2; // This isused because the file contains both name and amount, but we need only the number of wallets.

                    using StreamReader srBudget = new StreamReader(selectedAccount.Wallet[0].AmountAndNameFilePath);

                    for (int i = 0; i < walletsNumber - 1; i++)
                    {
                        selectedAccount.Wallet.Add(new Wallet());
                    }

                    for (int i = 0; i < walletsNumber; i++)
                    {
                        var decryptedAmount = Encryption.DecryptString(srBudget.ReadLine()!);
                        if (Encryption.IsPasswordWrong)
                        {
                            // break
                        }
                        else
                        {
                            selectedAccount.Wallet[i].WalletAmount = ParseHelper.ParseDouble(decryptedAmount);
                        }

                        var decryptName = Encryption.DecryptString(srBudget.ReadLine()!);
                        if (Encryption.IsPasswordWrong)
                        {
                            // break
                        }
                        else
                        {
                            selectedAccount.Wallet[i].WalletName = decryptName;
                        }
                    }

                    srBudget.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine("The account file could not be read: ");
                    Console.WriteLine(error.Message);
                    return false;
                }
            }

            // Budget file
            if (!File.Exists(selectedAccount.Budget.BudgetFilePath))
            {
                // Case of first run
                if (selectedAccount.Budget.Name == Constants.DefaultBudgetName)
                {
                    Console.Write("Set your spending budget: ");
                    selectedAccount.Budget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    Directory.CreateDirectory(path: Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name);
                    File.Create(selectedAccount.Budget.BudgetFilePath).Dispose();
                }
                else
                {
                    Console.Write("Enter the name of your budget: ");
                    selectedAccount.Budget.Name = Console.ReadLine();

                    Console.Write("Set your spending budget: ");
                    selectedAccount.Budget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    selectedAccount.Budget.BudgetFilePath = Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name + "\\Budget" + selectedAccount.Budget.Name + ".krs";
                    Directory.CreateDirectory(Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name);
                    File.Create(selectedAccount.Budget.BudgetFilePath).Dispose();
                }
            }
            else
            {
                try
                {
                    // To work the file should contain first the budget Amount and on the second line the name of the budget.
                    using StreamReader srBudget = new StreamReader(selectedAccount.Budget.BudgetFilePath);

                    var decryptedString = Encryption.DecryptString(srBudget.ReadLine()!);
                    if (Encryption.IsPasswordWrong)
                    {
                        // break
                    }
                    else
                    {
                        selectedAccount.Budget.Amount = ParseHelper.ParseDouble(decryptedString);
                    }

                    var decryptName = Encryption.DecryptString(srBudget.ReadLine()!);
                    if (Encryption.IsPasswordWrong)
                    {
                        // break
                    }
                    else
                    {
                        selectedAccount.Budget.Name = decryptName;
                    }

                    srBudget.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine("The budget file could not be read: ");
                    Console.WriteLine(error.Message);
                    return false;
                }
            }

            // Items file
            if (!File.Exists(selectedAccount.Budget.ItemsFilePath))
            {
                Console.WriteLine("Items file was missing so we created one for you.");

                selectedAccount.Budget.ItemsFilePath = Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name + "\\Items" + selectedAccount.Budget.Name + ".krs";
                File.Create(selectedAccount.Budget.ItemsFilePath).Dispose();
                this.itemsFileLineCount = 0;
            }
            else
            {
                this.itemsFileLineCount = File.ReadLines(selectedAccount.Budget.ItemsFilePath).Count();

                using StreamReader srItems = new StreamReader(selectedAccount.Budget.ItemsFilePath);
                try
                {
                    for (int i = 0; i < this.itemsFileLineCount; i++)
                    {
                        var decryptedString = Encryption.DecryptString(srItems.ReadLine()!);
                        if (Encryption.IsPasswordWrong)
                        {
                            break;
                        }
                        else
                        {
                            selectedAccount.Budget.UserInputItem.Add(decryptedString);
                        }
                    }
                    srItems.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine(error.Message);
                    srItems.Dispose();
                    return false;
                }
            }

            // Costs file
            if (!File.Exists(selectedAccount.Budget.CostsFilePath))
            {
                Console.WriteLine("Costs file was missing so we created one for you.");

                selectedAccount.Budget.CostsFilePath = Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name + "\\Costs" + selectedAccount.Budget.Name + ".krs";
                File.Create(selectedAccount.Budget.CostsFilePath).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(selectedAccount.Budget.CostsFilePath);
                try
                {
                    for (int i = 0; i < this.itemsFileLineCount; i++)
                    {
                        var decryptedString = Encryption.DecryptString(srCosts.ReadLine()!);
                        if (Encryption.IsPasswordWrong)
                        {
                            break;
                        }
                        else
                        {
                            selectedAccount.Budget.UserInputCost.Add(Convert.ToDouble(decryptedString));
                        }
                    }
                    srCosts.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine(error.Message);
                    srCosts.Dispose();
                    return false;
                }
            }

            // All transactions file
            if (!File.Exists(selectedAccount.Budget.AllTransactionsFilePath))
            {
                Console.WriteLine("CurrentTransaction file was missing so we created one for you.");

                selectedAccount.Budget.AllTransactionsFilePath = Constants.DatabaseFolderPath + selectedAccount.Wallet[0].WalletName + backslash + selectedAccount.Budget.Name + "\\AllTransactions" + selectedAccount.Budget.Name + ".krs";
                File.Create(selectedAccount.Budget.AllTransactionsFilePath).Dispose();
                this.allTransactionsLineCount = 0;
            }
            else
            {
                this.allTransactionsLineCount = File.ReadLines(selectedAccount.Budget.AllTransactionsFilePath).Count();

                using StreamReader trReader = new StreamReader(selectedAccount.Budget.AllTransactionsFilePath);
                try
                {
                    for (int i = 0; i < this.allTransactionsLineCount; i++)
                    {
                        var decryptedString = Encryption.DecryptString(trReader.ReadLine()!);
                        if (Encryption.IsPasswordWrong)
                        {
                            break;
                        }
                        else
                        {
                            selectedAccount.Budget.AllUserTransactionFile.Add(decryptedString);
                        }
                    }
                    trReader.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine(error.Message);
                    trReader.Dispose();
                    return false;
                }
            }

            // used for return OUT parameter
            decryptedAccount = selectedAccount;

            // Succesfully read needed files
            if (Encryption.IsPasswordWrong)
            {
                Console.WriteLine("Wrong password.");
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Displays a summary of the items.
        /// </summary>
        private void ListDataBaseSummary(Account selectedAccount)
        {
            Console.WriteLine("*********** {0} **********\n", selectedAccount.Budget.Name);

            double netWorth = 0;
            foreach (var wallet in selectedAccount.Wallet)
            {
                netWorth += wallet.WalletAmount;
            }
            Console.WriteLine(Constants.SeparatorHelper(netWorth, 6) + netWorth + " Net Worth");

            foreach (var wallet in selectedAccount.Wallet)
            {
                Console.WriteLine(Constants.SeparatorHelper(wallet.WalletAmount, 6) + wallet.WalletAmount + " " + wallet.WalletName);
            }
            Console.WriteLine();

            double totalCosts = 0;
            for (int i = 0; i < this.itemsFileLineCount; i++)
            {
                totalCosts += selectedAccount.Budget.UserInputCost[i];

                // This is used to add space between the amount of the item so they appear level.
                Console.WriteLine(Constants.SeparatorHelper(selectedAccount.Budget.UserInputCost[i], 6) + selectedAccount.Budget.UserInputCost[i] + " " + selectedAccount.Budget.UserInputItem[i]);
            }

            var stringInfo = Constants.SeparatorHelper(selectedAccount.Budget.Amount - totalCosts, 6) + (selectedAccount.Budget.Amount - totalCosts) + " Left of " + selectedAccount.Budget.Amount + " budgeted.";

            Console.WriteLine("\n" + Constants.SeparatorHelper(totalCosts, 6) + totalCosts + " TOTAL SPENT");
            Console.WriteLine(stringInfo);

            /* This is used for budget progress bar display. */
            var incrementCost = selectedAccount.Budget.Amount / stringInfo.Length;
            var displayedRemainingBudget = (selectedAccount.Budget.Amount - totalCosts) / incrementCost;
            var displayedSpending = (selectedAccount.Budget.Amount / incrementCost) - displayedRemainingBudget;
            var overSpentIncrement = (totalCosts - selectedAccount.Budget.Amount) / incrementCost;

            var progressBar = new System.Text.StringBuilder();

            if (overSpentIncrement > 0)
            {
                for (int j = 0; j < stringInfo.Length; j++)
                {
                    progressBar.Append('#');
                }

                for (int i = 0; i < overSpentIncrement; i++)
                {
                    progressBar.Append('!');
                }

                Console.WriteLine("[" + progressBar + "]");
            }
            else
            {
                for (int j = 0; j < displayedSpending; j++)
                {
                    progressBar.Append('#');
                }

                for (int i = 0; i < displayedRemainingBudget; i++)
                {
                    progressBar.Append('-');
                }

                Console.WriteLine("[" + progressBar + "]");
            }
            /* to here */

            Console.WriteLine();
        }

        /// <summary>
        /// Displays the last n-number of transactions.
        /// </summary>
        /// <param name="displayedItems">The number of items to show.</param>
        private static void ShowLastTransactions(Budget selectedBudget, int displayedItems)
        {
            Console.WriteLine("*********** List of recent transactions ***********");

            if (selectedBudget.AllUserTransactionFile.Count <= displayedItems)
            {
                for (int i = 0; i < selectedBudget.AllUserTransactionFile.Count; i++)
                {
                    Console.WriteLine(selectedBudget.AllUserTransactionFile[i]);
                }
            }
            else
            {
                for (int i = 0; i < displayedItems; i++)
                {
                    Console.WriteLine(selectedBudget.AllUserTransactionFile[selectedBudget.AllUserTransactionFile.Count - 1 - i]);
                }
            }

            Console.WriteLine();
        }

        #endregion Start

        #region Main menu

        /// <summary>
        /// Displays the main menu of the program.
        /// </summary>
        private void ShowMainMenu(Account selectedBudget)
        {
            DisplayMenuChoices();

            ShowLastTransactions(selectedBudget.Budget, 6);

            this.AskForUserMenuChoice(selectedBudget);
        }

        // This method must be used with AskForUserMenuChoice method.
        private static void DisplayMenuChoices()
        {
            Console.WriteLine("*********** Menu ***********");
            Console.WriteLine(
                "type 'y' to add new entrY in the current budget, \n" +
                "type 'b' to update account Balance, \n" +
                "type 'e' to save and Exit without uploading online, \n" +
                "type 'u' to save and exit and Upload the database online, \n" +
                "type 't' to show the last n-number of Transactions, \n" +
                "type 'o' for other Options. \n");
        }

        private void AskForUserMenuChoice(Account selectedAccount)
        {
            Console.Write("Enter your choice: ");
            var userInput = Console.ReadKey();

            if (userInput.Key == ConsoleKey.Y)
            {
                Console.WriteLine();
                this.AddOrUpdateBudgetItem(selectedAccount);
                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.B)
            {
                Console.WriteLine();
                this.UpdateBalance(selectedAccount);
                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("\nExiting...");
                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);

                Console.Clear();
                Console.WriteLine("\nUploading...");
                UploadOnline();
                Constants.PressEnterToContinue();
            }
            else if (userInput.Key == ConsoleKey.T)
            {
                Console.Clear();
                Console.WriteLine("Show how many of the last made transactions: ");
                ShowLastTransactions(selectedAccount.Budget, (int)ParseHelper.ParseDouble(Console.ReadLine()));
                DisplayMenuChoices();
                this.AskForUserMenuChoice(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.O)
            {
                Console.Clear();
                this.ShowOptionsMenu(selectedAccount);
            }
            else
            {
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
        }

        private void UpdateBalance(Account selectedAccount)
        {
            Console.WriteLine("What do you want to do with the current account?");
            Console.WriteLine("0: Cancel");
            Console.WriteLine("1: Add/remove funds");
            Console.WriteLine("2: Transfer funds");
            Console.WriteLine("3: Create new account");
            Console.WriteLine("4: Remove account");
            Console.WriteLine("5: Rename account");

            var userChoice = ParseHelper.ParseDouble(Console.ReadLine());

            if (userChoice == 0)
            {
                // Cancel.
            }
            else if (userChoice == 1)
            {
                Console.WriteLine("Current amount is: " + selectedAccount.Wallet[0].WalletAmount);
                Console.WriteLine("Increase/Decrease balance corection with:");
                var balance = ParseHelper.ParseDouble(Console.ReadLine());

                selectedAccount.Wallet[0].WalletAmount += balance;

                selectedAccount.Budget.TranasctionTime.Add(DateTime.Now.ToString());
                selectedAccount.Budget.AllUserTransactionFile.Add(Constants.SeparatorHelper(balance, 6) + "Balance corected with: " + balance + " at " + DateTime.Now.ToString());
                this.allTransactionsLineCount++;
            }
            else if (userChoice == 2)
            {
                if (selectedAccount.Wallet.Count < 1)
                {
                    // Cancel.
                }
                else
                {
                    ///Console.WriteLine("What amount you want to transfer between who?");

                    Console.WriteLine("Pick from which account to move funds: ");
                    for (int i = 0; i < selectedAccount.Wallet.Count; i++)
                    {
                        Console.WriteLine(i + ": " + selectedAccount.Wallet[i].WalletName + " contains " + selectedAccount.Wallet[i].WalletAmount);
                    }
                    var userChoseFirstAcc = ParseHelper.ParseDouble(Console.ReadLine());

                    Console.WriteLine("How much?");
                    var userChoseAmount = ParseHelper.ParseDouble(Console.ReadLine());

                    Console.WriteLine("Pick second account from which to move funds to: ");
                    for (int i = 0; i < selectedAccount.Wallet.Count; i++)
                    {
                        Console.WriteLine(i + ": " + selectedAccount.Wallet[i].WalletName + " contains " + selectedAccount.Wallet[i].WalletAmount);
                    }
                    var userChoseSecondAcc = ParseHelper.ParseDouble(Console.ReadLine());

                    // Checks if selected wallet exists
                    if (userChoseSecondAcc > selectedAccount.Wallet.Count || userChoseFirstAcc > selectedAccount.Wallet.Count)
                    {
                        Console.WriteLine("Account does not exist");
                        this.UpdateBalance(selectedAccount);
                    }
                    else
                    {
                        // The actual logic
                        selectedAccount.Wallet[(int)userChoseFirstAcc].WalletAmount -= userChoseAmount;
                        selectedAccount.Wallet[(int)userChoseSecondAcc].WalletAmount += userChoseAmount;
                    }
                }
            }
            else if (userChoice == 3)
            {
                // Adds a new wallet
                Console.WriteLine("What is the name of the new wallet?");
                selectedAccount.Wallet.Add(new Wallet { WalletName = ParseHelper.ParseStringInput(Console.ReadLine()) });
            }
            else if (userChoice == 4)
            {
                // Adds a new wallet
                Console.WriteLine("Which wallet you wish to remove?");
                // i starts from 1 because you should not delete the default wallet
                for (int i = 1; i < selectedAccount.Wallet.Count; i++)
                {
                    Console.WriteLine(i + ": " + selectedAccount.Wallet[i].WalletName + " contains " + selectedAccount.Wallet[i].WalletAmount);
                }
                var userChosenWalletToRemove = ParseHelper.ParseDouble(Console.ReadLine());

                if (userChosenWalletToRemove > selectedAccount.Wallet.Count)
                {
                    this.UpdateBalance(selectedAccount);
                }
                else
                {
                    selectedAccount.Wallet.Remove(selectedAccount.Wallet[(int)userChosenWalletToRemove]);
                }
            }
            else if (userChoice == 5)
            {
                // Adds a new wallet
                Console.WriteLine("Which wallet you wish to rename?");
                for (int i = 0; i < selectedAccount.Wallet.Count; i++)
                {
                    Console.WriteLine(i + ": " + selectedAccount.Wallet[i].WalletName + " contains " + selectedAccount.Wallet[i].WalletAmount);
                }
                var userChosenWalletToRename = ParseHelper.ParseDouble(Console.ReadLine());

                if (userChosenWalletToRename > selectedAccount.Wallet.Count)
                {
                    this.UpdateBalance(selectedAccount);
                }
                else
                {
                    Console.WriteLine("Enter the new name for this account");
                    selectedAccount.Wallet[(int)userChosenWalletToRename].WalletName = Console.ReadLine();
                }
            }
            else
            {
                Console.Clear();
                this.UpdateBalance(selectedAccount);
            }
        }

        private void AddOrUpdateBudgetItem(Account selectedAccount)
        {
            ////var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath);

            ////string accountName = string.Empty;
            ////Console.WriteLine("From which account did you spent: ");

            ////Console.WriteLine("\n0: Cancel.");
            ////for (int i = 0; i < dirList.Length; i++)
            ////{
            ////    accountName = dirList[i].Substring(dirList[i].IndexOf("\\") + 1);
            ////    Console.WriteLine(i + 1 + ": " + accountName);
            ////}

            ////var accountNumber = ParseHelper.ParseDouble(Console.ReadLine());

            ////if (accountNumber == 0)
            ////{
            ////    // Cancel.
            ////}
            ////else if (accountNumber > dirList.Length)
            ////{
            ////    Console.Clear();
            ////    Console.WriteLine("Wrong item selection");
            ////    Constants.PressEnterToContinue();
            ////    ///this.AddOrUpdateBudgetItem();
            ////}
            ////else
            ////{
            ////    Console.WriteLine(accountName + " selected...");
            ////    ///Directory.Delete(dirList[(int)accountNumber - 1], true);
            ////    ///Constants.PressEnterToContinue();
            ////}

            /**********************/

            Console.Write("\nHow much did you spend: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            Console.Write("What did you spend on: ");
            string itemInput = ParseHelper.ParseStringInput(Console.ReadLine());

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < this.itemsFileLineCount; i++)
            {
                if (itemInput == selectedAccount.Budget.UserInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    selectedAccount.Budget.UserInputCost[i] += costInput;

                    selectedAccount.Wallet[0].WalletAmount -= costInput;
                    break;
                }
            }

            if (!isDublicateItem)
            {
                selectedAccount.Wallet[0].WalletAmount -= costInput;
                selectedAccount.Budget.UserInputItem.Add(itemInput);
                selectedAccount.Budget.UserInputCost.Add(costInput);
                selectedAccount.Budget.TranasctionTime.Add(DateTime.Now.ToString());
                this.itemsFileLineCount++;
            }

            // This is used to add space between the amount of the item so they appear level.
            selectedAccount.Budget.AllUserTransactionFile.Add(Constants.SeparatorHelper(costInput, 6) + costInput + " " + itemInput + "  " + DateTime.Now.ToString());
            this.allTransactionsLineCount++;
        }

        private static void UploadOnline()
        {
            const string InitCreateDB = @"Scripts\InitCreateDB.bat";
            const string PushUpdateDB = @"Scripts\PushUpdateDB.bat";

            if (Directory.Exists(path: ".git"))
            {
                var process = Process.Start(PushUpdateDB);
                process.WaitForExit();
            }
            else
            {
                var process = Process.Start(InitCreateDB);
                process.WaitForExit();
            }
        }

        #endregion Main menu

        #region Options menu

        /// <summary>
        /// Displays a menu with additional options.
        /// </summary>
        private void ShowOptionsMenu(Account selectedAccount)
        {
            Console.WriteLine("*********** Options ***********");
            Console.WriteLine(
                "type 'x' to eXport database in readable form, \n" +
                "type 'r' to Remove item from current list, \n" +
                "type 'n' to reName item from current list, \n" +
                "type 'i' to Import csv file, \n" +
                "type 'c' to Change the budget name and amount, \n" +
                "type 's' to Switch to another budget, or create another one,\n" +
                "type 'a' to delete A budget, \n" +
                "type 'd' to DELETE ALL DATABASE, \n" +
                "press ESC to return to the main menu.");

            Console.Write("Enter your choice: ");
            var userInput = Console.ReadKey();

            if (userInput.Key == ConsoleKey.X)
            {
                Console.Clear();
                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                this.ExportReadable(selectedAccount.Budget);
                Console.WriteLine("View your summary in " + selectedAccount.Budget.SummaryFilePath);
                Constants.PressEnterToContinue();

                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.R)
            {
                Console.Clear();
                Console.WriteLine("Remove which item?");
                this.RemoveItemFromBudget(selectedAccount);
                Console.Clear();

                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);

                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.N)
            {
                Console.Clear();
                Console.WriteLine("Rename which item?");
                this.RenameBudgetItem(selectedAccount.Budget);
                Console.Clear();

                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.I)
            {
                Console.Clear();
                Console.WriteLine("Please make sure budget.csv is present in the main directory of the app.");
                Constants.PressEnterToContinue();
                this.ImportCSV(selectedAccount.Budget);
                Constants.PressEnterToContinue();

                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.C)
            {
                ChangeBudgetNameAndAmount(selectedAccount.Budget);
                Console.Clear();

                Encryption.SaveDatabase(selectedAccount, this.itemsFileLineCount, this.allTransactionsLineCount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.S)
            {
                Console.WriteLine("\nSwitching budgets...\n");
                var budgetToLoad = this.SwitchBudget(selectedAccount);
                if (budgetToLoad == "False")
                {
                    Console.WriteLine("Canceling...");
                    Console.Clear();
                    this.ShowOptionsMenu(selectedAccount);
                }
                else
                {
                    Console.Clear();
                    this.Start(LoadAccount(budgetToLoad));
                }
            }
            else if (userInput.Key == ConsoleKey.A)
            {
                Console.WriteLine("\nDeleting budget...");
                this.DeleteBudget(selectedAccount);

                Console.Clear();
                Console.WriteLine("Loading the default budget...");
                this.Start(LoadAccount(null));
            }
            else if (userInput.Key == ConsoleKey.D)
            {
                Console.WriteLine("\nWARNING: THIS WILL DELETE ALL OF YOUR DATABASE!\n" +
                "TYPE 'Delete' IF YOU WANT TO CONTINUE?, TYPE 'abort' TO CANCEL");
                var textInput = Console.ReadLine();

                if (textInput == "Delete")
                {
                    Console.WriteLine("Deleting all database...");
                    Directory.Delete(Constants.DatabaseFolderPath, true);
                    Console.WriteLine("************************");
                    Constants.PressEnterToContinue();
                    this.Start(LoadAccount(null));
                }
                else
                {
                    Console.WriteLine("Aborting...");
                    Console.Clear();
                    this.ListDataBaseSummary(selectedAccount);
                    this.ShowMainMenu(selectedAccount);
                }
            }
            else if (userInput.Key == ConsoleKey.Escape)
            {
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else
            {
                Console.Clear();
                this.ShowOptionsMenu(selectedAccount);
            }
        }

        private void RenameBudgetItem(Budget selectedBudget)
        {
            Console.WriteLine(0 + ": Cancel.");
            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + 1 + ": " + selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }

            Console.Write("Enter the number of the item you want to rename: ");
            var renameItem = ParseHelper.ParseDouble(Console.ReadLine());

            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                if (renameItem == i + 1)
                {
                    Console.WriteLine("Enter new name for this item: ");

                    var newName = ParseHelper.ParseStringInput(Console.ReadLine());

                    if (selectedBudget.UserInputItem.Contains(newName))
                    {
                        var tempCost = selectedBudget.UserInputCost[i];

                        selectedBudget.UserInputItem.Remove(selectedBudget.UserInputItem[i]);
                        selectedBudget.UserInputCost.Remove(selectedBudget.UserInputCost[i]);
                        this.itemsFileLineCount--;

                        for (int j = 0; j < selectedBudget.UserInputCost.Count; j++)
                        {
                            if (selectedBudget.UserInputItem[j] == newName)
                            {
                                selectedBudget.UserInputCost[j] += tempCost;
                                break;
                            }
                        }
                    }
                    else
                    {
                        selectedBudget.UserInputItem[i] = newName;
                    }
                    break;
                }
                else if (renameItem == 0)
                {
                    break;
                }
                else if (renameItem > selectedBudget.UserInputItem.Count)
                {
                    Console.Clear();
                    Console.WriteLine("Wrong item selection");
                    this.RenameBudgetItem(selectedBudget);
                }
            }
        }

        /// <summary>
        /// Use if you want to export in txt readable for humans (not encrypted).
        /// </summary>
        private void ExportReadable(Budget selectedBudget)
        {
            // Fix method to use already established method of reading the list
            using StreamWriter outputFile = new StreamWriter(selectedBudget.SummaryFilePath);
            outputFile.WriteLine("*********** {0} **********", selectedBudget.Name);

            for (int i = 0; i < this.itemsFileLineCount; i++)
            {
                outputFile.WriteLine(selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }

            double totalCosts = 0;
            for (int i = 0; i < this.itemsFileLineCount; i++)
            {
                totalCosts += selectedBudget.UserInputCost[i];
            }

            outputFile.WriteLine("\nYour spendings are: " + totalCosts);
            outputFile.WriteLine("Your amount left on budget is: " + (selectedBudget.Amount - totalCosts));

            outputFile.Dispose();
        }

        private void RemoveItemFromBudget(Account selectedBudget)
        {
            Console.WriteLine(0 + ": Cancel.");
            for (int i = 0; i < selectedBudget.Budget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + 1 + ": " + selectedBudget.Budget.UserInputItem[i] + " " + selectedBudget.Budget.UserInputCost[i]);
            }

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());

            for (int i = 0; i < selectedBudget.Budget.UserInputItem.Count; i++)
            {
                if (deleteItem == i + 1)
                {
                    selectedBudget.Budget.AllUserTransactionFile.Add(selectedBudget.Budget.UserInputCost[i] + " " + selectedBudget.Budget.UserInputItem[i] + " " + DateTime.Now.ToString() + " Deleted. ");
                    this.allTransactionsLineCount++;

                    var amountToRemove = selectedBudget.Budget.UserInputCost[i];
                    selectedBudget.Wallet[0].WalletAmount += amountToRemove;

                    selectedBudget.Budget.UserInputItem.Remove(selectedBudget.Budget.UserInputItem[i]);
                    selectedBudget.Budget.UserInputCost.Remove(selectedBudget.Budget.UserInputCost[i]);
                    this.itemsFileLineCount--;
                    break;
                }
                else if (deleteItem == 0)
                {
                    break;
                }
                else if (deleteItem > selectedBudget.Budget.UserInputItem.Count)
                {
                    Console.Clear();
                    Console.WriteLine("Wrong item selection");
                    this.RemoveItemFromBudget(selectedBudget);
                }
            }
        }

        /// <summary>
        /// This method imports budget.csv file that is based only with 2 items and is split by ',' (name,cost)
        /// </summary>
        private void ImportCSV(Budget selectedBudget)
        {
            if (!File.Exists("budget.csv"))
            {
                Console.WriteLine("!!! budget.csv file is missing !!!\nCanceling...");
            }
            else
            {
                try
                {
                    var csvTotalLines = File.ReadLines("budget.csv").Count();

                    using StreamReader srBudgetItems = new StreamReader("budget.csv");
                    List<string> csvItems = new List<string>();

                    for (int i = 0; i < csvTotalLines; i++)
                    {
                        csvItems.Add(srBudgetItems.ReadLine()!);

                        string itemInput = csvItems[i].Remove(csvItems[i].IndexOf(','));
                        string costInput = csvItems[i].Remove(0, csvItems[i].IndexOf(',') + 1);

                        // Check if item is already in the database
                        bool isDublicateItem = false;
                        for (int j = 0; j < this.itemsFileLineCount; j++)
                        {
                            if (itemInput == selectedBudget.UserInputItem[j])
                            {
                                isDublicateItem = true;

                                // Only increase the cost if item is in the database
                                selectedBudget.UserInputCost[j] += Convert.ToDouble(costInput);
                                break;
                            }
                        }

                        if (!isDublicateItem)
                        {
                            selectedBudget.UserInputItem.Add(itemInput);
                            selectedBudget.UserInputCost.Add(Convert.ToDouble(costInput));
                            selectedBudget.TranasctionTime.Add(DateTime.Now.ToString());
                            this.itemsFileLineCount++;
                        }
                    }
                    Console.WriteLine("Import complete!");
                    Console.WriteLine($"You imported {csvTotalLines} items at {DateTime.Now.ToString()}");

                    selectedBudget.AllUserTransactionFile.Add($"You imported {csvTotalLines} items at {DateTime.Now.ToString()}");
                    this.allTransactionsLineCount++;

                    srBudgetItems.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("You need to add budget.csv in the main folder");
                }
            }
        }

        private static void ChangeBudgetNameAndAmount(Budget selectedBudget)
        {
            Console.Write("\nEnter new name for the budget: ");
            selectedBudget.Name = ParseHelper.ParseStringInput(Console.ReadLine());

            Console.Write("Set your new budget: ");
            selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

            //// This is going to be used for renaming folders and files when you change the budget.

            ////var tempBudgetPath = DatabaseFolderPath + selectedBudget.Name + "\\Budget" + selectedBudget.Name + ".krs";
            ////var tempItemsPath = DatabaseFolderPath + selectedBudget.Name + "\\Items" + selectedBudget.Name + ".krs";
            ////var tempCostsPath = DatabaseFolderPath + selectedBudget.Name + "\\Costs" + selectedBudget.Name + ".krs";
            ////var tempAllTransactionsPath = DatabaseFolderPath + selectedBudget.Name + "\\AllTransactions" + selectedBudget.Name + ".krs";
            ////var tempSummaryPath = DatabaseFolderPath + selectedBudget.Name + "\\Summary" + selectedBudget.Name + ".txt";

            ////var tempDirectory = (DatabaseFolderPath + selectedBudget.Name);

            ////Console.Write("\nEnter new name for the budget: ");
            ////selectedBudget.Name = ParseHelper.ParseStringInput(Console.ReadLine());

            ////selectedBudget.BudgetPath = DatabaseFolderPath + selectedBudget.Name + "\\Budget" + selectedBudget.Name + ".krs";
            ////selectedBudget.ItemsPath = DatabaseFolderPath + selectedBudget.Name + "\\Items" + selectedBudget.Name + ".krs";
            ////selectedBudget.CostsPath = DatabaseFolderPath + selectedBudget.Name + "\\Costs" + selectedBudget.Name + ".krs";
            ////selectedBudget.AllTransactionsPath = DatabaseFolderPath + selectedBudget.Name + "\\AllTransactions" + selectedBudget.Name + ".krs";
            ////selectedBudget.SummaryPath = DatabaseFolderPath + selectedBudget.Name + "\\Summary" + selectedBudget.Name + ".txt";

            ////Directory.CreateDirectory(DatabaseFolderPath + selectedBudget.Name);

            ////if (File.Exists(tempBudgetPath))
            ////{
            ////    File.Move(tempBudgetPath, selectedBudget.BudgetPath);
            ////}
            ////if (File.Exists(tempItemsPath))
            ////{
            ////    File.Move(tempItemsPath, selectedBudget.ItemsPath);
            ////}
            ////if (File.Exists(tempCostsPath))
            ////{
            ////    File.Move(tempCostsPath, selectedBudget.CostsPath);
            ////}
            ////if (File.Exists(tempAllTransactionsPath))
            ////{
            ////    File.Move(tempAllTransactionsPath, selectedBudget.AllTransactionsPath);
            ////}
            ////if (File.Exists(tempSummaryPath))
            ////{
            ////    File.Move(tempSummaryPath, selectedBudget.SummaryPath);
            ////}

            ////Directory.Delete(tempDirectory, true);

            ////Console.Write("Set your new budget: ");
            ////selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

            ////return selectedBudget;
        }

        /// <summary>
        /// Gets the name of a budget from a list of the database.
        /// </summary>
        /// <returns>Returns a name of the budget to be loaded.</returns>
        private string SwitchBudget(Account currentAccount)
        {
            // ODO Rename folder or this will break when acc name is changed
            var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath + currentAccount.Wallet[0].WalletName);

            Console.WriteLine(0 + ": Cancel.");
            for (int i = 0; i < dirList.Length; i++)
            {
                // I use this to find the first and second backslashes.
                var budgetName = dirList[i].Substring(dirList[i].IndexOf("\\") + 1);
                Console.WriteLine(i + 1 + ": " + budgetName.Substring(budgetName.IndexOf("\\") + 1));
            }
            Console.WriteLine(dirList.Length + 1 + ": Add new budget.");

            Console.WriteLine("Enter your choice: ");
            var loadBudget = ParseHelper.ParseDouble(Console.ReadLine());

            if (loadBudget == dirList.Length + 1)
            {
                // Creates new budget
                return string.Empty;
            }
            else if (loadBudget == 0)
            {
                // Cancel
                return "False";
            }
            else if (loadBudget > dirList.Length + 1)
            {
                Console.Clear();
                Console.WriteLine("Wrong item selection");
                return this.SwitchBudget(currentAccount);
            }
            else
            {
                var name = dirList[(int)loadBudget - 1]
                    .Substring(dirList[(int)loadBudget - 1].IndexOf("\\") + 1);
                return name.Substring(name.IndexOf("\\") + 1);
            }
        }

        /// <summary>
        /// Deletes the directory containing the budget.
        /// </summary>
        private void DeleteBudget(Account currentAccount)
        {
            var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath + currentAccount.Wallet[0].WalletName);

            Console.WriteLine(0 + ": Cancel.");
            for (int i = 0; i < dirList.Length; i++)
            {
                // I use this to find the first and second backslashes.
                var budgetName = dirList[i].Substring(dirList[i].IndexOf("\\") + 1);
                Console.WriteLine(i + 1 + ": " + budgetName.Substring(budgetName.IndexOf("\\") + 1));
            }

            // This is for deleting...
            Console.Write("Enter the number of the budget you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());

            if (deleteItem == 0)
            {
                // Cancel.
            }
            else if (deleteItem > dirList.Length)
            {
                Console.Clear();
                Console.WriteLine("Wrong item selection");
                Constants.PressEnterToContinue();
                this.DeleteBudget(currentAccount);
            }
            else
            {
                Directory.Delete(dirList[(int)deleteItem - 1], true);
                Console.WriteLine("Budget deleted...");
                Constants.PressEnterToContinue();
            }
        }

        #endregion Options menu
    }
}