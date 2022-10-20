using Xunit;

namespace RV.Chess.Board.Tests
{
    public class FenTests
    {
        [Fact]
        public void Fen_ParsesCorrect()
        {
            var g = new Chessgame();
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 1");
            Assert.Equal(PieceType.Pawn, g.Board.GetPieceTypeAt("h8"));
            Assert.Equal(Side.Black, g.Board.GetPieceSideAt("h8"));
            Assert.Equal(PieceType.Queen, g.Board.GetPieceTypeAt("b7"));
            Assert.Equal(Side.White, g.Board.GetPieceSideAt("b7"));
            Assert.Equal(PieceType.King, g.Board.GetPieceTypeAt("e5"));
            Assert.Equal(Side.Black, g.Board.GetPieceSideAt("e5"));
            Assert.Equal(PieceType.King, g.Board.GetPieceTypeAt("d4"));
            Assert.Equal(Side.White, g.Board.GetPieceSideAt("d4"));
            Assert.Equal(PieceType.Queen, g.Board.GetPieceTypeAt("g2"));
            Assert.Equal(Side.Black, g.Board.GetPieceSideAt("g2"));
            Assert.Equal(PieceType.Pawn, g.Board.GetPieceTypeAt("a1"));
            Assert.Equal(Side.White, g.Board.GetPieceSideAt("a1"));
        }

        [Fact]
        public void Fen_ThrowsForIncorrectPiecePart()
        {
            var g = new Chessgame();
            Assert.Throws<InvalidDataException>(() => g.SetFen("7z/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("rnbqkbnr/pppppppp/8/7/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("rnbqkbnr/2ppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("rnbqkbnr/2ppppppp/8/8/8//PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("rnbqkbnr/pppppppp/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"));
        }

        [Fact]
        public void Fen_ParsesSideToMove()
        {
            var g = new Chessgame();
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b - - 0 1");
            Assert.Equal(Side.Black, g.SideToMove);
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 1");
            Assert.Equal(Side.White, g.SideToMove);

            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 z - - 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w- - 0 1"));
        }

        [Fact]
        public void Fen_ParsesCastlingRights()
        {
            var g = new Chessgame();

            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq - 0 1");
            Assert.True(g.CastlingRights.Rights.HasFlag(CastlingDirection.WhiteKingside | CastlingDirection.WhiteQueenside
                | CastlingDirection.BlackKingside | CastlingDirection.BlackQueenside));

            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQ - 0 1");
            Assert.False(g.CastlingRights.Rights.HasFlag(CastlingDirection.BlackKingside | CastlingDirection.BlackQueenside));

            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b kq - 0 1");
            Assert.False(g.CastlingRights.Rights.HasFlag(CastlingDirection.WhiteKingside | CastlingDirection.WhiteQueenside));

            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b K - 0 1");
            Assert.True(g.CastlingRights.Rights.HasFlag(CastlingDirection.WhiteKingside));
            Assert.False(g.CastlingRights.Rights.HasFlag(CastlingDirection.WhiteQueenside));

            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b - - 0 1");
            Assert.False(g.CastlingRights.Rights.HasFlag(CastlingDirection.WhiteKingside));
        }

        [Fact]
        public void Fen_ThrowsIncorrectCastlingRights()
        {
            var g = new Chessgame();

            Assert.Throws<InvalidDataException> (() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b 0 - 0 1"));
            Assert.Throws<InvalidDataException> (() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkqKQ - 0 1"));
        }

        [Fact]
        public void Fen_EnPassant()
        {
            var g = new Chessgame();
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq c3 0 1");
            Assert.Equal("c3", Chessboard.IdxToSquare(g.EnPassantSquareIdx));
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq - 0 1");
            Assert.Equal(-1, g.EnPassantSquareIdx);

            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq -- 0 1"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 b KQkq c9 0 1"));
        }

        [Fact]
        public void Fen_MoveNumber()
        {
            var g = new Chessgame();
            g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 38");
            Assert.Equal(38, g.CurrentMoveNumber);

            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 0"));
            Assert.Throws<InvalidDataException>(() => g.SetFen("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 z"));
        }

        [Theory]
        [InlineData("7p/1Q6/8/4k3/3K4/8/6q1/P7 w - - 0 38")]
        [InlineData("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2")]
        [InlineData("R6R/3Q4/1Q4Q1/4Q3/2Q4Q/Q4Q2/pp1Q4/kBNN1KB1 w - - 0 1")]
        [InlineData("r3r1k1/pp3pbp/1qp1b1p1/2B5/2BP4/Q1n2N2/P4PPP/2R2K1R b - - 5 18")]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R b KQkq - 0 8")]
        public void Fen_GeneratingCorrectly(string fen)
        {
            var g = new Chessgame();
            g.SetFen(fen);
            Assert.Equal(fen, g.Fen);
        }
    }
}
