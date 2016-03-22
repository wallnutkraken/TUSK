namespace TUSK
{
    public static class CallHome
    {
        public static void SomethingBad(string badness)
        {
            Program.TgBot.PostMessage(badness, 118857134);
        }
    }
}
