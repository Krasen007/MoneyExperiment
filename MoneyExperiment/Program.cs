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
#if RELEASE
            string localVer = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            Console.Title = "Money Experiment " + localVer;
#else
            Console.Title = "Money Experiment " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
#endif

#if RELEASE
            string remoteVer = string.Empty;

            try
            {

                //Console.WriteLine("Your version is: " + localVer);           

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

            // bool isElevated;	
            // using (WindowsIdentity identity = WindowsIdentity.GetCurrent())	
            // {	
            //     WindowsPrincipal principal = new WindowsPrincipal(identity);	
            //     isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);	
            // }	


            if (localVer == remoteVer)
            {
                // do nothing.
                _ = new Begin();	
            }
            else
            {
                Console.WriteLine("The new version avaliable to download is: " + remoteVer);
            }

            // if (isElevated)	
            // {	
            //     _ = new Begin();	
            // }	
            // else	
            // {	
            //     Console.WriteLine("You need admin privileges to run this app.\nPress any key to exit...");	
            //     Console.ReadKey();	
            // }	
#else
            _ = new Begin();
#endif
        }
    }
}