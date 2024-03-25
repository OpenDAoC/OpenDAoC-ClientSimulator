using System;
using System.Security.Cryptography;
using System.Text;
using ClientSimulator.PlayerClass;
using DOL.Database;
using MySqlConnector;

namespace ClientSimulator
{
    class PlayerCreator
    {
        public enum Class
        {
            PALADIN,
            WIZARD
        }

        MySqlConnection _dbConnection;

        public PlayerCreator()
        {
            _dbConnection = new MySqlConnection($"server={Program.DB_IP};user={Program.DB_USER};database={Program.DB};port={Program.DB_PORT};password={Program.DB_PASSWORD}");
        }

        public void Create(IPlayerClass pc)
        {
            CreateAccount(pc);
            CreateChar(pc);
        }

        public void CreateChar(IPlayerClass pc)
        {
            _dbConnection.Open();
            string sql = $"SELECT Name FROM dolcharacters WHERE Name='{pc.CharName}'";
            MySqlCommand cmd = new(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();
            if (accountExists)
                Console.WriteLine($"Char {pc.CharName} already exists!");
            else
            {
                _dbConnection.Open();
                Console.WriteLine($"Creating char named {pc.CharName}!");
                cmd = new MySqlCommand(pc.Sql, _dbConnection);
                cmd.ExecuteNonQuery();
                _dbConnection.Close();
            }
        }

        public void CreateAccount(IPlayerClass pc)
        {
            DbAccount account = new()
            {
                Name = pc.AccountName,
                Password = CryptPassword(pc.Password),
                PrivLevel = 1,
                Realm = 1,
                CreationDate = DateTime.Now,
                DiscordID = "AtlasSimulator",
                Language = "EN"
            };

            _dbConnection.Open();
            string sql = $"SELECT Name FROM account WHERE Name='{pc.AccountName}'";
            MySqlCommand cmd = new(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();

            if (accountExists)
                Console.WriteLine($"Account {pc.AccountName} already exists!");
            else
            {
                // Write Data
                _dbConnection.Open();
                Console.WriteLine($"Creating account named {pc.AccountName}!");
                string dateString = account.CreationDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql = string.Format("INSERT INTO `opendaoc`.`account`(`Name`,`Password`,`CreationDate`,`Realm`,`PrivLevel`,`Status`,`LastLoginIP`,`LastClientVersion`,`Language`,`IsMuted`,`LastTimeRowUpdated`,`Account_ID`,`IsWarned`,`IsTester`,`DiscordID`,`CharactersTraded`,`SoloCharactersTraded`,`Realm_Timer_Realm`,`Realm_Timer_Last_Combat`)" +
                    " VALUES('{0}','{1}','{2}',{3}, '{4}', '0', '127.0.0.1', '1124', '{5}', '0', '{6}', '{7}', '0', '0', '{8}', '0', '0', '{9}', '0001-01-01 00:00:15');", account.Name, account.Password, dateString, account.Realm, account.PrivLevel, account.Language, dateString, account.Name, account.DiscordID, account.Realm);
                cmd = new MySqlCommand(sql, _dbConnection);
                cmd.ExecuteNonQuery();
                _dbConnection.Close();
            }
        }

        private static string CryptPassword(string password)
        {
            char[] pw = password.ToCharArray();
            byte[] res = new byte[pw.Length * 2];

            for (int i = 0; i < pw.Length; i++)
            {
                res[i * 2] = (byte) (pw[i] >> 8);
                res[i * 2 + 1] = (byte) pw[i];
            }

            byte[] bytes = MD5.HashData(res);
            StringBuilder crypted = new();
            crypted.Append("##");

            for (int i = 0; i < bytes.Length; i++)
                crypted.Append(bytes[i].ToString("X"));

            return crypted.ToString();
        }
    }
}
