namespace RV.Chess.CBReader.Entities
{
    public class PlayerRecord
    {
        public PlayerRecord(int id, string lastName, string firstName, uint firstGameId)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            FirstGameId = firstGameId;
        }

        public int Id { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public uint FirstGameId { get; set; }

        public override string ToString() => $"{LastName}, {FirstName}";
    }
}
