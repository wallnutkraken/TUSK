using System;

namespace TUSK
{
    static class Responses
    {
        public static string[] Unsubscribtion =
        {
            "Yeah, whatever...",
            "I didn't like you anyway.",
            "END YOUR LIFE TOO, FUCKFACE",
            "Sadness :c",
            "I'm sure this is fair",
            "In all seriousness though",
            "Nick Garcia the vaper guy... yeah no END YOURSELF YOU UNSUCKER",
            "Enjoy your ban"
        };
        public static string[] Rare =
        {
            "tfw no robotfriend",
            "Adam please do not pick storm pleASE I WANT TO LIVE",
            "TRIANGLES TRIANGLES TRIANGLES",
            "receiving the finger of death doesnt interrupt channelling so i doubt a dick in the ass would"
        };

        public static string[] Subscription =
        {
            "Woo!",
            "I have no emotion towards you. That's good, I'm sure.",
            "Enjoy your time here! ^^",
            "Remove",
        };


        public static string Sub()
        {
            Random rng = new Random();
            string[] pool = Precentage.CheckChance(98) ? Rare : Subscription;
            bool uppercase = rng.NextDouble() >= 0.5;
            if (uppercase)
            {
                return pool[rng.Next(0, pool.Length)].ToUpper();
            }
            else
            {
                return pool[rng.Next(0, pool.Length)];
            }
        }

        public static string UnSub()
        {
            Random rng = new Random();
            string[] pool;
            if (Precentage.CheckChance(98))
            {
                pool = Rare;
            }
            else
            {
                pool = Unsubscribtion;
            }
            bool uppercase = rng.NextDouble() >= 0.5;
            if (uppercase)
            {
                return pool[rng.Next(0, pool.Length)].ToUpper();
            }
            else
            {
                return pool[rng.Next(0, pool.Length)];
            }
        }
    }
}
