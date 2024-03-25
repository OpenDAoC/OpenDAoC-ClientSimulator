using System;
using System.Net;
using System.Threading;
using ClientSimulator.PlayerClass;

namespace ClientSimulator
{
    class Program
    {
        public const string REMOTE_IP = "127.0.0.1";
        public const string LOCAL_IP = "127.0.0.1";
        public const int REMOTE_TCP_PORT = 10300;
        public const int REMOTE_UDP_PORT = 10400;
        public const string DB_IP = "localhost";
        public const string DB = "opendaoc";
        public const string DB_USER = "opendaoc";
        public const string DB_PASSWORD = "opendaoc";
        public const int DB_PORT = 3306;

        public static IPEndPoint SERVER_UDP_ENDPOINT = new(IPAddress.Parse(REMOTE_IP), REMOTE_UDP_PORT);

        internal static void Main(string[] args)
        {
            PlayerCreator pc = new();
            int numAccounts = 1000;
            int spread = 0;
            IPlayerClass[] clients = new IPlayerClass[numAccounts];
            Random rand = new();

            for (int i = 0; i < numAccounts; ++i)
            {
                GameLocation gameLocation = new(390482 + rand.Next(-spread,spread), 748466 + rand.Next(-spread,spread), 370, 1);
                clients[i] = new Wizard("FooWizzy"+i, "FooPassword", "FooWizzy"+i, gameLocation);
                pc.Create(clients[i]);
                clients[i].Login();
                Thread.Sleep(10);
            }

            // Keep program alive
            Console.ReadLine();
        }
    }
}
