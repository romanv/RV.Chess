namespace RV.Chess.Board
{
    public record Piece
    {
        public Piece(PieceType type, Side color)
        {
            Type = type;
            Side = color;
        }

        public Side Side { get; }

        public PieceType Type { get; }

        public void Deconstruct(out PieceType type, out Side color)
        {
            type = Type;
            color = Side;
        }

        public static PieceType GetTypeFromChar(char t)
        {
            return t switch
            {
                'r' or 'R' => PieceType.Rook,
                'n' or 'N' => PieceType.Knight,
                'b' or 'B' => PieceType.Bishop,
                'q' or 'Q' => PieceType.Queen,
                'k' or 'K' => PieceType.King,
                'p' or 'P' => PieceType.Pawn,
                _ => PieceType.None,
            };
        }

        public char TypeChar => Type switch
        {
            PieceType.Bishop => 'B',
            PieceType.King => 'K',
            PieceType.Knight => 'N',
            PieceType.Pawn => 'P',
            PieceType.Queen => 'Q',
            PieceType.Rook => 'R',
            _ => '?',
        };

        public char ToChar() => Side == Side.White ? TypeChar : char.ToLower(TypeChar);
    }
}
