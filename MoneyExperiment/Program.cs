// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using System;
    using System.IO;
    using System.Net;
    using MoneyExperiment.Helpers;

    public static class Program
    {
        private static void Main()
        {
            string localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Console.Title = "Money Experiment " + localVer;

            string remoteVer = string.Empty;

            try
            {
                // using StreamReader srLocalVer = new StreamReader("localVer.txt");
                // localVer = ParseHelper.ParseStringInput(srLocalVer.ReadLine()!);
                // srLocalVer.Close();
                Console.WriteLine("Your version is: " + localVer);

                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/Krasen007/MoneyExperiment/master/MoneyExperiment/remoteVer.txt");
                using StreamReader srRemoteVer = new StreamReader(stream);
                remoteVer = ParseHelper.ParseStringInput(srRemoteVer.ReadLine()!);
                srRemoteVer.Close();
                stream.Close();
                client.Dispose();
                Console.WriteLine("The new version avaliable is: " + remoteVer);

            }
            catch (IOException error)
            {
                Console.WriteLine("The version file could not be read: ");
                Console.WriteLine(error.Message);
            }

            if (localVer == remoteVer)
            {
                _ = new Begin();
            }
            else
            {
                System.Console.WriteLine("There is a new version avaliable to download.");
            }
        }
    }
}