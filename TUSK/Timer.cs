using System;

namespace TUSK
{
    public static class Timer
    {
        private static int _interval = 17;
        public static bool PostingTime()
        {
            if (Convert.ToInt32(DateTime.UtcNow.Subtract(Properties.Settings.Default.LastPost).TotalMinutes) >= _interval)
            {
                _interval = GenerateInterval(); 
                ConsoleHelper.WriteLineIf(RunArgs.Verbose, $"Minutes to next post: {_interval}", ConsoleColor.Yellow);
                return true;
            }
            return false;
        }

        public static void DumpTime()
        {
            if (Convert.ToInt32(DateTime.UtcNow.Subtract(Properties.Settings.Default.LastDump).TotalHours) >= 24)
            {
                ConsoleHelper.WriteLineIf(RunArgs.Verbose, "Daily db dump.", ConsoleColor.DarkRed);
                DatabaseAccess.DumpDbToFile();
                Properties.Settings.Default.LastDump = DateTime.UtcNow;
                Properties.Settings.Default.Save();
            }
        }

        private static int GenerateInterval()
        {
            return DateTime.UtcNow.Hour > 23 || DateTime.UtcNow.Hour < 6
                ? new Random().Next(40, 81)
                : new Random().Next(1, 51);
        }
    }
}
