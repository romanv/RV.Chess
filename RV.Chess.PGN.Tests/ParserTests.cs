using Xunit;
using RV.Chess.PGN;

namespace RV.PGNParser.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6", new string[] { "Nf6", "Bb2", "g6" })]
        [InlineData("1. c6+ 2. Qxd3 O-O-O\n", new string[] { "c6+", "Qxd3", "O-O-O" })]
        internal void ValidSAN(string text, string[] expected)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();
            Assert.Equal(expected, nodes.Where(n => n is SANSyntax).Select(n => n.Text));
        }

        [Theory]
        [InlineData("[FEN \"1q2r1k1/6p1/p6p/2P1P2P/2Pr4/1Q4N1/P5P1/1K2R3 b - - 0 31\"]\r\n\r\n31...Rd3 \r\n * ")]
        internal void ValidStartingSide(string text)
        {
            var game = PgnGame.FromString(text);
            Assert.Equal(Side.Black, (game.Moves.First() as PgnMoveNode)?.Side);
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1... Nf6! 2. Bb2 g6", new string[] { "!", "", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6? 2. Bb2 g6", new string[] { "?", "", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2!! g6", new string[] { "", "!!", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2!? g6", new string[] { "", "!?", "" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6?!", new string[] { "", "", "?!" })]
        [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6??", new string[] { "", "", "??" })]
        internal void ValidSANWithAnnotation(string text, string[] expectedSuffix)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();
            Assert.Equal(expectedSuffix, nodes.Where(n => n is SANSyntax).Select(n => ((SANSyntax)n).Annotation));
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1... Zf6 2. Bb2 g6")]
        internal void InvalidSAN(string text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            Assert.Throws<InvalidPgnException>(() => parser.Parse());
        }

        [Theory]
        [InlineData("[Event \"?\"]", "Event", "?")]
        [InlineData("[Black \"The brave and the KID III #4\"]", "Black", "The brave and the KID III #4")]
        internal void ValidTags(string text, string expectedKey, string expectedValue)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            Assert.IsType<TagPairSyntax>(nodes.First());
            var node = nodes.First() as TagPairSyntax;
            Assert.Equal(expectedKey, node?.Key);
            Assert.Equal(expectedValue, node?.Value);
        }

        [Theory]
        [InlineData("[Event \"?\"]\n1. b3", 1, "1.")]
        [InlineData("[Event \"?\"]\n1 b3", 1, "1")]
        [InlineData("[Event \"?\"]\n99... b3", 99, "99...")]
        internal void ValidMoveNumber(string text, int expectedNumber, string expectedText)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            var moveNumber = nodes.SingleOrDefault(n => n is MoveNumberSyntax);
            Assert.NotNull(moveNumber);
            var node = moveNumber as MoveNumberSyntax;
            Assert.Equal(expectedText, node?.Text);
            Assert.Equal(expectedNumber, node?.Number);
        }

        [Theory]
        [InlineData("1. b3 1/2-1/2", GameResult.Tie, "1/2-1/2")]
        [InlineData("1 b3 \n 1-0", GameResult.White, "1-0")]
        [InlineData("1 b3 \n\n 0-1", GameResult.Black, "0-1")]
        [InlineData("99... b3 *", GameResult.Unknown, "*")]
        internal void ValidGameTerminator(string text, GameResult expectedResult, string expectedText)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            var terminator = nodes.Last();
            Assert.IsType<GameTerminatorSyntax>(terminator);
            Assert.Equal(expectedResult, ((GameTerminatorSyntax)terminator).Result);
            Assert.Equal(expectedText, terminator.Text);
        }

        [Theory]
        [InlineData("1. b3 { hello }", CommentSyntax.CommentType.Brace, " hello ")]
        [InlineData("1 b3 \n; hello", CommentSyntax.CommentType.RestOfLine, " hello")]
        [InlineData("1 b3 \n; hello\n", CommentSyntax.CommentType.RestOfLine, " hello")]
        [InlineData("1. b3 { hello there } ...c6", CommentSyntax.CommentType.Brace, " hello there ")]
        [InlineData("1. b3 { hello, there } ...c6", CommentSyntax.CommentType.Brace, " hello, there ")]
        internal void Commentary(string text, CommentSyntax.CommentType expectedType, string expectedText)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            var comment = nodes.FirstOrDefault(n => n is CommentSyntax);
            Assert.NotNull(comment);
            Assert.Equal(expectedType, (comment as CommentSyntax)?.CommentaryType);
            Assert.Equal(expectedText, (comment as CommentSyntax)?.Value);
        }

        [Theory]
        [InlineData("3. Bb5 Bc5  (3... Nf6 4. d3) 4... Bc5", 4)]
        [InlineData("(3. Bb5 Bc5 3... Nf6 4. d3)", 7)]
        internal void RecursiveAnnotation(string text, int expectedItemsCount)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            var rav = nodes.SingleOrDefault(n => n is RAVSyntax);

            Assert.NotNull(rav);
            Assert.Equal(expectedItemsCount, ((RAVSyntax)rav!).Nodes.Length);
        }

        [Fact]
        internal void MultilevelRecursiveAnnotation()
        {
            var text = "(3. Bb5 Bc5 (3.Bb5 Bc5 3... Nf6 4.d3) 3... Nf6 4. d3)";
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(text, tokens);
            var nodes = parser.Parse();

            var topLevelRAV = nodes.SingleOrDefault(n => n is RAVSyntax);
            Assert.NotNull(topLevelRAV);
            var secondLevelRAV = ((RAVSyntax)topLevelRAV!).Nodes.SingleOrDefault(n => n is RAVSyntax);
            Assert.Equal(7, ((RAVSyntax)secondLevelRAV!).Nodes.Length);
        }
    }
}
