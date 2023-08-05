using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Extensions.ObjectPool;
using RV.Chess.Board.Types;
using RV.Chess.Board.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Game
{
    public class Chessgame
    {
        internal readonly Stack<FastMove> _moveList = new();
        private const int MAX_MOVES = 220;
        private readonly DefaultObjectPool<BoardState> _boardsPool = new(new DefaultPooledObjectPolicy<BoardState>());
        private readonly FastMove[] _moves = new FastMove[MAX_MOVES];
        private readonly ArrayPool<FastMove> _movesPool = ArrayPool<FastMove>.Shared;

        public Chessgame()
        {
        }

        public Chessgame(string fen)
        {
            SetFen(fen);
        }

        public CastlingRights CastlingRights { get; internal set; } = CastlingRights.All;
        public int CurrentMoveNumber { get; internal set; } = 1;
        public int EpSquare => BitOperations.TrailingZeroCount(EpSquareMask);
        public string Fen => FenGenerator.BuildFen(this);
        public int HalfMoveClockStart { get; internal set; } = 0;
        public Side SideToMove { get; internal set; } = Side.White;
        internal BoardState Board { get; } = new();
        internal ulong EpSquareMask { get; set; } = 0;

        public void Reset()
        {
            _moveList.Clear();
            Board.Reset();
            CurrentMoveNumber = 1;
            SideToMove = Side.White;
            CastlingRights = CastlingRights.All;
            EpSquareMask = 0;
        }

        public List<Move> Moves { get; } = new List<Move>();

        public uint Hash { get; internal set; } = Zobrist.DefaultPositionHash;

        public void ClearBoard()
        {
            Board.Clear();
        }

        public void AddPiece(PieceType type, Side side, int square)
        {
            Board.AddPiece(type, side, square);
        }

        public void SetCastling(CastlingRights rights)
        {
            CastlingRights = rights;
        }

        public void SetEnPassant(int square)
        {
            EpSquareMask = 1UL << square;
        }

        public bool SetFen(string fen)
        {
            return FenGenerator.ReadFen(this, fen);
        }

        public void SetMoveNo(int moveNo)
        {
            CurrentMoveNumber = moveNo;
        }

        public void SetSide(Side side)
        {
            SideToMove = side;
        }

        public ImmutableList<Move> GetLegalMoves()
        {
            var result = new List<Move>();
            var legal = GenerateMoves();

            foreach (var fm in legal)
            {
                var m = Move.FromFastMove(fm);
                m.San = SanGenerator.Generate(fm, legal);
                result.Add(m);
            }

            return result.ToImmutableList();
        }

        public bool TryMakeMove(string san, bool fillSan = true)
        {
            var legal = GenerateMoves();

            for (var i = 0; i < legal.Length; i++)
            {
                var fmSan = SanGenerator.Generate(legal[i], legal);

                if (fmSan == san)
                {
                    MakeMoveOnBoard(legal[i], legal, fillSan);
                    return true;
                }
            }

            return false;
        }

        public bool TryMakeMove(string from, string to, PieceType promoteTo = PieceType.None, bool fillSan = true)
        {
            var legal = GenerateMoves();
            var matching = Find(legal, from, to, promoteTo);

            if (matching == null)
            {
                return false;
            }

            MakeMoveOnBoard(matching.Value, legal, fillSan);
            return true;
        }

        public bool TryMakeMove(int from, int to, PieceType promoteTo = PieceType.None, bool fillSan = true)
        {
            var legal = GenerateMoves();
            var matching = Find(legal, from, to, promoteTo);

            if (matching == null)
            {
                return false;
            }

            MakeMoveOnBoard(matching.Value, legal, fillSan);
            return true;
        }

        public bool TryMakeMove(Move move, bool fillSan = true) =>
            TryMakeMove(move.From, move.To, move.PromoteTo, fillSan);

        public bool TryMakeNullMove()
        {
            MakeNullMove();
            return true;
        }

        public void UndoLastMove()
        {
            if (!_moveList.Any())
            {
                return;
            }

            var last = _moveList.Pop();
            CurrentMoveNumber -= (int)last.Side;
            SideToMove = SideToMove.Opposite();
            Hash ^= Zobrist.WhiteTurn;
            Moves.RemoveAt(Moves.Count - 1);

            if (last.Type == MoveType.Null)
            {
                EpSquareMask = last.EpSquareBefore > 0 ? 1UL << last.EpSquareBefore : 0;
                return;
            }

            if (last.IsCastling)
            {
                UndoCastlingMove(last);
            }
            else if (last.IsEnPassant)
            {
                Board.AddPieceUnsafe(PieceType.Pawn, last.Side.Opposite(), last.EpCaptureTarget);
                Board.RemovePieceAt(last.To);
                Board.AddPieceUnsafe(PieceType.Pawn, last.Side, last.From);
                Hash ^= Zobrist.GetPieceHash(last.EpCaptureTarget, PieceType.Pawn, last.Side.Opposite());
                Hash ^= Zobrist.GetPieceHash(last.To, last.Type.ToPieceType(), last.Side);
                Hash ^= Zobrist.GetPieceHash(last.From, PieceType.Pawn, last.Side);
            }
            else if (last.IsCapture)
            {
                Board.AddPieceUnsafe(last.CapturedPiece, last.Side.Opposite(), last.To);
                Hash ^= Zobrist.GetPieceHash(last.To, last.CapturedPiece, last.Side.Opposite());
                Hash ^= Zobrist.GetPieceHash(last.To, last.PieceAfterMove, last.Side);
                Hash ^= Zobrist.GetPieceHash(last.From, last.Type.ToPieceType(), last.Side);
            }
            else
            {
                Board.RemovePieceAt(last.To);
                Hash ^= Zobrist.GetPieceHash(last.To, last.PieceAfterMove, last.Side);
                Hash ^= Zobrist.GetPieceHash(last.From, last.Type.ToPieceType(), last.Side);
            }

            var originalMovedPiece = last.IsPromotion ? PieceType.Pawn : last.PieceAfterMove;
            Board.AddPieceUnsafe(originalMovedPiece, last.Side, last.From);
            EpSquareMask = last.EpSquareBefore > 0 ? 1UL << last.EpSquareBefore : 0;
            Hash ^= Zobrist.GetEnPassantHash(EpSquareMask);
            CastlingRights |= last.CastlingRightsBefore;
        }

        internal Span<FastMove> GenerateMoves() => GenerateMoves(_moves, Board, SideToMove);

        internal Span<FastMove> GenerateMoves(
            Span<FastMove> moves,
            BoardState branchBoard,
            Side side)
        {
            var ownKingSquare = branchBoard.GetOwnKingSquare(side);
            var checkers = Movement.GetSquareAttackers(branchBoard, ownKingSquare, side.Opposite());
            var checkersCount = BitOperations.PopCount(checkers);
            var epSquare = BitOperations.TrailingZeroCount(EpSquareMask);
            var cursor = 0;
            int legalCount;

            if (checkersCount > 1)
            {
                cursor = Movement.GetKingEvasions(ownKingSquare, side, moves, cursor, branchBoard,
                    CastlingRights, epSquare);
                return moves.Slice(0, cursor);
            }
            else if (checkersCount == 1)
            {
                cursor = Movement.GetKingEvasions(ownKingSquare, side, moves, cursor, branchBoard,
                    CastlingRights, epSquare);
                var pinned = Movement.GetPinnedPieces(branchBoard, side);
                cursor = Movement.GetCheckDefenses(side, moves, cursor, branchBoard, ownKingSquare,
                    checkers, pinned, CastlingRights, epSquare);
                legalCount = VerifyMoves(moves.Slice(0, cursor), branchBoard, side);
            }
            else
            {
                var pinned = Movement.GetPinnedPieces(branchBoard, side);
                cursor = GeneratePseudoLegalMoves(side, moves, cursor, branchBoard, pinned);
                legalCount = VerifyMoves(moves.Slice(0, cursor), branchBoard, side);
            }

            var legals = new FastMove[legalCount];

            for (int i = 0, j = 0; i < cursor; i++)
            {
                if (moves[i].IsLegal)
                {
                    legals[j++] = moves[i];
                }
            }

            return legals;
        }

        internal FastMove MakeMove(string from, string to, PieceType promoteTo = PieceType.None, bool fillSan = false)
        {
            var legal = GenerateMoves();
            var matching = Find(legal, from, to, promoteTo) ?? throw new InvalidMoveException(from, to, Fen);
            MakeMoveOnBoard(matching, legal, fillSan);
            return matching;
        }

        internal FastMove MakeMove(FastMove move, bool fillSan = false)
        {
            var legal = GenerateMoves();
            var matching = Find(legal, move) ?? throw new InvalidMoveException(move.From, move.To, Fen);
            MakeMoveOnBoard(matching, legal, fillSan);
            return matching;
        }

        internal FastMove MakeNullMove()
        {
            var nullMove = FastMove.CreateNull(
                SideToMove,
                BitOperations.TrailingZeroCount(EpSquareMask),
                CastlingRights);
            _moveList.Push(nullMove);
            Moves.Add(Move.FromFastMove(nullMove));

            if (SideToMove == Side.Black)
            {
                CurrentMoveNumber++;
            }

            EpSquareMask = 0;
            SideToMove = SideToMove.Opposite();
            Hash ^= Zobrist.WhiteTurn;

            return nullMove;
        }

        private static FastMove? Find(Span<FastMove> moves, string from, string to, PieceType promoteTo = PieceType.None)
        {
            for (var i = 0; i < moves.Length; i++)
            {
                if (moves[i].From == Coordinates.SquareToIdx(from)
                    && moves[i].To == Coordinates.SquareToIdx(to)
                    && moves[i].PromotionChar == promoteTo.TypeChar())
                {
                    return moves[i];
                }
            }

            return null;
        }

        private static FastMove? Find(Span<FastMove> moves, int from, int to, PieceType promoteTo = PieceType.None)
        {
            for (var i = 0; i < moves.Length; i++)
            {
                if (moves[i].From == from
                    && moves[i].To == to
                    && moves[i].PromotionChar == promoteTo.TypeChar())
                {
                    return moves[i];
                }
            }

            return null;
        }

        private static FastMove? Find(Span<FastMove> moves, FastMove move)
        {
            for (var i = 0; i < moves.Length; i++)
            {
                if (moves[i] == move)
                {
                    return moves[i];
                }
            }

            return null;
        }

        private int GeneratePseudoLegalMoves(
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState branchBoard,
            ulong pinned)
        {
            var allTargets = ~branchBoard.Occupied[(int)side];
            var captureTargets = allTargets & (branchBoard.Occupied[(int)side ^ 1] | EpSquareMask);
            var moveTargets = allTargets ^ captureTargets;
            var enemyKingSquare = branchBoard.GetEnemyKingSquare(side);
            var epSquare = BitOperations.TrailingZeroCount(EpSquareMask);

            if (side == Side.White)
            {
                cursor = Movement.GetWhitePawnMoves(moveTargets, captureTargets, moves, cursor,
                    branchBoard, pinned, CastlingRights, epSquare);
            }
            else
            {
                cursor = Movement.GetBlackPawnMoves(moveTargets, captureTargets, moves, cursor,
                    branchBoard, pinned, CastlingRights, epSquare);
            }

            cursor = Movement.GetRookMoves(allTargets, side, moves, cursor, branchBoard,
                pinned, enemyKingSquare, CastlingRights, epSquare);
            cursor = Movement.GetBishopMoves(allTargets, side, moves, cursor, branchBoard,
                pinned, enemyKingSquare, CastlingRights, epSquare);
            cursor = Movement.GetQueenMoves(allTargets, side, moves, cursor, branchBoard,
                pinned, enemyKingSquare, CastlingRights, epSquare);
            cursor = Movement.GetKnightMoves(allTargets, side, moves, cursor, branchBoard,
                pinned, enemyKingSquare, CastlingRights, epSquare);
            cursor = Movement.GetKingMoves(side, moves, cursor, branchBoard,
                CastlingRights, epSquare);

            return cursor;
        }

        private void MakeCastlingMove(FastMove move)
        {
            Debug.Assert(Board.GetPieceTypeAt(move.CastlingRookFrom) == PieceType.Rook);
            var side = Board.GetPieceSideAt(move.From);

            switch (move.CastlingRookFrom)
            {
                case 0:
                    Hash ^= Zobrist.Castling[1]; // Q
                    break;
                case 7:
                    Hash ^= Zobrist.Castling[0]; // K
                    break;
                case 56:
                    Hash ^= Zobrist.Castling[3]; // q
                    break;
                case 63:
                    Hash ^= Zobrist.Castling[2]; // k
                    break;
            }

            Board.RemovePieceAt(move.CastlingRookFrom);
            Board.AddPieceUnsafe(PieceType.Rook, side, move.CastlingRookTo);
            Board.RemovePieceAt(move.From);
            Board.AddPieceUnsafe(PieceType.King, side, move.To);
            CastlingRights = CastlingRights.WithoutSide(side);

            Hash ^= Zobrist.GetPieceHash(move.CastlingRookFrom, PieceType.Rook, side);
            Hash ^= Zobrist.GetPieceHash(move.CastlingRookTo, PieceType.Rook, side);
            Hash ^= Zobrist.GetPieceHash(move.From, PieceType.King, side);
            Hash ^= Zobrist.GetPieceHash(move.To, PieceType.King, side);
        }

        private void UndoCastlingMove(FastMove move)
        {
            Board.RemovePieceAt(move.CastlingRookTo);
            Board.AddPieceUnsafe(PieceType.Rook, move.Side, move.CastlingRookFrom);
            Board.RemovePieceAt(move.To);
            Board.AddPieceUnsafe(PieceType.King, move.Side, move.From);

            Hash ^= Zobrist.GetPieceHash(move.CastlingRookTo, PieceType.Rook, move.Side);
            Hash ^= Zobrist.GetPieceHash(move.CastlingRookFrom, PieceType.Rook, move.Side);
            Hash ^= Zobrist.GetPieceHash(move.From, PieceType.King, move.Side);
            Hash ^= Zobrist.GetPieceHash(move.To, PieceType.King, move.Side);

            switch (move.CastlingRookFrom)
            {
                case 0:
                    Hash ^= Zobrist.Castling[1]; // Q
                    break;
                case 7:
                    Hash ^= Zobrist.Castling[0]; // K
                    break;
                case 56:
                    Hash ^= Zobrist.Castling[3]; // q
                    break;
                case 63:
                    Hash ^= Zobrist.Castling[2]; // k
                    break;
            }
        }

        private void MakeMoveOnBoard(FastMove move, Span<FastMove> allLegal, bool fillSan)
        {
            var pieceType = Board.GetPieceTypeAt(move.From);
            var pieceSide = Board.GetPieceSideAt(move.From);

            Hash ^= Zobrist.GetEnPassantHash(EpSquareMask);

            if (pieceType == PieceType.Pawn)
            {
                UpdateEnPassantSquare(pieceSide, move);

                // special case, because capture target wouldn't be overwritten by the capturer in case of en passant
                if (move.IsEnPassant)
                {
                    Board.RemovePieceAt(move.EpCaptureTarget);
                    Hash ^= Zobrist.GetPieceHash(move.EpCaptureTarget, PieceType.Pawn, move.Side.Opposite());
                }
            }
            else
            {
                EpSquareMask = 0;
            }

            Hash ^= Zobrist.GetEnPassantHash(EpSquareMask);

            if (move.IsCastling)
            {
                MakeCastlingMove(move);
            }
            else
            {
                // remove castling rights if king or rook moves
                if (pieceType == PieceType.King)
                {
                    CastlingRights = CastlingRights.WithoutSide(pieceSide);
                }
                else if (pieceType == PieceType.Rook)
                {
                    CastlingRights = CastlingRights.RemoveByRookMove(move.From);
                }

                // remove castling rights if rook is captured
                if (move.IsCapture)
                {
                    if (!move.IsEnPassant)
                    {
                        Hash ^= Zobrist.GetPieceHash(move.To, move.CapturedPiece, move.Side.Opposite());
                    }

                    if (Board.GetPieceTypeAt(move.To) == PieceType.Rook)
                    {
                        CastlingRights = CastlingRights.RemoveByRookMove(move.To);
                    }
                }

                Board.RemovePieceAt(move.From);
                Board.AddPieceUnsafe(move.PieceAfterMove, pieceSide, move.To);
                Hash ^= Zobrist.GetPieceHash(move.From, move.Type.ToPieceType(), move.Side);
                Hash ^= Zobrist.GetPieceHash(move.To, move.Type.ToPieceType(), move.Side);
            }

            if (SideToMove == Side.Black)
            {
                CurrentMoveNumber++;
            }

            SideToMove = SideToMove.Opposite();
            Hash ^= Zobrist.WhiteTurn;
            _moveList.Push(move);
            var m = Move.FromFastMove(move);

            if (fillSan)
            {
                m.San = SanGenerator.Generate(move, allLegal);
            }

            Moves.Add(m);
        }

        private void UpdateEnPassantSquare(Side side, FastMove move)
        {
            if (side == Side.White && move.SourceRank == 2 && move.TargetRank == 4)
            {
                EpSquareMask = move.FromMask << 8;
            }
            else if (side == Side.Black && move.SourceRank == 7 && move.TargetRank == 5)
            {
                EpSquareMask = move.FromMask >> 8;
            }
            else
            {
                EpSquareMask = 0;
            }
        }

        private int VerifyMoves(
            Span<FastMove> moves,
            BoardState branchBoard,
            Side sideToMove)
        {
            var legalsCount = 0;

            for (var i = 0; i < moves.Length; i++)
            {
                var isLegal = true;

                // find discovered checks and mates by making/unmaking moves (recursively, if needed)
                if (!moves[i].IsCheck)
                {
                    var movingPieceType = branchBoard.GetPieceTypeAt(moves[i].From);
                    var captureTargetSquare = moves[i].IsEnPassant ? moves[i].EpCaptureTarget : moves[i].To;
                    var oldEnPassant = EpSquareMask;
                    branchBoard.RemovePieceAt(moves[i].From);

                    if (moves[i].IsCapture)
                    {
                        branchBoard.RemovePieceAt(captureTargetSquare);
                    }
                    else if (moves[i].IsCastling)
                    {
                        branchBoard.RemovePieceAt(moves[i].CastlingRookFrom);
                        branchBoard.AddPieceUnsafe(PieceType.Rook, sideToMove, moves[i].CastlingRookTo);
                    }

                    branchBoard.AddPieceUnsafe(moves[i].PieceAfterMove, sideToMove, moves[i].To);

                    // other captures are already checked durung the move generation phase
                    var ownKingExposedAfterMove = moves[i].IsEnPassant
                        ? Movement.IsSquareAttacked(branchBoard.GetOwnKingSquare(sideToMove), sideToMove.Opposite(), branchBoard)
                        : false;

                    if (ownKingExposedAfterMove)
                    {
                        isLegal = false;
                    }
                    else
                    {
                        var enemyKingAttackedAfterMove =
                            Movement.IsSquareAttacked(branchBoard.GetEnemyKingSquare(sideToMove), sideToMove, branchBoard);

                        if (enemyKingAttackedAfterMove)
                        {
                            moves[i].SetCheck();

                            // look for mate by checking, if the opposing side has any legal moves after we make the check
                            var nextPlyBoard = _boardsPool.Get();
                            nextPlyBoard.CopyFrom(branchBoard);
                            var nextPlyMoves = _movesPool.Rent(MAX_MOVES);

                            if (movingPieceType == PieceType.Pawn && Math.Abs(moves[i].From - moves[i].To) == 16)
                            {
                                EpSquareMask = moves[i].To > moves[i].From
                                    ? moves[i].FromMask << 8
                                    : moves[i].FromMask >> 8;
                            }
                            else
                            {
                                EpSquareMask = 0;
                            }

                            var evasionMoves = GenerateMoves(nextPlyMoves, nextPlyBoard, sideToMove.Opposite());

                            if (evasionMoves.IsEmpty)
                            {
                                moves[i].SetMate();
                            }

                            _movesPool.Return(nextPlyMoves);
                            _boardsPool.Return(nextPlyBoard);
                        }
                    }

                    // undo the tested move
                    branchBoard.RemovePieceAt(moves[i].To);
                    branchBoard.AddPieceUnsafe(movingPieceType, sideToMove, moves[i].From);

                    if (moves[i].IsCastling)
                    {
                        branchBoard.RemovePieceAt(moves[i].CastlingRookTo);
                        branchBoard.AddPieceUnsafe(PieceType.Rook, sideToMove, moves[i].CastlingRookFrom);
                    }

                    if (moves[i].IsCapture)
                    {
                        branchBoard.AddPieceUnsafe(moves[i].CapturedPiece, sideToMove.Opposite(), captureTargetSquare);
                    }

                    EpSquareMask = oldEnPassant;
                }

                if (isLegal)
                {
                    legalsCount++;
                    moves[i].Legalize();
                }
            }

            return legalsCount;
        }
    }
}
