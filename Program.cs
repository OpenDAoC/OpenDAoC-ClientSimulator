using System;
using System.Linq;
using System.Threading;
using AtlasSimulator.playerclasses;

namespace AtlasSimulator
{
    class Program
    {
        public const string Host = "127.0.0.1";
        public const int Port = 10300;

        internal static void Main(string[] args)
        {
            // Class to handle DB creation
            PlayerCreator pc = new PlayerCreator();
            
            // Simple Test case


            int numAccounts = 200;
            int spread = 50;
            var clients = new IPlayerClass[numAccounts];
            var rand = new Random();
            for (int i = 0; i < numAccounts; ++i)
            {
                GLocation g = new GLocation(391433 + rand.Next(-spread,spread),755797 + rand.Next(-spread,spread),227,1);
                clients[i] = new Wizard("FooWizzy"+i, "FooPassword", "FooWizzy"+i, g);
                pc.Create(clients[i] );
                clients[i] .login();
                Thread.Sleep(250);
            }

            // Keep program alive
            Console.ReadLine();
        }
    }
}