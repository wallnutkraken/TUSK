using System;

namespace TUSK
{
    internal static class Precentage
    {
        internal static bool CheckChance(int percentageChance)
        {
            if (percentageChance < 0 || percentageChance > 100)
            {
                throw new ArgumentException("Percentage was out of bounds.");
            }
            int numb = new Random().Next(1, 101);

            return numb < percentageChance;
        }
    }
}
