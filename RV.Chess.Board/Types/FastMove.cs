using System.Runtime.CompilerServices;
using RV.Chess.Board.Utils;

namespace RV.Chess.Board.Types
{
    // 0_0000_00000_000_0_0_0_0000_000000_000000
    // │ │    │     │   │ │ │ │    │      └───── From
    // │ │    │     │   │ │ │ │    └──────────── To                     >> 6
    // │ │    │     │   │ │ │ └───────────────── MoveType               >> 12
    // │ │    │     │   │ │ └─────────────────── Side                   >> 16
    // │ │    │     │   │ └───────────────────── IsCheck                >> 17
    // │ │    │     │   └─────────────────────── IsMate                 >> 18
    // │ │    │     └─────────────────────────── Captured piece         >> 19
    // │ │    └───────────────────────────────── EP square before       >> 22
    // │ └────────────────────────────────────── Castling rights before >> 27
    // └──────────────────────────────────────── Legality               >> 31
    internal struct FastMove : IEquatable<FastMove>
    {
        internal uint Move { get; set; }

        internal static FastMove Create(
            int from,
            int to,
            MoveType type,
            Side side,
            bool isCheck = false,
            bool isMate = false,
            PieceType capturedPiece = PieceType.None,
            int epBefore = 0,
            CastlingRights castlingBefore = CastlingRights.None)
        {
            return new FastMove
            {
                Move = (uint)(
                    from
                    | to << 6
                    | (int)type << 12
                    | (int)side << 16
                    | Unsafe.As<bool, int>(ref isCheck) << 17
                    | Unsafe.As<bool, int>(ref isMate) << 18
                    | (int)capturedPiece << 19
                    | EncodeEp(epBefore, side) << 22
                    | (int)castlingBefore << 27
                ),
            };
        }

        internal static FastMove CreateNull(
            Side side,
            int epBefore = 0,
            CastlingRights castlingBefore =
            CastlingRights.None)
        {
            return new FastMove
            {
                Move = (uint)(
                    (int)side << 16
                    | EncodeEp(epBefore, side) << 22
                    | (int)castlingBefore << 27
                ),
            };
        }

        internal readonly bool IsLegal => (Move & 2147483648) != 0;

        internal readonly int From => (int)(Move & 0b111111);

        internal readonly int To => (int)(Move >> 6 & 0b111111);

        internal readonly ulong FromMask => 1UL << From;

        internal readonly ulong ToMask => 1UL << To;

        internal readonly int EpCaptureTarget => Side == Side.White ? To - 8 : To + 8;

        internal readonly MoveType Type => (MoveType)(Move >> 12 & 0b1111);

        internal readonly Side Side => (Side)(Move >> 16 & 0b1);

        internal readonly bool IsCheck => (Move & 0x20000) != 0;

        internal readonly bool IsMate => (Move & 0x40000) != 0;

        internal readonly bool IsCapture => (Move & 0x380000) != 0;

        internal readonly bool IsEnPassant => To == EpSquareBefore && Type == MoveType.Pawn;

        internal readonly bool IsCastling => Type == MoveType.CastleLong || Type == MoveType.CastleShort;

        internal readonly bool IsPromotion =>
            Type == MoveType.PromoteQ
            || Type == MoveType.PromoteR
            || Type == MoveType.PromoteB
            || Type == MoveType.PromoteN;

        internal readonly PieceType CapturedPiece => (PieceType)(Move >> 19 & 0b111);

        internal readonly int EpSquareBefore
        {
            get
            {
                var ep = (int)(Move >> 22 & 0b11111);

                if ((ep & 0b1) == 0)
                {
                    return 0;
                }

                return 16 + 24 * (ep >> 4) + (ep >> 1 & 0b111);
            }
        }

        internal readonly CastlingRights CastlingRightsBefore => (CastlingRights)(Move >> 27 & 0b1111);

        internal readonly PieceType PieceAfterMove => Type.ToPieceType();

        internal readonly int CastlingRookFrom
        {
            get
            {
                if (!IsCastling)
                {
                    return -1;
                }

                return To > From ? From + 3 : From - 4;
            }
        }

        internal readonly int CastlingRookTo
        {
            get
            {
                if (!IsCastling)
                {
                    return -1;
                }

                return To > From ? To - 1 : To + 1;
            }
        }

        internal readonly int SourceRank => From / 8 + 1;

        internal readonly int TargetRank => To / 8 + 1;

        internal readonly bool MatchesPiece(PieceType pt) =>
            (int)pt == (int)Type || pt == PieceType.Pawn && IsPromotion;

        internal readonly bool IsKing =>
            Type == MoveType.King
            || Type == MoveType.CastleShort
            || Type == MoveType.CastleLong;

        internal readonly bool IsPawn =>
            Type == MoveType.Pawn
            || Type == MoveType.PromoteQ
            || Type == MoveType.PromoteR
            || Type == MoveType.PromoteN
            || Type == MoveType.PromoteB;

        internal readonly char PromotionChar => Type switch
        {
            MoveType.PromoteB => 'B',
            MoveType.PromoteN => 'N',
            MoveType.PromoteQ => 'Q',
            MoveType.PromoteR => 'R',
            _ => '?',
        };

        internal void SetCheck()
        {
            Move |= 1 << 17;
        }

        internal void SetMate()
        {
            Move |= 1 << 18;
        }

        internal void Legalize()
        {
            Move |= 2147483648;
        }

        public override readonly bool Equals(object? other)
        {
            if (other is FastMove fm)
            {
                return Equals(fm);
            }

            return false;
        }

        public readonly bool Equals(FastMove other) => From == other.From && To == other.To && Type == other.Type;

        public static bool operator ==(FastMove a, FastMove b) => a.Equals(b);

        public static bool operator !=(FastMove a, FastMove b) => !(a == b);

        public override readonly int GetHashCode() => (int)Move;

        public override readonly string ToString() => Move > 0
            ? $"{Coordinates.IdxToSquare(From)}{Coordinates.IdxToSquare(To)}"
            : string.Empty;

        private static int EncodeEp(int square, Side side)
        {
            if (square < 16 || square > 47)
            {
                return 0;
            }

            var squareShifted = side == Side.White ? square - 40 : square - 16;

            return ((int)side ^ 1) << 4 | squareShifted << 1 | 1;
        }
    }
}
