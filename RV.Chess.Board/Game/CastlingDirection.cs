namespace RV.Chess.Board
{
    [Flags]
    public enum CastlingDirection
    {
        None = 0,
        WhiteQueenside = 1,
        WhiteKingside = 2,
        BlackQueenside = 4,
        BlackKingside = 8,
    }
}
