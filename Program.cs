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
        private const string DatabaseFolderPath = @"Database";
        private static string SummaryPath = string.Empty;
        private static string ItemsPath = string.Empty;
        private static string CostsPath = string.Empty;
        private static string BudgetPath = string.Empty;

        /// Add option to add multiple budgets
        private static readonly List<Budget> budgetList = new List<Budget>();

        private static readonly Budget defaultBudget = new Budget();

        private static int fileLineCount;
        private static string userPassword = string.Empty;

        private static void Main()
        {
            // Prep
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            SummaryPath = @"Database\Summary" + defaultBudget.BudgetName + ".txt";
            ItemsPath = @"Database\Items" + defaultBudget.BudgetName + ".krs";
            CostsPath = @"Database\Costs" + defaultBudget.BudgetName + ".krs";
            BudgetPath = @"Database\Budget" + defaultBudget.BudgetName + ".krs";

            budgetList.Add(defaultBudget);

            // Start
            Console.WriteLine("*********** Welcome! ***********");
            Start();
        }

        public static void Start()
        {
            Login();

            if (DecryptDatabaseFiles())
            {
                ListDataBaseSummary();
            }
            else
            {
                // Try again.
                AesOperation.IsWrongPassword = false;
                Start();
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
        private static bool DecryptDatabaseFiles()
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
            if (!File.Exists(BudgetPath))
            {
                Console.Write("Enter the name of your budget: ");
                defaultBudget.BudgetName = Console.ReadLine();

                Console.Write("Set your spending budget: ");
                defaultBudget.BudgetAmount = ParseHelper.ParseDouble(Console.ReadLine());
                File.Create(BudgetPath).Dispose();
            }
            else
            {
                try
                {
                    // To work the file should contain first the budget Amount and on the second line the name of the budget.
                    using StreamReader srBudget = new StreamReader(BudgetPath);
                    defaultBudget.BudgetAmount = ParseHelper.ParseDouble(srBudget.ReadLine()!);
                    defaultBudget.BudgetName = srBudget.ReadLine()!;
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
            if (!File.Exists(ItemsPath))
            {
                Console.WriteLine("Items file was missing so we created one for you.");
                File.Create(ItemsPath).Dispose();
                fileLineCount = 0;
            }
            else
            {
                fileLineCount = File.ReadLines(ItemsPath).Count();

                using StreamReader srItems = new StreamReader(ItemsPath);
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
                            defaultBudget.UserInputItem.Add(decryptedString);
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
            if (!File.Exists(CostsPath))
            {
                Console.WriteLine("Costs file was missing so we created one for you.");
                File.Create(CostsPath).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(CostsPath);
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
                            defaultBudget.UserInputCost.Add(Convert.ToDouble(decryptedString));
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
                return true;
            }
        }

        private static void ListDataBaseSummary()
        {
            Console.WriteLine("*********** {0} **********\n", defaultBudget.BudgetName);

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                // This is used to add space between the amount of the item so they appear level.
                string separator;
                if (defaultBudget.UserInputCost[i].ToString().Length == 1)
                {
                    separator = "       ";
                }
                else if (defaultBudget.UserInputCost[i].ToString().Length == 2)
                {
                    separator = "      ";
                }
                else if (defaultBudget.UserInputCost[i].ToString().Length == 3)
                {
                    separator = "     ";
                }
                else if (defaultBudget.UserInputCost[i].ToString().Length == 4)
                {
                    separator = "    ";
                }
                else if (defaultBudget.UserInputCost[i].ToString().Length == 5)
                {
                    separator = "   ";
                }
                else if (defaultBudget.UserInputCost[i].ToString().Length == 6)
                {
                    separator = "  ";
                }
                else
                {
                    separator = " ";
                }

                Console.WriteLine(separator + defaultBudget.UserInputCost[i] + " " + defaultBudget.UserInputItem[i]);
                totalCosts += defaultBudget.UserInputCost[i];
            }

            Console.WriteLine("\nYour spendings are: " + totalCosts);
            Console.WriteLine("Your budget of " + defaultBudget.BudgetAmount + " is now left with total: " + (defaultBudget.BudgetAmount - totalCosts));
            Console.WriteLine();

            // Start
            ShowMainMenu();
        }

        private static void ShowMainMenu()
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
                AddOrUpdateItemList();
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("Exiting...");
                SaveDatabase();
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                Console.WriteLine("Uploading...");
                UploadOnline();
            }
            else if (userInput.Key == ConsoleKey.O)
            {
                Console.Clear();
                ShowOptionsMenu();
            }
            else
            {
                Console.Clear();
                ListDataBaseSummary();
            }
        }

        private static void ShowOptionsMenu()
        {
            Console.WriteLine("*********** Options ***********");
            Console.WriteLine("type 'x' to export database in readable form, \n" +
                "type 'r' to remove item from list, \n" +
                "type 'i' to import csv file, \n" +
                "type 'o' to change the budget name and amount, \n" +
                "type 's' to switch to another budget, \n" +
                "type 'd' to DELETE ALL DATABASE, \n" +
                "press ESC to return to the main menu.");

            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.X)
            {
                Console.WriteLine("View your summary in " + SummaryPath);
                ExportReadable();
            }
            else if (userInput.Key == ConsoleKey.R)
            {
                Console.WriteLine("Removing...");
                RemoveItem();
            }
            else if (userInput.Key == ConsoleKey.I)
            {
                Console.WriteLine("Importing...");
                ImportCSV();
            }
            else if (userInput.Key == ConsoleKey.O)
            {
                Console.Write("Enter new name for the budget: ");
                defaultBudget.BudgetName = Console.ReadLine();
                Console.Write("Set your new budget: ");
                defaultBudget.BudgetAmount = ParseHelper.ParseDouble(Console.ReadLine());

                Console.Clear();
                SaveDatabase();
                ListDataBaseSummary();
            }
            else if (userInput.Key == ConsoleKey.S)
            {
                Console.WriteLine("Switching budgets...");
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
                    Start();
                }
                else
                {
                    Console.WriteLine("Aborting...");
                    Console.Clear();
                    ListDataBaseSummary();
                }
            }
            else if (userInput.Key == ConsoleKey.Escape)
            {
                Console.Clear();
                ListDataBaseSummary();
            }
            else
            {
                Console.Clear();
                ShowOptionsMenu();
            }
        }


        // Not working yet...
        private static void SwitchBudget()
        {
            for (int i = 0; i < budgetList.Count; i++)
            {
                Console.WriteLine(i + ": " + budgetList[i].BudgetName);
            }
            Console.WriteLine(budgetList.Count + ": Abort.");

            Console.Write("Enter the number of the budget you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());


            for (int i = 0; i < budgetList.Count; i++)
            {
                if (deleteItem == i)
                {
                    budgetList.Remove(budgetList[i]);
                    break;
                }
                else if (deleteItem == budgetList.Count)
                {
                    break;
                }
            }

            Console.Clear();
            SaveDatabase();
            ListDataBaseSummary();
        }

        private static void AddOrUpdateItemList()
        {
            Console.Write("How much did you spend: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            Console.Write("What did you spend on: ");
            string itemInput = ParseHelper.ParseStringInput();

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < fileLineCount; i++)
            {
                if (itemInput == defaultBudget.UserInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    defaultBudget.UserInputCost[i] += costInput;
                }
            }

            if (!isDublicateItem)
            {
                defaultBudget.UserInputItem.Add(itemInput);
                defaultBudget.UserInputCost.Add(costInput);
                fileLineCount++;
            }

            SaveDatabase();
            Console.Clear();
            ListDataBaseSummary();
        }

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        private static void SaveDatabase()
        {
            using (StreamWriter outputFile = new StreamWriter(CostsPath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(userPassword, defaultBudget.UserInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(ItemsPath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(userPassword, defaultBudget.UserInputItem[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(BudgetPath))
            {
                outputFile.WriteLine(defaultBudget.BudgetAmount);
                outputFile.WriteLine(defaultBudget.BudgetName);
            }
        }

        /// <summary>
        /// Use if you want to export in txt readable for humans (not encrypted).
        /// </summary>
        private static void ExportReadable()
        {
            // Fix method to use already established method of reading the list
            SaveDatabase();

            using StreamWriter outputFile = new StreamWriter(SummaryPath);
            outputFile.WriteLine("*********** {0} **********", defaultBudget.BudgetName);

            for (int i = 0; i < fileLineCount; i++)
            {
                outputFile.WriteLine(defaultBudget.UserInputItem[i] + " " + defaultBudget.UserInputCost[i]);
            }

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                totalCosts += defaultBudget.UserInputCost[i];
            }

            outputFile.WriteLine("\nYour spendings are: " + totalCosts);
            outputFile.WriteLine("Your amount left on budget is: " + (defaultBudget.BudgetAmount - totalCosts));

            outputFile.Dispose();
        }

        private static void PullDatabase()
        {
            const string PullDB = @"Scripts\PullDB.bat";

            var process = Process.Start(PullDB);
            process.WaitForExit();
            Console.Clear();
        }

        private static void UploadOnline()
        {
            SaveDatabase();

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

        private static void RemoveItem()
        {
            for (int i = 0; i < defaultBudget.UserInputItem.Count; i++)
            {
                Console.WriteLine(i + ": " + defaultBudget.UserInputItem[i] + " " + defaultBudget.UserInputCost[i]);
            }
            Console.WriteLine(defaultBudget.UserInputItem.Count + ": Abort.");

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());


            for (int i = 0; i < defaultBudget.UserInputItem.Count; i++)
            {
                if (deleteItem == i)
                {
                    defaultBudget.UserInputItem.Remove(defaultBudget.UserInputItem[i]);
                    defaultBudget.UserInputCost.Remove(defaultBudget.UserInputCost[i]);
                    fileLineCount--;
                    break;
                }
                else if (deleteItem == defaultBudget.UserInputItem.Count)
                {
                    break;
                }
            }

            Console.Clear();
            SaveDatabase();
            ListDataBaseSummary();
        }

        /// <summary>
        /// This method imports budget.csv file that is based only with 2 items and is split by ','
        /// </summary>
        private static void ImportCSV()
        {
            if (!File.Exists("budget.csv"))
            {
                Console.WriteLine("!!! budget.csv file is missing !!!\n Aborting...");
                ListDataBaseSummary();
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

                            defaultBudget.UserInputItem.Add(itemName);
                            defaultBudget.UserInputCost.Add(Convert.ToDouble(itemCost));
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
                SaveDatabase();
                ListDataBaseSummary();
            }
        }
    }
}
