using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUSK
{
    static class CallHome
    {
        public static void SomethingBad(string badness)
        {
            Program.TgBot.PostMessage(badness, 118857134);
        }
    }
}
