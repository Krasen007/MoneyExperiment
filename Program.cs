// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;

    public class Program
    {
        protected Program()
        {
        }

        private const string Paths = @"database\Summary.txt";
        private const string Items = @"database\items.krs";
        private const string Costs = @"database\costs.krs";

        private static readonly List<string> myInputItem = new List<string>();
        private static readonly List<double> myInputCost = new List<double>();
        private static int lineCount;
        private static string UserKey;

        private static void Main()
        {
            Console.WriteLine("Welcome!");

            Login();

            DecryptDataBaseFiles();

            ListDataBaseSummary();
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

                UserKey = builder.ToString();
            }
            else if (passwordInput.ToString().Length >= 33)
            {
                Console.WriteLine("Your password is too long");
                Login();
            }
            else
            {
                UserKey = passwordInput.ToString();
            }
        }

        private static void DecryptDataBaseFiles()
        {
            if (!Directory.Exists("database"))
            {
                Console.WriteLine("database folder was missing so we created one for you");
                Directory.CreateDirectory("database");
            }

            if (!File.Exists(Items))
            {
                Console.WriteLine("items file was missing so we created one for you");
                File.Create(Items).Dispose();
                lineCount = 0;
            }
            else
            {
                lineCount = File.ReadLines(Items).Count();
                try
                {   // Open the text file using a stream reader.
                    using (StreamReader srItems = new StreamReader(Items))
                    {
                        // Read the stream to a string, and write the string to the console.
                        for (int i = 0; i < lineCount; i++)
                        {
                            var decryptedString = AesOperation.DecryptString(UserKey, srItems.ReadLine());
                            myInputItem.Add(decryptedString);
                        }
                        srItems.Close();
                    }

                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }

            if (!File.Exists(Costs))
            {
                Console.WriteLine("costs file was missing so we created one for you");
                File.Create(Costs).Dispose();

            }
            else
            {
                try
                {
                    using (StreamReader srCosts = new StreamReader(Costs))
                    {
                        for (int i = 0; i < lineCount; i++)
                        {
                            var decryptedString = AesOperation.DecryptString(UserKey, srCosts.ReadLine());
                            myInputCost.Add(Convert.ToDouble(decryptedString));
                        }
                        srCosts.Close();
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void ListDataBaseSummary()
        {
            Console.WriteLine("Here is your summary: ");

            double result = 0;
            for (int i = 0; i < lineCount; i++)
            {
                Console.WriteLine(myInputItem[i] + " " + myInputCost[i]);
                result += myInputCost[i];
            }

            Console.WriteLine("Your spendings are: " + result);

            // Start
            AddToList();
        }

        private static void AddToList()
        {
            Console.WriteLine("Do you want to add another?, type 'y'to add, type 'e' for exit, \n" +
                "type 'x' to export database in readable form");
            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.Y)
            {
                UpdateList();
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                ExitAndSaveProgram();
            }
            else if (userInput.Key == ConsoleKey.X)
            {
                Console.WriteLine("View your summary in database/Summary.txt");
                ExportReadable();
            }
            else
            {
                Console.Clear();
                ListDataBaseSummary();
            }
        }

        private static void UpdateList()
        {
            Console.Write("For what did you spend: ");
            string itemInput = ParseHelper.ParseStringInput();

            Console.Write("How much did it cost: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            // Check if item is already in the database
            bool dublicateItem = false;
            for (int i = 0; i < lineCount; i++)
            {
                if (itemInput == myInputItem[i])
                {
                    dublicateItem = true;

                    // Only increase the cost if item is in the database
                    myInputCost[i] += costInput;
                }
            }

            if (dublicateItem)
            {
                //
                /// Do not add item
            }
            else
            {
                myInputItem.Add(itemInput);
                myInputCost.Add(costInput);
                lineCount++;
            }

            Console.Clear();
            ListDataBaseSummary();
        }

        /// <summary>
        /// Export the strings into encrypted files.
        /// </summary>
        private static void ExitAndSaveProgram()
        {
            Console.WriteLine("Bye bye");

            // Used for import
            using (StreamWriter outputFile = new StreamWriter(Costs))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(UserKey, myInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Used for import
            using (StreamWriter outputFile = new StreamWriter(Items))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(UserKey, myInputItem[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }
        }

        /// <summary>
        /// Use if you want to export in txt readable for humans.
        /// </summary>
        private static void ExportReadable()
        {
            using (StreamWriter outputFile = new StreamWriter(Paths))
            {
                outputFile.WriteLine("Here is your summary: ");

                for (int i = 0; i < lineCount; i++)
                {
                    outputFile.WriteLine(myInputItem[i] + " " + myInputCost[i]);
                }

                double result = 0;
                for (int i = 0; i < lineCount; i++)
                {
                    result += myInputCost[i];
                }

                outputFile.WriteLine("Your spendings are: " + result);
            }
        }
    }
}
