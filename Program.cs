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

        // internal static void simpleCreateAndLogin()
        // {
        //     PlayerCreator pc = new PlayerCreator();
        //     string account = "FooAccount1";
        //     string password = "FooPassword1";
        //     string charName = "FooChar1";
        //     pc.CreateAccount(account, password);
        //     pc.CreateChar(charName, account);
        //     var client = new Client(account, password, charName);
        //     client.Login();
        //     Thread.Sleep(5000);
        //     client.SendUseSpell(0, 48, 5);
        // }
        //
        // internal static void spamAlbDragon()
        // {
        //     // Settings
        //     string nameSeed = "DragonTest";
        //     string password = "foo";
        //
        //     int numx = 10;
        //     int numy = 5;
        //     
        //     int posOffset = 50;
        //
        //     PlayerCreator.Position albDragPosition = new PlayerCreator.Position
        //     {
        //         x = 390969,
        //         y = 754234,
        //         z = 200,
        //         zone = 1
        //     };
        //
        //
        //     // Create accounts
        //     PlayerCreator pc = new PlayerCreator();
        //     int totalAccounts = numx * numy;
        //     int index = 0;
        //     for (int i = 0; i < numx; ++i)
        //     {
        //         for (int j = 0; j < numy; ++j, ++index)
        //         {
        //             PlayerCreator.Position currPos = new PlayerCreator.Position
        //             {
        //                 x = albDragPosition.x + posOffset * i,
        //                 y = albDragPosition.y + posOffset * j,
        //                 z = albDragPosition.z,
        //                 zone = albDragPosition.zone
        //             };
        //             string currName = nameSeed + index.ToString();
        //             pc.CreateAccount(currName, password);
        //             pc.CreateChar(currName, currName, currPos);
        //         }
        //     }
        //
        //     // Login accounts
        //     var clients = new Client[totalAccounts];
        //     for (int i = 0; i < totalAccounts; ++i)
        //     {
        //         string currName = nameSeed + i.ToString();
        //         clients[i] = new Client(currName, password, currName);
        //         clients[i].Login();
        //         Thread.Sleep(100);
        //     }
        // }

        internal static void Main(string[] args)
        {
            // Class to handle DB creation
            PlayerCreator pc = new PlayerCreator();
            
            // Simple Test case
            GLocation g = new GLocation(390969,754234,200,1);
            Paladin p = new Paladin("FooPally", "FooPassword", "FooPally", g);
            pc.Create(p);
            p.login();

            // Keep program alive
            Console.ReadLine();
        }
    }
}