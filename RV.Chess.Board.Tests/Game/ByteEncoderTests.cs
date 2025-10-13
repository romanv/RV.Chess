using RV.Chess.Board.Game;
using RV.Chess.Board.Utils;
using Xunit;

namespace RV.Chess.Board.Tests;

public class ByteEncoderTests
{
    [Theory]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 1", 9)]
    [InlineData("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1", 10)]
    [InlineData("4k3/4p3/8/8/8/8/4P3/4K3 w - - 0 1", 10)]
    [InlineData("4k3/8/8/4p3/8/8/4P3/4K3 w - e6 0 1", 11)]
    [InlineData("4k3/8/8/4p3/8/8/4PN2/4K3 w - e6 0 1", 11)]
    public void CorrectWrittenBytesCount(string fen, int byteCount)
    {
        var game = new Chessgame(fen);
        var bytes = new byte[192];
        var written = ByteEncoder.WriteBytes(game, bytes);
        Assert.Equal(byteCount, written);
    }

    [Theory]
    [InlineData("4k3/8/8/8/8/8/8/4K3 w - - 0 1")]
    [InlineData("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1")]
    [InlineData("4k3/4p3/8/8/8/8/4P3/4K3 w - - 0 1")]
    [InlineData("4k3/8/8/4p3/8/8/4P3/4K3 w - e6 0 1")]
    [InlineData("4k3/8/8/4p3/8/8/4PN2/4K3 w - e6 0 1")]
    [InlineData("r3k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/2KR3R w kq - 3 12")]
    [InlineData("2rq1rk1/pp3p1p/3p3Q/3Ppp2/3R4/1P3P2/P1P2nPP/1K5R w - e6 0 20")]
    [InlineData("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2")]
    public void EncodedFenMatchesOriginal(string fen)
    {
        var game = new Chessgame(fen);
        var bytes = new byte[192];
        ByteEncoder.WriteBytes(game, bytes);
        var decodedGame = new Chessgame();
        ByteEncoder.ReadBytes(bytes.AsSpan(), decodedGame);

        // Half move count is not stored in the byte-encoded position, therefore movecount FEN part must be ignored
        var expectedParts = fen.Split(' ');
        var actualParts = decodedGame.Fen.Split(' ');
        Assert.Equal(expectedParts[0], actualParts[0]); // Position
        Assert.Equal(expectedParts[1], actualParts[1]); // Side to move
        Assert.Equal(expectedParts[2], actualParts[2]); // Castling
        Assert.Equal(expectedParts[3], actualParts[3]); // En passant square
    }
}
