using RV.Chess.PGN;
using Xunit;

namespace RV.PGNParser.Tests
{
    public class LexerTests
    {
        [Theory]
        [InlineData("123", TokenKind.Integer, 123)]
        [InlineData("0", TokenKind.Integer, 0)]
        [InlineData("[", TokenKind.SquareBracketOpen, null)]
        [InlineData("]", TokenKind.SquareBracketClose, null)]
        [InlineData("(", TokenKind.ParenthesisOpen, null)]
        [InlineData(")", TokenKind.ParenthesisClose, null)]
        [InlineData("<", TokenKind.AngleBracketOpen, null)]
        [InlineData(">", TokenKind.AngleBracketClose, null)]
        [InlineData("{", TokenKind.CurlyBracketOpen, null)]
        [InlineData("}", TokenKind.CurlyBracketClose, null)]
        [InlineData(".", TokenKind.Period, null)]
        [InlineData("*", TokenKind.Asterisk, null)]
        [InlineData(";", TokenKind.Semicolon, null)]
        [InlineData(" ", TokenKind.Whitespace, null)]
        [InlineData("\t", TokenKind.Whitespace, null)]
        internal void BasicTokenTypesValues(string text, TokenKind expectedKind, object? expectedValue)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(expectedKind, tokens.First().Kind);
            Assert.Equal(expectedValue, tokens.First().Value);
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\" hello there \"", " hello there ")]
        [InlineData("\" abc \"", " abc ")]
        [InlineData("\" \\\" abc \"", " \" abc ")]
        [InlineData("\" \\\\ abc \"", " \\ abc ")]
        internal void ValidStrings(string text, object? expectedValue)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(expectedValue, tokens.First().Value);
        }

        [Theory]
        [InlineData("\" \\ abc \"")]
        [InlineData("\" \" abc \"")]
        internal void InvalidStrings(string text)
        {
            var lexer = new Lexer(text);
            Assert.Throws<InvalidPgnException>(() => lexer.ParseTokens());
        }

        [Theory]
        [InlineData("$1", "$1")]
        [InlineData("$0987654312", "$0987654312")]
        internal void ValidNumericAnnotations(string text, object? expectedValue)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(expectedValue, tokens.First().Value);
        }

        [Theory]
        [InlineData("$")]
        [InlineData("$abc")]
        internal void InvalidNumericAnnotations(string text)
        {
            var lexer = new Lexer(text);
            Assert.Throws<InvalidPgnException>(() => lexer.ParseTokens());
        }

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("abc123 ", "abc123")]
        internal void ValidSymbols(string text, string expectedValue)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            Assert.Equal(expectedValue, tokens.First().Value);
            Assert.Equal(TokenKind.Symbol, tokens.First().Kind);
        }

        [Theory]
        [InlineData("1. b3 1/2-1/2", "1/2-1/2")]
        [InlineData("1. b3 1-0", "1-0")]
        [InlineData("1. b3 0-1", "0-1")]
        [InlineData("1. b3 0-1 1. c3", "0-1")]
        [InlineData("1. b3 1/2-1/2 1. c3", "1/2-1/2")]
        internal void ValidGameTerminators(string text, string expectedText)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            var terminator = tokens.FirstOrDefault(t => t.Kind == TokenKind.GameTerminator);
            Assert.NotNull(terminator);
            Assert.Equal(expectedText, terminator?.Value);
        }

        [Theory]
        [InlineData("1. b3 1/3-1/2")]
        [InlineData("1. b3 1--")]
        internal void InvalidGameTerminators(string text)
        {
            var lexer = new Lexer(text);
            Assert.Throws<InvalidPgnException>(() => lexer.ParseTokens());
        }

        [Theory]
        [InlineData("1\n2", 1)]
        [InlineData("1\r\n2", 1)]
        [InlineData("1 2", 0)]
        [InlineData("1 \r\n\n 2", 2)]
        [InlineData("1 \n \n \n 2", 3)]
        internal void ValidNewLines(string text, int expectedNewLineCount)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.ParseTokens();

            Assert.Equal(expectedNewLineCount, tokens.Count(t => t.Kind is TokenKind.NewLine));
        }
    }
}
