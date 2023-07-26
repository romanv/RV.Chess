using System.Numerics;
using System.Runtime.Intrinsics.X86;
using RV.Chess.Board.Types;

namespace RV.Chess.Board.Game
{
    internal static class Movement
    {
        internal static readonly ulong[] FileMasks = new ulong[]
        {
            0x101010101010101, 0x202020202020202, 0x404040404040404, 0x808080808080808,
            0x1010101010101010, 0x2020202020202020, 0x4040404040404040, 0x8080808080808080
        };

        internal static readonly ulong[] RanksMasks = new ulong[]
        {
            0xff, 0xff00, 0xff0000, 0xff000000, 0xff00000000, 0xff0000000000, 0xff000000000000, 0xff00000000000000
        };

        private static readonly ulong[] AntidiagonalMasks = new ulong[]
        {
            0x1, 0x102, 0x10204, 0x1020408, 0x102040810, 0x10204081020, 0x1020408102040,
            0x102040810204080, 0x204081020408000, 0x408102040800000, 0x810204080000000,
            0x1020408000000000, 0x2040800000000000, 0x4080000000000000, 0x8000000000000000,
        };

        private static readonly ulong[] DiagonalMasks = new ulong[]
        {
            0x80, 0x8040, 0x804020, 0x80402010, 0x8040201008, 0x804020100804, 0x80402010080402,
            0x8040201008040201, 0x4020100804020100, 0x2010080402010000, 0x1008040201000000,
            0x804020100000000, 0x402010000000000, 0x201000000000000, 0x100000000000000
        };

        private static readonly ulong[] KingAttacks = new ulong[]
        {
            0x0000000000000302, 0x0000000000000705, 0x0000000000000E0A, 0x0000000000001C14,
            0x0000000000003828, 0x0000000000007050, 0x000000000000E0A0, 0x000000000000C040,
            0x0000000000030203, 0x0000000000070507, 0x00000000000E0A0E, 0x00000000001C141C,
            0x0000000000382838, 0x0000000000705070, 0x0000000000E0A0E0, 0x0000000000C040C0,
            0x0000000003020300, 0x0000000007050700, 0x000000000E0A0E00, 0x000000001C141C00,
            0x0000000038283800, 0x0000000070507000, 0x00000000E0A0E000, 0x00000000C040C000,
            0x0000000302030000, 0x0000000705070000, 0x0000000E0A0E0000, 0x0000001C141C0000,
            0x0000003828380000, 0x0000007050700000, 0x000000E0A0E00000, 0x000000C040C00000,
            0x0000030203000000, 0x0000070507000000, 0x00000E0A0E000000, 0x00001C141C000000,
            0x0000382838000000, 0x0000705070000000, 0x0000E0A0E0000000, 0x0000C040C0000000,
            0x0003020300000000, 0x0007050700000000, 0x000E0A0E00000000, 0x001C141C00000000,
            0x0038283800000000, 0x0070507000000000, 0x00E0A0E000000000, 0x00C040C000000000,
            0x0302030000000000, 0x0705070000000000, 0x0E0A0E0000000000, 0x1C141C0000000000,
            0x3828380000000000, 0x7050700000000000, 0xE0A0E00000000000, 0xC040C00000000000,
            0x0203000000000000, 0x0507000000000000, 0x0A0E000000000000, 0x141C000000000000,
            0x2838000000000000, 0x5070000000000000, 0xA0E0000000000000, 0x40C0000000000000,
        };

        private static readonly ulong[] KnightAttacks = new ulong[]
        {
            0x0000000000020400, 0x0000000000050800, 0x00000000000A1100, 0x0000000000142200,
            0x0000000000284400, 0x0000000000508800, 0x0000000000A01000, 0x0000000000402000,
            0x0000000002040004, 0x0000000005080008, 0x000000000A110011, 0x0000000014220022,
            0x0000000028440044, 0x0000000050880088, 0x00000000A0100010, 0x0000000040200020,
            0x0000000204000402, 0x0000000508000805, 0x0000000A1100110A, 0x0000001422002214,
            0x0000002844004428, 0x0000005088008850, 0x000000A0100010A0, 0x0000004020002040,
            0x0000020400040200, 0x0000050800080500, 0x00000A1100110A00, 0x0000142200221400,
            0x0000284400442800, 0x0000508800885000, 0x0000A0100010A000, 0x0000402000204000,
            0x0002040004020000, 0x0005080008050000, 0x000A1100110A0000, 0x0014220022140000,
            0x0028440044280000, 0x0050880088500000, 0x00A0100010A00000, 0x0040200020400000,
            0x0204000402000000, 0x0508000805000000, 0x0A1100110A000000, 0x1422002214000000,
            0x2844004428000000, 0x5088008850000000, 0xA0100010A0000000, 0x4020002040000000,
            0x0400040200000000, 0x0800080500000000, 0x1100110A00000000, 0x2200221400000000,
            0x4400442800000000, 0x8800885000000000, 0x100010A000000000, 0x2000204000000000,
            0x0004020000000000, 0x0008050000000000, 0x00110A0000000000, 0x0022140000000000,
            0x0044280000000000, 0x0088500000000000, 0x0010A00000000000, 0x0020400000000000,
        };

        private static readonly int[] MaskFileOffsets = new int[] { -1, 0, 1, -1 };

        private static readonly int[] MaskRankOffsets = new int[] { 0, -1, -1, -1 };

        private static readonly Mask[] Masks = BuildMasks();

        private static readonly ulong[][] PawnAttacks = new ulong[][]
        {
            new ulong[] {
                0x0000000000000200, 0x0000000000000500, 0x0000000000000A00, 0x0000000000001400,
                0x0000000000002800, 0x0000000000005000, 0x000000000000A000, 0x0000000000004000,
                0x0000000000020000, 0x0000000000050000, 0x00000000000A0000, 0x0000000000140000,
                0x0000000000280000, 0x0000000000500000, 0x0000000000A00000, 0x0000000000400000,
                0x0000000002000000, 0x0000000005000000, 0x000000000A000000, 0x0000000014000000,
                0x0000000028000000, 0x0000000050000000, 0x00000000A0000000, 0x0000000040000000,
                0x0000000200000000, 0x0000000500000000, 0x0000000A00000000, 0x0000001400000000,
                0x0000002800000000, 0x0000005000000000, 0x000000A000000000, 0x0000004000000000,
                0x0000020000000000, 0x0000050000000000, 0x00000A0000000000, 0x0000140000000000,
                0x0000280000000000, 0x0000500000000000, 0x0000A00000000000, 0x0000400000000000,
                0x0002000000000000, 0x0005000000000000, 0x000A000000000000, 0x0014000000000000,
                0x0028000000000000, 0x0050000000000000, 0x00A0000000000000, 0x0040000000000000,
                0x0200000000000000, 0x0500000000000000, 0x0A00000000000000, 0x1400000000000000,
                0x2800000000000000, 0x5000000000000000, 0xA000000000000000, 0x4000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
            },
            new ulong [] {
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000,
                0x0000000000000002, 0x0000000000000005, 0x000000000000000A, 0x0000000000000014,
                0x0000000000000028, 0x0000000000000050, 0x00000000000000A0, 0x0000000000000040,
                0x0000000000000200, 0x0000000000000500, 0x0000000000000A00, 0x0000000000001400,
                0x0000000000002800, 0x0000000000005000, 0x000000000000A000, 0x0000000000004000,
                0x0000000000020000, 0x0000000000050000, 0x00000000000A0000, 0x0000000000140000,
                0x0000000000280000, 0x0000000000500000, 0x0000000000A00000, 0x0000000000400000,
                0x0000000002000000, 0x0000000005000000, 0x000000000A000000, 0x0000000014000000,
                0x0000000028000000, 0x0000000050000000, 0x00000000A0000000, 0x0000000040000000,
                0x0000000200000000, 0x0000000500000000, 0x0000000A00000000, 0x0000001400000000,
                0x0000002800000000, 0x0000005000000000, 0x000000A000000000, 0x0000004000000000,
                0x0000020000000000, 0x0000050000000000, 0x00000A0000000000, 0x0000140000000000,
                0x0000280000000000, 0x0000500000000000, 0x0000A00000000000, 0x0000400000000000,
                0x0002000000000000, 0x0005000000000000, 0x000A000000000000, 0x0014000000000000,
                0x0028000000000000, 0x0050000000000000, 0x00A0000000000000, 0x0040000000000000,
            },
        };

        private static readonly ulong[][] RaysBetween = BuildRays();

        internal static int GetBishopMoves(
            ulong moveTargets,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            int enemyKingSquare,
            CastlingRights cRights,
            int epSquare)
        {
            var bishops = board.GetPieceBoard(PieceType.Bishop, side);

            while (bishops > 0)
            {
                var from = BitOperations.TrailingZeroCount(bishops);
                var pinMask = (1UL << from & pinned) > 0
                    ? GetPinMask(from, board.GetOwnKingSquare(side))
                    : 0xffffffffffffffff;
                var targets = GetDiagonalMoves(from, board.Occupied[2]) & moveTargets & pinMask;
                cursor = MovesFromTargets(from, side, moves, cursor, board,
                    MoveType.Bishop, targets, enemyKingSquare, cRights, epSquare);
                bishops &= bishops - 1;
            }

            return cursor;
        }

        internal static int GetKingEvasions(
            int from,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            CastlingRights cRights,
            int epSquare)
        {
            var targets = KingAttacks[from] & ~board.OwnBlockers(side);

            while (targets > 0)
            {
                var to = BitOperations.TrailingZeroCount(targets);

                if (!IsSquareAttacked(to, side.Opposite(), board, true))
                {
                    var victim = board.GetPieceTypeAt(to);
                    moves[cursor++] = FastMove.Create(from, to, MoveType.King, side, capturedPiece: victim,
                        epBefore: epSquare, castlingBefore: cRights);
                }

                targets &= targets - 1;
            }

            return cursor;
        }

        internal static int GetKingMoves(
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            CastlingRights cRights,
            int epSquare)
        {
            var from = board.GetOwnKingSquare(side);
            var targets = KingAttacks[from] & ~board.OwnBlockers(side);

            while (targets > 0)
            {
                var to = BitOperations.TrailingZeroCount(targets);

                if (!IsSquareAttacked(to, side.Opposite(), board, true))
                {
                    var victim = board.GetPieceTypeAt(to);
                    moves[cursor++] = FastMove.Create(from, to, MoveType.King, side, capturedPiece: victim,
                        epBefore: epSquare, castlingBefore: cRights);
                }

                targets &= targets - 1;
            }

            if (cRights != CastlingRights.None)
            {
                var canCastleKingside = side == Side.White && cRights.CanCastle(CastlingRights.WhiteKingside)
                    || side == Side.Black && cRights.CanCastle(CastlingRights.BlackKingside);
                var canCastleQueenside = side == Side.White && cRights.CanCastle(CastlingRights.WhiteQueenside)
                    || side == Side.Black && cRights.CanCastle(CastlingRights.BlackQueenside);

                if (canCastleKingside && CanCastleBetween(board, from, from + 3, 3, -3, side))
                {
                    moves[cursor++] = FastMove.Create(from, from + 2, MoveType.CastleShort, side,
                        castlingBefore: cRights, epBefore: epSquare);
                }

                if (canCastleQueenside && CanCastleBetween(board, from, from - 4, -3, 4, side))
                {
                    moves[cursor++] = FastMove.Create(from, from - 2, MoveType.CastleLong, side,
                        castlingBefore: cRights, epBefore: epSquare);
                }
            }

            return cursor;
        }

        internal static int GetKnightMoves(
            ulong moveTargets,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            int enemyKingSquare,
            CastlingRights cRights,
            int epSquare)
        {
            var knights = board.GetPieceBoard(PieceType.Knight, side);

            while (knights > 0)
            {
                var from = BitOperations.TrailingZeroCount(knights);

                if ((1UL << from & pinned) == 0)
                {
                    var targets = KnightAttacks[from] & moveTargets;
                    cursor = MovesFromTargets(from, side, moves, cursor, board,
                        MoveType.Knight, targets, enemyKingSquare, cRights, epSquare);
                }

                knights &= knights - 1;
            }

            return cursor;
        }

        internal static int GetQueenMoves(
            ulong moveTargets,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            int enemyKingSquare,
            CastlingRights cRights,
            int epSquare)
        {
            var queens = board.GetPieceBoard(PieceType.Queen, side);

            while (queens > 0)
            {
                var from = BitOperations.TrailingZeroCount(queens);
                var pinMask = (1UL << from & pinned) > 0
                    ? GetPinMask(from, board.GetOwnKingSquare(side))
                    : 0xffffffffffffffff;
                var targets = (GetDiagonalMoves(from, board.Occupied[2])
                    | GetLinearMoves(from, board.Occupied[2])) & moveTargets & pinMask;
                cursor = MovesFromTargets(from, side, moves, cursor, board,
                    MoveType.Queen, targets, enemyKingSquare, cRights, epSquare);
                queens &= queens - 1;
            }

            return cursor;
        }

        internal static int GetRookMoves(
            ulong moveTargets,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            int enemyKingSquare,
            CastlingRights cRights,
            int epSquare)
        {
            var rooks = board.GetPieceBoard(PieceType.Rook, side);

            while (rooks > 0)
            {
                var from = BitOperations.TrailingZeroCount(rooks);
                var pinMask = (1UL << from & pinned) > 0
                    ? GetPinMask(from, board.GetOwnKingSquare(side))
                    : 0xffffffffffffffff;
                var targets = GetLinearMoves(from, board.Occupied[2]) & moveTargets & pinMask;
                cursor = MovesFromTargets(from, side, moves, cursor, board,
                    MoveType.Rook, targets, enemyKingSquare, cRights, epSquare);
                rooks &= rooks - 1;
            }

            return cursor;
        }

        internal static int GetWhitePawnMoves(
            ulong moveTargets,
            ulong captureTargets,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            CastlingRights cRights,
            int epSquare)
        {
            var whitePawns = board.PieceBoards[6] & board.Occupied[0];
            var pinnedPawns = whitePawns & pinned;
            var freePawns = whitePawns ^ pinnedPawns;
            var singlePushes = freePawns << 8 & ~board.Occupied[2];
            var pushPromotions = singlePushes & RanksMasks[7] & moveTargets;
            var pushSimple = (singlePushes ^ pushPromotions) & moveTargets;
            var doublePushes = singlePushes << 8 & ~board.Occupied[2] & RanksMasks[3] & moveTargets;

            while (pushSimple > 0)
            {
                var to = BitOperations.TrailingZeroCount(pushSimple);
                var from = to - 8;
                moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.White, castlingBefore: cRights, epBefore: epSquare);
                pushSimple &= pushSimple - 1;
            }

            while (pushPromotions > 0)
            {
                var to = BitOperations.TrailingZeroCount(pushPromotions);
                var from = to - 8;
                cursor = AddPawnPromotions(from, to, Side.White, moves, cursor, cRights, epSquare);
                pushPromotions &= pushPromotions - 1;
            }

            while (doublePushes > 0)
            {
                var to = BitOperations.TrailingZeroCount(doublePushes);
                var from = to - 16;
                moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.White, castlingBefore: cRights, epBefore: epSquare);
                doublePushes &= doublePushes - 1;
            }

            while (freePawns > 0)
            {
                var from = BitOperations.TrailingZeroCount(freePawns);
                var attacks = PawnAttacks[0][from] & (board.Occupied[1] | 1UL << epSquare) & captureTargets;

                while (attacks > 0)
                {
                    var to = BitOperations.TrailingZeroCount(attacks);
                    var victim = board.GetPieceTypeAt(to == epSquare ? to - 8 : to);

                    if ((Bmi1.X64.ExtractLowestSetBit(freePawns) & RanksMasks[6]) > 0)
                    {
                        cursor = AddPawnPromotionsWithCapture(from, to, victim, Side.White,
                            moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.White,
                            capturedPiece: victim, epBefore: epSquare, castlingBefore: cRights);
                    }

                    attacks &= attacks - 1;
                }

                freePawns &= freePawns - 1;
            }

            while (pinnedPawns > 0)
            {
                var from = BitOperations.TrailingZeroCount(pinnedPawns);
                var pawnMask = Bmi1.X64.ExtractLowestSetBit(pinnedPawns);
                var ownKingSquare = board.GetOwnKingSquare(Side.White);
                var pinMask = (pawnMask & pinned) > 0
                    ? GetPinMask(from, ownKingSquare)
                    : 0xffffffffffffffff;
                var targets = pinMask & moveTargets;
                var singlePush = pawnMask << 8 & targets;
                var doublePush = singlePush << 8 & ~board.Occupied[2] & RanksMasks[3] & targets;
                var captures = PawnAttacks[0][from] & captureTargets & pinMask;

                if (singlePush > 0)
                {
                    var to = from + 8;

                    if (to / 8 == 0)
                    {
                        cursor = AddPawnPromotions(from, to, Side.White, moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.White,
                            castlingBefore: cRights, epBefore: epSquare);
                    }

                    if (doublePush > 0)
                    {
                        moves[cursor++] = FastMove.Create(from, to + 8, MoveType.Pawn, Side.White,
                            castlingBefore: cRights, epBefore: epSquare);
                    }
                }

                if (captures > 0)
                {
                    var to = BitOperations.TrailingZeroCount(captures);
                    var victim = board.GetPieceTypeAt(to == epSquare ? to - 8 : to);

                    if (to / 8 == 0)
                    {
                        cursor = AddPawnPromotionsWithCapture(from, to, victim, Side.White, moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.White,
                            capturedPiece: victim, castlingBefore: cRights, epBefore: epSquare);
                    }
                }

                pinnedPawns &= pinnedPawns - 1;
            }

            return cursor;
        }

        internal static int GetBlackPawnMoves(
            ulong moveTargets,
            ulong captureTargets,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            ulong pinned,
            CastlingRights cRights,
            int epSquare)
        {
            var blackPawns = board.PieceBoards[6] & board.Occupied[1];
            var pinnedPawns = blackPawns & pinned;
            var freePawns = blackPawns ^ pinnedPawns;
            var singlePushes = freePawns >> 8 & ~board.Occupied[2];
            var pushPromotions = singlePushes & RanksMasks[0] & moveTargets;
            var pushSimple = (singlePushes ^ pushPromotions) & moveTargets;
            var doublePushes = singlePushes >> 8 & ~board.Occupied[2] & RanksMasks[4] & moveTargets;

            while (pushSimple > 0)
            {
                var to = BitOperations.TrailingZeroCount(pushSimple);
                var from = to + 8;
                moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.Black, castlingBefore: cRights, epBefore: epSquare);
                pushSimple &= pushSimple - 1;
            }

            while (pushPromotions > 0)
            {
                var to = BitOperations.TrailingZeroCount(pushPromotions);
                var from = to + 8;
                cursor = AddPawnPromotions(from, to, Side.Black, moves, cursor, cRights, epSquare);
                pushPromotions &= pushPromotions - 1;
            }

            while (doublePushes > 0)
            {
                var to = BitOperations.TrailingZeroCount(doublePushes);
                var from = to + 16;
                moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.Black, castlingBefore: cRights, epBefore: epSquare);
                doublePushes &= doublePushes - 1;
            }

            while (freePawns > 0)
            {
                var from = BitOperations.TrailingZeroCount(freePawns);
                var attacks = PawnAttacks[1][from] & (board.Occupied[0] | 1UL << epSquare) & captureTargets;

                while (attacks > 0)
                {
                    var to = BitOperations.TrailingZeroCount(attacks);
                    var victim = board.GetPieceTypeAt(to == epSquare ? to + 8 : to);

                    if ((Bmi1.X64.ExtractLowestSetBit(freePawns) & RanksMasks[1]) > 0)
                    {
                        cursor = AddPawnPromotionsWithCapture(from, to, victim, Side.Black,
                            moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.Black,
                            capturedPiece: victim, epBefore: epSquare, castlingBefore: cRights);
                    }

                    attacks &= attacks - 1;
                }

                freePawns &= freePawns - 1;
            }

            while (pinnedPawns > 0)
            {
                var from = BitOperations.TrailingZeroCount(pinnedPawns);
                var pawnMask = Bmi1.X64.ExtractLowestSetBit(pinnedPawns);
                var ownKingSquare = board.GetOwnKingSquare(Side.Black);
                var pinMask = (pawnMask & pinned) > 0
                    ? GetPinMask(from, ownKingSquare)
                    : 0xffffffffffffffff;
                var targets = pinMask & moveTargets;
                var singlePush = pawnMask >> 8 & targets;
                var doublePush = singlePush >> 8 & ~board.Occupied[2] & RanksMasks[4] & targets;
                var captures = PawnAttacks[1][from] & captureTargets & pinMask;

                if (singlePush > 0)
                {
                    var to = from - 8;

                    if (to / 8 == 0)
                    {
                        cursor = AddPawnPromotions(from, to, Side.Black, moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.Black,
                            castlingBefore: cRights, epBefore: epSquare);
                    }

                    if (doublePush > 0)
                    {
                        moves[cursor++] = FastMove.Create(from, to - 8, MoveType.Pawn, Side.Black,
                            castlingBefore: cRights, epBefore: epSquare);
                    }
                }

                if (captures > 0)
                {
                    var to = BitOperations.TrailingZeroCount(captures);
                    var victim = board.GetPieceTypeAt(to == epSquare ? to + 8 : to);

                    if (to / 8 == 0)
                    {
                        cursor = AddPawnPromotionsWithCapture(from, to, victim, Side.Black, moves, cursor, cRights, epSquare);
                    }
                    else
                    {
                        moves[cursor++] = FastMove.Create(from, to, MoveType.Pawn, Side.Black,
                            capturedPiece: victim, castlingBefore: cRights, epBefore: epSquare);
                    }
                }

                pinnedPawns &= pinnedPawns - 1;
            }

            return cursor;
        }

        internal static int GetCheckDefenses(
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            int ownKingSquare,
            ulong checkers,
            ulong pinned,
            CastlingRights cRights,
            int epSquare)
        {
            var blockTargets = 0UL;
            var checkerSquare = BitOperations.TrailingZeroCount(checkers);
            var checkerType = board.GetPieceTypeAt(checkerSquare);
            var epCheckerTarget = 0UL;
            // this function is only called on pieces that have no blockers between them and the king
            // therefore we can freely use the ray between pieces without additional blocker check
            if (checkerType == PieceType.Rook || checkerType == PieceType.Bishop || checkerType == PieceType.Queen)
            {
                blockTargets |= RaysBetween[ownKingSquare][checkerSquare];
            }
            else if (checkerType == PieceType.Pawn
                && (side == Side.White && checkerSquare == epSquare - 8
                    || side == Side.Black && checkerSquare == epSquare + 8))
            {
                epCheckerTarget = 1UL << epSquare;
            }

            var targets = blockTargets | 1UL << checkerSquare;
            // king moves are generated separately as evasions outside of this function, pawns are in a separate loop
            var ownPieces = board.Occupied[(int)side]
                ^ (board.PieceBoards[1] | board.PieceBoards[6]) & board.Occupied[(int)side];
            var enemyKingSquare = board.GetEnemyKingSquare(side);

            if (side == Side.White)
            {
                cursor = GetWhitePawnMoves(blockTargets, checkers | epCheckerTarget, moves,
                    cursor, board, pinned, cRights, epSquare);
            }
            else
            {
                cursor = GetBlackPawnMoves(blockTargets, checkers | epCheckerTarget, moves,
                    cursor, board, pinned, cRights, epSquare);
            }

            while (ownPieces > 0)
            {
                var from = BitOperations.TrailingZeroCount(ownPieces);
                var type = board.GetPieceTypeAt(from);
                var pinMask = (1UL << from & pinned) > 0
                    ? GetPinMask(from, ownKingSquare)
                    : 0xffffffffffffffff;

                switch (type)
                {
                    case PieceType.Rook:
                        var rookTargets = GetLinearMoves(from, board.Occupied[2]) & targets & pinMask;
                        cursor = MovesFromTargets(from, side, moves, cursor, board, MoveType.Rook,
                            rookTargets, enemyKingSquare, cRights, epSquare);
                        break;
                    case PieceType.Bishop:
                        var bishopTargets = GetDiagonalMoves(from, board.Occupied[2]) & targets & pinMask;
                        cursor = MovesFromTargets(from, side, moves, cursor, board, MoveType.Bishop,
                            bishopTargets, enemyKingSquare, cRights, epSquare);
                        break;
                    case PieceType.Queen:
                        var queenTargets = (GetDiagonalMoves(from, board.Occupied[2])
                            | GetLinearMoves(from, board.Occupied[2])) & targets & pinMask;
                        cursor = MovesFromTargets(from, side, moves, cursor, board, MoveType.Queen,
                            queenTargets, enemyKingSquare, cRights, epSquare);
                        break;
                    case PieceType.Knight:
                        var knightTargets = KnightAttacks[from] & targets & pinMask;
                        cursor = MovesFromTargets(from, side, moves, cursor, board, MoveType.Knight,
                            knightTargets, enemyKingSquare, cRights, epSquare);
                        break;
                }

                ownPieces &= ownPieces - 1;
            }

            return cursor;
        }

        internal static ulong GetPinnedPieces(BoardState board, Side sideToMove)
        {
            var pinners = GetPinners(board, sideToMove);
            var pinnedPieces = 0UL;

            while (pinners > 0)
            {
                var pinnerSquare = BitOperations.TrailingZeroCount(pinners);
                var piecesOnAttackRay = RaysBetween[pinnerSquare][board.GetOwnKingSquare(sideToMove)] & board.Occupied[2];
                // the piece is pinned only if it is alone between the king and the attacker
                if (piecesOnAttackRay.HasSingleBitSet())
                {
                    pinnedPieces |= piecesOnAttackRay;
                }

                pinners &= pinners - 1;
            }

            return pinnedPieces;
        }

        internal static ulong GetSquareAttackers(BoardState board, int square, Side attackerSide)
        {
            var rooks = board.GetPieceBoard(PieceType.Rook, attackerSide);
            var bishops = board.GetPieceBoard(PieceType.Bishop, attackerSide);
            var queens = board.GetPieceBoard(PieceType.Queen, attackerSide);
            var knights = board.GetPieceBoard(PieceType.Knight, attackerSide);
            var king = board.GetPieceBoard(PieceType.King, attackerSide);
            var pawns = board.GetPieceBoard(PieceType.Pawn, attackerSide);
            var knightAttackers = KnightAttacks[square] & knights;
            var rookAttackers = GetLinearMoves(square, board.Occupied[2]) & (rooks | queens);
            var bishopAttackers = GetDiagonalMoves(square, board.Occupied[2]) & (bishops | queens);
            var pawnAttackers = PawnAttacks[(int)attackerSide ^ 1][square] & pawns;
            var kingAttackers = KingAttacks[square] & king;

            return knightAttackers | rookAttackers | bishopAttackers | pawnAttackers | kingAttackers;
        }

        internal static bool IsSquareAttacked(int square, Side attackerSide, BoardState board, bool ignoreKing = false)
        {
            var blockers = ignoreKing
                ? board.Occupied[2] ^ board.PieceBoards[1] & board.Occupied[(int)attackerSide ^ 1]
                : board.Occupied[2];

            if ((PawnAttacks[(int)attackerSide ^ 1][square]
                & board.GetPieceBoard(PieceType.Pawn, attackerSide)) > 0)
            {
                return true;
            }

            if ((KingAttacks[square] & board.GetPieceBoard(PieceType.King, attackerSide)) > 0)
            {
                return true;
            }

            if ((KnightAttacks[square] & board.GetPieceBoard(PieceType.Knight, attackerSide)) > 0)
            {
                return true;
            }

            var rookAttacks = GetLinearMoves(square, blockers);
            var queens = board.GetPieceBoard(PieceType.Queen, attackerSide);

            if ((rookAttacks & board.GetPieceBoard(PieceType.Rook, attackerSide)) > 0
                || (rookAttacks & queens) > 0)
            {
                return true;
            }

            var bishopAttacks = GetDiagonalMoves(square, blockers);

            if ((bishopAttacks & board.GetPieceBoard(PieceType.Bishop, attackerSide)) > 0
                || (bishopAttacks & queens) > 0)
            {
                return true;
            }

            return false;
        }

        private static int AddPawnPromotions(
            int from,
            int to,
            Side side,
            Span<FastMove> moves,
            int cursor,
            CastlingRights cRights,
            int epSquare)
        {
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteB, side, epBefore: epSquare, castlingBefore: cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteN, side, epBefore: epSquare, castlingBefore: cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteQ, side, epBefore: epSquare, castlingBefore: cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteR, side, epBefore: epSquare, castlingBefore: cRights);
            return cursor;
        }

        private static int AddPawnPromotionsWithCapture(
            int from,
            int to,
            PieceType victim,
            Side side,
            Span<FastMove> moves,
            int cursor,
            CastlingRights cRights,
            int epSquare)
        {
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteB, side, false, false, victim, epSquare, cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteN, side, false, false, victim, epSquare, cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteQ, side, false, false, victim, epSquare, cRights);
            moves[cursor++] = FastMove.Create(from, to, MoveType.PromoteR, side, false, false, victim, epSquare, cRights);
            return cursor;
        }

        private static Mask[] BuildMasks()
        {
            var patterns = new Mask[256];

            for (var dir = 0; dir < 4; dir++)
            {
                for (var pos = 0; pos < 64; pos++)
                {
                    var lower = BuildPattern(pos, MaskFileOffsets[dir], MaskRankOffsets[dir]);
                    var upper = BuildPattern(pos, -MaskFileOffsets[dir], -MaskRankOffsets[dir]);
                    var combined = lower | upper;
                    patterns[dir * 64 + pos] = new Mask
                    {
                        Lower = lower,
                        Upper = upper,
                        Combined = combined,
                    };
                }
            }

            return patterns;
        }

        private static ulong BuildPattern(int square, int dirCol, int dirRow)
        {
            var col = square % 8;
            var row = square / 8;
            var pattern = 0UL;

            for (var i = 0; i <= 7; i++)
            {
                col += dirCol;
                row += dirRow;

                if (col < 0 || col > 7 || row < 0 || row > 7)
                {
                    break;
                }

                pattern |= 1UL << row * 8 + col;
            }

            return pattern;
        }

        private static ulong[][] BuildRays()
        {
            var rays = new ulong[64][];

            for (var from = 0; from < 64; from++)
            {
                rays[from] = new ulong[64];

                for (var to = 0; to < 64; to++)
                {
                    var target = 1UL << to;
                    var fromSourceBishopRays = GetDiagonalMoves(from, 1UL << to);

                    if ((fromSourceBishopRays & target) > 0)
                    {
                        // diagonal ray crosses the target square
                        var fromTargetBishopRays = GetDiagonalMoves(to, 1UL << from);
                        rays[from][to] = fromSourceBishopRays & fromTargetBishopRays;
                    }
                    else
                    {
                        var fromSourceRookRays = GetLinearMoves(from, 1UL << to);

                        if ((fromSourceRookRays & target) > 0)
                        {
                            // straight ray crosses the target square
                            var fromTargetRookRays = GetLinearMoves(to, 1UL << from);
                            rays[from][to] = fromSourceRookRays & fromTargetRookRays;
                        }
                        else
                        {
                            rays[from][to] = 0;
                        }
                    }
                }
            }

            return rays;
        }

        private static bool CanCastleBetween(
            BoardState board,
            int kingSquare,
            int rookSquare,
            int kingShift,
            int rookShift,
            Side kingSide)
        {
            var kingMovementRay = RaysBetween[kingSquare][kingSquare + kingShift];
            var rookMovementRay = RaysBetween[rookSquare][rookSquare + rookShift];

            if ((board.Occupied[2] & rookMovementRay) > 0)
            {
                return false;
            }

            while (kingMovementRay > 0)
            {
                var square = BitOperations.TrailingZeroCount(kingMovementRay);

                // can't castle if there are pieces on the castle line or castling squares are under attack
                if (IsSquareAttacked(square, kingSide.Opposite(), board))
                {
                    return false;
                }

                kingMovementRay &= kingMovementRay - 1;
            }

            return true;
        }

        // using https://www.chessprogramming.org/Obstruction_Difference
        private static ulong GetDiagonalMoves(int square, ulong occupied) =>
            GetLineAttacks(in Masks[square + 128], occupied) | GetLineAttacks(in Masks[square + 192], occupied);

        private static ulong GetLinearMoves(int square, ulong occupied) =>
            GetLineAttacks(in Masks[square], occupied) | GetLineAttacks(in Masks[square + 64], occupied);

        private static ulong GetLineAttacks(in Mask pattern, ulong blockers)
        {
            var lower = pattern.Lower & blockers;
            var upper = pattern.Upper & blockers;
            var ms1b = 0x8000000000000000 >> BitOperations.LeadingZeroCount(lower | 1);
            var odiff = upper ^ upper - ms1b;
            return pattern.Combined & odiff;
        }

        private static ulong GetPinMask(int pieceSquare, int kingSquare)
        {
            if (pieceSquare / 8 == kingSquare / 8)
            {
                return RanksMasks[pieceSquare / 8];
            }
            else if (pieceSquare % 8 == kingSquare % 8)
            {
                return FileMasks[pieceSquare % 8];
            }
            else if ((pieceSquare - kingSquare) % 9 == 0)
            {
                return DiagonalMasks[7 + pieceSquare / 8 - pieceSquare % 8];
            }
            else if ((pieceSquare - kingSquare) % 7 == 0)
            {
                return AntidiagonalMasks[pieceSquare / 8 + pieceSquare % 8];
            }

            return 0;
        }

        private static ulong GetPinners(BoardState board, Side sideToMove)
        {
            var kingSquare = board.GetOwnKingSquare(sideToMove);
            var enemyPieces = board.Occupied[(int)sideToMove ^ 1];
            var enemyQueens = board.PieceBoards[2] & enemyPieces;
            var enemyRooks = board.PieceBoards[3] & enemyPieces;
            var enemyBishops = board.PieceBoards[4] & enemyPieces;
            var kingLinearRays = GetLinearMoves(kingSquare, enemyRooks);
            var pinnerRooks = kingLinearRays & enemyRooks;
            var kingDiagonalRays = GetDiagonalMoves(kingSquare, enemyBishops);
            var pinnerBishops = kingDiagonalRays & enemyBishops;
            var pinnerQueens = (kingLinearRays | kingDiagonalRays) & enemyQueens;
            return pinnerRooks | pinnerBishops | pinnerQueens;
        }

        private static int MovesFromTargets(
            int from,
            Side side,
            Span<FastMove> moves,
            int cursor,
            BoardState board,
            MoveType type,
            ulong targets,
            int enemyKingSquare,
            CastlingRights cDir,
            int epSquare)
        {
            while (targets > 0)
            {
                var to = BitOperations.TrailingZeroCount(targets);
                var enemyPieces = board.Occupied[(int)side ^ 1];
                var isCapture = (1UL << to & enemyPieces) > 0;
                var isCheck = to == enemyKingSquare;

                if (isCapture)
                {
                    var victim = board.GetPieceTypeAt(to);
                    moves[cursor++] = FastMove.Create(from, to, type, side, isCheck: isCheck,
                        capturedPiece: victim, castlingBefore: cDir, epBefore: epSquare);
                }
                else
                {
                    moves[cursor++] = FastMove.Create(from, to, type, side, isCheck: isCheck, castlingBefore: cDir, epBefore: epSquare);
                }

                targets &= targets - 1;
            }

            return cursor;
        }

        private readonly struct Mask
        {
            internal ulong Combined { get; init; }
            internal ulong Lower { get; init; }
            internal ulong Upper { get; init; }
        }
    }
}
