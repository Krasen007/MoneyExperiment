// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using System;
    using System.Security.Principal;

    public static class Program
    {
        private static void Main()
        {
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            #if RELEASE
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (isElevated)
            {
                _ = new Begin();
            }
            else
            {
                Console.WriteLine("You need admin privileges to run this app.\nPress any key to exit...");
                Console.ReadKey();
            }
            #endif
        }
    }
}