using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.MoveDecoding
{
    internal class MoveDecodingState
    {
        // First index is a side: 0 for white 1 for black
        // Second index is a piece type - see MoveType enum
        // Third index is a piece number from the operation code
        private readonly int[][][] _pieces = new int[2][][]
        {
            new int[6][]
            {
                new[] { 4 },
                new[] { 3, -1, -1 },
                new[] { 0, 7, -1 },
                new[] { 2, 5, -1 },
                new[] { 1, 6, -1 },
                new[] { 8, 9, 10, 11, 12, 13, 14, 15 },
            },
            new int[6][]
            {
                new[] { 60 },
                new[] { 59, -1, -1 },
                new[] { 56, 63, -1 },
                new[] { 58, 61, -1 },
                new[] { 57, 62, -1 },
                new[] { 48, 49, 50, 51, 52, 53, 54, 55 },
            }
        };

        private int _enPassantSquare = -1;

        public CastlingRights CastlingRights { get; private set; }

        internal static MoveDecodingState GetCleanState()
        {
            var state = new MoveDecodingState();

            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < state._pieces[0][i].Length; j++)
                {
                    state._pieces[0][i][j] = -1;
                    state._pieces[1][i][j] = -1;
                }
            }

            return state;
        }

        internal MoveDecodingState Clone()
        {
            var clone = new MoveDecodingState();

            for (var i = 0; i < 6; i++)
            {
                clone._pieces[0][i] = (int[])_pieces[0][i].Clone();
                clone._pieces[1][i] = (int[])_pieces[1][i].Clone();
            }

            return clone;
        }

        internal void SetEnPassant(int ep)
        {
            _enPassantSquare = ep;
        }

        internal void SetCastlingRights(CastlingRights rights, bool validate = true)
        {
            CastlingRights = rights;

            // Validate provided rights against the actual position on board
            if (validate)
            {
                if (_pieces[0][0][0] != 4)
                {
                    CastlingRights &= ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside);
                }
                else
                {
                    if (Array.TrueForAll(_pieces[0][2], p => p != 0))
                    {
                        CastlingRights &= ~CastlingRights.WhiteQueenside;
                    }

                    if (Array.TrueForAll(_pieces[0][2], p => p != 7))
                    {
                        CastlingRights &= ~CastlingRights.WhiteKingside;
                    }
                }

                if (_pieces[1][0][0] != 60)
                {
                    CastlingRights &= ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
                }
                else
                {
                    if (Array.TrueForAll(_pieces[1][2], p => p != 56))
                    {
                        CastlingRights &= ~CastlingRights.BlackQueenside;
                    }

                    if (Array.TrueForAll(_pieces[1][2], p => p != 63))
                    {
                        CastlingRights &= ~CastlingRights.BlackKingside;
                    }
                }
            }
        }

        internal Result<CbMove> MakeMove(MoveOperation op, Side sideToMove)
        {
            (_, var isFailed, (var sourceSquare, var targetSquare), var errors) =
                SquaresFromOperation(op, sideToMove);

            if (isFailed)
            {
                return Result.Fail(errors.FirstOrDefault()?.Message);
            }

            return MakeMove(sourceSquare, targetSquare);
        }

        internal Result<CbMove> MakeMove(byte sourceSquare, byte targetSquare, PieceType promoteTo = PieceType.None)
        {
            // If piece is one of the excessive promoted pieces (like 4th Queen),
            // it won't be included in the state, but we still have to remove it's capture target
            var isTrackedPiece = TryGetPieceAt(sourceSquare, out var movingPiece);

            var isPawn = isTrackedPiece && movingPiece.Length == 8;
            var isKing = isTrackedPiece && movingPiece.Length == 1;
            var isEnPassant = targetSquare == _enPassantSquare;
            var captureTargetSquare = (isPawn && isEnPassant) ? _enPassantSquare : targetSquare;
            _enPassantSquare = -1;

            if (TryGetPieceAt(captureTargetSquare, out var capturedPiece))
            {
                Remove(capturedPiece, captureTargetSquare);
            }

            if (isTrackedPiece)
            {
                if (isPawn)
                {
                    if (promoteTo != PieceType.None)
                    {
                        // If pawn is promoted, then white is the one promoted at the 8th rank and black at the 0th
                        var side = targetSquare > 55 ? 0 : 1;
                        var promoteToIdx = (int)promoteTo - 1;

                        if (promoteToIdx == 0 || promoteToIdx > 5)
                        {
                            return Result.Fail($"Invalid promotion to PieceType {promoteToIdx}");
                        }

                        var promotionTarget = _pieces[side][promoteToIdx];
                        var newPieceNo = Array.IndexOf(promotionTarget, -1);

                        if (newPieceNo > -1)
                        {
                            promotionTarget[newPieceNo] = targetSquare;
                        }

                        var idx = Array.IndexOf(movingPiece, sourceSquare);
                        movingPiece[idx] = -1;
                    }
                    else
                    {
                        var jump = targetSquare - sourceSquare;
                        if (jump > 8)
                        {
                            // White pawn had moved two squares
                            _enPassantSquare = sourceSquare + 8;
                        }
                        else if (jump < -8)
                        {
                            // Black pawn had moved two squares
                            _enPassantSquare = sourceSquare - 8;
                        }
                    }
                }
                else if (isKing)
                {
                    var isShortCastle = targetSquare - sourceSquare == 2;
                    var isLongCastle = sourceSquare - targetSquare == 2;

                    if ((isShortCastle || isLongCastle) && TryGetSideAt(sourceSquare, out var kingSide))
                    {
                        if (kingSide == Side.White)
                        {
                            CastlingRights &= ~(CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside);
                        }
                        else
                        {
                            CastlingRights &= ~(CastlingRights.BlackKingside | CastlingRights.BlackQueenside);
                        }

                        var castledRookSourceSquare = sourceSquare + (isShortCastle ? 3 : -4);
                        var castledRookTargetSquare = isLongCastle
                            ? castledRookSourceSquare + 3
                            : castledRookSourceSquare - 2;

                        if (TryGetPieceAt(castledRookSourceSquare, out var rook))
                        {
                            var rookIdx = Array.IndexOf(rook, castledRookSourceSquare);
                            rook[rookIdx] = castledRookTargetSquare;
                        }
                        else
                        {
                            return Result.Fail("Rook is missing from castling square");
                        }
                    }
                }

                if (promoteTo == PieceType.None)
                {
                    var idx = Array.IndexOf(movingPiece, sourceSquare);
                    movingPiece[idx] = targetSquare;
                }
            }

            return new CbMove
            {
                From = sourceSquare,
                To = targetSquare,
                PromoteTo = promoteTo,
            };
        }

        internal bool IsPawnAt(int square) => _pieces[0][5].Contains(square) || _pieces[1][5].Contains(square);

        private bool TryGetSideAt(int sourceSquare, out Side side)
        {
            side = Side.None;

            if (TryGetPieceAt(sourceSquare, out var piece))
            {
                if (_pieces[0].Contains(piece))
                {
                    side = Side.White;
                }
                else
                {
                    side = Side.Black;
                }

                return true;
            }

            return false;
        }

        private bool TryGetPieceAt(int sourceSquare, out int[] pieces)
        {
            pieces = Array.Empty<int>();

            var white = _pieces[0].SingleOrDefault(whitePieces => whitePieces.Contains(sourceSquare));

            if (white != null)
            {
                pieces = white;
                return true;
            }

            var black = _pieces[1].SingleOrDefault(blackPieces => blackPieces.Contains(sourceSquare));

            if (black != null)
            {
                pieces = black;
                return true;
            }

            return false;
        }

        private Result<(byte, byte)> SquaresFromOperation(MoveOperation op, Side sideToMove)
        {
            var sourceSquare = GetSourceSquare(op.Type, op.PieceNo, sideToMove);

            if (sourceSquare.IsFailed || sourceSquare.Value < 0)
            {
                return Result.Fail("Can't perform move operation - moving piece not found");
            }

            var currX = sourceSquare.Value % 8;
            var currY = sourceSquare.Value / 8;
            var targetX = op.Type == MoveType.Pawn && sideToMove == Side.Black
                ? (currX - op.X + 8) % 8
                : (currX + op.X + 8) % 8;
            var targetY = op.Type == MoveType.Pawn && sideToMove == Side.Black
                ? (currY - op.Y + 8) % 8
                : (currY + op.Y + 8) % 8;
            var targetSquare = targetY * 8 + targetX;

            if (targetSquare < 0 || targetSquare > 63)
            {
                return Result.Fail("Wrong target square");
            }

            return ((byte)sourceSquare.Value, (byte)targetSquare);
        }

        internal void AddPiece(PieceType piece, Side side, int squareIdx)
        {
            if (piece == PieceType.None)
            {
                return;
            }

            var targetPiece = _pieces[(int)side][(int)piece - 1];
            var newPieceNo = Array.IndexOf(targetPiece, -1);

            if (newPieceNo > -1)
            {
                targetPiece[newPieceNo] = squareIdx;
            }
        }

        private static void Remove(int[] piece, int square)
        {
            if (piece.Length == 1)
            {
                throw new InvalidOperationException("Can't remove the king");
            }
            else
            {
                var idx = Array.IndexOf(piece, square);

                if (idx < 0)
                {
                    throw new InvalidOperationException($"Can't remove a non-existing piece at {square}");
                }

                if (piece.Length == 3)
                {
                    // Regular pieces need to be shifted to the left.
                    switch (idx)
                    {
                        case 0:
                            piece[0] = piece[1];
                            piece[1] = piece[2];
                            break;
                        case 1:
                            piece[1] = piece[2];
                            break;
                    }

                    piece[2] = -1;
                }
                else if (piece.Length == 8)
                {
                    // Pawns can be removed
                    piece[idx] = -1;
                }
            }
        }

        private Result<int> GetSourceSquare(MoveType moveType, int pieceNo, Side side)
        {
            if ((int)moveType > 5 || pieceNo > _pieces[(int)side][(int)moveType].Length)
            {
                return Result.Fail($"Invalid move type {moveType}");
            }

            return _pieces[(int)side][(int)moveType][pieceNo];
        }
    }
}
