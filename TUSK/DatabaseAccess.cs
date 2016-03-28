using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.IO;

namespace TUSK
{
    internal static class DatabaseAccess
    {
        private static SqliteConnection _db;
        private static bool _usingDb = false;
        public static bool UsingDb => _usingDb;

        public static void DumpDbToFile()
        {
            ConsoleHelper.WriteIf(RunArgs.Verbose, "Beginning timely database dump...");
            string filename = $"dbDump-{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Hour}.{DateTime.Now.Minute}.log";
            System.IO.File.WriteAllLines(filename, FormatHelpers.CollectionToString(GetAllMessages()));
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
        }
        private static void Connect()
        {
            _usingDb = true;
            _db = new SqliteConnection("Data Source=data.sqlite;Version=3;");
            _db.Open();
        }

        private static void Disconnect()
        {
            _db.Close();
            _db = null;
            _usingDb = false;
        }

        internal static void CreateTable()
        {
            Connect();
            SqliteCommand cmd = _db.CreateCommand();
            cmd.CommandText = "CREATE TABLE Messages (Text TEXT, Id int)";
            cmd.ExecuteNonQuery();

            cmd = _db.CreateCommand();
            cmd.CommandText = "CREATE TABLE Chats (ChatID int)";
            cmd.ExecuteNonQuery();

            Disconnect();
        }

        internal static List<long> GetActiveChats()
        {
            List<long> chatIds = new List<long>();

            Connect();
            using (SqliteCommand cmd = _db.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Chats";
                try
                {
                    SqliteDataReader reader = cmd.ExecuteReader();

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
            using (SqliteCommand cmd = _db.CreateCommand())
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
            return CountMessages("Messages");
        }

        internal static int CountMessages(string tablename)
        {
            Connect();
            long val;
            using (SqliteCommand cmd = _db.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(*) FROM {tablename}";
                val = 0;
                try
                {
                    val = (long)cmd.ExecuteScalar();
                }
                catch (Exception e)
                {
                    FormatHelpers.Error(e.Message);
                }
            }
            Disconnect();
            return Convert.ToInt32(val);
        }

        internal static void AddMessage(long chatId, string textValue)
        {
            Connect();
            using (SqliteCommand cmd = _db.CreateCommand())
            {
                cmd.CommandText = $"INSERT INTO Messages (Id, Text) VALUES ({Globals.NextId}, @message)";
                cmd.Parameters.Add("@message", System.Data.DbType.String);
                cmd.Parameters["@message"].Value = textValue;
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
                    FormatHelpers.Error(e.StackTrace);
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
            Console.WriteLine("COMPLETED.");
            Environment.Exit(0x0);
        }


        public static List<ITelegramDbEntry> GetAllMessages()
        {
            Connect();
            SqliteCommand cmd = _db.CreateCommand();
            cmd.CommandText = "SELECT Text, Id FROM Messages";
            List<TelegramDbEntry> messages = new List<TelegramDbEntry>();

            try
            {
                SqliteDataReader reader = cmd.ExecuteReader();
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
            SqliteCommand cmd = _db.CreateCommand();
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
