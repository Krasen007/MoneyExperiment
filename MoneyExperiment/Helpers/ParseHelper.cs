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

    public static class ParseHelper
    {
        public static string ParseStringInput(string stringInput)
        {
            bool isNull = true;

            while (isNull)
            {
                if (string.IsNullOrEmpty(stringInput))
                {
                    Console.Write("Please type what did you spent for: ");
                    return ParseStringInput(Console.ReadLine());
                }
                else if (stringInput.EndsWith(" "))
                {
                    stringInput = stringInput.Remove(stringInput.Length - 1);
                }
                else
                {
                    isNull = false;
                }
            }

            return stringInput;
        }

        /// <summary>
        /// Converts user input of a string to a double.
        /// </summary>
        /// <param name="value">The given input</param>
        /// <returns>Returns converted string input to double.</returns>
        public static double ParseDouble(string value)
        {
            double result = 0;
            try
            {
                result = Convert.ToDouble(value);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (FormatException)
            {
                if (value.EndsWith(","))
                {
                    Console.Write("Add more numbers \nHow much did you spend: ");
                }
                else if (value.EndsWith("."))
                {
                    Console.Write("Don't end on '.' \nHow much did you spend: ");
                }
                else if (string.IsNullOrEmpty(value))
                {
                    Console.Write("Don't leave empty fields. \nHow much did you spend: ");
                }
                else
                {
                    Console.Write("Use only numbers, or Use ',' instead of '.' \nHow much did you spend: ");
                }

                value = Console.ReadLine();
                return ParseDouble(value);
            }
#pragma warning restore CA1031 // Do not catch general exception types
#pragma warning disable CA1031 // Do not catch general exception types
            catch (OverflowException)
            {
                Console.WriteLine("'{0}' is outside the range of a Double.", value);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }
    }
}