using Xunit;

namespace RV.Chess.Board.Tests
{
    public class BoardTests
    {
        [Fact]
        public void CorrectSquareConversion()
        {
            Assert.Equal(0, Chessboard.SquareToIdx("a1"));
            Assert.Equal(63, Chessboard.SquareToIdx("h8"));
            Assert.Equal(-1, Chessboard.SquareToIdx("a0"));
        }

        [Fact]
        public void CorrectOccupied()
        {
            var board = new Chessboard();
            Assert.True(board.IsOccupied("a1"));
            Assert.True(board.IsOccupied("a7"));
            Assert.False(board.IsOccupied("h3"));
            Assert.False(board.IsOccupied("c4"));
            Assert.Throws<InvalidDataException>(() => board.IsOccupied("a0"));
        }

        [Fact]
        public void CorrectPieceAdded()
        {
            var board = new Chessboard();
            Assert.False(board.IsOccupied("a3"));
            board.AddPiece(PieceType.Bishop, Side.Black, "a3");
            Assert.True(board.IsOccupied("a3"));
            var (type, color) = board.GetPieceAt("a3");
            Assert.Equal(PieceType.Bishop, type);
            Assert.Equal(Side.Black, color);
        }

        [Fact]
        public void CorrectPieceRemoved()
        {
            var board = new Chessboard();
            Assert.True(board.IsOccupied("a1"));
            board.RemovePieceAt("a1");
            Assert.False(board.IsOccupied("a1"));
        }

        [Fact]
        public void CorrectKingsSquare()
        {
            var board = new Chessboard();
            Assert.Equal(4, board.GetKingSquare(Side.White));
            Assert.Equal(60, board.GetKingSquare(Side.Black));
        }
    }
}
