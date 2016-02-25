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
            if (args.Contains("--dump"))
            {
                Dump = true;
            }
            if (args.Contains("--post"))
            {
                Post = true;
            }
            if (args.Contains("--verbose") || args.Contains("-v"))
            {
                Verbose = true;
            }
        }
    }
}
