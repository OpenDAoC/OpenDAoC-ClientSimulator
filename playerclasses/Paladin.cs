using System.Threading;
using DOL.Database.UniqueID;

namespace ClientSimulator.PlayerClass
{
    public class Paladin : IPlayerClass
    {
        public string accountName { get; }
        public string charname { get; }
        public string password { get; }
        public string sql { get; }
        private Client _client;
        public string AccountName { get; }
        public string CharName { get; }
        public string Password { get; }
        public string Sql { get; }

        public Paladin(string account, string password, string charName, GameLocation gameLocation)
        {
            // Assign
            AccountName = account;
            Password = password;
            CharName = charName;

            // setup client
            _client = new Client(account, password, charName, gameLocation);

            // handle adding to the database
            ClassHelpers.ClassData paladinData = ClassHelpers.GetPaladinData();

            Sql = string.Format(
                $"INSERT INTO `{Program.DB}`.`dolcharacters` (`Constitution`, `Dexterity`, `Strength`, `Quickness`, `Intelligence`," +
                " `Piety`, `Empathy`, `Charisma`, `MaxEndurance`, `Endurance`, `Concentration`, `AccountName`, `AccountSlot`," +
                "`Name`, `Race`, `Level`, `Class`, `Realm`, `CreationModel`, `CurrentModel`, `Region`, `Xpos`," +
                " `Ypos`, `Zpos`, `BindXpos`, `BindYpos`, `BindZpos`, `BindRegion`, `MaxSpeed`, `SerializedSpecs`, `DOLCharacters_ID`)" +
                " VALUES('400', '400', '400', '400', '400', '400', '400', '400', '100', '100', '100'," +
                " '{0}', '1', '{1}', '{2}', '50', '{3}', '{4}', '{5}', '{5}', '{6}', '{7}'," +
                " '{8}', '{9}','{7}', '{8}', '{9}', '{5}', '191', '{10}', '{11}');",
                AccountName, CharName, paladinData.ValidRaces[0], paladinData.ClassID, paladinData.Realm, paladinData.CreationModel,
                gameLocation.zone, gameLocation.x,
                gameLocation.y, gameLocation.z, paladinData.SpecString, IdGenerator.GenerateID());

            // setup actions
            _client.ActionTimer = new Timer(ActionCallback, null, 850, 8500);
        }

        public void Login()
        {
            _client.Login();
        }

        public void ActionCallback(object source)
        {
            // Cast Self AF Buff
            _client.SendUseSkill(0, 13, 1);
        }
    }
}
