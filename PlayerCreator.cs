using System;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using AtlasSimulator.playerclasses;

namespace AtlasSimulator
{
    class PlayerCreator
    {
        public enum Class
        {
            Paladin,
            Wizard
        }

        MySqlConnection _dbConnection;

        public PlayerCreator()
        {
            _dbConnection = new MySqlConnection("server=localhost;user=atlas;database=atlas;port=3306;password=atlas");
        }
        
        public void Create(PlayerClass pc){
            CreateAccount(pc);
            CreateChar(pc);
        }

        public void CreateChar(PlayerClass pc)
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

        public void CreateAccount(PlayerClass pc)
        {
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
                string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                sql = String.Format("INSERT INTO account(Name,password,CreationDate,Realm,PrivLevel,Language,Account_ID) " +
                    "VALUES('{0}','{1}','{2}',{3},{4},'{5}','{6}');", pc.accountName, CryptPassword(pc.password), dateString, 1, 3, "EN", pc.accountName);
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
