using System.Text.Json.Nodes;
using RV.Chess.Board.Game;
using RV.Chess.Board.Tests.Utils;
using RV.Chess.Shared.Types;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class SanGenerationTests
    {
        [Fact]
        public void Rook_Basic_Correct()
        {
            var rookMoves = new Chessgame("7k/8/8/8/8/8/p7/R1p4K w - - 0 1").GetLegalMoves()
                .Where(m => m.Piece is PieceType.Rook);
            Assert.Single(rookMoves.Where(m => m.San == "Rb1"));
            Assert.Single(rookMoves.Where(m => m.San == "Rxa2"));
            Assert.Single(rookMoves.Where(m => m.San == "Rxc1"));
        }

        [Fact]
        public void Disambiguate_TwoPiecesByRank_Correct()
        {
            var rookMoves = new Chessgame("7k/8/8/8/P7/RP6/pP6/RN5K w - - 0 1").GetLegalMoves()
                .Where(m => m.Piece is PieceType.Rook);
            Assert.Single(rookMoves.Where(m => m.San == "R1xa2"));
            Assert.Single(rookMoves.Where(m => m.San == "R3xa2"));
        }

        [Theory]
        [InlineData(@"castling.json")]
        [InlineData(@"famous.json")]
        [InlineData(@"pawns.json")]
        [InlineData(@"promotions.json")]
        [InlineData(@"standard.json")]
        [InlineData(@"taxing.json")]
        internal void CompletePositions(string jsonPath)
        {
            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), jsonPath);

            if (!File.Exists(path))
            {
                throw new ArgumentException($"Data file does not exist at {path}");
            }

            using var jsonStream = File.OpenRead(jsonPath);

            var obj = JsonNode.Parse(jsonStream)?.AsObject();
            var cases = obj!["testCases"]!.AsArray();

            foreach (var testCase in cases)
            {
                var startingFen = (string)testCase!["start"]?.AsObject()["fen"]!;
                var expectedMoves = testCase!["expected"]?.AsArray().Select(m => (string)m!["move"]!);
                var moves = new Chessgame(startingFen).GetLegalMoves();
                Assert.Equal(expectedMoves!.Count(), moves.Count());
                Assert.True(moves.All(m => expectedMoves!.Where(em => em == m.San).Count() == 1));
            }
        }
    }
}
