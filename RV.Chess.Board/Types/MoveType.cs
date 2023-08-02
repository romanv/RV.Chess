using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Types
{
    internal enum MoveType
    {
        Null = 0,
        King = 1,
        Queen = 2,
        Rook = 3,
        Bishop = 4,
        Knight = 5,
        Pawn = 6,
        PromoteQ = 7,
        PromoteB = 8,
        PromoteN = 9,
        PromoteR = 10,
        CastleLong = 11,
        CastleShort = 12,
    }

    public static class MoveTypeExtensions
    {
        internal static PieceType ToPieceType(this MoveType m)
        {
            return m switch
            {
                MoveType.King or MoveType.CastleLong or MoveType.CastleShort => PieceType.King,
                MoveType.Queen or MoveType.PromoteQ => PieceType.Queen,
                MoveType.Rook or MoveType.PromoteR => PieceType.Rook,
                MoveType.Bishop or MoveType.PromoteB => PieceType.Bishop,
                MoveType.Knight or MoveType.PromoteN => PieceType.Knight,
                MoveType.Pawn => PieceType.Pawn,
                _ => PieceType.None,
            };
        }
    }
}
