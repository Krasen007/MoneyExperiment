// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using MoneyExperiment.Model;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Begin
    {
        // Can probably be moved to separate file. Used only in decrypt db and save db.
        private string userPassword;

        private int fileLineCount;
        private int allTransactionsLineCount;

        public Begin()
        {
            Console.WriteLine("*********** Welcome **********");
            this.userPassword = this.AskForPassword();
            this.PullDataBase();
            this.Start(this.LoadAccount(null));
        }

        /// <summary>
        /// Ask for user to set password.
        /// </summary>
        /// <returns>string of User input</returns>
        private string AskForPassword()
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

                return builder.ToString();
            }
            else if (passwordInput.ToString().Length >= Constants.PasswordLength + 2)
            {
                Console.WriteLine("Your password is too long.");
                return this.AskForPassword();
            }
            else
            {
                return passwordInput.ToString();
            }
        }

        /// <summary>
        /// Gets the updated database from remote.
        /// </summary>
        private void PullDataBase()
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
        /// <param name="name">Selected budget to load.</param>
        /// <returns>Budget item with set fields.</returns>
        private Account LoadAccount(string? name)
        {
            Account accountToLoad = new Account
            {
                /// ODO add ability to change acc name.
                AccName = Constants.DefaultAccountName,
            };

            if (name == null)
            {
                accountToLoad.Budget = new Budget
                {
                    Name = Constants.DefaultBudgetName
                };
            }
            else
            {
                accountToLoad.Budget = new Budget
                {
                    Name = name
                };
            }

            accountToLoad.AccAmountFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\Amount" + accountToLoad.AccName + ".krs";
            accountToLoad.Budget.BudgetFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\" + accountToLoad.Budget.Name + "\\Budget" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.ItemsFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\" + accountToLoad.Budget.Name + "\\Items" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.CostsFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\" + accountToLoad.Budget.Name + "\\Costs" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.AllTransactionsFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\" + accountToLoad.Budget.Name + "\\AllTransactions" + accountToLoad.Budget.Name + ".krs";
            accountToLoad.Budget.SummaryFilePath = Constants.DatabaseFolderPath + accountToLoad.AccName + "\\" + accountToLoad.Budget.Name + "\\Summary" + accountToLoad.Budget.Name + ".txt";

            return accountToLoad;
        }

        #region Start

        /// <summary>
        /// Main logic of the program.
        /// </summary>
        /// <param name="selectedAccount">The Budget to operate on.</param>
        private void Start(Account selectedAccount)
        {
            ///this.PerformIntegrityCheck(selectedAccount);

            if (this.DecryptDatabaseFiles(selectedAccount, out Account decryptedAccount))
            {
                this.SaveDatabase(decryptedAccount);
                // Start UI
                this.ListDataBaseSummary(decryptedAccount);
                this.ShowMainMenu(decryptedAccount);
            }
            else
            {
                // Try again.
                Encryption.IsPasswordWrong = false;
                this.userPassword = this.AskForPassword();
                this.Start(decryptedAccount);
            }
        }

        //// private bool PerformIntegrityCheck(Account selectedAccount)
        //// {
        ////     return true;
        //// }

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

            if (selectedAccount.AccName == Constants.DefaultAccountName)
            {
                //// Case of first run
                ////if (selectedBudget.Name == Constants.DefaultBudgetName)
                ////{
                ////    Console.Write("Set your spending budget: ");
                ////    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                ////    Directory.CreateDirectory(Constants.DatabaseFolderPath + selectedBudget.Name);
                ////    File.Create(selectedBudget.BudgetPath).Dispose();
                ////}
                ////else
                ////{
                ////    Console.Write("Enter the name of your budget: ");
                ////    selectedBudget.Name = Console.ReadLine();

                ////    Console.Write("Set your spending budget: ");
                ////    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                ////    selectedBudget.BudgetPath = Constants.DatabaseFolderPath + selectedBudget.Name + "\\Budget" + selectedBudget.Name + ".krs";
                Directory.CreateDirectory(Constants.DatabaseFolderPath + selectedAccount.AccName);
                ////    File.Create(selectedAccount.Name).Dispose();
                ////}
            }

            // Acc Amount file
            if (!File.Exists(selectedAccount.AccAmountFilePath))
            {
                File.Create(selectedAccount.AccAmountFilePath).Dispose();
            }
            else
            {
                try
                {
                    using StreamReader srBudget = new StreamReader(selectedAccount.AccAmountFilePath);
                    selectedAccount.AccAmount = ParseHelper.ParseDouble(srBudget.ReadLine()!);
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

                    Directory.CreateDirectory(path: Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name);
                    File.Create(selectedAccount.Budget.BudgetFilePath).Dispose();
                }
                else
                {
                    Console.Write("Enter the name of your budget: ");
                    selectedAccount.Budget.Name = Console.ReadLine();

                    Console.Write("Set your spending budget: ");
                    selectedAccount.Budget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    selectedAccount.Budget.BudgetFilePath = Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name + "\\Budget" + selectedAccount.Budget.Name + ".krs";
                    Directory.CreateDirectory(Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name);
                    File.Create(selectedAccount.Budget.BudgetFilePath).Dispose();
                }
            }
            else
            {
                try
                {
                    // To work the file should contain first the budget Amount and on the second line the name of the budget.
                    using StreamReader srBudget = new StreamReader(selectedAccount.Budget.BudgetFilePath);
                    selectedAccount.Budget.Amount = ParseHelper.ParseDouble(srBudget.ReadLine()!);
                    selectedAccount.Budget.Name = srBudget.ReadLine()!;
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

                selectedAccount.Budget.ItemsFilePath = Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name + "\\Items" + selectedAccount.Budget.Name + ".krs";
                File.Create(selectedAccount.Budget.ItemsFilePath).Dispose();
                this.fileLineCount = 0;
            }
            else
            {
                this.fileLineCount = File.ReadLines(selectedAccount.Budget.ItemsFilePath).Count();

                using StreamReader srItems = new StreamReader(selectedAccount.Budget.ItemsFilePath);
                try
                {
                    for (int i = 0; i < this.fileLineCount; i++)
                    {
                        var decryptedString = Encryption.DecryptString(this.userPassword, srItems.ReadLine()!);
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

                selectedAccount.Budget.CostsFilePath = Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name + "\\Costs" + selectedAccount.Budget.Name + ".krs";
                File.Create(selectedAccount.Budget.CostsFilePath).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(selectedAccount.Budget.CostsFilePath);
                try
                {
                    for (int i = 0; i < this.fileLineCount; i++)
                    {
                        var decryptedString = Encryption.DecryptString(this.userPassword, srCosts.ReadLine()!);
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

                selectedAccount.Budget.AllTransactionsFilePath = Constants.DatabaseFolderPath + selectedAccount.AccName + backslash + selectedAccount.Budget.Name + "\\AllTransactions" + selectedAccount.Budget.Name + ".krs";
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
                        var decryptedString = Encryption.DecryptString(this.userPassword, trReader.ReadLine()!);
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

            Console.WriteLine(Constants.SeparatorHelper(selectedAccount.AccAmount, 6) + selectedAccount.AccAmount + " " + selectedAccount.AccName + "\n");

            ////var netWorthAccount = new Account
            ////{
            ////    Name = "Net worth",
            ////    Amount = 500
            ////};

            ////var spendingAccount = new Account
            ////{
            ////    Name = "Wallet",
            ////    Amount = netWorthAccount.Amount - 60
            ////};

            double totalCosts = 0;
            for (int i = 0; i < this.fileLineCount; i++)
            {
                // This is used to add space between the amount of the item so they appear level.
                Console.WriteLine(Constants.SeparatorHelper(selectedAccount.Budget.UserInputCost[i], 6) + selectedAccount.Budget.UserInputCost[i] + " " + selectedAccount.Budget.UserInputItem[i]);
                totalCosts += selectedAccount.Budget.UserInputCost[i];
            }

            Console.WriteLine("\n" + Constants.SeparatorHelper(totalCosts, 6) + totalCosts + " TOTAL SPENT");
            Console.WriteLine(Constants.SeparatorHelper(selectedAccount.Budget.Amount - totalCosts, 6) + (selectedAccount.Budget.Amount - totalCosts) + " Left of " + selectedAccount.Budget.Amount + " budgeted.");
            Console.WriteLine();
        }

        /// <summary>
        /// Displays the last n-number of transactions.
        /// </summary>
        /// <param name="displayedItems">The number of items to show.</param>
        private void ShowLastTransactions(Budget selectedBudget, int displayedItems)
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
            this.DisplayMenuChoices();

            this.ShowLastTransactions(selectedBudget.Budget, 6);

            this.AskForUserMenuChoice(selectedBudget);
        }

        // This method must be used with AskForUserMenuChoice method.
        private void DisplayMenuChoices()
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
                this.SaveDatabase(selectedAccount);
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.B)
            {
                Console.WriteLine();
                this.UpdateBalance(selectedAccount);
                this.SaveDatabase(selectedAccount);
                Console.Clear();
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("\nExiting...");
                this.SaveDatabase(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                this.SaveDatabase(selectedAccount);

                Console.WriteLine("\nUploading...");
                this.UploadOnline();
                Constants.PressEnterToContinue();
            }
            else if (userInput.Key == ConsoleKey.T)
            {
                Console.Clear();
                Console.WriteLine("Show how many of the last made transactions: ");
                this.ShowLastTransactions(selectedAccount.Budget, (int)ParseHelper.ParseDouble(Console.ReadLine()));
                this.DisplayMenuChoices();
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
            System.Console.WriteLine("What do you want to do with the current account?");
            System.Console.WriteLine("0: Cancel");
            System.Console.WriteLine("1: Add/remove funds");
            System.Console.WriteLine("2: Transfer funds");

            var userChoice = ParseHelper.ParseDouble(Console.ReadLine());

            if (userChoice == 0)
            {
                // Cancel
            }
            else if (userChoice == 1)
            {
                System.Console.WriteLine("Current amount is: " + selectedAccount.AccAmount);
                System.Console.WriteLine("Increase/Decrease balance corection with:");
                var balance = ParseHelper.ParseDouble(Console.ReadLine());

                selectedAccount.AccAmount += balance;
            }
            else
            {
                Console.WriteLine("Not implemented yet");
                Console.WriteLine("Current amount is: " + selectedAccount.AccAmount);
            }

        }

        private void AddOrUpdateBudgetItem(Account selectedAccount)
        {
            var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath);

            string accountName = string.Empty;
            Console.WriteLine("From which account did you spent: ");

            Console.WriteLine("\n0: Cancel.");
            for (int i = 0; i < dirList.Length; i++)
            {
                accountName = dirList[i].Substring(dirList[i].IndexOf("\\") + 1);
                Console.WriteLine(i + 1 + ": " + accountName);
            }

            var accountNumber = ParseHelper.ParseDouble(Console.ReadLine());

            if (accountNumber == 0)
            {
                // Cancel.
            }
            else if (accountNumber > dirList.Length)
            {
                Console.Clear();
                Console.WriteLine("Wrong item selection");
                Constants.PressEnterToContinue();
                ///this.AddOrUpdateBudgetItem();
            }
            else
            {
                Console.WriteLine(accountName + " selected...");
                ///Directory.Delete(dirList[(int)accountNumber - 1], true);
                ///Constants.PressEnterToContinue();
            }

            /**********************/

            Console.Write("\nHow much did you spend: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            Console.Write("What did you spend on: ");
            string itemInput = ParseHelper.ParseStringInput(Console.ReadLine());

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < this.fileLineCount; i++)
            {
                if (itemInput == selectedAccount.Budget.UserInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    selectedAccount.Budget.UserInputCost[i] += costInput;

                    selectedAccount.AccAmount -= costInput;
                    break;
                }
            }

            if (!isDublicateItem)
            {
                selectedAccount.AccAmount -= costInput;
                selectedAccount.Budget.UserInputItem.Add(itemInput);
                selectedAccount.Budget.UserInputCost.Add(costInput);
                selectedAccount.Budget.TranasctionTime.Add(DateTime.Now.ToString());
                this.fileLineCount++;
            }

            // This is used to add space between the amount of the item so they appear level.
            selectedAccount.Budget.AllUserTransactionFile.Add(Constants.SeparatorHelper(costInput, 6) + costInput + " " + itemInput + "  " + DateTime.Now.ToString());
            this.allTransactionsLineCount++;
        }

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        private void SaveDatabase(Account selectedAccount)
        {
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.CostsFilePath))
            {
                for (int i = 0; i < this.fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedAccount.Budget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.ItemsFilePath))
            {
                for (int i = 0; i < this.fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedAccount.Budget.UserInputItem[i]);
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedAccount.Budget.AllTransactionsFilePath))
            {
                for (int i = 0; i < this.allTransactionsLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedAccount.Budget.AllUserTransactionFile[i]);
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
            using (StreamWriter outputFile = new StreamWriter(selectedAccount.AccAmountFilePath))
            {
                outputFile.WriteLine(selectedAccount.AccAmount);
            }
        }

        private void UploadOnline()
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
                this.SaveDatabase(selectedAccount);
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
                this.RemoveItemFromBudget(selectedAccount.Budget);
                Console.Clear();

                this.SaveDatabase(selectedAccount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.N)
            {
                Console.Clear();
                Console.WriteLine("Rename which item?");
                this.RenameBudgetItem(selectedAccount.Budget);
                Console.Clear();

                this.SaveDatabase(selectedAccount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.I)
            {
                Console.Clear();
                Console.WriteLine("Importing...");
                this.ImportCSV(selectedAccount.Budget);
                Constants.PressEnterToContinue();

                this.SaveDatabase(selectedAccount);
                this.ListDataBaseSummary(selectedAccount);
                this.ShowMainMenu(selectedAccount);
            }
            else if (userInput.Key == ConsoleKey.C)
            {
                this.ChangeBudgetNameAndAmount(selectedAccount.Budget);
                Console.Clear();

                this.SaveDatabase(selectedAccount);
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
                    this.Start(this.LoadAccount(budgetToLoad));
                }
            }
            else if (userInput.Key == ConsoleKey.A)
            {
                Console.WriteLine("\nDeleting budget...");
                this.DeleteBudget(selectedAccount);

                Console.Clear();
                Console.WriteLine("Loading the default budget...");
                this.Start(this.LoadAccount(null));
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
                    this.Start(this.LoadAccount(null));
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
                    selectedBudget.UserInputItem[i] = ParseHelper.ParseStringInput(Console.ReadLine());
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

            for (int i = 0; i < this.fileLineCount; i++)
            {
                outputFile.WriteLine(selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }

            double totalCosts = 0;
            for (int i = 0; i < this.fileLineCount; i++)
            {
                totalCosts += selectedBudget.UserInputCost[i];
            }

            outputFile.WriteLine("\nYour spendings are: " + totalCosts);
            outputFile.WriteLine("Your amount left on budget is: " + (selectedBudget.Amount - totalCosts));

            outputFile.Dispose();
        }

        private void RemoveItemFromBudget(Budget selectedBudget)
        {
            Console.WriteLine(0 + ": Cancel.");
            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + 1 + ": " + selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());

            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                if (deleteItem == i + 1)
                {
                    selectedBudget.AllUserTransactionFile.Add(selectedBudget.UserInputCost[i] + " " + selectedBudget.UserInputItem[i] + " " + DateTime.Now.ToString() + " Deleted. ");
                    this.allTransactionsLineCount++;
                    selectedBudget.UserInputItem.Remove(selectedBudget.UserInputItem[i]);
                    selectedBudget.UserInputCost.Remove(selectedBudget.UserInputCost[i]);
                    this.fileLineCount--;
                    break;
                }
                else if (deleteItem == 0)
                {
                    break;
                }
                else if (deleteItem > selectedBudget.UserInputItem.Count)
                {
                    Console.Clear();
                    Console.WriteLine("Wrong item selection");
                    this.RemoveItemFromBudget(selectedBudget);
                }
            }
        }

        /// <summary>
        /// This method imports budget.csv file that is based only with 2 items and is split by ','
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

                        string itemName = csvItems[i].Remove(csvItems[i].IndexOf(','));
                        string itemCost = csvItems[i].Remove(0, csvItems[i].IndexOf(',') + 1);

                        selectedBudget.UserInputItem.Add(itemName);
                        selectedBudget.UserInputCost.Add(Convert.ToDouble(itemCost));
                        this.fileLineCount++;
                    }
                    srBudgetItems.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("You need to add budget.csv in the main folder");
                }
            }
        }

        private void ChangeBudgetNameAndAmount(Budget selectedBudget)
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
            var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath + currentAccount.AccName);

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
            var dirList = Directory.GetDirectories(Constants.DatabaseFolderPath + currentAccount.AccName);

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