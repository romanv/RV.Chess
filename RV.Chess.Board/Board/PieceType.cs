namespace RV.Chess.Board
{
    public enum PieceType
    {
        Bishop,
        King,
        Knight,
        Pawn,
        Queen,
        Rook,
        None,
    }

    public static class PieceTypeExtensions
    {
        public static char TypeChar(this PieceType type) => type switch
        {
            PieceType.Bishop => 'B',
            PieceType.King => 'K',
            PieceType.Knight => 'N',
            PieceType.Pawn => 'P',
            PieceType.Queen => 'Q',
            PieceType.Rook => 'R',
            _ => '?',
        };

        public static char ToChar(this PieceType type, Side side) => side == Side.White ? type.TypeChar() : char.ToLower(type.TypeChar());
    }
}
