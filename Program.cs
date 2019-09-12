// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Program
    {
        protected Program()
        {
        }

        ///private const string Paths = @"database\Summary.txt";
        private const string Items = @"database\items.txt";
        private const string Costs = @"database\costs.txt";

        private static readonly List<string> myInputItem = new List<string>();
        private static readonly List<double> myInputCost = new List<double>();
        private static int lineCount = File.ReadLines(Items).Count();

        private static void Main()
        {
            Console.WriteLine("Welcome!");


            try
            {   // Open the text file using a stream reader.
                using (StreamReader srItems = new StreamReader(Items))
                {
                    // Read the stream to a string, and write the string to the console.
                    for (int i = 0; i < lineCount; i++)
                    {
                        myInputItem.Add(srItems.ReadLine());
                    }
                    srItems.Close();
                }

                using (StreamReader srCosts = new StreamReader(Costs))
                {
                    for (int i = 0; i < lineCount; i++)
                    {
                        myInputCost.Add(Convert.ToDouble(srCosts.ReadLine()));
                    }
                    srCosts.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            ListSummary();
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
            var userInput = Console.ReadLine();

            if (userInput == "y")
            {
                UpdateList();
            }
            else if (userInput == "e")
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
            var stringInput = Console.ReadLine();

            Console.Write("How much did it cost: ");
            double costInput = Convert.ToDouble(Console.ReadLine());

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
                    outputFile.WriteLine(myInputCost[i]);
                }
            }

            // Used for import
            using (StreamWriter outputFile = new StreamWriter(Items))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    outputFile.WriteLine(myInputItem[i]);
                }
            }
        }
    }
}
