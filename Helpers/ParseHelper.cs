﻿// Krasen Ivanov 2019

namespace MoneyExperiment.Helpers
{
    using System;

    public static class ParseHelper
    {
        public static string ParseStringInput()
        {
            var stringInput = Console.ReadLine();
            bool isNull = true;

            while (isNull)
            {
                if (string.IsNullOrEmpty(stringInput))
                {
                    Console.WriteLine("Please type what did you spent for.");
                    stringInput = Console.ReadLine();
                } else if (stringInput.EndsWith(" "))
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
        public static double ParseDouble(string value)
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
                    value = Console.ReadLine();
                    ParseDouble(value);
                }
                else if (string.IsNullOrEmpty(value))
                {
                    // Show message box with info.
                    Console.WriteLine("Don't leave empty fields");
                    value = Console.ReadLine();
                    ParseDouble(value);
                }
                else
                {
                    Console.WriteLine("Use only numbers, or Use , instead of .");
                    value = Console.ReadLine();
                    ParseDouble(value);
                }
            }
            catch (OverflowException)
            {
                Console.WriteLine("'{0}' is outside the range of a Double.", value);
            }

            return result;
        }
    }
}
