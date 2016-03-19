using System;
using System.Threading;

namespace TUSK
{
    internal class ControlThread
    {
        private Thread _inputThread;
        private string LastThought { get; set; }

        private void Input()
        {
            while (true)
            {
                char c = char.ToLower(Console.ReadKey(true).KeyChar);
                switch (c)
                {
                    case 'p':
                        Program.TgBot.SendOutMessages();
                        break;
                    case 'q':
                        while (DatabaseAccess.UsingDb)
                        {
                            Thread.Sleep(5);
                        }
                        Environment.Exit(0x0);
                        break;
                    case 'c':
                        ConsoleHelper.WriteLineIf(RunArgs.Verbose, 
                            $"The database currently has {DatabaseAccess.CountMessages()} messages stored.", 
                            ConsoleColor.Blue);
                        break;
                    case 't':
                        LastThought = Program.TgBot.Generate();
                        ConsoleHelper.WriteLineIf(RunArgs.Verbose, $"Thought: {LastThought}", ConsoleColor.Green);
                        break;
                    case 'g':
                        if (LastThought == null)
                        {
                            ConsoleHelper.WriteLineIf(RunArgs.Verbose, "No preexisting thought exists.",
                                ConsoleColor.Cyan);
                        }
                        else
                        {
                            Program.TgBot.SendOutMessages(LastThought);
                        }
                        break;
                    default:
                        if (RunArgs.Verbose)
                        {
                            Console.WriteLine("\nP -- Post generated string");
                            Console.WriteLine("Q -- Safely terminate the program");
                            Console.WriteLine("C -- Show how many lines of messages are stored in the database");
                            Console.WriteLine("T -- Generate a string from a chain state and display it");
                            Console.WriteLine("G -- Send out the last \"thought\"\n");
                        }
                        break;
                }
            }
        }
        public void Start()
        {
            _inputThread = new Thread(Input);
            _inputThread.Start();
        }

        public void Stop()
        {
            _inputThread.Abort();
        }
    }
}
