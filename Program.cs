// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Program
    {
        protected Program()
        {
        }

        ///private const string Paths = @"database\Summary.txt";
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

            CheckForMissingFiles();

            ListSummary();
        }

        private static void Login()
        {
            Console.WriteLine("Please enter a secret key for the symmetric algorithm.");

            StringBuilder passwordInput = new StringBuilder();
            StringBuilder temp = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    break;
                }
                else
                {
                    Console.Clear();
                    temp.Append("*");
                    Console.Write(temp.ToString());
                }

                if (key.Key == ConsoleKey.Backspace && passwordInput.Length > 0)
                {
                    Console.Clear();
                    temp.Remove(passwordInput.Length - 1, 2);

                    Console.Write(temp.ToString());
                    passwordInput.Remove(passwordInput.Length - 1, 1);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    passwordInput.Append(key.KeyChar);
                }
            }

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

        private static void CheckForMissingFiles()
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

        private static void ListSummary()
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
            Console.WriteLine("Do you want to add another?, y/n, type 'e' for exit and save");
            var userInput = Console.ReadKey(true);


            if (userInput.Key == ConsoleKey.Y)
            {
                UpdateList();
            }
            else if (userInput.Key == ConsoleKey.E)
            {
                ExitAndSaveProgram();
            }
            else
            {
                Console.Clear();
                ListSummary();
            }
        }

        private static void UpdateList()
        {
            Console.Write("For what did you spend: ");
            string stringInput = CheckStringInput();

            Console.Write("How much did it cost: ");
            double costInput = MyParse(Console.ReadLine()); // Convert.ToDouble(Console.ReadLine());

            bool dublicate = false;
            for (int i = 0; i < lineCount; i++)
            {
                if (stringInput == myInputItem[i])
                {
                    dublicate = true;
                    myInputCost[i] += costInput;
                }
            }

            if (dublicate)
            {
                //
                /// Do not add item
            }
            else
            {
                myInputItem.Add(stringInput);
                myInputCost.Add(costInput);
                lineCount++;
            }

            Console.Clear();
            ListSummary();
        }

        private static string CheckStringInput()
        {
            var stringInput = Console.ReadLine();
            bool isNull = true;

            while (isNull)
            {
                if (string.IsNullOrEmpty(stringInput))
                {
                    Console.WriteLine("Please type what did you spent for.");
                    stringInput = Console.ReadLine();
                }
                else
                {
                    isNull = false;
                }
            }

            return stringInput;
        }

        private static double MyParse(string value)
        {
            double result = 0;
            try
            {
                if (value.EndsWith(","))
                {
                    // Show message box with info.
                    /// MessageBox.Show("Add more numbers.", "Tip");
                    Console.WriteLine("Add more numbers.");
                }
                else
                {
                    result = Convert.ToDouble(value);
                }
            }
            catch (FormatException)
            {
                if (value.EndsWith("."))
                {
                    Console.WriteLine("Use , instead of .");
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // Show message box with info.
                    Console.WriteLine("Don't leave empty fields");
                }
                else
                {
                    Console.WriteLine("Use only numbers, or Use , instead of .");
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine("'{0}' is outside the range of a Double.", value);
            }

            return result;
        }



        private static void ExitAndSaveProgram()
        {
            Console.WriteLine("Bye bye");

            /// Use if you want to export
            /// Readable for humans            
            ////using (StreamWriter outputFile = new StreamWriter(Paths))
            ////{
            ////    outputFile.WriteLine("Here is your summary: ");

            ////    for (int i = 0; i < lineCount; i++)
            ////    {
            ////        outputFile.WriteLine(myInputItem[i] + " " + myInputCost[i]);
            ////    }

            ////    double result = 0;
            ////    for (int i = 0; i < lineCount; i++)
            ////    {
            ////        result += myInputCost[i];
            ////    }

            ////    outputFile.WriteLine("Your spendings are: " + result);
            ////}

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
    }
}
