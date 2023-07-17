using RV.Chess.Board.Game;
using RV.Chess.Board.Types;
using RV.Chess.Board.Utils;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class BoardTests
    {
        [Fact]
        public void CorrectSquareConversion()
        {
            Assert.Equal(0, Coordinates.SquareToIdx("a1"));
            Assert.Equal(63, Coordinates.SquareToIdx("h8"));
            Assert.Equal(-1, Coordinates.SquareToIdx("a0"));
        }

        [Fact]
        public void CorrectOccupied()
        {
            var board = new BoardState();
            Assert.True(board.IsOccupied("a1"));
            Assert.True(board.IsOccupied("a7"));
            Assert.False(board.IsOccupied("h3"));
            Assert.False(board.IsOccupied("c4"));
        }

        [Fact]
        public void CorrectPieceAdded()
        {
            var board = new BoardState();
            Assert.False(board.IsOccupied("a3"));
            board.AddPiece(PieceType.Bishop, Side.Black, Coordinates.SquareToIdx("a3"));
            Assert.True(board.IsOccupied("a3"));
            var type = board.GetPieceTypeAt(Coordinates.SquareToIdx("a3"));
            var side = board.GetPieceSideAt(Coordinates.SquareToIdx("a3"));
            Assert.Equal(PieceType.Bishop, type);
            Assert.Equal(Side.Black, side);
        }

        [Fact]
        public void CorrectPieceRemoved()
        {
            var board = new BoardState();
            Assert.True(board.IsOccupied("a1"));
            board.RemovePieceAt("a1");
            Assert.False(board.IsOccupied("a1"));
        }

        [Fact]
        public void CorrectKingsSquare()
        {
            var board = new BoardState();
            Assert.Equal(4, board.GetOwnKingSquare(Side.White));
            Assert.Equal(60, board.GetOwnKingSquare(Side.Black));
        }
    }
}
