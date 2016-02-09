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
            TgBot = new Bot(Properties.Settings.Default.APIKey);
            if (Properties.Settings.Default.LastPost.Year < 2000)
            {
                Properties.Settings.Default.LastPost = DateTime.UtcNow;
                Properties.Settings.Default.Save();
            }
            TgBot.Init();
            TgBot.Run();
        }
    }
}
