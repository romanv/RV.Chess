namespace RV.Chess.CBReader.Entities
{
    public class AnnotatorRecord
    {
        public AnnotatorRecord(int id, string name, int gamesAnnotated)
        {
            Id = id;
            Name = name;
            GamesAnnotated = gamesAnnotated;
        }

        public int Id { get; }

        public string Name { get; }

        public int GamesAnnotated { get; }

        public override string ToString() => Name;
    }
}
