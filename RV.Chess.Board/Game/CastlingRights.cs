using System.Text;

namespace RV.Chess.Board
{
    public class CastlingRights
    {
        public CastlingDirection Rights { get; private set; } =
            CastlingDirection.WhiteKingside
            | CastlingDirection.WhiteQueenside
            | CastlingDirection.BlackKingside
            | CastlingDirection.BlackQueenside;

        public void Reset()
        {
            Rights = CastlingDirection.WhiteKingside
            | CastlingDirection.WhiteQueenside
            | CastlingDirection.BlackKingside
            | CastlingDirection.BlackQueenside;
        }

        public static CastlingRights All => new();

        public static CastlingRights None => new()
        {
            Rights = CastlingDirection.None,
        };

        public void Set(CastlingDirection rights)
        {
            Rights = rights;
        }

        public void Add(CastlingDirection direction)
        {
            Rights |= direction;
        }

        public bool Can(CastlingDirection direction) => Rights.HasFlag(direction);

        public void RemoveAll()
        {
            Rights = CastlingDirection.None;
        }

        public void Remove(params CastlingDirection[] directions)
        {
            for (var i = 0; i < directions.Length; i++)
            {
                Rights &= ~directions[i];
            }
        }

        public void RemoveForSide(Side side)
        {
            if (side == Side.White)
            {
                Rights &= ~(CastlingDirection.WhiteKingside | CastlingDirection.WhiteQueenside);
            }
            else
            {
                Rights &= ~(CastlingDirection.BlackKingside | CastlingDirection.BlackQueenside);
            }
        }

        public void RemoveFromRookMove(int fromIdx)
        {
            if (fromIdx == 7)
            {
                Rights &= ~CastlingDirection.WhiteKingside;
            }
            else if (fromIdx == 0)
            {
                Rights &= ~CastlingDirection.WhiteQueenside;
            }
            else if (fromIdx == 63)
            {
                Rights &= ~CastlingDirection.BlackKingside;
            }
            else if (fromIdx == 56)
            {
                Rights &= ~CastlingDirection.BlackQueenside;
            }
        }

        public static int GetRookSourceSquare(CastlingDirection direction)
        {
            return direction switch
            {
                CastlingDirection.WhiteKingside => 7,
                CastlingDirection.WhiteQueenside => 0,
                CastlingDirection.BlackKingside => 63,
                CastlingDirection.BlackQueenside => 56,
                _ => -1,
            };
        }

        public static int GetRookTargetSquare(CastlingDirection direction)
        {
            return direction switch
            {
                CastlingDirection.WhiteKingside => 5,
                CastlingDirection.WhiteQueenside => 3,
                CastlingDirection.BlackKingside => 61,
                CastlingDirection.BlackQueenside => 59,
                _ => -1,
            };
        }

        public override string ToString()
        {
            if (Rights == CastlingDirection.None)
            {
                return "-";
            }

            var result = new StringBuilder();

            if (Rights.HasFlag(CastlingDirection.WhiteKingside))
            {
                result.Append('K');
            }

            if (Rights.HasFlag(CastlingDirection.WhiteQueenside))
            {
                result.Append('Q');
            }

            if (Rights.HasFlag(CastlingDirection.BlackKingside))
            {
                result.Append('k');
            }

            if (Rights.HasFlag(CastlingDirection.BlackQueenside))
            {
                result.Append('q');
            }

            return result.ToString();
        }
    }
}
