using System;
using System.Text;

namespace MoneyExperiment.Helpers
{
    public static class Constants
    {
        public static readonly string ReleasesURL = "https://github.com/Krasen007/MoneyExperiment/releases";

        public static readonly string DatabaseFolderPath = @"Database\";

        public static readonly string DefaultBudgetName = "Budget 1";

        public static readonly string DefaultWalletName = "Default Wallet";

        public static readonly int PasswordLength = 31;

        public static void PressEnterToContinue()
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        /// <summary>
        /// Adds needed space between items.
        /// </summary>
        /// <param name="amount">What to be taken of account.</param>
        /// <param name="spaces">The offset.</param>
        /// <returns>The needed space " "</returns>
        public static string SeparatorHelper(double amount, int spaces)
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
    }
}