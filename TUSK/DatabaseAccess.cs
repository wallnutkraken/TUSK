using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.IO;

namespace TUSK
{
    internal static class DatabaseAccess
    {
        private static SqlConnection _db;
        private static bool _usingDb = false;
        public static bool UsingDb => _usingDb;

        public static void DumpDbToFile()
        {
            ConsoleHelper.WriteIf(RunArgs.Verbose, "Beginning timely database dump...");
            System.IO.File.WriteAllLines($"dbDump-{DateTime.Now.ToShortDateString()}-{DateTime.Now.Hour}.{DateTime.Now.Minute}.log", FormatHelpers.CollectionToString(GetAllMessages()));
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
        }
        private static void Connect()
        {
            _usingDb = true;
            _db = new SqlConnection(Properties.Settings.Default.DataConnectionString);
            _db.Open();
        }

        private static void Disconnect()
        {
            _db.Close();
            _db = null;
            _usingDb = false;
        }

        internal static List<long> GetActiveChats()
        {
            List<long> chatIds = new List<long>();

            Connect();
            using (SqlCommand cmd = _db.CreateCommand())
            {
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
            }
            Disconnect();

            return chatIds;
        }

        internal static void AddChat(long chatId)
        {
            Connect();
            using (SqlCommand cmd = _db.CreateCommand())
            {
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
            }
            Disconnect();
        }

        internal static void AddMessage(ITelegramDbEntry message)
        {
            AddMessage(message.Id, message.Text);
        }

        internal static int CountMessages()
        {
            Connect();
            int val;
            using (SqlCommand cmd = _db.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Messages";
                val = 0;
                try
                {
                    val = (int)cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    FormatHelpers.Error(e.Message);
                }
            }
            Disconnect();
            return val;
        }

        internal static void AddMessage(long chatId, string text)
        {
            Connect();
            using (SqlCommand cmd = _db.CreateCommand())
            {
                cmd.CommandText = $"INSERT INTO Messages (Id, Text) VALUES ({Properties.Settings.Default.NextId}, @message)";
                cmd.Parameters.Add("@message", System.Data.SqlDbType.NVarChar);
                cmd.Parameters["@message"].Value = text;
                try
                {
                    int rows = cmd.ExecuteNonQuery();
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
            SqlCommand cmd = _db.CreateCommand();
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
            SqlCommand cmd = _db.CreateCommand();
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
