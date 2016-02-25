using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUSK
{
    class Program
    {
        public static Bot TgBot;
        static void Main(string[] args)
        {
            RunArgs.Handle(args);
            TgBot = new Bot(Properties.Settings.Default.APIKey);
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "TG Api initialized.");
            if (Properties.Settings.Default.LastPost.Year < 2000)
            {
                ConsoleHelper.WriteLineIf(RunArgs.Verbose, "Last post date is default. Setting to right now.");
                Properties.Settings.Default.LastPost = DateTime.UtcNow;
                Properties.Settings.Default.Save();
            }
            ConsoleHelper.WriteIf(RunArgs.Verbose, "Beginning initialization... ");
            TgBot.Init();
            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "done.");
            ConsoleHelper.WriteIf(RunArgs.Verbose, "Starting bot...");
            TgBot.Run();
        }
    }
}
