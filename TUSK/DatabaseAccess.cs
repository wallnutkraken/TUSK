using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace TUSK
{
    static class DatabaseAccess
    {
        private static SqlConnection db;
        private static void Connect()
        {
            db = new SqlConnection(Properties.Settings.Default.DataConnectionString);
            db.Open();
        }

        private static void Disconnect()
        {
            db.Close();
            db = null;
        }

        public static List<long> GetActiveChats()
        {
            List<long> chatIds = new List<long>();

            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = "SELECT * FROM Chats";
            try
            {
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    chatIds.Add(long.Parse(reader["ChatID"].ToString()));
                }
            }
            catch (Exception e)
            {
                Fancy.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();

            return chatIds;
        }

        public static void AddChat(long chatId)
        {
            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = $"INSERT INTO Chats VALUES ({chatId})";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Fancy.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();
        }

        public static void AddMessage(long chatId, string text)
        {
            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = $"INSERT INTO Messages (Id, Text) VALUES ({Properties.Settings.Default.NextId}, @message)";
            cmd.Parameters.Add("@message", System.Data.SqlDbType.NVarChar);
            cmd.Parameters["@message"].Value = text;
            try
            {
                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine($"Rows affected: {rows}");
                Properties.Settings.Default.NextId++;
                Properties.Settings.Default.Save();
            } 
            catch (Exception e)
            {
                Fancy.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();
        }

        public static List<string> GetAllMessages()
        {
            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = "SELECT Text FROM Messages";
            List<string> messages = new List<string>();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add(reader["Text"].ToString());
                }
            }
            catch (Exception e)
            {
                Fancy.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();

            return messages;
        }

        public static void DeactivateChat(long chatId)
        {
            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = $"DELETE FROM Chats WHERE ChatId = {chatId}";
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Fancy.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();
        }
    }
}
