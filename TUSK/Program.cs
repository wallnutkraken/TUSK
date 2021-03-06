﻿using System;
using System.Threading;

namespace TUSK
{
    class Program
    {
        public static Bot TgBot;
        static void Main(string[] args)
        {
            RunArgs.Handle(args);
            while (true)
            {
                try
                {
                    Run();
                }
                catch (Exception e)
                {
                    FormatHelpers.Error(e.Message);
                    ConsoleHelper.WriteLineIf(RunArgs.Verbose,
                        "An unhandled error has occured. Waiting 60 seconds to restart...");
                    Thread.Sleep(60*1000);
                }
            }

        }

        private static void Run()
        {
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
