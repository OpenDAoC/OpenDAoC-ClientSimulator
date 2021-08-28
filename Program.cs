using System;
using System.Linq;
using System.Threading;
using System.Threading;

namespace AtlasSimulator
{
    class Program
    {
        public const string Host = "127.0.0.1";
        public const int Port = 10300;
        public const int UdpPort = 10400;

        internal static void simpleCreateAndLogin()
        {
            PlayerCreator pc = new PlayerCreator();
            string account = "FooAccount1";
            string password = "FooPassword1";
            string charName = "FooChar1";
            pc.CreateAccount(account, password);
            pc.CreateChar(charName, account);
            var client = new Client(account, password, charName);
            client.Login();
        }

        internal static void spamAlbDragon()
        {
            // Settings
            string nameSeed = "DragonTest";
            string password = "foo";

            int numx = 2;
            int numy = 2;
            
            int posOffset = 50;

            PlayerCreator.Position albDragPosition = new PlayerCreator.Position
            {
                x = 390969,
                y = 754234,
                z = 200,
                zone = 1
            };


            // Create accounts
            PlayerCreator pc = new PlayerCreator();
            int totalAccounts = numx * numy;
            int index = 0;
            for (int i = 0; i < numx; ++i)
            {
                for (int j = 0; j < numy; ++j, ++index)
                {
                    PlayerCreator.Position currPos = new PlayerCreator.Position
                    {
                        x = albDragPosition.x + posOffset * i,
                        y = albDragPosition.y + posOffset * j,
                        z = albDragPosition.z,
                        zone = albDragPosition.zone
                    };
                    Console.WriteLine(String.Format("Loop i = {0} j = {1}", i, j));
                    string currName = nameSeed + index.ToString();
                    pc.CreateAccount(currName, password);
                    pc.CreateChar(currName, currName, currPos);
                }
            }

            // Login accounts
            var clients = new Client[totalAccounts];
            for (int i = 0; i < totalAccounts; ++i)
            {
                string currName = nameSeed + i.ToString();
                clients[i] = new Client(currName, password, currName);
                clients[i].Login();
                Thread.Sleep(100);
            }
        }

        internal static void Main(string[] args)
        {
            // Basic test case
            simpleCreateAndLogin();

            // Alb Dragon Test
            //spamAlbDragon();

            // Keep program alive
            Console.ReadLine();
        }
    }
}