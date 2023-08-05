namespace RV.Chess.CBReader.MoveDecoding
{
    internal enum MoveType
    {
        King = 0,
        Queen = 1,
        Rook = 2,
        Bishop = 3,
        Knight = 4,
        Pawn = 5,
        Null = 6,
        Multi = 7,
        Ignore = 8,
        VariationStart = 9,
        VariationEnd = 10,
    }
}
