using RV.Chess.PGN;
using RV.Chess.Shared.Types;
using Xunit;

namespace RV.PGNParser.Tests;

public class ParserTests
{
    private static PgnGame GetGame(string input)
    {
        using var parser = PgnParser.FromString(input);
        return parser.GetGames().First();
    }

    [Theory]
    [Trait("Category", "Tags")]
    [InlineData("[Event \"?\"] *", "Event", "?")]
    [InlineData("[Black \"The brave and the KID III #4\"] *", "Black", "The brave and the KID III #4")]
    [InlineData("[Black \"With escaped \\\" symbol \"] *", "Black", "With escaped \" symbol ")]
    internal void TagsValid(string input, string key, string value)
    {
        var game = GetGame(input);
        Assert.Empty(game.Errors);
        Assert.True(game.Tags.ContainsKey(key));
        var tag = Assert.Single(game.Tags);
        Assert.Equal(value, tag.Value);
    }

    [Theory]
    [Trait("Category", "Tags")]
    [InlineData("[Event ?\"]\n[White \"?\"]\n *")]
    [InlineData("[Black \"The brave and the KID III #4]\n[White \"?\"]\n *")]
    [InlineData("[\nBlack \"The brave and the KID III #4\"]\n[White \"?\"]\n *")]
    internal void TagsInvalid(string input)
    {
        var game = GetGame(input);
        Assert.NotNull(game);
        Assert.Single(game.Errors, e => e.Type == PgnErrorType.TagError);
    }

    [Theory]
    [Trait("Category", "Comments")]
    [InlineData("; hello \n*", " hello ")]
    [InlineData("\n; hello\n *", " hello")]
    internal void CommentarySingleLine(string input, string expected)
    {
        var game = GetGame(input);
        Assert.Empty(game.Errors);
        var comment = game.Moves.SingleOrDefault(n => n is PgnCommentNode);
        Assert.NotNull(comment);
        Assert.Equal(expected, (comment as PgnCommentNode)?.Comment);
    }

    [Theory]
    [Trait("Category", "Comments")]
    [InlineData("{ hello } *", " hello ")]
    [InlineData(" { hello\nthere } *", " hello\nthere ")]
    internal void CommentaryMultiline(string input, string expected)
    {
        var game = GetGame(input);
        Assert.Empty(game.Errors);
        var comment = game.Moves.SingleOrDefault(n => n is PgnCommentNode);
        Assert.NotNull(comment);
        Assert.Equal(expected, (comment as PgnCommentNode)?.Comment);
    }

    [Theory]
    [Trait("Category", "Castling")]
    [InlineData("0-0", "O-O")]
    [InlineData("0-0-0", "O-O-O")]
    [InlineData("0-0+", "O-O+")]
    [InlineData("0-0#", "O-O#")]
    [InlineData("0-0-0+", "O-O-O+")]
    [InlineData("0-0-0#", "O-O-O#")]
    internal void CastlingWithZeroes(string input, string expected)
    {
        var game = GetGame(input);
        Assert.NotNull(game);
        var move = Assert.Single(game.Moves);
        Assert.True(move is PgnMoveNode mn && mn.San == expected);
    }

    [Theory]
    [Trait("Category", "Castling")]
    [InlineData("0-0- \n[Black \"?\"]")]
    [InlineData("0- \n[Black \"?\"]")]
    [InlineData("0-- \n[Black \"?\"]")]
    [InlineData("0++ \n[Black \"?\"]")]
    internal void CastlingInvalid(string input)
    {
        var game = GetGame(input);
        Assert.NotNull(game);
        Assert.Single(game.Errors);
        Assert.Single(game.Errors, e => e.Type == PgnErrorType.MovetextError);
    }

    [Theory]
    [Trait("Category", "Terminators")]
    [InlineData("1/2-1/2", GameResult.Tie)]
    [InlineData("\n 1-0", GameResult.White)]
    [InlineData("\n\n 0-1", GameResult.Black)]
    [InlineData("*", GameResult.Unknown)]
    internal void GameTerminatorValid(string input, GameResult expectedResult)
    {
        var game = GetGame(input);
        Assert.NotNull(game);
        var terminator = game.Moves[^1];
        Assert.IsType<PgnTerminatorNode>(terminator);
        Assert.Equal(expectedResult, ((PgnTerminatorNode)terminator).Terminator);
    }

    [Theory]
    [Trait("Category", "Terminators")]
    [InlineData("[Test \"Test\"]\n\n")]
    [InlineData("[Test \"Test\"]\n\n1/2-1/3")]
    [InlineData("[Test \"Test\"]\n\n0-0")]
    [InlineData("[Test \"Test\"]\n\n1-1")]
    internal void GameTerminatorInvalid(string input)
    {
        var game = GetGame(input);
        Assert.Single(game.Errors, e => e.Type == PgnErrorType.UnrecoverableError);
    }

    [Theory]
    [Trait("Category", "MoveNumber")]
    [InlineData("1. O-O *", 1)]
    [InlineData("33. O-O *", 33)]
    [InlineData("45 O-O-O 1..b3 *", 45)]
    [InlineData("45. O-O-O 1..b3 *", 45)]
    [InlineData("45... O-O-O 1..b3 *", 45)]
    internal void MoveNumberValid(string input, int moveNo)
    {
        var game = GetGame(input);
        Assert.Equal(moveNo, (game.Moves[0] as PgnMoveNode)?.MoveNumber);
    }

    [Theory]
    [Trait("Category", "MoveNumber")]
    [InlineData("1z. O-O *")]
    [InlineData("3-. O-O *")]
    [InlineData("4x. O-O-O 1..b3 *")]
    internal void MoveNumberInvalid(string input)
    {
        var game = GetGame(input);
        Assert.Contains(game.Errors, e => e.Type == PgnErrorType.MovetextError);
    }

    [Fact]
    [Trait("Category", "MoveSides")]
    internal void MoveSide()
    {
        var game = GetGame("[Event \"?\"]\n\n3. Bb5 Bc5 4. Bc5 *");
        Assert.Equal(Side.White, (game.Moves[0] as PgnMoveNode)!.Side);
        Assert.Equal(Side.Black, (game.Moves[1] as PgnMoveNode)!.Side);
        Assert.Equal(Side.White, (game.Moves[2] as PgnMoveNode)!.Side);
    }

    [Theory]
    [InlineData("[FEN \"r2qr1k1/1b3ppp/p4n2/1ppPn3/5B2/2P5/PPB2PPP/RN1QR1K1 b - - 1 1\"]\r\n15...Qxd5 16.Qxd5 *", Side.Black)]
    [InlineData("[FEN \"r2qr1k1/1b3ppp/p4n2/1ppPn3/5B2/2P5/PPB2PPP/RN1QR1K1 w - - 1 1\"]\r\n15.Qxd5 16.Qxd5 *", Side.White)]
    [Trait("Category", "MoveSides")]
    internal void CorrectMoveSideWithCustomStartingPosition(string position, Side expected)
    {
        var game = GetGame(position);
        Assert.Equal(expected, (game.Moves[0] as PgnMoveNode)!.Side);
    }

    [Fact]
    [Trait("Category", "NAG")]
    internal void NagAnnotation()
    {
        var game = GetGame("$2 *");
        var nag = game.Moves.SingleOrDefault(n => n is PgnAnnotationGlyphNode);
        Assert.NotNull(nag);
        Assert.Equal("2", (nag as PgnAnnotationGlyphNode)?.NAG);
    }

    [Theory]
    [Trait("Category", "SAN")]
    [InlineData("[Event \"?\"]\n1... Nf6 2. Bb2 g6 *", new string[] { "Nf6", "Bb2", "g6" })]
    [InlineData("1. c6+ 2. Qxd3 O-O-O\n 1/2-1/2", new string[] { "c6+", "Qxd3", "O-O-O" })]
    [InlineData("1. c8=Q bxc1=B# 0-1", new string[] { "c8=Q", "bxc1=B#" })]
    internal void San(string input, string[] expected)
    {
        var game = GetGame(input);
        Assert.Equal(expected, game.Moves.Where(n => n is PgnMoveNode).Select(n => (n as PgnMoveNode)?.San));
    }

    [Theory]
    [Trait("Category", "Suffixes")]
    [InlineData("Nf3 *", false, false, "")]
    [InlineData("Nf3+ *", true, false, "")]
    [InlineData("O-O# *", false, true, "")]
    [InlineData("Qf3#-+ *", false, true, "-+")]
    [InlineData("Qf3-+ *", false, false, "-+")]
    [InlineData("Nf3!? *", false, false, "!?")]
    [InlineData("Nf3++- *", true, false, "+-")]
    internal void Suffix(string input, bool isCheck, bool isMate, string expectedSuffix)
    {
        var game = GetGame(input);
        Assert.Equal(isCheck, (game.Moves[0] as PgnMoveNode)!.San.EndsWith("+"));
        Assert.Equal(isMate, (game.Moves[0] as PgnMoveNode)!.San.EndsWith("#"));
        Assert.Equal(expectedSuffix, (game.Moves[0] as PgnMoveNode)!.Annotation);
    }

    [Theory]
    [Trait("Category", "RAV")]
    [InlineData("1.e4 e5 2.Nf3 Nc6 (2...Nf6 3.Nxe5) 6.d4 *", 2)]
    [InlineData("(3. Bb5 Bc5 3... Nf6 4. d3) *", 4)]
    internal void RecursiveAnnotationBasic(string input, int expectedItemsCount)
    {
        var game = GetGame(input);
        var rav = game.Moves.SingleOrDefault(n => n is PgnVariationNode);
        Assert.NotNull(rav);
        Assert.Equal(expectedItemsCount, (rav as PgnVariationNode)?.Moves.Count);
    }

    [Fact]
    [Trait("Category", "RAV")]
    internal void RecursiveAnnotationMultiLevel()
    {
        var game = GetGame("1.e4 e5 2.Nf3 Nc6 (2...Nf6 3.Nxe5 d6 (3...Nxc3 4.Qxf5) 4.d4) 4...d5 *");
        var topLevelRav = game.Moves.SingleOrDefault(n => n is PgnVariationNode);
        Assert.NotNull(topLevelRav);
        Assert.Equal(5, ((PgnVariationNode)topLevelRav!).Moves.Count);
        var secondLevelRav = (topLevelRav as PgnVariationNode)?.Moves.SingleOrDefault(n => n is PgnVariationNode);
        Assert.Equal(2, (secondLevelRav as PgnVariationNode)?.Moves.Count);
    }

    [Fact]
    [Trait("Category", "RAV")]
    internal void RecursiveAnnotationCorrectMoveSide()
    {
        var game = GetGame("1.e4 e5 2.Nf3 Nc6 (2...Nf6 3.Nxe5) *");
        var rav = game.Moves.SingleOrDefault(n => n is PgnVariationNode);
        Assert.Equal(Side.Black, (game.Moves[3] as PgnMoveNode)?.Side);
        Assert.Equal(Side.Black, ((rav as PgnVariationNode)?.Moves[0] as PgnMoveNode)?.Side);
    }

    [Fact]
    [Trait("Category", "RAV")]
    internal void RecursiveAnnotationCorrectMoveSideWhenReturnToMainLine()
    {
        var game = GetGame("1.e4 e5 2.Nf3 Nc6 (2...Nf6 3.Nxe5) 3.Qxf8 *");
        Assert.Equal(Side.White, (game.Moves[5] as PgnMoveNode)?.Side);
    }
}
