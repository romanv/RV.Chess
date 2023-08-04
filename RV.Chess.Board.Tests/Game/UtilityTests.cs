using RV.Chess.Board.Game;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class UtilityTests
    {
        [Theory]
        [InlineData("r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", 2976971930)]
        [InlineData("r1bqk2r/pppp1ppp/2n2n2/1Bb1p3/4P3/2P2N2/PP1P1PPP/RNBQ1RK1 b kq - 0 5", 4289297830)]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R w KQkq - 0 1", 3598253172)]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R b KQkq - 0 1", 783211230)]
        public void Zobrist_HashesAreConsistent(string fen, uint expected)
        {
            var game = new Chessgame(fen);
            Assert.Equal(expected, game.Hash);
        }

        [Theory]
        [InlineData("r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", "O-O")]
        [InlineData("r1bqk2r/pppp1ppp/2n2n2/1Bb1p3/4P3/2P2N2/PP1P1PPP/RNBQ1RK1 b kq - 0 5", "O-O")]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R w KQkq - 0 1", "O-O-O")]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R b KQkq - 0 1", "O-O-O")]
        public void Zobrist_UndoingCastlingRestoresHash(string fen, string move)
        {
            var game = new Chessgame(fen);
            var hash = game.Hash;
            game.TryMakeMove(move);
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Theory]
        [InlineData("K7/8/8/8/4pP2/8/8/7k b - f3 0 1", "exf3")]
        [InlineData("rnbqkb1r/ppp1pppp/5n2/3pP3/8/5N2/PPPP1PPP/RNBQKB1R w KQkq d6 0 2", "exd6")]
        public void Zobrist_UndoingEnPassantRestoresHash(string fen, string move)
        {
            var game = new Chessgame(fen);
            var hash = game.Hash;
            game.TryMakeMove(move);
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Theory]
        [InlineData("rnbq1b1r/pppPkppp/5n2/4p3/8/5N2/PPPP1PPP/RNBQKB1R w KQ - 1 4", "dxc8=Q")]
        [InlineData("rnQq1b1r/ppp1kppp/5n2/8/2BP4/5N2/PPPKpPPP/RNBQ3R b - - 1 7", "e1=Q+")]
        public void Zobrist_UndoingPromotionRestoresHash(string fen, string move)
        {
            var game = new Chessgame(fen);
            var hash = game.Hash;
            game.TryMakeMove(move);
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Fact]
        public void Zobrist_SettingFenGivesSameHashAsMoving()
        {
            var game1 = new Chessgame();
            game1.TryMakeMove(12, 28); // e4
            game1.TryMakeMove(52, 36); // e5
            var game2 = new Chessgame("rnbqkbnr/pppp1ppp/8/4p3/4P3/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 2");
            Assert.Equal(game2.Fen, game1.Fen);
            Assert.Equal(game2.Hash, game1.Hash);
        }
    }
}
