using System.Numerics;
using RV.Chess.Board.Game;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Utils
{
    public static class CBPositionSearchBoosterEncoder
    {
        public const byte IGNORE_FINAL_PAWN_COUNT_MASK = 0b10001000;

        public static byte[] Encode(Chessgame game)
        {
            var result = new byte[52];

            result[3] = (byte)(PawnsCountAndNoPieceFlags(game.Board) & IGNORE_FINAL_PAWN_COUNT_MASK);
            var pieceCount = PiecesCount(game.Board);
            result[4] = (byte)(pieceCount & 255);
            result[5] = (byte)(pieceCount >> 8);
            var pawns = PawnsOnRanks(game.Board);
            Array.Copy(pawns, 0, result, 6, pawns.Length);
            var kings = KingsOnFilesAndRanks(game.Board);
            Array.Copy(kings, 0, result, 16, kings.Length);
            var whiteQueenRookOnRanks = PiecesOnRanks(game.Board, PieceType.Queen, PieceType.Rook, Side.White);
            Array.Copy(whiteQueenRookOnRanks, 0, result, 20, whiteQueenRookOnRanks.Length);
            var blackQueenRookOnRanks = PiecesOnRanks(game.Board, PieceType.Queen, PieceType.Rook, Side.Black);
            Array.Copy(blackQueenRookOnRanks, 0, result, 28, blackQueenRookOnRanks.Length);
            var whiteKnightBishopOnRanks = PiecesOnRanks(game.Board, PieceType.Knight, PieceType.Bishop, Side.White);
            Array.Copy(whiteKnightBishopOnRanks, 0, result, 36, whiteKnightBishopOnRanks.Length);
            var blackKnightBishopOnRanks = PiecesOnRanks(game.Board, PieceType.Knight, PieceType.Bishop, Side.Black);
            Array.Copy(blackKnightBishopOnRanks, 0, result, 44, blackKnightBishopOnRanks.Length);

            return result;
        }

        private static byte PawnsCountAndNoPieceFlags(BoardState b)
        {
            /*
                0-2 	Number of white pawns left in final position (7 if 7 or 8 pawns left)
		        3 	    Set if at some point in the game white had no pieces (only pawns)
		        4-6 	Number of black pawns left in final position (7 if 7 or 8 pawns left)
		        7 	    Set if at some point in the game black had no pieces (only pawns)
            */
            var whitePawnsCount = Math.Min(7, b.CountPieces(PieceType.Pawn, Side.White));
            var blackPawnsCount = Math.Min(7, b.CountPieces(PieceType.Pawn, Side.Black));
            var whitePiecesBoard = b.OwnBlockers(Side.White)
                & ~(b.GetPieceBoard(PieceType.King, Side.White) | b.GetPieceBoard(PieceType.Pawn, Side.White));
            var blackPiecesBoard = b.OwnBlockers(Side.Black)
                & ~(b.GetPieceBoard(PieceType.King, Side.Black) | b.GetPieceBoard(PieceType.Pawn, Side.Black));
            var hasNoWhitePieces = BitOperations.PopCount(whitePiecesBoard) == 0 ? 1 : 0;
            var hasNoBlackPieces = BitOperations.PopCount(blackPiecesBoard) == 0 ? 1 : 0;

            return (byte)(hasNoBlackPieces << 7 | blackPawnsCount << 4 | hasNoWhitePieces << 3 | whitePawnsCount);
        }

        private static int PiecesCount(BoardState b)
        {
            /*
                One piece:
                0 	If White had no Queen at some point in the game
		        1 	If White had at most one Rook at some point in the game
		        2 	If White had at most one Bishop at some point in the game
		        3 	If White had at most one Knight at some point in the game
		        4 	If Black had no Queen at some point in the game
		        5 	If Black had at most one Rook at some point in the game
		        6 	If Black had at most one Bishop at some point in the game
		        7 	If Black had at most one Knight at some point in the game
                No pieces:
                0 	If White had no Queen at some point in the game (same as previous byte)
		        1 	If White had no Rook at some point in the game
		        2 	If White had no Bishop at some point in the game
		        3 	If White had no Knight at some point in the game
		        4 	If Black had no Queen at some point in the game
		        5 	If Black had no Rook at some point in the game
		        6 	If Black had no Bishop at some point in the game
		        7 	If Black had no Knight at some point in the game
            */

            var whiteNoQueen = b.GetPieceBoard(PieceType.Queen, Side.White) == 0;
            var whiteRookCount = b.CountPieces(PieceType.Rook, Side.White);
            var blackRookCount = b.CountPieces(PieceType.Rook, Side.Black);
            var whiteBishopCount = b.CountPieces(PieceType.Bishop, Side.White);
            var blackNoQueen = b.GetPieceBoard(PieceType.Queen, Side.Black) == 0;
            var blackBishopCount = b.CountPieces(PieceType.Bishop, Side.Black);
            var whiteKnightCount = b.CountPieces(PieceType.Knight, Side.White);
            var blackKnightCount = b.CountPieces(PieceType.Knight, Side.Black);

            return (whiteNoQueen ? 1 : 0) +
                (whiteRookCount <= 1 ? 2 : 0) +
                (whiteBishopCount <= 1 ? 4 : 0) +
                (whiteKnightCount <= 1 ? 8 : 0) +
                (blackNoQueen ? 16 : 0) +
                (blackRookCount <= 1 ? 32 : 0) +
                (blackBishopCount <= 1 ? 64 : 0) +
                (blackKnightCount <= 1 ? 128 : 0) +
                (whiteNoQueen ? 256 : 0) +
                (whiteRookCount == 0 ? 512 : 0) +
                (whiteBishopCount == 0 ? 1024 : 0) +
                (whiteKnightCount == 0 ? 2048 : 0) +
                (blackNoQueen ? 4096 : 0) +
                (blackRookCount == 0 ? 8192 : 0) +
                (blackBishopCount == 0 ? 16384 : 0) +
                (blackKnightCount == 0 ? 32768 : 0);
        }

        private static byte[] PawnsOnRanks(BoardState b)
        {
            /*
                Bit 0-7 set if a White Pawn has occupied aX, bX, ..., hX respectively
            */
            return new byte[10]
            {
                PawnsOnRankMasks(b, Side.White, 3),
                PawnsOnRankMasks(b, Side.White, 4),
                PawnsOnRankMasks(b, Side.White, 5),
                PawnsOnRankMasks(b, Side.White, 6),
                PawnsOnRankMasks(b, Side.White, 7),
                PawnsOnRankMasks(b, Side.Black, 2),
                PawnsOnRankMasks(b, Side.Black, 3),
                PawnsOnRankMasks(b, Side.Black, 4),
                PawnsOnRankMasks(b, Side.Black, 5),
                PawnsOnRankMasks(b, Side.Black, 6),
            };
        }

        private static byte PawnsOnRankMasks(BoardState b, Side side, int rank)
        {
            var pawns = b.GetPieceBoard(PieceType.Pawn, side);
            var mask = Movement.RanksMasks[rank - 1];
            return (byte)((pawns & mask) >> (rank - 1) * 8);
        }

        private static byte[] KingsOnFilesAndRanks(BoardState b)
        {
            /*
                Bit 0-7 set if the White King has been on rank 1-8
                Bit 0-7 set if the Black King has been on rank 1-8
                Bit 0-7 set if the White King has been on file a-h
                Bit 0-7 set if the Black King has been on file a-h
            */

            var result = new byte[4];
            var whiteKing = b.GetPieceBoard(PieceType.King, Side.White);
            var blackKing = b.GetPieceBoard(PieceType.King, Side.Black);

            for (var bitIdx = 0; bitIdx < 8; bitIdx++)
            {
                var rankMask = Movement.RanksMasks[bitIdx];
                var fileMask = Movement.FileMasks[bitIdx];

                result[0] = (byte)(result[0] | ((whiteKing & rankMask) > 0 ? 1 : 0) << bitIdx);
                result[1] = (byte)(result[1] | ((blackKing & rankMask) > 0 ? 1 : 0) << bitIdx);
                result[2] = (byte)(result[2] | ((whiteKing & fileMask) > 0 ? 1 : 0) << bitIdx);
                result[3] = (byte)(result[3] | ((blackKing & fileMask) > 0 ? 1 : 0) << bitIdx);
            }

            return result;
        }

        private static byte[] PiecesOnRanks(BoardState b, PieceType pa, PieceType pb, Side side)
        {
            var result = new byte[8];

            for (var i = 0; i < 8; i++)
            {
                result[i] = PiecesOnSingleRank(b, pa, pb, side, i + 1);
            }

            return result;
        }

        private static byte PiecesOnSingleRank(BoardState b, PieceType pa, PieceType pb, Side side, int rank)
        {
            // Bit 0-7 set if any piece of type and color has occupied aX, bX, ..., hX, respectively
            var combinedRankOccupancy = (b.GetPieceBoard(pa, side) | b.GetPieceBoard(pb, side))
                & Movement.RanksMasks[rank - 1];

            return (byte)(combinedRankOccupancy >> (rank - 1) * 8);
        }

        private static int CountPieces(this BoardState b, PieceType type, Side side)
        {
            return BitOperations.PopCount(b.GetPieceBoard(type, side));
        }
    }
}
