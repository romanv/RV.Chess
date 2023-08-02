using System.Text;

namespace RV.Chess.Shared.Types
{
    [Flags]
    public enum CastlingRights
    {
        None = 0,
        WhiteQueenside = 1,
        WhiteKingside = 2,
        BlackQueenside = 4,
        BlackKingside = 8,
        AllWhite = WhiteQueenside | WhiteKingside,
        AllBlack = BlackQueenside | BlackKingside,
        All = WhiteQueenside | WhiteKingside | BlackQueenside | BlackKingside,
    }

    public static class CastlingRightsExtensions
    {
        public static bool CanCastle(this CastlingRights r, CastlingRights dir) => ((int)r & (int)dir) != 0;

        public static CastlingRights Without(this CastlingRights r, CastlingRights remove) => r & ~remove;

        public static CastlingRights WithoutSide(this CastlingRights r, Side side)
        {
            return side == Side.White
                ? r & ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside)
                : r & ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
        }

        public static CastlingRights RemoveByRookMove(this CastlingRights r, int from)
        {
            return from switch
            {
                0 => r & ~CastlingRights.WhiteQueenside,
                7 => r & ~CastlingRights.WhiteKingside,
                56 => r & ~CastlingRights.BlackQueenside,
                63 => r & ~CastlingRights.BlackKingside,
                _ => r,
            };
        }

        public static string AsString(this CastlingRights r)
        {
            if (r == CastlingRights.None)
            {
                return "-";
            }

            var result = new StringBuilder();

            if (r.CanCastle(CastlingRights.WhiteKingside))
            {
                result.Append('K');
            }

            if (r.CanCastle(CastlingRights.WhiteQueenside))
            {
                result.Append('Q');
            }

            if (r.CanCastle(CastlingRights.BlackKingside))
            {
                result.Append('k');
            }

            if (r.CanCastle(CastlingRights.BlackQueenside))
            {
                result.Append('q');
            }

            return result.ToString();
        }
    }
}
