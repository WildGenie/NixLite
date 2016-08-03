using Nix;
using System;

namespace NixLiteTest
{
    class Program
    {
        static void Main(string[] args)
        {

            NixLite nix = new NixLite();

            Console.WriteLine("Press ENTER for start");
            Console.ReadLine();

            try
            {

                if (nix.OpenProcessAndThread(SProcess.GetProcessFromProcessName("NostaleX.dat")))
                    Console.WriteLine("Nostale openned");

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }

        }
    }
}
