using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUSK
{
    static class Timer
    {
        public static bool PostingTime()
        {
            if (DateTime.UtcNow.Subtract(Properties.Settings.Default.LastPost).Minutes >= 30)
                return true;
            return false;
        }
    }
}
