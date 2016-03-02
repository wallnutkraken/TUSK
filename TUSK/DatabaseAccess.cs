using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.IO;

namespace TUSK
{
    internal static class DatabaseAccess
    {
        private static SqlConnection db;

        private static void DumpDbToFile()
        {
            Connect();
            System.IO.File.WriteAllLines($"dbDump-{DateTime.Now.ToShortDateString()}-{DateTime.Now.Hour}.{DateTime.Now.Minute}.log", FormatHelpers.CollectionToString(GetAllMessages()));
            Disconnect();
        }
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

        internal static List<long> GetActiveChats()
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
                FormatHelpers.Error(e.Message);
                CallHome.SomethingBad($"{DateTime.UtcNow.ToString()} || {e.Message}");
            }
            Disconnect();

            return chatIds;
        }

        internal static void AddChat(long chatId)
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
                FormatHelpers.Error(e.Message);
                CallHome.SomethingBad($"{DateTime.UtcNow.ToString()} || {e.Message}");
            }
            Disconnect();
        }

        internal static void AddMessage(ITelegramDbEntry message)
        {
            AddMessage(message.Id, message.Text);
        }

        internal static void AddMessage(long chatId, string text)
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
                if (e.Message.StartsWith("Violation of PRIMARY KEY"))
                {
                    Disconnect();
                    return;
                }
                FormatHelpers.Error(e.Message);
                CallHome.SomethingBad($"{DateTime.UtcNow.ToString()} || {e.Message}");
            }
            Disconnect();
        }

        internal static void AddMessagesFromText(string path)
        {
            string[] file = File.ReadAllLines(path);
            foreach (string line in file)
            {
                AddMessage(0, line);
            }
            Console.WriteLine("Done.");
            Environment.Exit(0);
        }

        public static void AddMessagesFromLog(string path)
        {
            string[] file = File.ReadAllLines(path);
            foreach (string line in file)
            {
                try
                {
                    AddMessage(TelegramDbEntry.Parse(line));
                }
                catch (Exception e)
                {
                    FormatHelpers.Error($"{e.Message} WHEN LOADING MESSAGES FROM LOG");
                }
            }
        }


        public static List<ITelegramDbEntry> GetAllMessages()
        {
            Connect();
            SqlCommand cmd = db.CreateCommand();
            cmd.CommandText = "SELECT Text, Id FROM Messages";
            List<TelegramDbEntry> messages = new List<TelegramDbEntry>();

            try
            {
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add(new TelegramDbEntry(uint.Parse(reader["Id"].ToString()), reader["Text"].ToString()));
                }
            }
            catch (Exception e)
            {
                FormatHelpers.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();

            return messages.Cast<ITelegramDbEntry>().ToList(); /* <-- What the fuck is this monster */
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
                FormatHelpers.Error(e.Message);
                CallHome.SomethingBad(DateTime.UtcNow.ToString() + "|| " + e.Message);
            }
            Disconnect();
        }
    }
}
