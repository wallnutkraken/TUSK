namespace TUSK
{
    interface ITelegramDbEntry
    {
        string Text { get; set; }
        long Id { get; set; }

        string ToString();
    }
}
