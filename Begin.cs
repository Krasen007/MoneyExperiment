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
        private const string DatabaseFolderPath = @"Database\";
        private const string DefaultBudgetName = "Budget 1";
        private const int PasswordLength = 31;
        private string userPassword = string.Empty;
        private int fileLineCount;
        private int allTransactionsLineCount;

        public Begin()
        {
            Console.WriteLine("*********** Welcome **********");
            this.Login();
            this.Start(this.LoadBudget(null));
        }

        private Budget LoadBudget(string? name)
        {
            if (name == null)
            {
                Budget budgetToLoad = new Budget
                {
                    Name = DefaultBudgetName
                };
                budgetToLoad.BudgetPath = DatabaseFolderPath + budgetToLoad.Name + "\\Budget" + budgetToLoad.Name + ".krs";
                budgetToLoad.ItemsPath = DatabaseFolderPath + budgetToLoad.Name + "\\Items" + budgetToLoad.Name + ".krs";
                budgetToLoad.CostsPath = DatabaseFolderPath + budgetToLoad.Name + "\\Costs" + budgetToLoad.Name + ".krs";
                budgetToLoad.AllTransactionsPath = DatabaseFolderPath + budgetToLoad.Name + "\\AllTransactions" + budgetToLoad.Name + ".krs";
                budgetToLoad.SummaryPath = DatabaseFolderPath + budgetToLoad.Name + "\\Summary" + budgetToLoad.Name + ".txt";

                return budgetToLoad;
            }
            else
            {
                Budget budgetToLoad = new Budget
                {
                    Name = name
                };
                budgetToLoad.BudgetPath = DatabaseFolderPath + budgetToLoad.Name + "\\Budget" + budgetToLoad.Name + ".krs";
                budgetToLoad.ItemsPath = DatabaseFolderPath + budgetToLoad.Name + "\\Items" + budgetToLoad.Name + ".krs";
                budgetToLoad.CostsPath = DatabaseFolderPath + budgetToLoad.Name + "\\Costs" + budgetToLoad.Name + ".krs";
                budgetToLoad.AllTransactionsPath = DatabaseFolderPath + budgetToLoad.Name + "\\AllTransactions" + budgetToLoad.Name + ".krs";
                budgetToLoad.SummaryPath = DatabaseFolderPath + budgetToLoad.Name + "\\Summary" + budgetToLoad.Name + ".txt";

                return budgetToLoad;
            }
        }

        #region Start

        public void Start(Budget selectedBudget)
        {
            if (this.DecryptDatabaseFiles(selectedBudget))
            {
                this.SaveDatabase(selectedBudget);
                // Start UI
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else
            {
                // Try again.
                Encryption.IsPasswordWrong = false;
                this.Login();
                this.Start(selectedBudget);
            }
        }

        private void Login()
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

            // My check
            if (passwordInput.ToString().Length <= PasswordLength)
            {
                StringBuilder builder = new StringBuilder(passwordInput.ToString());

                for (int i = PasswordLength; i >= passwordInput.ToString().Length; i--)
                {
                    builder.Append("-");
                }

                this.userPassword = builder.ToString();
            }
            else if (passwordInput.ToString().Length >= PasswordLength + 2)
            {
                Console.WriteLine("Your password is too long.");
                this.Login();
            }
            else
            {
                this.userPassword = passwordInput.ToString();
            }
        }

        /// <summary>
        /// Decrypts the user database with the provided password.
        /// </summary>
        /// <returns>Return true on succesful decrypt.</returns>
        private bool DecryptDatabaseFiles(Budget selectedBudget)
        {
            // Database folder
            if (!Directory.Exists(DatabaseFolderPath))
            {
                Console.WriteLine("Database folder was missing so we created one for you.");
                Directory.CreateDirectory(DatabaseFolderPath);
            }

            // Budget file
            if (!File.Exists(selectedBudget.BudgetPath))
            {
                if (selectedBudget.Name == DefaultBudgetName)
                {
                    Console.Write("Set your spending budget: ");
                    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    Directory.CreateDirectory(DatabaseFolderPath + selectedBudget.Name);
                    File.Create(selectedBudget.BudgetPath).Dispose();
                }
                else
                {
                    Console.Write("Enter the name of your budget: ");
                    selectedBudget.Name = Console.ReadLine();

                    Console.Write("Set your spending budget: ");
                    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    selectedBudget.BudgetPath = DatabaseFolderPath + selectedBudget.Name + "\\Budget" + selectedBudget.Name + ".krs";
                    Directory.CreateDirectory(DatabaseFolderPath + selectedBudget.Name);
                    File.Create(selectedBudget.BudgetPath).Dispose();
                }
            }
            else
            {
                try
                {
                    const string PullDB = @"Scripts\PullDB.bat";

                    var process = Process.Start(PullDB);
                    process.WaitForExit();
                    this.PressEnterToContinue();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }

                try
                {
                    // To work the file should contain first the budget Amount and on the second line the name of the budget.
                    using StreamReader srBudget = new StreamReader(selectedBudget.BudgetPath);
                    selectedBudget.Amount = ParseHelper.ParseDouble(srBudget.ReadLine()!);
                    selectedBudget.Name = srBudget.ReadLine()!;
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
            if (!File.Exists(selectedBudget.ItemsPath))
            {
                Console.WriteLine("Items file was missing so we created one for you.");

                selectedBudget.ItemsPath = DatabaseFolderPath + selectedBudget.Name + "\\Items" + selectedBudget.Name + ".krs";
                File.Create(selectedBudget.ItemsPath).Dispose();
                this.fileLineCount = 0;
            }
            else
            {
                this.fileLineCount = File.ReadLines(selectedBudget.ItemsPath).Count();

                using StreamReader srItems = new StreamReader(selectedBudget.ItemsPath);
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
                            selectedBudget.UserInputItem.Add(decryptedString);
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
            if (!File.Exists(selectedBudget.CostsPath))
            {
                Console.WriteLine("Costs file was missing so we created one for you.");

                selectedBudget.CostsPath = DatabaseFolderPath + selectedBudget.Name + "\\Costs" + selectedBudget.Name + ".krs";
                File.Create(selectedBudget.CostsPath).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(selectedBudget.CostsPath);
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
                            selectedBudget.UserInputCost.Add(Convert.ToDouble(decryptedString));
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
            if (!File.Exists(selectedBudget.AllTransactionsPath))
            {
                Console.WriteLine("CurrentTransaction file was missing so we created one for you.");

                selectedBudget.AllTransactionsPath = DatabaseFolderPath + selectedBudget.Name + "\\AllTransactions" + selectedBudget.Name + ".krs";
                File.Create(selectedBudget.AllTransactionsPath).Dispose();
                this.allTransactionsLineCount = 0;
            }
            else
            {
                this.allTransactionsLineCount = File.ReadLines(selectedBudget.AllTransactionsPath).Count();

                using StreamReader trReader = new StreamReader(selectedBudget.AllTransactionsPath);
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
                            selectedBudget.AllUserTransactionFile.Add(decryptedString);
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

        private void ListDataBaseSummary(Budget selectedBudget)
        {
            Console.WriteLine("*********** {0} **********\n", selectedBudget.Name);

            double totalCosts = 0;
            for (int i = 0; i < this.fileLineCount; i++)
            {
                // This is used to add space between the amount of the item so they appear level.     
                Console.WriteLine(this.SeparatorHelper(selectedBudget.UserInputCost[i], 6) + selectedBudget.UserInputCost[i] + " " + selectedBudget.UserInputItem[i]);
                totalCosts += selectedBudget.UserInputCost[i];
            }

            Console.WriteLine("\n" + this.SeparatorHelper(totalCosts, 6) + totalCosts + " TOTAL SPENT");
            Console.WriteLine(this.SeparatorHelper(selectedBudget.Amount - totalCosts, 6) + (selectedBudget.Amount - totalCosts) + " Left of " + selectedBudget.Amount + " budgeted.");
            Console.WriteLine();
        }

        private string SeparatorHelper(double amount, int spaces)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < spaces; i++)
            {
                if (amount.ToString().Length == i)
                {
                    for (int j = 0; j <= (spaces - amount.ToString().Length); j++)
                    {
                        sb.Append(" ");
                    }
                }
            }

            return sb.ToString();
        }

        private void ShowLastTransactions(Budget selectedBudget)
        {
            Console.WriteLine("*********** List of recent transactions ***********");

            const int displayedItems = 6;

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

        // Here
        private void ShowMainMenu(Budget selectedBudget)
        {
            this.DisplayMenuChoices();

            this.ShowLastTransactions(selectedBudget);

            this.AskForUserMenuChoice(selectedBudget);
        }

        // Must be used with AskForUserMenuChoice method.
        private void DisplayMenuChoices()
        {
            Console.WriteLine("*********** Menu ***********");
            Console.WriteLine("Do you want to add another?\n" +
                "type 'y' to add new entry, \n" +
                "type 'e' to save and exit without uploading online, \n" +
                "type 'u' to save and exit and upload the database online, \n" +
                "type 'o' for other options. \n");
        }

        private void AskForUserMenuChoice(Budget selectedBudget)
        {
            Console.Write("Enter your choice: ");
            var userInput = Console.ReadKey();

            if (userInput.Key == ConsoleKey.Y)
            {
                this.AddOrUpdateItemList(selectedBudget);
                this.SaveDatabase(selectedBudget);
                Console.Clear();
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("\nExiting...");
                this.SaveDatabase(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                this.SaveDatabase(selectedBudget);

                Console.WriteLine("\nUploading...");
                this.UploadOnline();
                this.PressEnterToContinue();
            }
            else if (userInput.Key == ConsoleKey.O)
            {
                Console.Clear();
                this.ShowOptionsMenu(selectedBudget);
            }
            else
            {
                Console.Clear();
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
        }

        private void PressEnterToContinue()
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadKey(true);
            Console.Clear();
        }

        private void AddOrUpdateItemList(Budget selectedBudget)
        {
            Console.Write("\nHow much did you spend: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            Console.Write("What did you spend on: ");
            string itemInput = ParseHelper.ParseStringInput();

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < this.fileLineCount; i++)
            {
                if (itemInput == selectedBudget.UserInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    selectedBudget.UserInputCost[i] += costInput;
                }
            }

            if (!isDublicateItem)
            {
                selectedBudget.UserInputItem.Add(itemInput);
                selectedBudget.UserInputCost.Add(costInput);
                selectedBudget.TranasctionTime.Add(DateTime.Now.ToString());
                this.fileLineCount++;
            }

            // This is used to add space between the amount of the item so they appear level.
            selectedBudget.AllUserTransactionFile.Add(this.SeparatorHelper(costInput, 6) + costInput + " " + itemInput + "  " + DateTime.Now.ToString());
            this.allTransactionsLineCount++;
        }

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        private void SaveDatabase(Budget selectedBudget)
        {
            using (StreamWriter outputFile = new StreamWriter(selectedBudget.CostsPath))
            {
                for (int i = 0; i < this.fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedBudget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedBudget.ItemsPath))
            {
                for (int i = 0; i < this.fileLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedBudget.UserInputItem[i]);
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedBudget.AllTransactionsPath))
            {
                for (int i = 0; i < this.allTransactionsLineCount; i++)
                {
                    var encryptedString = Encryption.EncryptString(this.userPassword, selectedBudget.AllUserTransactionFile[i]);
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(selectedBudget.BudgetPath))
            {
                outputFile.WriteLine(selectedBudget.Amount);
                outputFile.WriteLine(selectedBudget.Name);
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
        private void ShowOptionsMenu(Budget selectedBudget)
        {
            Console.WriteLine("*********** Options ***********");
            Console.WriteLine("type 'x' to export database in readable form, \n" +
                "type 'r' to remove item from current list, \n" +
                "type 'i' to import csv file, \n" +
                "type 'c' to change the budget name and amount, \n" +
                "type 's' to switch to another budget, or create another one,\n" +
                "type 'a' to delete a budget, \n" +
                "type 'd' to DELETE ALL DATABASE, \n" +
                "press ESC to return to the main menu.");

            Console.WriteLine("Enter your choice: ");
            var userInput = Console.ReadKey();

            if (userInput.Key == ConsoleKey.X)
            {
                this.SaveDatabase(selectedBudget);
                this.ExportReadable(selectedBudget);
                Console.WriteLine("View your summary in " + selectedBudget.SummaryPath);
                this.PressEnterToContinue();

                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.R)
            {
                Console.Clear();
                Console.WriteLine("Remove which item?");
                this.RemoveItem(selectedBudget);
                Console.Clear();

                this.SaveDatabase(selectedBudget);
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.I)
            {
                Console.Clear();
                Console.WriteLine("\nImporting...");
                this.ImportCSV(selectedBudget);
                this.PressEnterToContinue();

                this.SaveDatabase(selectedBudget);
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.C)
            {
                this.ChangeNameAndAmount(selectedBudget);
                Console.Clear();

                this.SaveDatabase(selectedBudget);
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.S)
            {
                Console.WriteLine("\nSwitching budgets...\n");
                var budgetToLoad = this.SwitchBudget();
                if (budgetToLoad == "False")
                {
                    Console.WriteLine("Canceling...");
                    Console.Clear();
                    this.ShowOptionsMenu(selectedBudget);
                }
                else
                {
                    this.Start(this.LoadBudget(budgetToLoad));
                }
            }
            else if (userInput.Key == ConsoleKey.A)
            {
                Console.WriteLine("\nDeleting budget...");
                this.DeleteBudget();

                Console.Clear();
                Console.WriteLine("Loading the default budget...");
                this.Start(this.LoadBudget(null));
            }
            else if (userInput.Key == ConsoleKey.D)
            {
                Console.WriteLine("\nWARNING: THIS WILL DELETE ALL OF YOUR DATABASE!\n" +
                "TYPE 'Delete' IF YOU WANT TO CONTINUE?, TYPE 'abort' TO CANCEL");
                var textInput = Console.ReadLine();

                if (textInput == "Delete")
                {
                    Console.WriteLine("Deleting all database...");
                    Directory.Delete(DatabaseFolderPath, true);
                    Console.WriteLine("***************");
                    this.Start(selectedBudget);
                }
                else
                {
                    Console.WriteLine("Aborting...");
                    Console.Clear();
                    this.ListDataBaseSummary(selectedBudget);
                    this.ShowMainMenu(selectedBudget);
                }
            }
            else if (userInput.Key == ConsoleKey.Escape)
            {
                Console.Clear();
                this.ListDataBaseSummary(selectedBudget);
                this.ShowMainMenu(selectedBudget);
            }
            else
            {
                Console.Clear();
                this.ShowOptionsMenu(selectedBudget);
            }
        }

        /// <summary>
        /// Use if you want to export in txt readable for humans (not encrypted).
        /// </summary>
        private void ExportReadable(Budget selectedBudget)
        {
            // Fix method to use already established method of reading the list
            using StreamWriter outputFile = new StreamWriter(selectedBudget.SummaryPath);
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

        private void RemoveItem(Budget selectedBudget)
        {
            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + ": " + selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }
            Console.WriteLine(selectedBudget.UserInputItem.Count + ": Cancel.");

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());

            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                if (deleteItem == i)
                {
                    selectedBudget.AllUserTransactionFile.Add(selectedBudget.UserInputCost[i] + " " + selectedBudget.UserInputItem[i] + " " + DateTime.Now.ToString() + " Deleted. ");
                    this.allTransactionsLineCount++;
                    selectedBudget.UserInputItem.Remove(selectedBudget.UserInputItem[i]);
                    selectedBudget.UserInputCost.Remove(selectedBudget.UserInputCost[i]);
                    this.fileLineCount--;
                    break;
                }
                else if (deleteItem == selectedBudget.UserInputItem.Count)
                {
                    break;
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
                Console.WriteLine("!!! budget.csv file is missing !!!\n Canceling...");
            }
            else
            {
                try
                {
                    var csvTotalLines = File.ReadLines("budget.csv").Count();

                    using StreamReader srBudgetItems = new StreamReader("budget.csv");
                    {
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

        private void ChangeNameAndAmount(Budget selectedBudget)
        {
            Console.Write("Enter new name for the budget: ");
            selectedBudget.Name = Console.ReadLine();
            Console.Write("Set your new budget: ");
            selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());
        }

        private string SwitchBudget()
        {
            var dirList = Directory.GetDirectories(DatabaseFolderPath);

            for (int i = 0; i < dirList.Length; i++)
            {
                Console.WriteLine(i + ": " + dirList[i].Substring(dirList[i].IndexOf("\\") + 1));
            }
            Console.WriteLine((dirList.Length) + ": Add new budget.");
            Console.WriteLine(dirList.Length + 1 + ": Cancel.");

            Console.WriteLine("Enter your choice: ");
            var loadBudget = ParseHelper.ParseDouble(Console.ReadLine());

            if (loadBudget == dirList.Length)
            {
                return string.Empty;
            }
            else if (loadBudget == dirList.Length + 1)
            {
                return "False";
            }
            else
            {
                var name = dirList[(int)loadBudget].Substring(dirList[(int)loadBudget].IndexOf("\\") + 1);
                return name;
            }
        }

        private void DeleteBudget()
        {
            var dirList = Directory.GetDirectories(DatabaseFolderPath);

            for (int i = 0; i < dirList.Length; i++)
            {
                Console.WriteLine(i + ": " + dirList[i].Substring(dirList[i].IndexOf("\\") + 1));
            }
            Console.WriteLine((dirList.Length) + ": Cancel...");

            // This is for deleting...
            Console.Write("Enter the number of the budget you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());

            for (int i = 0; i < dirList.Length; i++)
            {
                if (deleteItem == i)
                {
                    Directory.Delete(dirList[i], true);
                    break;
                }
                else if (deleteItem == dirList.Length)
                {
                    break;
                }
            }
        }

        #endregion Options menu
    }
}