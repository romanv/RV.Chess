using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace RV.Chess.Board
{
    public class Chessgame
    {
        private readonly Chessboard _board = new();

        public Chessgame()
        {
        }

        public Chessgame(string fen)
        {
            SetFen(fen);
        }

        public Chessboard Board => _board;

        public static Chessgame FromFen(string fen)
        {
            var game = new Chessgame();
            game.SetFen(fen);

            return game;
        }

        public void Reset()
        {
            _board.Reset();
            CurrentMoveNumber = 1;
            HalfMoveClock = 0;
            SideToMove = Side.White;
            CastlingRights = CastlingRights.All;
            EnPassantSquareIdx = -1;
            Moves.Clear();
        }

        public int CurrentMoveNumber { get; internal set; } = 1;

        public int HalfMoveClock { get; internal set; } = 1;

        public Side SideToMove { get; internal set; } = Side.White;

        public CastlingRights CastlingRights { get; internal set; } = CastlingRights.All;

        public int EnPassantSquareIdx { get; internal set; } = -1;

        public List<Move> Moves { get; internal set; } = new();

        public void SetFen(string fen)
        {
            FEN.PutFENDataIntoChessgame(this, fen);
        }

        public string Fen => FEN.BuildFEN(this);

        public void MakeMove(string san)
        {
            var matchingLegalMove = GenerateMoves().FirstOrDefault(m => m.San == san);

            if (matchingLegalMove == null)
            {
                throw new InvalidOperationException($"Move {san} is not possible in position {Fen}");
            }

            MakeMoveOnBoard(matchingLegalMove);
        }

        public void MakeMove(string from, string to)
        {
            var matchingLegalMove = GenerateMoves()
                .FirstOrDefault(m => m.From == from && m.To == to);

            if (matchingLegalMove == null)
            {
                throw new InvalidOperationException($"Move {from}{to} is not possible in position {Fen}");
            }

            MakeMoveOnBoard(matchingLegalMove);
        }

        public void MakeMove(Move move)
        {
            var matchingLegalMove = GenerateMoves()
                .FirstOrDefault(m => m.From == move.From && m.To == move.To && m.PromoteTo == move.PromoteTo);

            if (matchingLegalMove == null)
            {
                throw new InvalidOperationException($"Move {move.San} is not possible in position {Fen}");
            }

            MakeMoveOnBoard(matchingLegalMove);
        }

        private void UpdateEnPassantSquare(Side side, Move move)
        {
            if (side == Side.White && move.SourceRank == 2 && move.TargetRank == 4)
            {
                EnPassantSquareIdx = move.FromIdx + 8;
            }
            else if (side == Side.Black && move.SourceRank == 7 && move.TargetRank == 5)
            {
                EnPassantSquareIdx = move.FromIdx - 8;
            }
            else
            {
                EnPassantSquareIdx = -1;
            }
        }

        private void MakeCastlingMove(Move move)
        {
            if (_board.GetPieceTypeAt(move.CastlingRookSourceSquareIdx) != PieceType.Rook)
            {
                throw new Exception($"Invalid castling move in position {Fen} (no rook at square #{move.CastlingRookSourceSquareIdx})");
            }

            var side = _board.GetPieceSideAt(move.FromIdx);
            _board.RemovePieceAt(move.CastlingRookSourceSquareIdx);
            _board.AddPiece(PieceType.Rook, side, move.CastlingRookTargetSquareIdx);
            _board.RemovePieceAt(move.FromIdx);
            _board.AddPiece(PieceType.King, side, move.ToIdx);
            CastlingRights.RemoveForSide(side);
        }

        private void MakeMoveOnBoard(Move move)
        {
            var pieceType = _board.GetPieceTypeAt(move.FromIdx);
            var pieceSide = _board.GetPieceSideAt(move.FromIdx);

            if (pieceType == PieceType.Pawn)
            {
                UpdateEnPassantSquare(pieceSide, move);

                // special case, because capture target wouldn't be overwritten by the capturer in case of en passant
                if (move.IsEnPassant)
                {
                    _board.RemovePieceAt(move.EnPassantCaptureTarget);
                }
            }
            else
            {
                EnPassantSquareIdx = -1;
            }

            if (move.IsCastling)
            {
                MakeCastlingMove(move);
            }
            else
            {
                // remove castling rights if king or rook moves
                if (pieceType == PieceType.King)
                {
                    CastlingRights.RemoveForSide(pieceSide);
                }
                else if (pieceType == PieceType.Rook)
                {
                    CastlingRights.RemoveFromRookMove(move.FromIdx);
                }

                // remove castling rights if rook is captured
                if (move.IsCapture && _board.GetPieceTypeAt(move.ToIdx) == PieceType.Rook)
                {
                    CastlingRights.RemoveFromRookMove(move.ToIdx);
                }

                _board.RemovePieceAt(move.FromIdx);
            }

            if (move.PromoteTo != PieceType.None)
            {
                _board.AddPiece(move.PromoteTo, pieceSide, move.ToIdx);
            }
            else
            {
                _board.AddPiece(pieceType, pieceSide, move.ToIdx);
            }

            if (pieceType == PieceType.Pawn || move.IsCapture)
            {
                HalfMoveClock = 0;
            }
            else
            {
                HalfMoveClock++;
            }

            if (SideToMove == Side.Black)
            {
                CurrentMoveNumber++;
            }

            SideToMove = SideToMove.Opposite();
            Moves.Add(move);
        }

        public ImmutableArray<Move> GenerateMoves() => GenerateMoves(_board, SideToMove);

        public ImmutableArray<Move> GenerateMoves(Chessboard board, Side sideToMove, bool fastMode = false)
        {
            var ownKingSquare = board.GetKingSquare(sideToMove);
            var pinned = Movement.GetPinnedPieces(board, sideToMove);
            var checkers = Movement.GetSquareAttackers(board, ownKingSquare, sideToMove.Opposite());
            var allMoves = GenerateAllMoves(board, sideToMove, checkers > 0);
            var legalMoves = RemoveIllegalMoves(board, sideToMove, allMoves, pinned, fastMode);

            // if there is only one checking piece, it can potentially be blocked
            if (checkers > 0)
            {
                var defensiveMoves = new List<Move>();

                if (checkers.HasSingleBitSet())
                {
                    if (TryFindCheckBlockers(board, sideToMove, legalMoves, checkers, out var blockers))
                    {
                        defensiveMoves.AddRange(blockers);
                    }

                    var checkerCaptures = legalMoves.Where(m =>
                        (m.FromMask & pinned) == 0
                        && (
                            m.IsCapture && m.ToIdx == checkers.LastSignificantBitIndex()
                            || m.IsEnPassant && m.EnPassantCaptureTarget == checkers.LastSignificantBitIndex()
                        )
                    );

                    if (checkerCaptures.Any())
                    {
                        defensiveMoves.AddRange(checkerCaptures);
                    }
                }

                var evasions = legalMoves.Where(m => m.PieceType == PieceType.King
                    && !Movement.IsSquareAttacked(board, m.ToIdx, sideToMove.Opposite())
                    && !defensiveMoves.Contains(m));

                defensiveMoves.AddRange(evasions);

                if (!fastMode)
                {
                    SanGenerator.Generate(defensiveMoves);
                }

                return defensiveMoves.ToImmutableArray();
            }

            if (!fastMode)
            {
                SanGenerator.Generate(legalMoves);
            }

            return legalMoves.ToImmutableArray();
        }

        /// <summary>
        /// Should never be called, if there is more, than one checking piece
        /// </summary>
        private static bool TryFindCheckBlockers(Chessboard board, Side sideToMove,
            IEnumerable<Move> legalMoves, ulong checkers, [MaybeNullWhen(false)] out IEnumerable<Move> blockingMoves)
        {
            // find, which checkers are sliders and can be blocked
            var attackerSide = sideToMove.Opposite();
            var attackerRooks = board.GetPieceBoard(PieceType.Rook, attackerSide);
            var attackerBishops = board.GetPieceBoard(PieceType.Bishop, attackerSide);
            var attackerQueens = board.GetPieceBoard(PieceType.Queen, attackerSide);
            var sliders = (attackerRooks | attackerBishops | attackerQueens) & checkers;

            if (sliders > 0)
            {
                // find the attack ray between the king and the checker
                // then check, if there is a legal move that covers any square on that ray
                var checkerSquare = sliders.LastSignificantBitIndex();
                var attackRay = Movement.RayBetween(checkerSquare, board.GetOwnKingSquare(sideToMove));
                blockingMoves = legalMoves.Where(m => (m.ToMask & attackRay) > 0);
                return blockingMoves.Any();
            }

            blockingMoves = default;
            return false;
        }

        private IList<Move> GenerateAllMoves(Chessboard board, Side sideToMove, bool kingInCheck)
        {
            var allMoves = new List<Move>();
            var piecesToMove = board.OwnBlockers(sideToMove);
            var ownBlockers = board.OwnBlockers(sideToMove);
            var enemyKingSquare = board.GetKingSquare(sideToMove.Opposite());

            while (piecesToMove > 0)
            {
                var sourceSquare = piecesToMove.LastSignificantBitIndex();
                var pieceType = board.GetPieceTypeAt(sourceSquare);

                if (pieceType is PieceType.Pawn)
                {
                    var moves = Movement.GetPawnMovesFrom(board, sourceSquare, ownBlockers, EnPassantSquareIdx);
                    allMoves.AddRange(moves);
                }
                else if (pieceType is PieceType.Rook)
                {
                    var moves = Movement.GetRookMovesFrom(board, sourceSquare, ownBlockers, enemyKingSquare);
                    allMoves.AddRange(moves);
                }
                else if (pieceType is PieceType.Bishop)
                {
                    var moves = Movement.GetBishopMovesFrom(board, sourceSquare, ownBlockers, enemyKingSquare);
                    allMoves.AddRange(moves);
                }
                else if (pieceType is PieceType.Queen)
                {
                    var moves = Movement.GetQueenMovesFrom(board, sourceSquare, ownBlockers, enemyKingSquare);
                    allMoves.AddRange(moves);
                }
                else if (pieceType is PieceType.Knight)
                {
                    var moves = Movement.GetKnightMovesFrom(board, sourceSquare, ownBlockers, enemyKingSquare);
                    allMoves.AddRange(moves);
                }
                else if (pieceType is PieceType.King)
                {
                    var moves = Movement.GetKingMovesFrom(board, sourceSquare, ownBlockers, kingInCheck, CastlingRights);
                    allMoves.AddRange(moves);
                }

                piecesToMove &= ~(1UL << sourceSquare);
            }

            return allMoves;
        }

        private IList<Move> RemoveIllegalMoves(Chessboard board, Side sideToMove, IEnumerable<Move> allMoves, ulong pinned, bool fastMode)
        {
            var legalMoves = new List<Move>();

            foreach (var move in allMoves)
            {
                var isLegal = false;

                if (move.PieceType == PieceType.King)
                {
                    if (Movement.IsKingMoveSafe(board, move, sideToMove))
                    {
                        isLegal = true;
                    }
                }
                else if (move.IsEnPassant)
                {
                    var captureTargetType = board.GetPieceTypeAt(move.EnPassantCaptureTarget);
                    board.RemovePieceAt(move.FromIdx);
                    board.RemovePieceAt(move.EnPassantCaptureTarget);
                    board.AddPiece(PieceType.Pawn, sideToMove, move.ToIdx);

                    if (!Movement.IsSquareAttacked(board, board.GetKingSquare(sideToMove), sideToMove.Opposite()))
                    {
                        isLegal = true;
                    }

                    board.RemovePieceAt(move.ToIdx);
                    board.AddPiece(captureTargetType, sideToMove.Opposite(), move.EnPassantCaptureTarget);
                    board.AddPiece(PieceType.Pawn, sideToMove, move.FromIdx);
                }
                else if ((move.FromMask & pinned) == 0)
                {
                    // piece is not pinned to the king
                    isLegal = true;
                }
                else if (Movement.IsPinnedPieceMoveSafe(move, board.GetOwnKingSquare(sideToMove)))
                {
                    isLegal = true;
                }

                if (isLegal && !move.IsCheck)
                {
                    // find discovered checks separately by making a move and checking if it is legit, then rolling it back
                    var movingPieceType = board.GetPieceTypeAt(move.FromIdx);
                    var captureTargetSquare = move.IsEnPassant ? move.EnPassantCaptureTarget : move.ToIdx;
                    var captureTargetType = board.GetPieceTypeAt(captureTargetSquare);
                    var oldEnPassant = EnPassantSquareIdx;
                    board.RemovePieceAt(move.FromIdx);

                    if (captureTargetType != PieceType.None)
                    {
                        board.RemovePieceAt(captureTargetSquare);
                    }

                    if (move.IsCastling)
                    {
                        board.RemovePieceAt(move.CastlingRookSourceSquareIdx);
                        board.AddPiece(PieceType.Rook, sideToMove, move.CastlingRookTargetSquareIdx);
                    }

                    var pieceTypeAfterMove = move.PromoteTo != PieceType.None ? move.PromoteTo : movingPieceType;
                    board.AddPiece(pieceTypeAfterMove, sideToMove, move.ToIdx);

                    var enemyKingAttackedAfterMove =
                        Movement.IsSquareAttacked(board, board.GetEnemyKingSquare(sideToMove), sideToMove);
                    var ownKingExposedAfterMove =
                        Movement.IsSquareAttacked(board, board.GetOwnKingSquare(sideToMove), sideToMove.Opposite());

                    if (!fastMode && enemyKingAttackedAfterMove && !ownKingExposedAfterMove)
                    {
                        move.SetCheck(true);

                        // check if opposing side has any legal moves after we make the check
                        var nextPlyBoard = new Chessboard(board);

                        if (movingPieceType == PieceType.Pawn && Math.Abs(move.FromIdx - move.ToIdx) == 16)
                        {
                            EnPassantSquareIdx = move.ToIdx > move.FromIdx
                                ? move.FromIdx + 8
                                : move.ToIdx + 8;
                        }
                        else
                        {
                            EnPassantSquareIdx = -1;
                        }

                        // no need to generate SAN notations for evasions, since we only want to check for their existence
                        var evasionMoves = GenerateMoves(nextPlyBoard, sideToMove.Opposite(), false);

                        if (!evasionMoves.Any())
                        {
                            move.SetMate(true);
                        }
                    }

                    // undo the tested move
                    board.RemovePieceAt(move.ToIdx);
                    board.AddPiece(movingPieceType, sideToMove, move.FromIdx);

                    if (move.IsCastling)
                    {
                        board.RemovePieceAt(move.CastlingRookTargetSquareIdx);
                        board.AddPiece(PieceType.Rook, sideToMove, move.CastlingRookSourceSquareIdx);
                    }

                    if (captureTargetType != PieceType.None)
                    {
                        board.AddPiece(captureTargetType, sideToMove.Opposite(), captureTargetSquare);
                    }

                    if (ownKingExposedAfterMove)
                    {
                        isLegal = false;
                    }

                    EnPassantSquareIdx = oldEnPassant;
                }

                if (isLegal)
                {
                    legalMoves.Add(move);
                }
            }

            return legalMoves;
        }
    }
}
