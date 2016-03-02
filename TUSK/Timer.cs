using System;

namespace TUSK
{
    public static class Timer
    {
        private static int _interval = 17;
        public static bool PostingTime()
        {
            if (DateTime.UtcNow.Subtract(Properties.Settings.Default.LastPost).Minutes >= _interval)
            {
                _interval = new Random().Next(1, 51);
                ConsoleHelper.WriteLineIf(RunArgs.Verbose, $"Minutes to next post: {_interval}");
                return true;
            }
            return false;
        }
    }
}
