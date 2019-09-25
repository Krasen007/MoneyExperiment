// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Program
    {
        protected Program()
        {
        }

        private const string Paths = @"Database\Summary.txt";
        private const string Items = @"Database\Items.krs";
        private const string Costs = @"Database\Costs.krs";
        private const string Budget = @"Database\Budget.krs";
        private const string Database = @"Database";


        private static readonly List<string> myInputItem = new List<string>();
        private static readonly List<double> myInputCost = new List<double>();
        private static double myBudget;

        private static int lineCount;
        private static string UserKey;

        private static void Main()
        {
            Console.WriteLine("*********** Welcome! ***********");
            Start();
            Console.WriteLine("Start method completed");
            Console.ReadKey();
        }

        public static void Start()
        {
            try
            {
                Login();
                Console.WriteLine("Login success");
                Console.ReadKey();

                if (!DecryptDataBaseFiles())
                {
                    Console.WriteLine("DDBFiles failed");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    ListDataBaseSummary();
                    Console.WriteLine("LDBS success.");
                    Console.ReadKey();
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Error on start.");
                // Start();
                Console.ReadKey();
                throw;
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

                UserKey = builder.ToString();
            }
            else if (passwordInput.ToString().Length >= 33)
            {
                Console.WriteLine("Your password is too long.");
                Login();
            }
            else
            {
                UserKey = passwordInput.ToString();
            }
        }

        private static bool DecryptDataBaseFiles()
        {
            if (!File.Exists(Budget))
            {
                Console.Write("Set your spending budget: ");
                myBudget = ParseHelper.ParseDouble(Console.ReadLine());
                File.Create(Budget).Dispose();
            }
            else
            {
                try
                {
                    using StreamReader srCosts = new StreamReader(Budget);
                    myBudget = ParseHelper.ParseDouble(srCosts.ReadLine());
                    srCosts.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine("The budget file could not be read: ");
                    Console.WriteLine(error.Message);
                    return false;
                }
            }

            // Database folder
            if (!Directory.Exists(Database))
            {
                Console.WriteLine("Database folder was missing so we created one for you.");
                Directory.CreateDirectory(Database);
            }
            else
            {
                PullDatabase();
            }

            // Items
            if (!File.Exists(Items))
            {
                Console.WriteLine("Items file was missing so we created one for you.");
                File.Create(Items).Dispose();
                lineCount = 0;
            }
            else
            {
                lineCount = File.ReadLines(Items).Count();

                using StreamReader srItems = new StreamReader(Items);
                try
                {
                    for (int i = 0; i < lineCount; i++)
                    {
                        var decryptedString = AesOperation.DecryptString(UserKey, srItems.ReadLine());
                        myInputItem.Add(decryptedString);
                    }
                    srItems.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine("The items file could not be read: ");
                    Console.WriteLine(error.Message);
                    srItems.Dispose();
                    return false;
                }
            }

            // Costs
            if (!File.Exists(Costs))
            {
                Console.WriteLine("Costs file was missing so we created one for you.");
                File.Create(Costs).Dispose();
            }
            else
            {
                using StreamReader srCosts = new StreamReader(Costs);
                try
                {
                    for (int i = 0; i < lineCount; i++)
                    {
                        var decryptedString = AesOperation.DecryptString(UserKey, srCosts.ReadLine());
                        myInputCost.Add(Convert.ToDouble(decryptedString));
                    }
                    srCosts.Close();
                }
                catch (IOException error)
                {
                    Console.WriteLine("The costs file could not be read: ");
                    Console.WriteLine(error.Message);
                    srCosts.Dispose();
                    return false;
                }
            }

            return true;
        }

        private static void ListDataBaseSummary()
        {
            Console.WriteLine("*********** Summary: **********");

            double totalCosts = 0;
            for (int i = 0; i < lineCount; i++)
            {
                Console.WriteLine(myInputItem[i] + " " + myInputCost[i]);
                totalCosts += myInputCost[i];
            }

            Console.WriteLine("Your spendings are: " + totalCosts);
            Console.WriteLine("Your amount left on budget is: " + (myBudget - totalCosts));
            Console.WriteLine();

            // Start
            ShowMenu();
        }

        private static void ShowMenu()
        {
            System.Console.WriteLine("*********** Menu: ***********");
            Console.WriteLine("Do you want to add another?\n" +
                "type 'y' to add new entry, \n" +
                "type 'e' to exit without uploading online, \n" +
                "type 'x' to export database in readable form, \n" +
                "type 'u' to exit and upload the database online.");
            var userInput = Console.ReadKey(true);

            if (userInput.Key == ConsoleKey.Y)
            {
                AddOrUpdateList();
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                Console.WriteLine("Exiting...");
                SaveDatabase();
            }
            else if (userInput.Key == ConsoleKey.X)
            {
                Console.WriteLine("View your summary in " + Paths);
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

        private static void AddOrUpdateList()
        {
            Console.Write("For what did you spend: ");
            string itemInput = ParseHelper.ParseStringInput();

            Console.Write("How much did it cost: ");
            double costInput = ParseHelper.ParseDouble(Console.ReadLine());

            // Check if item is already in the database
            bool isDublicateItem = false;
            for (int i = 0; i < lineCount; i++)
            {
                if (itemInput == myInputItem[i])
                {
                    isDublicateItem = true;

                    // Only increase the cost if item is in the database
                    myInputCost[i] += costInput;
                }
            }

            if (!isDublicateItem)
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
            using (StreamWriter outputFile = new StreamWriter(Costs))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(UserKey, myInputCost[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            using (StreamWriter outputFile = new StreamWriter(Items))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    var encryptedString = AesOperation.EncryptString(UserKey, myInputItem[i].ToString());
                    outputFile.WriteLine(encryptedString);
                }
            }

            // Perhaps its not needed to encrypt, maybe its going to be easy to edit too.
            using (StreamWriter outputFile = new StreamWriter(Budget))
            {
                outputFile.WriteLine(myBudget);
            }
        }

        /// <summary>
        /// Use if you want to export in txt readable for humans.
        /// </summary>
        private static void ExportReadable()
        {
            SaveDatabase();

            using StreamWriter outputFile = new StreamWriter(Paths);
            outputFile.WriteLine("*********** Summary: **********");

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
            outputFile.Dispose();
        }

        private static void UploadOnline()
        {
            SaveDatabase();

            const string CreateDB = @"Scripts\CreateDB.bat";
            const string UpdateDB = @"Scripts\UpdateDB.bat";

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
            const string PullDB = @"Scripts\PullDB.bat";

            var process = Process.Start(PullDB);
            process.WaitForExit();
            Console.Clear();
        }
    }
}
