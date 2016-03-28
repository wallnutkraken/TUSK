using System;

namespace TUSK
{
    public static class Globals
    {
        public static DateTime LastDump { get; set; } = DateTime.UtcNow;
        public static DateTime LastPost { get; set; } = new DateTime();
        public static Int32 NextId { get; set; }
        public static Int32 Offset { get; set; }

        public static void RunDebug()
        {
            if (RunArgs.Debug)
            {
                Console.WriteLine($"Last post: {Globals.LastPost}");
                Console.WriteLine($"Last dump: {Globals.LastDump}");
                Console.WriteLine($"NextId: {NextId}");
                Console.WriteLine($"Offset: {Offset}");
                int chatcount = DatabaseAccess.CountMessages("Chats");
                Console.WriteLine($"Active chats: {chatcount}"); 
            }
        }
    }
}

