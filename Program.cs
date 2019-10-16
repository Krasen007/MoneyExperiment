// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using System;

    public static class Program
    {
        private static void Main()
        {
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            new Begin();
        }
    }
}