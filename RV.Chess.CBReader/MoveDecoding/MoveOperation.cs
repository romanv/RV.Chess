namespace RV.Chess.CBReader.MoveDecoding
{
    internal record MoveOperation
    {
        public MoveType Type { get; private set; }

        public MoveOperation(MoveType type, int pieceNo, int x, int y)
        {
            Type = type;
            PieceNo = pieceNo;
            X = x;
            Y = y;
        }

        public int PieceNo { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }
    }
}
