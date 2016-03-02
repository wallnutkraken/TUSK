namespace TUSK
{
    class TelegramDbEntry : ITelegramDbEntry
    {
        public static TelegramDbEntry Parse(string source)
        {
            char[] s = { ':' };
            string[] elems = source.Split(s, 2);
            return new TelegramDbEntry(int.Parse(elems[0]), elems[1]);
        }
        public long Id { get; set; }
        public string Text { get; set; }
        public TelegramDbEntry(long id, string text)
        {
            Id = id;
            Text = text;
        }
        public TelegramDbEntry(string text) : this(0, text)
        {
            
        }
        public override string ToString()
        {
            return $"{Id}:{Text}";
        }
    }
}
