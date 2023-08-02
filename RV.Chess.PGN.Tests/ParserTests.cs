using Xunit;
using RV.Chess.PGN;

namespace RV.PGNParser.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("[Event \"?\"] *", "Event", "?")]
        [InlineData("[Black \"The brave and the KID III #4\"] *", "Black", "The brave and the KID III #4")]
        [InlineData("[Black \"With escaped \\\" symbol \"] *", "Black", "With escaped \" symbol ")]
        internal void Tags_Valid(string text, string key, string value)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First();
            Assert.True(game.IsSuccess);
            Assert.Single(game.Value.Tags);
            Assert.True(game.Value.Tags.ContainsKey(key));
            Assert.Equal(value, game.Value.Tags[key]);
        }

        [Theory]
        [InlineData("[Event ?\"] *")]
        [InlineData("[Black \"The brave and the KID III #4] *")]
        [InlineData("[\nBlack \"The brave and the KID III #4\"] *")]
        internal void Tags_Invalid(string text)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().FirstOrDefault();
            Assert.True(game == null || game.IsFailed);
        }

        [Theory]
        [InlineData("1. b3 1/2-1/2", GameResult.Tie)]
        [InlineData("1 b3 \n 1-0", GameResult.White)]
        [InlineData("1 b3 \n\n 0-1", GameResult.Black)]
        [InlineData("99... b3 *", GameResult.Unknown)]
        internal void GameTerminator_Valid(string text, GameResult expectedResult)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First().Value;
            var terminator = game.Moves.Last();
            Assert.IsType<PgnTerminatorNode>(terminator);
            Assert.Equal(expectedResult, ((PgnTerminatorNode)terminator).Terminator);
        }

        [Theory]
        [InlineData("1. b3 1/2z-1/2")]
        [InlineData("1 b3 \n 1-1")]
        [InlineData("1 b3 \n\n 1/2-1\\2")]
        internal void GameTerminator_Invalid(string text)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().FirstOrDefault();
            Assert.Null(game);
        }

        [Theory]
        [InlineData("1. b3 O-O *", "O-O")]
        [InlineData("1. b3 O-O+ *", "O-O+")]
        [InlineData("1. b3 O-O-O# *", "O-O-O#")]
        [InlineData("1 b3 \n O-O-O 1..b3 *", "O-O-O")]
        [InlineData("1 b3 \n\n 0-0 *", "O-O")]
        [InlineData("1 b3 \n\n 0-0-0 *", "O-O-O")]
        internal void Castling_Valid(string text, string expected)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First().Value;
            Assert.Single(game.Moves, c => c is PgnMoveNode mn && mn.San == expected);
        }

        [Fact]
        internal void Castling_InsideRAV()
        {
            using var parser = PgnParser.FromString("(1. b3 O-O-O) *");
            var moves = parser.GetGames().First().Value.Moves;
            Assert.IsType<PgnVariationNode>(moves.First());
            Assert.Equal("1... O-O-O", (moves.First() as PgnVariationNode)?.Moves[1].ToString());
        }

        [Theory]
        [InlineData("1 b3 \n O-0-O 1..b3 *")]
        [InlineData("1 b3 \n\n O+O *")]
        [InlineData("1 b3 \n\n O_-O-O *")]
        internal void Castling_Invalid(string text)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First();
            Assert.True(game.IsFailed);
        }

        [Theory]
        [InlineData("1. O-O *", 1)]
        [InlineData("33. O-O *", 33)]
        [InlineData("45 O-O-O 1..b3 *", 45)]
        [InlineData("45. O-O-O 1..b3 *", 45)]
        [InlineData("45... O-O-O 1..b3 *", 45)]
        internal void MoveNumber_Valid(string text, int moveNo)
        {
            using var parser = PgnParser.FromString(text);
            var move = parser.GetGames().First().Value.Moves.First();
            Assert.Equal(moveNo, (move as PgnMoveNode)?.MoveNumber);
        }

        [Theory]
        [InlineData("1z. O-O *")]
        [InlineData("3-. O-O *")]
        [InlineData("4x. O-O-O 1..b3 *")]
        internal void MoveNumber_Invalid(string text)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First();
            Assert.True(game.IsFailed);
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6 *", new string[] { "Nf6", "Bb2", "g6" })]
        [InlineData("1. c6+ 2. Qxd3 O-O-O\n 1/2-1/2", new string[] { "c6+", "Qxd3", "O-O-O" })]
        [InlineData("1. c8=Q bxc1=B# 0-1", new string[] { "c8=Q", "bxc1=B#" })]
        internal void San_Valid(string text, string[] expected)
        {
            using var parser = PgnParser.FromString(text);
            var moves = parser.GetGames().First().Value.Moves;
            Assert.Equal(expected, moves.Where(n => n is PgnMoveNode).Select(n => (n as PgnMoveNode)?.San));
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1... Nz6 2. Bb2 g6 *")]
        [InlineData("1. c6- 2. Qxd3 O-O-O\n 1/2-1/2")]
        [InlineData("1. c8=P bxc1=B# 0-1")]
        internal void San_Invalid(string text)
        {
            using var parser = PgnParser.FromString(text);
            var game = parser.GetGames().First();
            Assert.True(game.IsFailed);
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1... Nf6! 2. Bb2 g6 *", new string[] { "!", "", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6? 2. Bb2 g6 0-1", new string[] { "?", "", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2!! g6 1-0", new string[] { "", "!!", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2!? g6 1/2-1/2", new string[] { "", "!?", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6?! *", new string[] { "", "", "?!" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6?? *", new string[] { "", "", "??" })]
        internal void San_Suffix(string text, string[] expectedSuffix)
        {
            using var parser = PgnParser.FromString(text);
            var moves = parser.GetGames().First().Value.Moves;
            Assert.Equal(expectedSuffix, moves.Where(n => n is PgnMoveNode).Select(n => (n as PgnMoveNode)?.Annotation));
        }

        [Theory]
        [InlineData("3. Bb5 Bc5  (3... Nf6 4. d3) 4... Bc5 *", 2)]
        [InlineData("(3. Bb5 Bc5 3... Nf6 4. d3) *", 4)]
        internal void RecursiveAnnotation_Basic(string text, int expectedItemsCount)
        {
            using var parser = PgnParser.FromString(text);
            var rav = parser.GetGames().First().Value.Moves.SingleOrDefault(n => n is PgnVariationNode);
            Assert.NotNull(rav);
            Assert.Equal(expectedItemsCount, (rav as PgnVariationNode)?.Moves.Count);
        }

        [Fact]
        internal void RecursiveAnnotation_MultiLevel()
        {
            using var parser = PgnParser.FromString("(3. Bb5 Bc5 (3.Bb5 Bc5 3... Nf6 4.d3) 3... Nf6 4. d3) *");
            var topLevelRav = parser.GetGames().First().Value.Moves.SingleOrDefault(n => n is PgnVariationNode);
            Assert.NotNull(topLevelRav);
            Assert.Equal(5, ((PgnVariationNode)topLevelRav!).Moves.Count);
            var secondLevelRav = (topLevelRav as PgnVariationNode)?.Moves.SingleOrDefault(n => n is PgnVariationNode);
            Assert.Equal(4, (secondLevelRav as PgnVariationNode)?.Moves.Count);
        }

        [Fact]
        internal void RecursiveAnnotation_CorrectMoveColor()
        {
            using var parser = PgnParser.FromString("1.e4 c5 2.d4 2...cxd4 3.c3 dxc3 (3...d3) *");
            var moves = parser.GetGames().First().Value.Moves;
            var rav = moves.SingleOrDefault(n => n is PgnVariationNode);
            Assert.Equal(Side.Black, (moves[5] as PgnMoveNode)?.Side);
            Assert.Equal(Side.Black, ((rav as PgnVariationNode)?.Moves.First() as PgnMoveNode)?.Side);
        }

        [Fact]
        internal void RecursiveAnnotation_CorrectMoveColor_WhenReturnToMainLine()
        {
            using var parser = PgnParser.FromString("1.e4 c5 (1...e5 2.Nc3) 2. Nc3 *");
            var moves = parser.GetGames().First().Value.Moves;
            Assert.Equal(Side.White, (moves[3] as PgnMoveNode)?.Side);
        }

        [Theory]
        [InlineData("1. b3 { hello } *", "hello")]
        [InlineData("1 b3 \n; hello \n*", "hello")]
        [InlineData("1 b3 \n; hello\n *", "hello")]
        [InlineData("1. b3 { hello there } ...c6 *", "hello there")]
        [InlineData("1. b3 { hello, \\} there } ...c6 *", "hello, } there")]
        internal void Commentary(string text, string expectedText)
        {
            using var parser = PgnParser.FromString(text);
            var comment = parser.GetGames().First().Value.Moves.SingleOrDefault(n => n is PgnCommentNode);
            Assert.NotNull(comment);
            Assert.Equal(expectedText, (comment as PgnCommentNode)?.Comment);
        }

        [Theory]
        [InlineData("21. Re1 Rd8 $2 22.Rxe6 *", "2")]
        internal void NagAnnotation(string text, string expectedText)
        {
            using var parser = PgnParser.FromString(text);
            var nag = parser.GetGames().First().Value.Moves.SingleOrDefault(n => n is PgnAnnotationGlyphNode);
            Assert.NotNull(nag);
            Assert.Equal(expectedText, (nag as PgnAnnotationGlyphNode)?.NAG);
        }
    }
}
