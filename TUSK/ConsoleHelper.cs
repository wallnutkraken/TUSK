using System;

namespace TUSK
{
    static class ConsoleHelper
    {
        public static void WriteIf(bool check, object value, ConsoleColor foreground)
        {
            if (check)
            {
                ConsoleColor fg = Console.ForegroundColor;
                Console.ForegroundColor = foreground;
                Console.Write(value.ToString());
                Console.ForegroundColor = fg;
            }
        }

        public static void WriteIf(bool check, object value)
        {
            WriteIf(check, value, Console.ForegroundColor);
        }

        public static void WriteLineIf(bool check, object value)
        {
            WriteLineIf(check, value, Console.ForegroundColor);
        }


        public static void WriteLineIf(bool check, object value, ConsoleColor foreground)
        {
            WriteIf(check, $"{value}\n", foreground);
        }
    }
}
