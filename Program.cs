﻿// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public static class Program
    {
        private const string SummaryPath = @"Database\Summary.txt";
        private const string ItemsPath = @"Database\Items.krs";
        private const string CostsPath = @"Database\Costs.krs";
        private const string BudgetPath = @"Database\Budget.krs";
        private const string DatabaseFolderPath = @"Database";

        private static readonly List<string> userInputItem = new List<string>();
        private static readonly List<double> userInputCost = new List<double>();
        private static double budgetAmount;
        private static string budgetName;
        
        private static int fileLineCount;
        private static string userPassword;

        private static void Main()
        {
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("*********** Welcome! ***********");
            Start();
        }

        public static void Start()
        {
            Login();

            if (DecryptDataBaseFiles())
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
        private static bool DecryptDataBaseFiles()
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
                budgetName = Console.ReadLine();

                Console.Write("Set your spending budget: ");
                budgetAmount = ParseHelper.ParseDouble(Console.ReadLine());
                File.Create(BudgetPath).Dispose();
            }
            else
            {
                try
                {
                    // To work the file should contain first the budget Amount and on the second line the name of the budget.
                    using StreamReader srBudget = new StreamReader(BudgetPath);
                    budgetAmount = ParseHelper.ParseDouble(srBudget.ReadLine());
                    budgetName = srBudget.ReadLine();
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
                        var decryptedString = AesOperation.DecryptString(userPassword, srItems.ReadLine());
                        if (AesOperation.IsWrongPassword)
                        {
                            break;
                        }
                        else
                        {
                            userInputItem.Add(decryptedString);
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
                        var decryptedString = AesOperation.DecryptString(userPassword, srCosts.ReadLine());
                        if (AesOperation.IsWrongPassword)
                        {
                            break;
                        }
                        else
                        {
                            userInputCost.Add(Convert.ToDouble(decryptedString));
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
            Console.WriteLine("*********** {0} **********", budgetName);

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                string separator;
                if (userInputCost[i].ToString().Length == 1)
                {
                    separator = "       ";
                }
                else if (userInputCost[i].ToString().Length == 2)
                {
                    separator = "      ";
                }
                else if (userInputCost[i].ToString().Length == 3)
                {
                    separator = "     ";
                }
                else if (userInputCost[i].ToString().Length == 4)
                {
                    separator = "    ";
                }
                else if (userInputCost[i].ToString().Length == 5)
                {
                    separator = "   ";
                }
                else if (userInputCost[i].ToString().Length == 6)
                {
                    separator = "  ";
                }
                else
                {
                    separator = " ";
                }

                Console.WriteLine(separator + userInputCost[i] + " " + userInputItem[i]);
                totalCosts += userInputCost[i];
            }

            Console.WriteLine("\nYour spendings are: " + totalCosts);
            Console.WriteLine("Your budget of " + budgetAmount + " is now left with total: " + (budgetAmount - totalCosts));
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
                budgetName = Console.ReadLine();
                Console.Write("Set your new budget: ");
                budgetAmount = ParseHelper.ParseDouble(Console.ReadLine());

                Console.Clear();
                SaveDatabase();
                ListDataBaseSummary();
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
                if (itemInput == userInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    userInputCost[i] += costInput;
                }
            }

            if (!isDublicateItem)
            {
                userInputItem.Add(itemInput);
                userInputCost.Add(costInput);
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
                    var encryptedString = AesOperation.EncryptString(userPassword, userInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(ItemsPath))
            {
                for (int i = 0; i < fileLineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(userPassword, userInputItem[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(BudgetPath))
            {
                outputFile.WriteLine(budgetAmount);
                outputFile.WriteLine(budgetName);
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
            outputFile.WriteLine("*********** {0} **********", budgetName);

            for (int i = 0; i < fileLineCount; i++)
            {
                outputFile.WriteLine(userInputItem[i] + " " + userInputCost[i]);
            }

            double totalCosts = 0;
            for (int i = 0; i < fileLineCount; i++)
            {
                totalCosts += userInputCost[i];
            }

            outputFile.WriteLine("\nYour spendings are: " + totalCosts);
            outputFile.WriteLine("Your amount left on budget is: " + (budgetAmount - totalCosts));

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
            for (int i = 0; i < userInputItem.Count; i++)
            {
                Console.WriteLine(i + ": " + userInputItem[i] + " " + userInputCost[i]);
            }
            Console.WriteLine(userInputItem.Count + ": Abort.");

            Console.Write("Enter the number of the item you want to remove: ");
            var deleteItem = ParseHelper.ParseDouble(Console.ReadLine());


            for (int i = 0; i < userInputItem.Count; i++)
            {
                if (deleteItem == i)
                {
                    userInputItem.Remove(userInputItem[i]);
                    userInputCost.Remove(userInputCost[i]);
                    fileLineCount--;
                    break;
                }
                else if (deleteItem == userInputItem.Count)
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
            List<string> csvItems = new List<string>();

            try
            {
                var csvTotalLines = File.ReadLines("budget.csv").Count();

                using StreamReader srBudgetItems = new StreamReader("budget.csv");
                {
                    for (int i = 0; i < csvTotalLines; i++)
                    {
                        csvItems.Add(srBudgetItems.ReadLine());

                        string itemName = csvItems[i].Remove(csvItems[i].IndexOf(','));
                        string itemCost = csvItems[i].Remove(0, csvItems[i].IndexOf(',') + 1);

                        userInputItem.Add(itemName);
                        userInputCost.Add(Convert.ToDouble(itemCost));
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
