using System.Text;

namespace RV.Chess.Board
{
    [Flags]
    public enum CastlingDirection
    {
        None = 0,
        WhiteKingside = 1,
        WhiteQueenside = 2,
        BlackKingside = 4,
        BlackQueenside = 8,
    }
}
