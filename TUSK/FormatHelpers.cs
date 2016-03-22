using System;
using System.Collections.Generic;
using System.Linq;

namespace TUSK
{
    static class FormatHelpers
    {
        public static void Error(string text)
        {
            ConsoleColor prevFg = Console.ForegroundColor;
            ConsoleColor prevBg = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("ERROR: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(text);
            Console.ForegroundColor = prevFg;
            Console.BackgroundColor = prevBg;
        }

        public static IEnumerable<string> CollectionToString<T>(IEnumerable<T> collection)
        {
            return (from object element in collection select element.ToString()).ToList();
        }
    }
}
