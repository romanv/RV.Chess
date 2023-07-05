using Xunit;

namespace RV.Chess.Board.Tests
{
    public class MovingTests
    {
        [Theory]
        [InlineData(
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
            "e2", "e4",
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
        )]
        [InlineData(
            "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1",
            "d7", "d5",
            "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2"
        )]
        [InlineData(
            "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2",
            "f1", "b5",
            "rnbqkbnr/ppp1pppp/8/1B1p4/4P3/8/PPPP1PPP/RNBQK1NR b KQkq - 1 2"
        )]
        [InlineData(
            "r3k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/2KR3R w kq - 3 12",
            "c1", "b1",
            "r3k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/1K1R3R b kq - 4 12"
        )]
        public void Moves_By_Coordinates(string initialFen, string from, string to, string resultFen)
        {
            var game = new Chessgame();
            game.SetFen(initialFen);
            game.MakeMove(from, to);
            Assert.Equal(resultFen, game.Fen);
        }

        [Theory]
        [InlineData(
            "r2qkbnr/pp1npppp/3p4/2p5/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 0 5",
            "O-O",
            "r2qkbnr/pp1npppp/3p4/2p5/4P3/5N2/PPPP1PPP/RNBQ1RK1 b kq - 1 5"
        )]
        [InlineData(
            "r3kbnr/pp1npppp/3p4/q1p5/4P3/1PN2N2/P1PP1PPP/R1BQK2R b KQkq - 0 6",
            "O-O-O",
            "2kr1bnr/pp1npppp/3p4/q1p5/4P3/1PN2N2/P1PP1PPP/R1BQK2R w KQ - 1 7"
        )]
        public void Moves_Castling(string initialFen, string san, string resultFen)
        {
            var game = new Chessgame();
            game.SetFen(initialFen);
            game.MakeMove(san);
            Assert.Equal(resultFen, game.Fen);
        }

        [Theory]
        [InlineData("r1b1k2r/ppppqNpp/2n2n2/4p3/2B1P3/8/PPPP1bPP/RNBQ1K1R w kq - 2 7", "Nxh8",
            "r1b1k2N/ppppq1pp/2n2n2/4p3/2B1P3/8/PPPP1bPP/RNBQ1K1R b q - 0 7")]
        [InlineData("rn1qk2r/pp3ppp/2p1p1b1/4N3/2BP2PP/2B2P2/PPn1Q3/R4K1R b kq - 1 14", "Nxa1",
            "rn1qk2r/pp3ppp/2p1p1b1/4N3/2BP2PP/2B2P2/PP2Q3/n4K1R w kq - 0 15")]
        public void Captures_RookCaptureDisablesCastling(string startingFen, string move, string expectedFen)
        {
            var game = new Chessgame();
            game.SetFen(startingFen);
            game.MakeMove(move);
            Assert.Equal(expectedFen, game.Fen);
        }

        [Theory]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "Kf1",
            (CastlingDirection.BlackKingside | CastlingDirection.BlackQueenside))]
        [InlineData("r1bqk2r/pp2npbp/2npp1p1/2p5/4PP2/1PPP1NP1/P5BP/RNBQK2R b KQkq - 0 9", "Kd7",
            (CastlingDirection.WhiteKingside | CastlingDirection.WhiteQueenside))]
        public void Moving_King_Disables_Castling(string initialFen, string san, CastlingDirection expectedRights)
        {
            var game = new Chessgame();
            game.SetFen(initialFen);
            game.MakeMove(san);
            Assert.Equal(game.CastlingRights.Rights, expectedRights);
        }

        [Theory]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d8=Q+",
            "r1bQ1knr/pp2p2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R b KQ - 0 13")]
        public void Moving_Promotions(string initialFen, string san, string expectedFen)
        {
            var game = new Chessgame();
            game.SetFen(initialFen);
            game.MakeMove(san);
            Assert.Equal(expectedFen, game.Fen);
        }

        [Theory]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R w KQkq - 0 10", "Ra2", CastlingDirection.WhiteQueenside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R w KQkq - 0 10", "Rg1", CastlingDirection.WhiteKingside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R b KQkq - 0 10", "Rg8", CastlingDirection.BlackKingside)]
        [InlineData("r1bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/RNBQK2R b KQkq - 0 10", "Rb8", CastlingDirection.BlackQueenside)]
        public void Moving_Rook_Disables_SideCastling(string initialFen, string san, CastlingDirection removedRight)
        {
            var game = new Chessgame();
            game.SetFen(initialFen);
            Assert.True(game.CastlingRights.Rights.HasFlag(removedRight));
            game.MakeMove(san);
            Assert.False(game.CastlingRights.Rights.HasFlag(removedRight));
        }

        [Theory]
        [InlineData("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20", "dxe6",
            "2rq1rk1/pp3p1p/3pP2Q/5p2/3R4/1P3P2/P1P2nPP/1K5R b - - 0 20")]
        public void Moves_Captures(string fen, string san, string resultFen)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            game.MakeMove(san);
            Assert.Equal(resultFen, game.Fen);
        }

        [Theory]
        [InlineData("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20", "dxe6")]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "d8=Q+")]
        [InlineData("r1b2knr/pp1Pp2p/2p4b/3p3q/3P4/2N2N2/PPP2PPP/R3KB1R w KQ - 0 13", "dxc8=Q+")]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "Nd4")]
        public void Moves_Undo(string fen, string san)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            game.MakeMove(san);
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
        }

        [Theory]
        [InlineData("2bqk2r/p3npbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/1NBQK2R w Kk - 0 10", "O-O",
                CastlingDirection.WhiteKingside | CastlingDirection.BlackKingside)]
        [InlineData("r3k3/p1bqnpbp/2npp1p1/1pp5/4PP2/PPPP1NP1/6BP/1NBQK2R b Kq - 0 10", "O-O-O",
                CastlingDirection.WhiteKingside | CastlingDirection.BlackQueenside)]
        public void Moves_Undo_RestoresCastlingRights(string fen, string san, CastlingDirection castling)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            game.MakeMove(san);
            game.UndoLastMove();
            Assert.Equal(fen, game.Fen);
            Assert.Equal(castling, game.CastlingRights.Rights);
        }

        [Fact]
        public void Moves_Undo_RestoresEnPassantSquare()
        {
            var game = new Chessgame();
            game.SetFen("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20");
            Assert.Equal(44, game.EnPassantSquareIdx);
            game.MakeMove("dxe6");
            Assert.Equal(-1, game.EnPassantSquareIdx);
            game.UndoLastMove();
            Assert.Equal(44, game.EnPassantSquareIdx);
        }
    }
}
