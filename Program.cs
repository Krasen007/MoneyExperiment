// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;

    public class Program
    {
        protected Program()
        {
        }

        private const string Paths = @"Database\Summary.txt";
        private const string Items = @"Database\Items.krs";
        private const string Costs = @"Database\Costs.krs";

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
                Console.WriteLine("Database folder was missing so we created one for you");
                Directory.CreateDirectory("database");
            }
            else
            {
                PullDatabase();
            }

            if (!File.Exists(Items))
            {
                Console.WriteLine("Items file was missing so we created one for you");
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
                Console.WriteLine("Costs file was missing so we created one for you");
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
            Console.WriteLine();

            // Start
            AddToList();
        }

        private static void AddToList()
        {
            Console.WriteLine("Do you want to add another?\n" +
                "type 'y' to add entry, \n" +
                "type 'e' for exit, \n" +
                "type 'x' to export database in readable form, \n" +
                "type 'u' to upload database online.");
            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.Y)
            {
                UpdateList();
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("Exiting...");
                SaveDatabase();
            }
            else if (userInput.Key == ConsoleKey.X)
            {
                Console.WriteLine("View your summary in database/Summary.txt");
                ExportReadable();
            }
            else if (userInput.Key == ConsoleKey.U)
            {
                Console.WriteLine("Uploading...");
                UploadOnline();
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
        private static void SaveDatabase()
        {
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
            SaveDatabase();

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

        private static void UploadOnline()
        {
            SaveDatabase();

            const string CreateDB = @"CreateDB.bat";
            const string UpdateDB = @"UpdateDB.bat";

            if (Directory.Exists(@".git"))
            {
                var process = Process.Start(UpdateDB);
                process.WaitForExit();
            }
            else
            {
                var process = Process.Start(CreateDB);
                process.WaitForExit();
            }
        }

        private static void PullDatabase()
        {
            const string PullDB = @"PullDB.bat";

            var process = Process.Start(PullDB);            
            process.WaitForExit();
            Console.Clear();
        }
    }
}
