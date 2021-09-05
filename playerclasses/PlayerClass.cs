using System;
using System.Threading;

namespace AtlasSimulator.playerclasses
{
    public class PlayerClass
    {
        public string accountName { get; set;  }
        public string charname { get; set; }
        public string password { get; set; }
        public bool isLoggedIn { get; set; }
        public string sql { get; set; }
        public Client _client { get; set; }

        protected Timer _actionTimer;

        public bool actionEnabled { get; set; }

        public void login()
        {
            _client.Login();
            isLoggedIn = true;

        }

        virtual public void ActionCallback(Object source)
        {
            // Cast PBAOE
            _client.SendUseSpell(0, 48, 5);
        }
    }
}