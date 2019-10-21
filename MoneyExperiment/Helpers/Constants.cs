using System;
using System.Collections.Generic;
using System.Text;

namespace MoneyExperiment.Helpers
{
    public static class Constants
    {
        public const string ReleasesURL = "https://github.com/Krasen007/MoneyExperiment/releases";

        public const string DatabaseFolderPath = @"Database\";

        public const string DefaultBudgetName = "Budget 1";

        public const int PasswordLength = 31;

        public static void PressEnterToContinue()
        {
            Console.WriteLine("Press enter to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
