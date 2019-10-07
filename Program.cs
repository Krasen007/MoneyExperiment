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

    public static class Program
    {
        private const string DatabaseFolderPath = @"Database\";

        private static int fileLineCount;
        private static string userPassword = string.Empty;

        private static void Main()
        {
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            Console.WriteLine("*********** Welcome! ***********");
            LoadBudget(null);
        }

        private static void LoadBudget(string? name)
        {
            if (name == null)
            {
                Budget budgetToLoad = new Budget
                {
                    Name = "Budget"
                };
                budgetToLoad.BudgetPath = @"Database\" + budgetToLoad.Name + "\\Budget" + budgetToLoad.Name + ".krs";
                budgetToLoad.ItemsPath = @"Database\" + budgetToLoad.Name + "\\Items" + budgetToLoad.Name + ".krs";
                budgetToLoad.CostsPath = @"Database\" + budgetToLoad.Name + "\\Costs" + budgetToLoad.Name + ".krs";

                Start(budgetToLoad);
            }
            else
            {
                Budget budgetToLoad = new Budget
                {
                    Name = name
                };
                budgetToLoad.BudgetPath = @"Database\" + budgetToLoad.Name + "\\Budget" + budgetToLoad.Name + ".krs";
                budgetToLoad.ItemsPath = @"Database\" + budgetToLoad.Name + "\\Items" + budgetToLoad.Name + ".krs";
                budgetToLoad.CostsPath = @"Database\" + budgetToLoad.Name + "\\Costs" + budgetToLoad.Name + ".krs";

                Start(budgetToLoad);
            }
        }

        public static void Start(Budget selectedBudget)
        {
            Login();

            if (DecryptDatabaseFiles(selectedBudget))
            {
                ListDataBaseSummary(selectedBudget);
            }
            else
            {
                // Try again.
                AesOperation.IsWrongPassword = false;
                Start(selectedBudget);
            }
        }

        private static void Login()
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
            if (passwordInput.ToString().Length <= 31)
            {
                StringBuilder builder = new StringBuilder(passwordInput.ToString());

                for (int i = 31; i >= passwordInput.ToString().Length; i--)
                {
                    builder.Append("-");
                }

                userPassword = builder.ToString();
            }
            else if (passwordInput.ToString().Length >= 33)
            {
                Console.WriteLine("Your password is too long.");
                Login();
            }
            else
            {
                userPassword = passwordInput.ToString();
            }
        }

        /// <summary>
        /// Decrypts the user database with the provided password.
        /// </summary>
        /// <returns>Return true on succesful decrypt.</returns>
        private static bool DecryptDatabaseFiles(Budget selectedBudget)
        {
            // Database folder
            if (!Directory.Exists(DatabaseFolderPath))
            {
                Console.WriteLine("Database folder was missing so we created one for you.");
                Directory.CreateDirectory(DatabaseFolderPath);
            }
            else
            {
                PullDatabase();
            }

            // Budget file
            if (!File.Exists(selectedBudget.BudgetPath))
            {
                if (selectedBudget.Name == "Budget")
                {
                    Console.Write("Set your spending budget: ");
                    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    Directory.CreateDirectory(@"Database\" + selectedBudget.Name);
                    File.Create(selectedBudget.BudgetPath).Dispose();
                }
                else
                {
                    Console.Write("Enter the name of your budget: ");
                    selectedBudget.Name = Console.ReadLine();

                    Console.Write("Set your spending budget: ");
                    selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                    selectedBudget.BudgetPath = @"Database\" + selectedBudget.Name + "\\Budget" + selectedBudget.Name + ".krs";
                    Directory.CreateDirectory(@"Database\" + selectedBudget.Name);
                    File.Create(selectedBudget.BudgetPath).Dispose();
                }
            }
            else
            {
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

                selectedBudget.ItemsPath = @"Database\" + selectedBudget.Name + "\\Items" + selectedBudget.Name + ".krs";
                File.Create(selectedBudget.ItemsPath).Dispose();
                fileLineCount = 0;
            }
            else
            {
                fileLineCount = File.ReadLines(selectedBudget.ItemsPath).Count();

                using StreamReader srItems = new StreamReader(selectedBudget.ItemsPath);
                try
                {
                    for (int i = 0; i < fileLineCount; i++)
                    {
                        var decryptedString = AesOperation.DecryptString(userPassword, srItems.ReadLine()!);
                        if (AesOperation.IsWrongPassword)
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

                selectedBudget.CostsPath = @"Database\" + selectedBudget.Name + "\\Costs" + selectedBudget.Name + ".krs";
                File.Create(selectedBudget.CostsPath).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(selectedBudget.CostsPath);
                try
                {
                    for (int i = 0; i < fileLineCount; i++)
                    {
                        var decryptedString = AesOperation.DecryptString(userPassword, srCosts.ReadLine()!);
                        if (AesOperation.IsWrongPassword)
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

            // Succesfully read needed files
            if (AesOperation.IsWrongPassword)
            {
                Console.WriteLine("Wrong password.");
                return false;
            }
            else
            {
                SaveDatabase(selectedBudget);
                return true;
            }
        }

        private static void ListDataBaseSummary(Budget selectedBudget)
        {
            Console.WriteLine("*********** {0} **********\n", selectedBudget.Name);

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                // This is used to add space between the amount of the item so they appear level.
                string separator;
                if (selectedBudget.UserInputCost[i].ToString().Length == 1)
                {
                    separator = "       ";
                }
                else if (selectedBudget.UserInputCost[i].ToString().Length == 2)
                {
                    separator = "      ";
                }
                else if (selectedBudget.UserInputCost[i].ToString().Length == 3)
                {
                    separator = "     ";
                }
                else if (selectedBudget.UserInputCost[i].ToString().Length == 4)
                {
                    separator = "    ";
                }
                else if (selectedBudget.UserInputCost[i].ToString().Length == 5)
                {
                    separator = "   ";
                }
                else if (selectedBudget.UserInputCost[i].ToString().Length == 6)
                {
                    separator = "  ";
                }
                else
                {
                    separator = " ";
                }

                Console.WriteLine(separator + selectedBudget.UserInputCost[i] + " " + selectedBudget.UserInputItem[i]);
                totalCosts += selectedBudget.UserInputCost[i];
            }

            Console.WriteLine("\nYour spendings are: " + totalCosts);
            Console.WriteLine("Your budget of " + selectedBudget.Amount + " is now left with total: " + (selectedBudget.Amount - totalCosts));
            Console.WriteLine();

            // Start
            ShowMainMenu(selectedBudget);
        }

        private static void ShowMainMenu(Budget selectedBudget)
        {
            Console.WriteLine("*********** Menu ***********");
            Console.WriteLine("Do you want to add another?\n" +
                "type 'y' to add new entry, \n" +
                "type 'e' to save and exit without uploading online, \n" +
                "type 'u' to save and exit and upload the database online, \n" +
                "type 'o' for other options.");

            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.Y)
            {
                AddOrUpdateItemList(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("Exiting...");
                SaveDatabase(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                Console.WriteLine("Uploading...");
                UploadOnline(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.O)
            {
                Console.Clear();
                ShowOptionsMenu(selectedBudget);
            }
            else
            {
                Console.Clear();
                ListDataBaseSummary(selectedBudget);
            }
        }

        private static void ShowOptionsMenu(Budget selectedBudget)
        {
            Console.WriteLine("*********** Options ***********");
            Console.WriteLine("type 'x' to export database in readable form, \n" +
                "type 'r' to remove item from list, \n" +
                "type 'i' to import csv file, \n" +
                "type 'c' to change the budget name and amount, \n" +
                "type 's' to switch to another budget, \n" +
                "type 'd' to DELETE ALL DATABASE, \n" +
                "press ESC to return to the main menu.");

            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.X)
            {
                Console.WriteLine("View your summary in " + selectedBudget.SummaryPath);
                ExportReadable(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.R)
            {
                Console.WriteLine("Removing...");
                RemoveItem(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.I)
            {
                Console.WriteLine("Importing...");
                ImportCSV(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.C)
            {
                Console.Write("Enter new name for the budget: ");
                selectedBudget.Name = Console.ReadLine();
                Console.Write("Set your new budget: ");
                selectedBudget.Amount = ParseHelper.ParseDouble(Console.ReadLine());

                Console.Clear();
                SaveDatabase(selectedBudget);
                ListDataBaseSummary(selectedBudget);
            }
            else if (userInput.Key == ConsoleKey.S)
            {
                Console.WriteLine("Switching budgets...");
                // Probably add List<Budgets> selected budgts?
                SwitchBudget();
            }
            else if (userInput.Key == ConsoleKey.D)
            {
                Console.WriteLine("WARNING: THIS WILL DELETE ALL OF YOUR DATABASE!\n" +
                "TYPE 'Delete' IF YOU WANT TO CONTINUE?, TYPE 'abort' TO CANCEL");
                var textInput = Console.ReadLine();

                if (textInput == "Delete")
                {
                    Console.WriteLine("Deleting all database...");
                    Directory.Delete(DatabaseFolderPath, true);
                    Console.WriteLine("***************");
                    Start(selectedBudget);
                }
                else
                {
                    Console.WriteLine("Aborting...");
                    Console.Clear();
                    ListDataBaseSummary(selectedBudget);
                }
            }
            else if (userInput.Key == ConsoleKey.Escape)
            {
                Console.Clear();
                ListDataBaseSummary(selectedBudget);
            }
            else
            {
                Console.Clear();
                ShowOptionsMenu(selectedBudget);
            }
        }

        // Not working properly yet...
        private static void SwitchBudget()
        {
            var dirList = Directory.GetDirectories(DatabaseFolderPath);

            for (int i = 0; i < dirList.Length; i++)
            {
                Console.WriteLine(i + ": " + dirList[i].Substring(dirList[i].IndexOf("\\") + 1));
            }
            Console.WriteLine((dirList.Length) + ": Add new budget.");


            Console.WriteLine("What buget to load?");
            var loadBudget = ParseHelper.ParseDouble(Console.ReadLine());

            if (loadBudget == dirList.Length)
            {
                LoadBudget(string.Empty);
            }
            else
            {
                var name = dirList[(int)loadBudget].Substring(dirList[(int)loadBudget].IndexOf("\\") + 1);
                LoadBudget(name);
            }

            //// This is for deleting...
            ////Console.Write("Enter the number of the budget you want to remove: ");
            ////var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());


            ////for (int i = 0; i < budgetList.Count; i++)
            ////{
            ////    if (deleteItem == i)
            ////    {
            ////        budgetList.Remove(budgetList[i]);
            ////        break;
            ////    }
            ////    else if (deleteItem == budgetList.Count)
            ////    {
            ////        break;
            ////    }
            ////}

            ///Console.Clear();
            ///SaveDatabase(selectedBudget);
            ///ListDataBaseSummary(selectedBudget);
        }

        private static void AddOrUpdateItemList(Budget selectedBudget)
        {
            Console.Write("How much did you spend: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            Console.Write("What did you spend on: ");
            string itemInput = ParseHelper.ParseStringInput();

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < fileLineCount; i++)
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
                fileLineCount++;
            }

            SaveDatabase(selectedBudget);
            Console.Clear();
            ListDataBaseSummary(selectedBudget);
        }

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        private static void SaveDatabase(Budget selectedBudget)
        {
            using (StreamWriter outputFile = new StreamWriter(selectedBudget.CostsPath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(userPassword, selectedBudget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(selectedBudget.ItemsPath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(userPassword, selectedBudget.UserInputItem[i].ToString());
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

        /// <summary>
        /// Use if you want to export in txt readable for humans (not encrypted).
        /// </summary>
        private static void ExportReadable(Budget selectedBudget)
        {
            // Fix method to use already established method of reading the list
            SaveDatabase(selectedBudget);
            selectedBudget.SummaryPath = @"Database\" + selectedBudget.Name + "\\Summary" + selectedBudget.Name + ".txt";

            using StreamWriter outputFile = new StreamWriter(selectedBudget.SummaryPath);
            outputFile.WriteLine("*********** {0} **********", selectedBudget.Name);

            for (int i = 0; i < fileLineCount; i++)
            {
                outputFile.WriteLine(selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                totalCosts += selectedBudget.UserInputCost[i];
            }

            outputFile.WriteLine("\nYour spendings are: " + totalCosts);
            outputFile.WriteLine("Your amount left on budget is: " + (selectedBudget.Amount - totalCosts));

            outputFile.Dispose();
        }

        private static void PullDatabase()
        {
            const string PullDB = @"Scripts\PullDB.bat";

            var process = Process.Start(PullDB);
            process.WaitForExit();
            Console.Clear();
        }

        private static void UploadOnline(Budget selectedBudget)
        {
            SaveDatabase(selectedBudget);

            const string InitCreateDB = @"Scripts\InitCreateDB.bat";
            const string PushUpdateDB = @"Scripts\PushUpdateDB.bat";

            if (Directory.Exists(@".git"))
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

        private static void RemoveItem(Budget selectedBudget)
        {
            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + ": " + selectedBudget.UserInputItem[i] + " " + selectedBudget.UserInputCost[i]);
            }
            Console.WriteLine(selectedBudget.UserInputItem.Count + ": Abort.");

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());


            for (int i = 0; i < selectedBudget.UserInputItem.Count; i++)
            {
                if (deleteItem == i)
                {
                    selectedBudget.UserInputItem.Remove(selectedBudget.UserInputItem[i]);
                    selectedBudget.UserInputCost.Remove(selectedBudget.UserInputCost[i]);
                    fileLineCount--;
                    break;
                }
                else if (deleteItem == selectedBudget.UserInputItem.Count)
                {
                    break;
                }
            }

            Console.Clear();
            SaveDatabase(selectedBudget);
            ListDataBaseSummary(selectedBudget);
        }

        /// <summary>
        /// This method imports budget.csv file that is based only with 2 items and is split by ','
        /// </summary>
        private static void ImportCSV(Budget selectedBudget)
        {
            if (!File.Exists("budget.csv"))
            {
                Console.WriteLine("!!! budget.csv file is missing !!!\n Aborting...");
                ListDataBaseSummary(selectedBudget);
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
                            fileLineCount++;
                        }

                    }
                    srBudgetItems.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("You need to add budget.csv in the main folder");
                }
                SaveDatabase(selectedBudget);
                ListDataBaseSummary(selectedBudget);
            }
        }
    }
}
