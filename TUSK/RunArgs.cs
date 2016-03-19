using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUSK
{
    public class RunArgs
    {
        public static bool Dump { get; private set; }
        public static bool Post { get; private set; }
        public static bool Verbose { get; private set; }
        public static void Handle(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--dump")
                {
                    Dump = true;
                }
                else if (args[i] == "--post")
                {
                    Post = true;
                }
                else if (args[i] == "--verbose" || args[i] == "-v")
                {
                    Verbose = true;
                }
                else if (args[i] == "--text-file")
                {
                    try
                    {
                        DatabaseAccess.AddMessagesFromText(args[i + 1]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        FormatHelpers.Error("No text file location specified");
                        Environment.Exit(0x1);
                    }
                }
                else if (args[i] == "--log-file")
                {
                    try
                    {
                        DatabaseAccess.AddMessagesFromLog(args[i + 1]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        FormatHelpers.Error("No text file location specified");
                        Environment.Exit(0x1);
                    }
                }
                else if (args[i] == "-r")
                {
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();
                    Environment.Exit(0);
                }
            }
        }
    }
}
