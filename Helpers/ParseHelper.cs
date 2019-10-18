// Krasen Ivanov 2019

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
                    stringInput = Console.ReadLine();
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