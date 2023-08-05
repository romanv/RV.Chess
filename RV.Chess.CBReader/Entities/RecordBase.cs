namespace RV.Chess.CBReader.Entities
{
    public record RecordBase
    {
        public uint Id { get; set; } = 0;

        public uint CbId => Id + 1;
    }
}
