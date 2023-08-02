namespace RV.Chess.PGN.Readers
{
    public interface IPgnReader : IDisposable
    {
        public bool TryGetGameChunk(out PgnGameChunk chunk);

        public void Reset();
    }
}
