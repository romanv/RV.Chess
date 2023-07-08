namespace RV.Chess.Board
{
    public sealed record Move
    {
        public Move(PieceType pieceType, Side side, string from, string to, bool isCapture = false,
            bool isCheck = false, bool isEnPassant = false, PieceType promoteTo = PieceType.None,
            CastlingDirection castling = CastlingDirection.None)
        {
            From = from;
            FromIdx = Chessboard.SquareToIdx(from);
            IsCapture = isCapture;
            IsCheck = isCheck;
            IsEnPassant = isEnPassant;
            PieceType = pieceType;
            Side = side;
            PromoteTo = promoteTo;
            To = to;
            ToIdx = Chessboard.SquareToIdx(to);
            FromMask = 1UL << FromIdx;
            ToMask = 1UL << ToIdx;
            Castling = castling;
        }

        public Move(PieceType pieceType, Side side, int from, int to, bool isCapture = false,
            bool isCheck = false, bool isEnPassant = false, PieceType promoteTo = PieceType.None,
            CastlingDirection castling = CastlingDirection.None)
        {
            FromIdx = from;
            From = Chessboard.IdxToSquare(from);
            IsCapture = isCapture;
            IsCheck = isCheck;
            IsEnPassant = isEnPassant;
            PieceType = pieceType;
            Side = side;
            PromoteTo = promoteTo;
            ToIdx = to;
            To = Chessboard.IdxToSquare(to);
            FromMask = 1UL << FromIdx;
            ToMask = 1UL << ToIdx;
            Castling = castling;
        }

        public static Move NullMove(Side side)
        {
            return new Move(PieceType.None, side, 0, 0)
            {
                IsNullMove = true,
            };
        }

        public void SetCheck(bool check)
        {
            IsCheck = check;
        }

        public void SetMate(bool mate)
        {
            IsMate = mate;
        }

        public void SetSan(string san)
        {
            San = san;
        }

        public bool IsNullMove { get; private set; }

        public PieceType PieceType { get; }

        public Side Side { get; }

        public int FromIdx { get; }

        public string From { get; }

        public int ToIdx { get; }

        public string To { get; }

        public bool IsCapture { get; }

        public bool IsCheck { get; private set; }

        public bool IsMate { get; private set; }

        public bool IsEnPassant { get; private set; }

        public int SourceRank => (FromIdx / 8) + 1;

        public int TargetRank => (ToIdx / 8) + 1;

        public bool IsCastling =>
            Castling.HasFlag(CastlingDirection.WhiteKingside)
            || Castling.HasFlag(CastlingDirection.WhiteQueenside)
            || Castling.HasFlag(CastlingDirection.BlackKingside)
            || Castling.HasFlag(CastlingDirection.BlackQueenside);

        public int EnPassantCaptureTarget => IsEnPassant
            ? Side == Side.White
                ? ToIdx - 8
                : ToIdx + 8
            : -1;

        public int CastlingRookSourceSquareIdx
        {
            get
            {
                if (!IsCastling)
                {
                    return -1;
                }

                return ToIdx > FromIdx ? FromIdx + 3 : FromIdx - 4;
            }
        }

        public int CastlingRookTargetSquareIdx
        {
            get
            {
                if (!IsCastling)
                {
                    return -1;
                }

                return ToIdx > FromIdx ? ToIdx - 1 : ToIdx + 1;
            }
        }

        public CastlingDirection Castling { get; private set; } = CastlingDirection.None;

        public PieceType PromoteTo { get; }

        public string San { get; private set; } = string.Empty;

        internal ulong FromMask { get; }

        internal ulong ToMask { get; }

        public override string ToString()
        {
            return $"{From}{To}";
        }

        public char PromotionChar => PromoteTo switch
        {
            PieceType.Bishop => 'B',
            PieceType.King => 'K',
            PieceType.Knight => 'N',
            PieceType.Pawn => 'P',
            PieceType.Queen => 'Q',
            PieceType.Rook => 'R',
            _ => '?',
        };

    }
}
