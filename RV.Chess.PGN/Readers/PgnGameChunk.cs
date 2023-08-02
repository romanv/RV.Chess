namespace RV.Chess.PGN.Readers
{
    public ref struct PgnGameChunk
    {
        public int Col { get; set; }

        public int Row { get; set; }

        public long ChunkStartPos { get; set; }

        public ReadOnlySpan<char> Text { get; set; }
    }
}
