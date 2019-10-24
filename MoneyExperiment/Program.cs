﻿// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Principal;

    public static class Program
    {
        private static void Main()
        {
            string localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Console.Title = "Money Experiment " + localVer;

            string remoteVer = string.Empty;
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://raw.githubusercontent.com/Krasen007/MoneyExperiment/master/MoneyExperiment/remoteVer.txt");
                using StreamReader srRemoteVer = new StreamReader(stream);
                remoteVer = ParseHelper.ParseStringInput(srRemoteVer.ReadLine()!);
                srRemoteVer.Close();
                stream.Close();
                client.Dispose();
            }
            catch (IOException error)
            {
                Console.WriteLine("The version file could not be read: ");
                Console.WriteLine(error.Message);
            }

#if RELEASE
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (localVer == remoteVer)
            {
                if (isElevated)
                {
                    _ = new Begin();
                }
                else
                {
                    Console.WriteLine("You need admin privileges to run this app.");
                    Constants.PressEnterToContinue();
                }
            }
            else
            {
                Console.WriteLine("New version is avaliable to download: " + remoteVer);
                Console.WriteLine("You can download it from here: " + Constants.ReleasesURL);
                Constants.PressEnterToContinue();
            }
#else
            //1.3.10.0
            var localVerPart = localVer.Substring(4);
            string localVerDouble = string.Empty;
            if (localVerPart.Contains("."))
            {
                var part1 = localVerPart.Substring(0, localVerPart.IndexOf("."));
                var part2 = localVerPart.Substring(localVerPart.IndexOf(".") + 1);
                localVerDouble = part1 + part2;
            }

            string remoteVerDouble = string.Empty;
            var remoteVerPart = remoteVer.Substring(4);
            if (remoteVerPart.Contains("."))
            {
                var part1 = remoteVerPart.Substring(0, remoteVerPart.IndexOf("."));
                var part2 = remoteVerPart.Substring(remoteVerPart.IndexOf(".") + 1);
                remoteVerDouble = part1 + part2;
            }

            if (ParseHelper.ParseDouble(localVerDouble) >= ParseHelper.ParseDouble(remoteVerDouble))
            {
                _ = new Begin();
            }
            else
            {
                Console.WriteLine("New version is avaliable to download: " + remoteVer);
                Console.WriteLine("You can download it from here: " + Constants.ReleasesURL);
                Constants.PressEnterToContinue();
            }
#endif
        }
    }
}