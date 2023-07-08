using System.Collections.Immutable;

namespace RV.Chess.Board
{
    internal static class Movement
    {
        internal static readonly ulong[] RANK_MASKS = new ulong[]
        {
            0xFF, 0xFF00, 0xFF0000, 0xFF000000, 0xFF00000000,
            0xFF0000000000, 0xFF000000000000, 0xFF00000000000000,
        };

        internal static readonly ulong[] FILE_MASKS = new ulong[]
        {
            0x101010101010101, 0x202020202020202, 0x404040404040404, 0x808080808080808,
            0x1010101010101010, 0x2020202020202020, 0x4040404040404040, 0x8080808080808080,
        };

        private static readonly ulong[] DIAGONAL_MASKS = new ulong[]
        {
            0x1, 0x102, 0x10204, 0x1020408, 0x102040810, 0x10204081020, 0x1020408102040,
            0x102040810204080, 0x204081020408000, 0x408102040800000, 0x810204080000000,
            0x1020408000000000, 0x2040800000000000, 0x4080000000000000, 0x8000000000000000,
        };

        private static readonly ulong[] ANTI_DIAGONAL_MASKS = new ulong[]
        {
            0x80, 0x8040, 0x804020, 0x80402010, 0x8040201008, 0x804020100804, 0x80402010080402,
            0x8040201008040201, 0x4020100804020100, 0x2010080402010000, 0x1008040201000000,
            0x804020100000000, 0x402010000000000, 0x201000000000000, 0x100000000000000,
        };

        private static readonly int[] KING_OFFSETS = { 9, 1, -7, -8, -9, -1, 7, 8 };
        private static readonly int[] KNIGHT_OFFSETS = { 17, 10, -6, -15, -17, -10, 6, 15 };
        private static readonly int[] WHITE_PAWN_OFFSETS = { 7, 9 };
        private static readonly int[] BLACK_PAWN_OFFSETS = { -7, -9 };

        private static readonly ulong[] KingAttacks = GenerateAttacks(KING_OFFSETS);
        private static readonly ulong[] KnightAttacks = GenerateAttacks(KNIGHT_OFFSETS);
        private static readonly ulong[] PawnAttacksWhite = GenerateAttacks(WHITE_PAWN_OFFSETS);
        private static readonly ulong[] PawnAttacksBlack = GenerateAttacks(BLACK_PAWN_OFFSETS);

        internal static IList<Move> GetBishopMovesFrom(Chessboard board, int bishopSquare, ulong ownBlockers, int enemyKingSquare)
        {
            var targetSquares = GetDiagonalMoves(bishopSquare, board.OccupiedBoard);
            targetSquares &= ~ownBlockers;
            var side = board.GetPieceSideAt(bishopSquare);
            var result = GenerateMovesFromBitboard(bishopSquare, PieceType.Bishop, side, board.OccupiedBoard,
                ownBlockers, targetSquares, enemyKingSquare);

            return result;
        }

        internal static IList<Move> GetQueenMovesFrom(Chessboard board, int queenSquare, ulong ownBlockers, int enemyKingSquare)
        {
            var targetSquares = GetDiagonalMoves(queenSquare, board.OccupiedBoard)
                | GetHorizontalVerticalMoves(queenSquare, board.OccupiedBoard);
            targetSquares &= ~ownBlockers;
            var side = board.GetPieceSideAt(queenSquare);
            var result = GenerateMovesFromBitboard(queenSquare, PieceType.Queen, side, board.OccupiedBoard,
                ownBlockers, targetSquares, enemyKingSquare);

            return result;
        }

        internal static IList<Move> GetRookMovesFrom(Chessboard board, int rookSquare, ulong ownBlockers, int enemyKingSquare)
        {
            var targetSquares = GetHorizontalVerticalMoves(rookSquare, board.OccupiedBoard);
            targetSquares &= ~ownBlockers;
            var side = board.GetPieceSideAt(rookSquare);
            var result = GenerateMovesFromBitboard(rookSquare, PieceType.Rook, side, board.OccupiedBoard,
                ownBlockers, targetSquares, enemyKingSquare);

            return result;
        }

        internal static IList<Move> GetKingMovesFrom(Chessboard board, int kingSquare, ulong ownBlockers,
            bool kingInCheck, CastlingRights castlingRights)
        {
            var side = board.GetPieceSideAt(kingSquare);
            // impossible enemy king square (64) is used, because king can't make a check anyway
            var moves = GetMovesFromPrecalculatedAttacks(kingSquare, PieceType.King, side, board.OccupiedBoard,
                ownBlockers, KingAttacks, 64);

            // castling
            if (castlingRights.Rights != CastlingDirection.None && !kingInCheck)
            {
                var canCastleKingside = side == Side.White && castlingRights.Can(CastlingDirection.WhiteKingside)
                    || side == Side.Black && castlingRights.Can(CastlingDirection.BlackKingside);
                var canCastleQueenside = side == Side.White && castlingRights.Can(CastlingDirection.WhiteQueenside)
                    || side == Side.Black && castlingRights.Can(CastlingDirection.BlackQueenside);

                if (canCastleKingside
                    && CanCastleBetween(board, kingSquare, kingSquare + 3, 3, -3, side))
                {
                    var castleDirection = side == Side.White ? CastlingDirection.WhiteKingside : CastlingDirection.BlackKingside;
                    moves.Add(new Move(PieceType.King, side, kingSquare, kingSquare + 2, castling: castleDirection));
                }

                if (canCastleQueenside
                    && CanCastleBetween(board, kingSquare, kingSquare - 4, -3, 4, side))
                {
                    var castleDirection = side == Side.White ? CastlingDirection.WhiteQueenside : CastlingDirection.BlackQueenside;
                    moves.Add(new Move(PieceType.King, side, kingSquare, kingSquare - 2, castling: castleDirection));
                }
            }

            return moves;
        }

        private static bool CanCastleBetween(
            Chessboard board,
            int kingSquare,
            int rookSquare,
            int kingShift,
            int rookShift,
            Side kingSide)
        {
            var kingMovementRay = RayBetween(kingSquare, kingSquare + kingShift);
            var rookMovementRay = RayBetween(rookSquare, rookSquare + rookShift);

            if ((board.OccupiedBoard & rookMovementRay) > 0)
            {
                return false;
            }

            while (kingMovementRay > 0)
            {
                var square = kingMovementRay.LastSignificantBitIndex();

                // can't castle if there are pieces on the castle line or castling squares are under attack
                if (IsSquareAttacked(board, square, kingSide.Opposite()))
                {
                    return false;
                }

                kingMovementRay &= ~(1UL << square);
            }

            return true;
        }

        internal static IList<Move> GetKnightMovesFrom(Chessboard board, int knightSquare, ulong ownBlockers, int enemyKingSquare)
        {
            var side = board.GetPieceSideAt(knightSquare);
            return GetMovesFromPrecalculatedAttacks(knightSquare, PieceType.Knight, side, board.OccupiedBoard,
                ownBlockers, KnightAttacks, enemyKingSquare);
        }

        internal static IList<Move> GetPawnMovesFrom(Chessboard board, int sourceSquare, int enPassantSquare)
        {
            var result = new List<Move>();
            var side = board.GetPieceSideAt(sourceSquare);

            // quiet 1-square moves and promotions
            var singleMoveTargetSquare = side == Side.White ? sourceSquare + 8 : sourceSquare - 8;
            var isPromotion = (side == Side.White && singleMoveTargetSquare > 55)
                || (side == Side.Black && singleMoveTargetSquare < 8);

            if (!board.OccupiedBoard.OccupiedAt(singleMoveTargetSquare))
            {
                if (isPromotion)
                {
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, singleMoveTargetSquare, promoteTo: PieceType.Bishop));
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, singleMoveTargetSquare, promoteTo: PieceType.Knight));
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, singleMoveTargetSquare, promoteTo: PieceType.Queen));
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, singleMoveTargetSquare, promoteTo: PieceType.Rook));
                }
                else
                {
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, singleMoveTargetSquare));
                }
            }

            // quiet 2-squares move
            var isDoubleMoveAvailable = side == Side.White
                ? (sourceSquare >= 8 && sourceSquare <= 15)
                : (sourceSquare >= 48 && sourceSquare <= 55);

            if (isDoubleMoveAvailable)
            {
                var doubleMoveTargetSquare = side == Side.White ? sourceSquare + 16 : sourceSquare - 16;

                if (!board.OccupiedBoard.OccupiedAt(singleMoveTargetSquare)
                    && !board.OccupiedBoard.OccupiedAt(doubleMoveTargetSquare))
                {
                    result.Add(new Move(PieceType.Pawn, side, sourceSquare, doubleMoveTargetSquare));
                }
            }

            var attacks = side == Side.White ? PawnAttacksWhite[sourceSquare] : PawnAttacksBlack[sourceSquare];

            while (attacks > 0)
            {
                var targetSquare = attacks.LastSignificantBitIndex();
                var targetMask = 1UL << targetSquare;
                var isEnPassant = enPassantSquare == targetSquare;

                if ((board.GetOccupiedBySide(side.Opposite()) & targetMask) > 0 || isEnPassant)
                {
                    var withPromotion = (side == Side.White && targetSquare > 55)
                    || (side == Side.Black && targetSquare < 8);

                    if (withPromotion)
                    {
                        result.Add(new Move(PieceType.Pawn, side, sourceSquare, targetSquare, true, false, isEnPassant, PieceType.Bishop));
                        result.Add(new Move(PieceType.Pawn, side, sourceSquare, targetSquare, true, false, isEnPassant, PieceType.Knight));
                        result.Add(new Move(PieceType.Pawn, side, sourceSquare, targetSquare, true, false, isEnPassant, PieceType.Queen));
                        result.Add(new Move(PieceType.Pawn, side, sourceSquare, targetSquare, true, false, isEnPassant, PieceType.Rook));
                    }
                    else
                    {
                        result.Add(new Move(PieceType.Pawn, side, sourceSquare, targetSquare, true, false, isEnPassant));
                    }
                }

                attacks &= ~(1UL << targetSquare);
            }

            return result;
        }

        internal static ulong GetPinnedPieces(Chessboard board, Side sideToMove)
        {
            var pinners = GetPinners(board, sideToMove);
            var pinnedPieces = 0UL;

            while (pinners > 0)
            {
                var pinnerSquare = pinners.LastSignificantBitIndex();
                var piecesOnAttackRay = RayBetween(pinnerSquare, board.GetKingSquare(sideToMove)) & board.OccupiedBoard;

                // the piece is pinned only if it is alone between the king and the attacker
                if (piecesOnAttackRay.HasSingleBitSet())
                {
                    pinnedPieces |= piecesOnAttackRay;
                }

                pinners &= ~(1UL << pinnerSquare);
            }

            return pinnedPieces;
        }

        internal static ulong GetSquareAttackers(Chessboard board, int square, Side attackerSide)
        {
            var attackerRooks = board.GetPieceBoard(PieceType.Rook, attackerSide);
            var attackerBishops = board.GetPieceBoard(PieceType.Bishop, attackerSide);
            var attackerQueens = board.GetPieceBoard(PieceType.Queen, attackerSide);
            var attackerKnights = board.GetPieceBoard(PieceType.Knight, attackerSide);
            var attackerKing = board.GetPieceBoard(PieceType.King, attackerSide);
            // to find pawn attacks to our square, we can use pawn attacks FROM our square with reversed sides
            var reverseSidePawnAttacks = attackerSide == Side.White ? PawnAttacksBlack[square] : PawnAttacksWhite[square];
            var attackerPawns = board.GetPieceBoard(PieceType.Pawn, attackerSide);
            // remove own king from occupancy, since it cant block checks anyway
            var occupiedNoOwnKing = board.OccupiedBoard
                & ~board.GetPieceBoard(PieceType.King, attackerSide.Opposite());
            var knightAttackers = KnightAttacks[square] & attackerKnights;
            var rookAttackers = GetHorizontalVerticalMoves(square, occupiedNoOwnKing)
                & (attackerRooks | attackerQueens);
            var bishopAttackers = GetDiagonalMoves(square, occupiedNoOwnKing)
                & (attackerBishops | attackerQueens);
            var pawnAttackers = reverseSidePawnAttacks & attackerPawns;
            var kingAttackers = KingAttacks[square] & attackerKing;

            return knightAttackers | rookAttackers | bishopAttackers | pawnAttackers | kingAttackers;
        }

        internal static bool IsKingMoveSafe(Chessboard board, Move m, Side kingSide)
            => GetSquareAttackers(board, m.ToIdx, kingSide.Opposite()) == 0;

        internal static bool IsSquareAttacked(Chessboard board, int square, Side attackerSide)
        {
            var knightAttackSources = KnightAttacks[square];

            if ((knightAttackSources & board.GetPieceBoard(PieceType.Knight, attackerSide)) > 0)
            {
                return true;
            }

            var rookAttacks = GetHorizontalVerticalMoves(square, board.OccupiedBoard);

            if ((rookAttacks & board.GetPieceBoard(PieceType.Rook, attackerSide)) > 0
                || (rookAttacks & board.GetPieceBoard(PieceType.Queen, attackerSide)) > 0)
            {
                return true;
            }

            var bishopAttacks = GetDiagonalMoves(square, board.OccupiedBoard);

            if ((bishopAttacks & board.GetPieceBoard(PieceType.Bishop, attackerSide)) > 0
                || (bishopAttacks & board.GetPieceBoard(PieceType.Queen, attackerSide)) > 0)
            {
                return true;
            }

            var pawnAttacks = attackerSide == Side.White ? PawnAttacksBlack[square] : PawnAttacksWhite[square];

            if ((pawnAttacks & board.GetPieceBoard(PieceType.Pawn, attackerSide)) > 0)
            {
                return true;
            }

            if ((KingAttacks[square] & board.GetPieceBoard(PieceType.King, attackerSide)) > 0)
            {
                return true;
            }

            return false;
        }

        internal static bool IsPinnedPieceMoveSafe(Move m, int ownKingSquare)
        {
            // if the piece was pinned, it should stay on the same line with the king to keep the pin
            // (from, to and king squares should all be aligned)
            // difference between aligned indices is 9*x for the diagonal and 7*x for anti-diagonal
            switch (m.PieceType)
            {
                case PieceType.Pawn:
                    var wasPinnedAlongFile = (m.FromIdx % 8) == (ownKingSquare % 8);
                    var wasPinnedAlongRank = (m.FromIdx / 8) == (ownKingSquare / 8);

                    if (wasPinnedAlongRank)
                    {
                        return false;
                    }
                    else if (wasPinnedAlongFile)
                    {
                        // allow movement along the file
                        return (m.ToIdx % 8) == (ownKingSquare % 8);
                    }
                    else
                    {
                        // allow movement along the diagonal
                        return (m.ToIdx - ownKingSquare) % 9 == 0 || (m.ToIdx - ownKingSquare) % 7 == 0;
                    }
                case PieceType.Bishop:
                    var bishopSameDiagonal = (m.ToIdx - ownKingSquare) % 7 == 0 || (m.ToIdx - ownKingSquare) % 9 == 0;
                    return bishopSameDiagonal;
                case PieceType.Rook:
                    var rookSameRank = (m.FromIdx / 8) == (m.ToIdx / 8) && (m.FromIdx / 8) == (ownKingSquare / 8);
                    var rookSameFile = (m.FromIdx % 8) == (m.ToIdx % 8) && (m.FromIdx % 8) == (ownKingSquare % 8);
                    return rookSameRank || rookSameFile;
                case PieceType.Queen:
                    var sameRankBefore = (m.FromIdx / 8) == (ownKingSquare / 8);
                    var sameRankAfter = (m.ToIdx / 8) == (ownKingSquare / 8);
                    var sameFileBefore = (m.FromIdx % 8) == (ownKingSquare % 8);
                    var sameFileAfter = (m.ToIdx % 8) == (ownKingSquare % 8);
                    var sameDiagonalBefore = (m.FromIdx - ownKingSquare) % 9 == 0;
                    var sameDiagonalAfter = (m.ToIdx - ownKingSquare) % 9 == 0;
                    var sameAntiDiagonalBefore = (m.FromIdx - ownKingSquare) % 7 == 0;
                    var sameAntiDiagonalAfter = (m.ToIdx - ownKingSquare) % 7 == 0;
                    return (sameRankBefore && sameRankAfter)
                        || (sameFileBefore && sameFileAfter)
                        || (sameDiagonalBefore && sameDiagonalAfter)
                        || (sameAntiDiagonalBefore && sameAntiDiagonalAfter);
                case PieceType.Knight:
                    return false;
            }

            return true;
        }

        internal static ulong RayBetween(int from, int to)
        {
            var target = 1UL << to;
            // find, which attack type includes the target square
            var fromSourceBishopRays = GetDiagonalMoves(from, 1UL << to);

            if ((fromSourceBishopRays & target) > 0)
            {
                // diagonal ray crosses the target square
                var fromTargetBishopRays = GetDiagonalMoves(to, 1UL << from);
                return fromSourceBishopRays & fromTargetBishopRays;
            }

            var fromSourceRookRays = GetHorizontalVerticalMoves(from, 1UL << to);

            if ((fromSourceRookRays & target) > 0)
            {
                // straight ray crosses the target square
                var fromTargetRookRays = GetHorizontalVerticalMoves(to, 1UL << from);
                return fromSourceRookRays & fromTargetRookRays;
            }

            return 0;
        }

        private static ulong GetPinners(Chessboard board, Side sideToMove)
        {
            var kingSquare = board.GetKingSquare(sideToMove);
            var enemySide = sideToMove.Opposite();

            if (kingSquare < 0)
            {
                return 0;
            }

            var enemyRooks = board.GetPieceBoard(PieceType.Rook, enemySide);
            var enemyBishops = board.GetPieceBoard(PieceType.Bishop, enemySide);
            var enemyQueens = board.GetPieceBoard(PieceType.Queen, enemySide);
            var kingLinearRays = GetHorizontalVerticalMoves(kingSquare, enemyRooks);
            var pinnerRooks = kingLinearRays & enemyRooks;
            var kingDiagonalRays = GetDiagonalMoves(kingSquare, enemyBishops);
            var pinnerBishops = kingDiagonalRays & enemyBishops;
            var pinnerQueens = (kingLinearRays | kingDiagonalRays) & enemyQueens;

            return pinnerRooks | pinnerBishops | pinnerQueens;
        }

        private static ulong[] GenerateAttacks(int[] offsets)
        {
            var attacks = new ulong[64];

            for (var sourceSquare = 0; sourceSquare < 64; sourceSquare++)
            {
                foreach (var offset in offsets)
                {
                    var targetSquare = sourceSquare + offset;

                    if (targetSquare >= 0 && targetSquare <= 63 && Distance(sourceSquare, targetSquare) <= 2)
                    {
                        attacks[sourceSquare] = attacks[sourceSquare] | (1UL << targetSquare);
                    }
                }
            }

            return attacks;
        }

        private static int Distance(int squareA, int squareB)
        {
            var fileA = squareA / 8;
            var fileB = squareB / 8;
            var rankA = squareA % 8;
            var rankB = squareB % 8;
            return Math.Max(Math.Abs(fileA - fileB), Math.Abs(rankA - rankB));
        }

        private static ulong MaskedSlide(ulong occupied, ulong pieceBitboard, ulong mask)
        {
            var left = (occupied & mask) - 2 * pieceBitboard;
            var right = ((occupied & mask).Reverse() - 2 * pieceBitboard.Reverse()).Reverse();
            var both = left ^ right;
            var slide = both & mask;

            return slide;
        }

        private static ulong GetDiagonalMoves(int square, ulong occupied)
        {
            var mask = 1UL << square;
            var diagonal = MaskedSlide(occupied, mask, DIAGONAL_MASKS[square / 8 + square % 8]);
            var antiDiagonal = MaskedSlide(occupied, mask, ANTI_DIAGONAL_MASKS[square / 8 + 7 - square % 8]);

            return diagonal | antiDiagonal;
        }

        private static ulong GetHorizontalVerticalMoves(int square, ulong occupied)
        {
            var mask = 1UL << square;
            var horizontal = MaskedSlide(occupied, mask, RANK_MASKS[square / 8]);
            var vertical = MaskedSlide(occupied, mask, FILE_MASKS[square % 8]);

            return horizontal | vertical;
        }

        private static IList<Move> GetMovesFromPrecalculatedAttacks(int sourceSquareIdx, PieceType pieceType, Side side,
            ulong occupied, ulong ownBlockers, ulong[] attacks, int enemyKingSquare)
        {
            var targetSquares = attacks[sourceSquareIdx] & ~ownBlockers;
            var result = GenerateMovesFromBitboard(sourceSquareIdx, pieceType, side, occupied,
                ownBlockers, targetSquares, enemyKingSquare);

            return result;
        }

        private static IList<Move> GenerateMovesFromBitboard(int sourceSquareIdx, PieceType pieceType, Side side,
            ulong occupied, ulong ownBlockers, ulong targetSquares, int enemyKingSquare)
        {
            var result = new List<Move>();

            while (targetSquares > 0)
            {
                var targetSquareIdx = targetSquares.LastSignificantBitIndex();
                var isCapture = occupied.OccupiedAt(targetSquareIdx) && !ownBlockers.OccupiedAt(targetSquareIdx);
                var isCheck = targetSquareIdx == enemyKingSquare;
                result.Add(new Move(pieceType, side, sourceSquareIdx, targetSquareIdx, isCapture, isCheck));
                targetSquares &= ~(1UL << targetSquareIdx);
            }

            return result;
        }
    }
}
