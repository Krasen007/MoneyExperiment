// Krasen Ivanov 2019

namespace MoneyExperiment
{
    using MoneyExperiment.Helpers;
    using System;
    using System.IO;
    using System.Net;

    ////using System.Security.Principal;

    public static class Program
    {
        private static void Main()
        {
            string localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Console.Title = "Money Experiment " + localVer;

            // Check windows program files folder for installation of git.
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)))
            {
                Console.WriteLine("WARNING: Git is not installed globally.\nSome features will not work correctly.\nDownload the latest version from: https://git-scm.com/downloads");
            }

            // Retrieves the latest version number
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
            catch (WebException error)
            {
                Console.WriteLine("The latest avaliable version could not be read.");
                Console.WriteLine(error.Message);
                remoteVer = localVer;
            }

            // Disabled for now. It appears the app works normal without permissions.
            //// #if RELEASE
            ////             bool isElevated;
            ////             using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            ////             {
            ////                 WindowsPrincipal principal = new WindowsPrincipal(identity);
            ////                 isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            ////             }

            ////             if (localVer == remoteVer)
            ////             {
            ////                 if (isElevated)
            ////                 {
            ////                     _ = new Begin();
            ////                 }
            ////                 else
            ////                 {
            ////                     Console.WriteLine("You need admin privileges to run this app.");
            ////                     Constants.PressEnterToContinue();
            ////                 }
            ////             }
            ////             else
            ////             {
            ////                 CompareVersions(localVer, remoteVer);
            ////             }
            //// #else
            if (CompareVersions(localVer, remoteVer))
            {
                _ = new Begin();
            }
            else
            {
                Console.WriteLine("Program wil now terminate.");
                Constants.PressEnterToContinue();
            }
            // // #endif
        }

        /// <summary>
        /// Checks the remote version and the local version and coverts them to double.
        /// </summary>
        /// <param name="localVer">Current version of the app.</param>
        /// <param name="remoteVer">The latest version avaliable of the app.</param>
        private static bool CompareVersions(string localVer, string remoteVer)
        {
            //Compare only the last 4 digits of the versions.
            var localVerPart = localVer.Substring(4);
            string localVerString = string.Empty;
            if (localVerPart.Contains("."))
            {
                var part1 = localVerPart.Substring(0, localVerPart.IndexOf("."));
                var part2 = localVerPart.Substring(localVerPart.IndexOf(".") + 1);
                localVerString = part1 + part2;
            }

            string remoteVerString = string.Empty;
            var remoteVerPart = remoteVer.Substring(4);
            if (remoteVerPart.Contains("."))
            {
                var part1 = remoteVerPart.Substring(0, remoteVerPart.IndexOf("."));
                var part2 = remoteVerPart.Substring(remoteVerPart.IndexOf(".") + 1);
                remoteVerString = part1 + part2;
            }

            if (ParseHelper.ParseDouble(localVerString) >= ParseHelper.ParseDouble(remoteVerString))
            {
                return true;
            }
            else if (ParseHelper.ParseDouble(localVerString) > ParseHelper.ParseDouble(remoteVerString) - 5) // Only the last 5 versions will work.
            {
                Console.WriteLine("Your version is " + localVer);
                Console.WriteLine("New version is avaliable to download: " + remoteVer);
                Console.WriteLine("You can download it from here: " + Constants.ReleasesURL);
                Constants.PressEnterToContinue();
                return true;
            }
            else
            {
                Console.WriteLine($"Your version {localVer} is outdated. \n" +
                    $"The new version is {remoteVer}");
                Console.WriteLine("You can download it from here: " + Constants.ReleasesURL);
                return false;
            }
        }
    }
}