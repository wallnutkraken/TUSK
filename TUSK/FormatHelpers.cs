using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<string> toStrs = new List<string>();
            foreach (object element in collection)
            {
                toStrs.Add(element.ToString());
            }

            return toStrs;
        }
    }
}
