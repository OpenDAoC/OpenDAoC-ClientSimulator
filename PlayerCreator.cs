using System;
using System.Security.Cryptography;
using System.Text;
using DOL.Database;
using MySql.Data.MySqlClient;
using AtlasSimulator;
using AtlasSimulator.playerclasses;
using log4net;

namespace AtlasSimulator
{
    class PlayerCreator
    {
        public class Position
        {
            public int x, y, z;
            public int zone;
        };

        public enum Class
        {
            Paladin,
            Wizard
        }

        MySqlConnection _dbConnection;

        public PlayerCreator()
        {
            _dbConnection = new MySqlConnection("server=localhost;user=root;database=atlas;port=3306;password=atlas");
        }
        
        public void Create(IPlayerClass pc){
            CreateAccount(pc);
            CreateChar(pc);
        }

        public void CreateChar(IPlayerClass pc)
        {
            _dbConnection.Open();
            string sql = String.Format("SELECT Name FROM dolcharacters WHERE Name='{0}'", pc.charname);
            MySqlCommand cmd = new MySqlCommand(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();
            if (accountExists)
            {
                // Char exists
                Console.WriteLine(String.Format("Char {0} already exists!", pc.charname));
            }
            else
            {
                _dbConnection.Open();
                Console.WriteLine(String.Format("Creating char named {0}!", pc.charname));
                cmd = new MySqlCommand(pc.sql, _dbConnection);
                cmd.ExecuteNonQuery();
                _dbConnection.Close();
            }
            
        }

        public void CreateAccount(IPlayerClass pc)
        {
            Account account = new Account
            {
                Name = pc.accountName,
                Password = CryptPassword(pc.password),
                PrivLevel = (uint)3,
                Realm = (int)1,
                CreationDate = DateTime.Now,
                Language = "EN"
            };

            _dbConnection.Open();
            string sql = String.Format("SELECT Name FROM account WHERE Name='{0}'",pc.accountName);
            MySqlCommand cmd = new MySqlCommand(sql, _dbConnection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool accountExists = rdr.Read();
            _dbConnection.Close();
            if (accountExists) {
                // Account exists
                Console.WriteLine(String.Format("Account {0} already exists!", pc.accountName));
            } else
            {
                // Write Data
                _dbConnection.Open();
                Console.WriteLine(String.Format("Creating account named {0}!", pc.accountName));
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
