using System.Numerics;
using RV.Chess.Board.Game;
using RV.Chess.Shared.Types;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class MovingTests
    {
        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "e2", "e4",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1"
        )]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
            "d7", "d5",
            "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2"
        )]
        [InlineData("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2",
            "f1", "b5",
            "rnbqkbnr/ppp1pppp/8/1B1p4/4P3/8/PPPP1PPP/RNBQK1NR b KQkq - 1 2"
        )]
        [InlineData("r3k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/2KR3R w kq - 3 12",
            "c1", "b1",
            "r3k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/1K1R3R b kq - 4 12"
        )]
        public void Coordinates_Basic(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Theory]
        [InlineData("rnbqkbnr/pp1ppppp/2p5/4P3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2",
            "d7", "d5",
            "rnbqkbnr/pp2pppp/2p5/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3"
        )]
        [InlineData("rnbqkbnr/pp2pppp/2p5/4P3/3p4/7P/PPPP1PP1/RNBQKBNR w KQkq - 0 4",
            "c2", "c4",
            "rnbqkbnr/pp2pppp/2p5/4P3/2Pp4/7P/PP1P1PP1/RNBQKBNR b KQkq c3 0 4"
        )]
        public void Coordinates_EnPassant(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Fact]
        public void NullMove_DoesNotChangePiecePositions()
        {
            var fen = "2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20";
            var game = new Chessgame(fen);
            game.MakeNullMove();
            Assert.Equal(fen.Split(' ')[0], game.Fen.Split(' ')[0]);
        }

        [Fact]
        public void NullMove_ChangesSideToMove()
        {
            var game = new Chessgame("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20");
            game.MakeNullMove();
            Assert.Equal("b", game.Fen.Split(' ')[1]);
        }

        [Theory]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d7", "d8",
            "r1bB1knr/pp2p2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R b KQ - 0 13")]
        public void Promotions_Basic(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to, PieceType.Bishop);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Theory]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d8=R+",
            "r1bR1knr/pp2p2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R b KQ - 0 13")]
        public void Promotions_BySan_PromotesToCorrectPiece(string fen, string san, string fenAfter)
        {
            var game = new Chessgame(fen);
            var move = game.GetLegalMoves().Find(m => m.San == san);
            game.TryMakeMove(move!);
            Assert.Equal(game.Fen, fenAfter);
        }

        [Theory]
        [InlineData("r2qkbnr/pp1npppp/3p4/2p5/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 0 5", "e1", "g1",
            "r2qkbnr/pp1npppp/3p4/2p5/4P3/5N2/PPPP1PPP/RNBQ1RK1 b kq - 1 5")]
        [InlineData("r3kbnr/pp1npppp/3p4/q1p5/4P3/1PN2N2/P1PP1PPP/R1BQK2R b KQkq - 0 6", "e8", "c8",
            "2kr1bnr/pp1npppp/3p4/q1p5/4P3/1PN2N2/P1PP1PPP/R1BQK2R w KQ - 1 7")]
        public void Castling_Basic(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Theory]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "e1", "f1",
            (CastlingRights.BlackKingside | CastlingRights.BlackQueenside))]
        [InlineData("r1bqk2r/pp2npbp/2npp1p1/2p5/4PP2/1PPP1NP1/P5BP/RNBQK2R b KQkq - 0 9", "e8", "d7",
            (CastlingRights.WhiteKingside | CastlingRights.WhiteQueenside))]
        public void Castling_MovingKingDisablesBothRights(string fen, string from, string to, CastlingRights expectedRights)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            Assert.Equal(game.CastlingRights, expectedRights);
        }

        [Theory]
        [InlineData("r1b1k2r/ppppqNpp/2n2n2/4p3/2B1P3/8/PPPP1bPP/RNBQ1K1R w kq - 2 7", "f7", "h8",
            "r1b1k2N/ppppq1pp/2n2n2/4p3/2B1P3/8/PPPP1bPP/RNBQ1K1R b q - 0 7")]
        [InlineData("rn1qk2r/pp3ppp/2p1p1b1/4N3/2BP2PP/2B2P2/PPn1Q3/R4K1R b kq - 1 14", "c2", "a1",
            "rn1qk2r/pp3ppp/2p1p1b1/4N3/2BP2PP/2B2P2/PP2Q3/n4K1R w kq - 0 15")]
        public void Castling_CapturingRookRemovesRight(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Theory]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R w KQkq - 0 10", "a1", "a2", CastlingRights.WhiteQueenside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R w KQkq - 0 10", "h1", "g1", CastlingRights.WhiteKingside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R b KQkq - 0 10", "h8", "g8", CastlingRights.BlackKingside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R b KQkq - 0 10", "a8", "b8", CastlingRights.BlackQueenside)]
        public void Castling_MovingRookRemovesRight(string fen, string from, string to, CastlingRights removedRight)
        {
            var game = new Chessgame(fen);
            Assert.True(game.CastlingRights.CanCastle(removedRight));
            game.MakeMove(from, to);
            Assert.False(game.CastlingRights.CanCastle(removedRight));
        }

        [Theory]
        [InlineData("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20", "d5", "e6",
            "2rq1rk1/pp3p1p/3pP2Q/5p2/3R4/1P3P2/P1P2nPP/1K5R b - - 0 20")]
        public void Captures_Basic(string fen, string from, string to, string fenAfter)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            game.MakeMove(from, to);
            Assert.Equal(fenAfter, game.Fen);
        }

        [Theory]
        [InlineData("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20", "d5", "e6", PieceType.None)]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d7", "d8", PieceType.Queen)]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d7", "c8", PieceType.Queen)]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "f3", "d4", PieceType.None)]
        public void Undo_Basic(string fen, string from, string to, PieceType promoteTo)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to, promoteTo);
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
        }

        [Theory]
        [InlineData("2bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/1NBQK2R w Kk - 0 10", "e1", "g1",
                CastlingRights.WhiteKingside | CastlingRights.BlackKingside)]
        [InlineData("r3k3/p1bqnpbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/1NBQK2R b Kq - 0 10", "e8", "c8",
                CastlingRights.WhiteKingside | CastlingRights.BlackQueenside)]
        public void Undo_UndoingCastlingRestoresCastlingRights(string fen, string from, string to, CastlingRights castling)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
            Assert.Equal(castling, game.CastlingRights);
        }

        [Theory]
        [InlineData("r1bq2k1/1p1n2bn/p2p4/2pP1r2/P6p/2N1BP2/1P2B2P/R2QKN1R w K - 0 18", "h1", "g1", CastlingRights.WhiteKingside)]
        public void Undo_UndoingRookMoveRestoresCastlingRights(string fen, string from, string to, CastlingRights castling)
        {
            var game = new Chessgame(fen);
            game.MakeMove(from, to);
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
            Assert.Equal(castling, game.CastlingRights);
        }

        [Fact]
        public void Undo_EnPassant_UndoingCaptureRestoresSquare()
        {
            var game = new Chessgame("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20");
            Assert.Equal(44, BitOperations.TrailingZeroCount(game.EpSquareMask));
            game.MakeMove("d5", "e6");
            Assert.Equal(0UL, game.EpSquareMask);
            game.UndoLastMove();
            Assert.Equal(44, BitOperations.TrailingZeroCount(game.EpSquareMask));
        }

        [Fact]
        public void Undo_EnPassant_UndoingPawnMoveRemovesSquare()
        {
            var game = new Chessgame("r6r/pQ3pp1/3kbn1p/2b1N3/3qp3/8/PPPP1PPP/RNB1K2R w KQ - 3 16");
            game.MakeMove("f2", "f4");
            game.UndoLastMove();
            Assert.Equal(0UL, game.EpSquareMask);
        }

        [Fact]
        public void Undo_RemovesNullMove()
        {
            var fen = "2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20";
            var game = new Chessgame(fen);
            game.MakeNullMove();
            var moveCount = game._moveList.Count;
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
            Assert.Equal(moveCount - 1, game._moveList.Count);
        }

        [Fact]
        public void SanDisambiguation_AcceptsUnnecessaryFileDisambiguation()
        {
            var fen = "r1bq1b1r/ppp3pp/2n1k3/3np3/2B5/2N2Q2/PPPP1PPP/R1B1K2R b KQ - 3 8";
            var game = new Chessgame(fen);
            Assert.True(game.TryMakeMove("Ncb4"));
        }

        [Fact]
        public void SanDisambiguation_AcceptsUnnecessaryRankDisambiguation()
        {
            var fen = "8/p2b1pk1/1pq2rpp/4Q3/P3P2P/BPPR1R2/6K1/5r2 b - - 1 1";
            var game = new Chessgame(fen);
            Assert.True(game.TryMakeMove("R1xf3"));
        }
    }
}
