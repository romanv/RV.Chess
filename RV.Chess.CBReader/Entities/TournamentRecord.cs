namespace RV.Chess.CBReader.Entities
{
    public class TournamentRecord
    {
        public TournamentRecord(int id, string title, string location, DateOnly date)
        {
            Id = id;
            Title = title.Trim();
            Location = location.Trim();
            Date = date;
        }

        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Location { get; private set; }

        public DateOnly Date { get; private set; }
    }
}
