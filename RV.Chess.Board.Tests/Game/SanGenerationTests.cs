using System.Text.Json.Nodes;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class SanGenerationTests
    {
        [Fact]
        public void Rook_Basic_Correct()
        {
            var game = new Chessgame();
            game.SetFen("7k/8/8/8/8/8/p7/R1p4K w - - 0 1");
            var rookMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Rook);

            Assert.Single(rookMoves.Where(m => m.San == "Rb1"));
            Assert.Single(rookMoves.Where(m => m.San == "Rxa2"));
        }

        [Fact]
        public void Disambiguate_TwoPiecesByRank_Correct()
        {
            var game = new Chessgame();
            game.SetFen("7k/8/8/8/P7/RP6/pP6/RN5K w - - 0 1");
            var rookMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Rook);

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
                var game = new Chessgame(startingFen);
                var moves = game.GenerateMoves();

                Assert.Equal(expectedMoves!.Count(), moves.Length);
                Assert.True(moves.All(m => expectedMoves!.Where(em => em == m.San).Count() == 1));
            }
        }
    }
}
