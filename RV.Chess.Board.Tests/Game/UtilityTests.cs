using Xunit;

namespace RV.Chess.Board.Tests
{
    public class UtilityTests
    {
        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1", 2185010000)]
        [InlineData("r2qkb1r/1b2nppp/p1npp3/1p6/4PBPP/PNN5/1PP2P2/R2QKB1R b KQkq - 2 10", 3663386480)]
        [InlineData("r4rk1/p1p2q2/2npn1p1/1p2p2p/4Pp1P/2PP1NP1/PPN1QP2/2KR1R2 b - - 3 21", 3672840077)]
        [InlineData("1r2r1k1/2pqn3/3pN1p1/p2P3p/1p1NP2P/2P2PR1/P3Q3/1K1R4 b - - 0 32", 661151459)]
        [InlineData("1r4k1/2pqn3/3pN1p1/p2P3p/2Q1P2P/1N3PR1/P7/Kq6 w - - 0 38", 3284458125)]
        [InlineData("5Qn1/5NRk/q2p4/3p1p1p/4P2P/5P2/P1K5/8 b - - 0 50", 589006652)]
        public void Zobrist_Hash(string fen, uint expected)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var hash = game.ZobristHash();
            Assert.Equal(expected, hash);
        }
    }
}
