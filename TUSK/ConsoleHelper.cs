using System;

namespace TUSK
{
    static class ConsoleHelper
    {
        public static void WriteIf(bool check, object value)
        {
            if (check)
            {
                Console.Write(value.ToString());
            }
        }

        public static void WriteLineIf(bool check, object value)
        {
            WriteIf(check, $"{value}\n");
        }
    }
}
