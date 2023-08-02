namespace RV.Chess.Shared.Types
{
    public enum PieceType
    {
        None = 0,
        King = 1,
        Queen = 2,
        Rook = 3,
        Bishop = 4,
        Knight = 5,
        Pawn = 6,
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

        public static char ToChar(this PieceType type, Side side) =>
            side == Side.White ? type.TypeChar() : char.ToLower(type.TypeChar());
    }
}
