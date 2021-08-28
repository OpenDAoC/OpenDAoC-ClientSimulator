using System;
using System.Security.Cryptography;
using System.Text;
using DOL.Database;
using MySql.Data.MySqlClient;

namespace AtlasSimulator
{
    class PlayerCreator
    {
        public class Position
        {
            public int x, y, z;
            public int zone;
        };

        MySqlConnection _dbConnection;

        public PlayerCreator()
        {
            _dbConnection = new MySqlConnection("server=localhost;user=root;database=atlas;port=3306;password=atlas");
        }

        private string generatePallySql(string name, string accountName, Position pos = null)
        {
            DOLCharacters c = new DOLCharacters
            {
                AccountName = accountName,
                Name = name
            };

            int xpos = 390969;
            int ypos = 754234;
            int zpos = 200;
            int zone = 1;

            if (pos != null)
            {
                xpos = pos.x;
                ypos = pos.y;
                zpos = pos.z;
                zone = pos.zone;
            }

            string sql = String.Format("INSERT INTO `atlas`.`dolcharacters` (`Constitution`, `Dexterity`, `Strength`, `Quickness`, `Intelligence`," +
                 " `Piety`, `Empathy`, `Charisma`, `MaxEndurance`, `Endurance`, `Concentration`, `AccountName`, `AccountSlot`, `CreationDate`," +
                 " `LastPlayed`, `Name`, `GuildID`, `Gender`, `Race`, `Level`, `Class`, `Realm`, `CreationModel`, `CurrentModel`, `Region`, `Xpos`," +
                 " `Ypos`, `Zpos`, `BindXpos`, `BindYpos`, `BindZpos`, `BindRegion`, `BindHeading`, `Direction`, `MaxSpeed`, `SerializedCraftingSkills`," +
                 " `SerializedAbilities`, `SerializedSpecs`, `SerializedRealmAbilities`, `DisabledSpells`, `DisabledAbilities`, `SerializedFriendsList`," +
                 " `SerializedIgnoreList`, `SpellQueue`, `FlagClassName`, `GuildRank`, `DeathTime`, `RespecAmountRealmSkill`, `IsLevelRespecUsed`," +
                 " `SafetyFlag`, `CraftingPrimarySkill`, `CustomisationStep`, `EyeSize`, `LipSize`, `HairStyle`, `CurrentTitleType`, `GainXP`, `GainRP`," +
                 " `Autoloot`, `LastFreeLeveled`, `GuildNote`, `LastTimeRowUpdated`, `DOLCharacters_ID`, `LastLevelUp`)" +
                 // con through name
                 " VALUES('70', '60', '70', '60', '60', '500', '60', '60', '100', '100', '100', '{1}', '105', '2021-08-26 10:16:54', '2021-08-26 10:16:54', '{0}', " +
                 // guild id through BindYPos
                 "'aba35a1c-8e77-41d9-831f-e952df60fedc', '1', '1', '50', '1', '1', '37122', '37122', '{3}', '{4}', '{5}', '{6}', '{4}', '{5}'," +
                 // BindZPos 
                 " '{6}', '{3}', '2980', '2980', '191', '1|1;2|1;3|1;4|1;6|1;7|1;8|1;9|1;10|1;11|1;12|1;13|1;14|1;15|1', '', 'Slash|1;Thrust|1;Crush|1;Two Handed|1;Chants|50;Shields|1;Parry|1', '', '', '', '', '', '1', '1', '8'," +
                 " '-9223372036854775808', '2', '1', '1', '15', '2', '68', '68', '16', '', '1', '1', '1', '2021-08-26 10:16:54', '', '2021-08-26 14:16:54', '{2}', '2021-08-26 10:16:54');",
                 c.Name,c.AccountName,c.ObjectId,zone,xpos, ypos,zpos);

            return sql;
        }

        public void CreateChar(string name, string accountName, Position pos = null)
        {
            _dbConnection.Open();
            string sql = String.Format("SELECT Name FROM dolcharacters WHERE Name='{0}'", name);
            MySqlCommand cmd = new MySqlCommand(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();
            if (accountExists)
            {
                // Char exists
                Console.WriteLine(String.Format("Char {0} already exists!", name));
            }
            else
            {
                _dbConnection.Open();
                Console.WriteLine(String.Format("Creating char named {0}!", name));
                sql = generatePallySql(name, accountName, pos);
                cmd = new MySqlCommand(sql, _dbConnection);
                cmd.ExecuteNonQuery();
                _dbConnection.Close();
            }
            
        }

        public void CreateAccount(string accountName, string password)
        {
            Account account = new Account
            {
                Name = accountName,
                Password = CryptPassword(password),
                PrivLevel = (uint)3,
                Realm = (int)1,
                CreationDate = DateTime.Now,
                Language = "EN"
            };

            _dbConnection.Open();
            string sql = String.Format("SELECT Name FROM account WHERE Name='{0}'",accountName);
            MySqlCommand cmd = new MySqlCommand(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();
            if (accountExists) {
                // Account exists
                Console.WriteLine(String.Format("Account {0} already exists!", accountName));
            } else
            {
                // Write Data
                _dbConnection.Open();
                Console.WriteLine(String.Format("Creating account named {0}!", accountName));
                string dateString = account.CreationDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql = String.Format("INSERT INTO account(Name,password,CreationDate,Realm,PrivLevel,Language,Account_ID) " +
                    "VALUES('{0}','{1}','{2}',{3},{4},'{5}','{6}');", account.Name, account.Password, dateString, account.Realm, account.PrivLevel, account.Language, account.Name);
                cmd = new MySqlCommand(sql, _dbConnection);
                cmd.ExecuteNonQuery();
                _dbConnection.Close();
            }
            
        }

        private string CryptPassword(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            char[] pw = password.ToCharArray();

            var res = new byte[pw.Length * 2];
            for (int i = 0; i < pw.Length; i++)
            {
                res[i * 2] = (byte)(pw[i] >> 8);
                res[i * 2 + 1] = (byte)(pw[i]);
            }

            byte[] bytes = md5.ComputeHash(res);

            var crypted = new StringBuilder();
            crypted.Append("##");
            for (int i = 0; i < bytes.Length; i++)
            {
                crypted.Append(bytes[i].ToString("X"));
            }

            return crypted.ToString();
        }
    }
}
