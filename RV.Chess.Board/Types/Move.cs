using RV.Chess.Board.Game;
using RV.Chess.Board.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Types
{
    public sealed record Move
    {
        internal static Move FromFastMove(FastMove m)
        {
            if (m.Type == MoveType.Null)
            {
                return new Move
                {
                    Side = m.Side,
                    IsNullMove = true,
                    San = "--",
                };
            }

            return new Move
            {
                FromIdx = m.From,
                From = Coordinates.IdxToSquare(m.From),
                ToIdx = m.To,
                To = Coordinates.IdxToSquare(m.To),
                Piece = m.Type.ToPieceType(),
                Side = m.Side,
                Castling = GetCastling(m),
                IsCapture = m.IsCapture,
                IsCheck = m.IsCheck,
                IsMate = m.IsMate,
                IsEnPassant = m.IsEnPassant,
                PromoteTo = GetPromotion(m),
            };
        }

        public int FromIdx { get; init; }

        public string From { get; init; } = string.Empty;

        public int ToIdx { get; init; }

        public string To { get; init; } = string.Empty;

        public PieceType Piece { get; init; }

        public Side Side { get; init; }

        public bool IsCapture { get; init; }

        public bool IsCheck { get; init; }

        public bool IsMate { get; init; }

        public bool IsEnPassant { get; init; }

        public bool IsNullMove { get; init; }

        public int EnPassantCaptureTarget =>
            IsEnPassant
                ? Side == Side.White
                    ? ToIdx - 8
                    : ToIdx + 8
                : -1;

        public CastlingRights Castling { get; init; } = CastlingRights.None;

        public PieceType PromoteTo { get; init; }

        public string San { get; internal set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrEmpty(San) ? $"{From}{To}" : San;
        }

        public string ToUciString()
        {
            if (IsNullMove)
                return "-";
            if (PromoteTo != PieceType.None)
                return $"{From}{To}{PromoteTo.ToChar()}";

            return $"{From}{To}";
        }

        internal FastMove ToFastMove(Chessgame gameState)
        {
            MoveType moveType;

            if (PromoteTo != PieceType.None)
            {
                moveType = PromoteTo switch
                {
                    PieceType.Queen => MoveType.PromoteQ,
                    PieceType.Rook => MoveType.PromoteR,
                    PieceType.Bishop => MoveType.PromoteB,
                    PieceType.Knight => MoveType.PromoteN,
                    _ => throw new ArgumentException("Unexpected promotion piece type"),
                };
            }
            else if (Castling != CastlingRights.None)
            {
                moveType =
                    (
                        Castling == CastlingRights.WhiteKingside
                        || Castling == CastlingRights.BlackKingside
                    )
                        ? MoveType.CastleShort
                        : MoveType.CastleLong;
            }
            else
            {
                moveType = Piece switch
                {
                    PieceType.King => MoveType.King,
                    PieceType.Queen => MoveType.Queen,
                    PieceType.Rook => MoveType.Rook,
                    PieceType.Bishop => MoveType.Bishop,
                    PieceType.Knight => MoveType.Knight,
                    PieceType.Pawn => MoveType.Pawn,
                    _ => throw new ArgumentException("Invalid piece type for the move"),
                };
            }

            var capturedPiece = gameState.Board.GetPieceTypeAt(ToIdx);

            return FastMove.Create(
                FromIdx,
                ToIdx,
                moveType,
                Side,
                IsCheck,
                IsMate,
                capturedPiece,
                gameState.EpSquare,
                gameState.CastlingRights
            );
        }

        private static CastlingRights GetCastling(FastMove m)
        {
            if (!m.IsCastling)
            {
                return CastlingRights.None;
            }

            if (m.Side == Side.White)
            {
                return m.Type == MoveType.CastleLong
                    ? CastlingRights.WhiteQueenside
                    : CastlingRights.WhiteKingside;
            }

            return m.Type == MoveType.CastleLong
                ? CastlingRights.BlackQueenside
                : CastlingRights.BlackKingside;
        }

        private static PieceType GetPromotion(FastMove m)
        {
            if (!m.IsPromotion)
            {
                return PieceType.None;
            }

            switch (m.Type)
            {
                case MoveType.PromoteQ:
                    return PieceType.Queen;
                case MoveType.PromoteB:
                    return PieceType.Bishop;
                case MoveType.PromoteN:
                    return PieceType.Knight;
                case MoveType.PromoteR:
                    return PieceType.Rook;
                default:
                    return PieceType.None;
            }
        }
    }
}
