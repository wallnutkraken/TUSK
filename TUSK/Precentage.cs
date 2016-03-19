using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUSK
{
    abstract class Precentage
    {
        internal static bool CheckChance(int percentageChance)
        {
            if (percentageChance < 0 || percentageChance > 100)
            {
                throw new ArgumentException("Percentage was out of bounds.");
            }
            int numb = new Random().Next(0, 100);
            bool[] bools = new bool[100]; /* 100 size array */
            for (int i = 0; i < percentageChance; i++)
            {
                bools[i] = true;
            }
            return bools[numb];
        }
    }
}
