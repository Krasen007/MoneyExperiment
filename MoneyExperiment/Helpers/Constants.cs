/*
    Money Experiment Experimental console budgeting app.
    Built on .net core. Use it to sync between PCs.
    Copyright (C) 2019  Krasen Ivanov

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace MoneyExperiment.Helpers
{
    using System;
    using System.Text;

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