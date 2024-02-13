using RV.Chess.Board.Game;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class UtilityTests
    {
        [Fact]
        public void Zobrist_DoublePawnMove_Undo()
        {
            var game = new Chessgame("rnbqkbnr/1ppppppp/p7/8/8/PP6/2PPPPPP/RNBQKBNR b KQkq - 0 3");
            var hash = game.Hash;
            game.TryMakeMove("b5");
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Fact]
        public void Zobrist_SinglePawnMove_Undo()
        {
            var game = new Chessgame("rnbqkbnr/1ppppppp/p7/8/8/PP6/2PPPPPP/RNBQKBNR b KQkq - 0 3");
            var hash = game.Hash;
            game.TryMakeMove("a5");
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Theory]
        [InlineData("r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", 2976971930)]
        [InlineData("r1bqk2r/pppp1ppp/2n2n2/1Bb1p3/4P3/2P2N2/PP1P1PPP/RNBQ1RK1 b kq - 0 5", 4289297830)]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R w KQkq - 0 1", 3598253172)]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R b KQkq - 0 1", 783211230)]
        public void Zobrist_HashesAreStable(string fen, uint expected)
        {
            var game = new Chessgame(fen);
            Assert.Equal(expected, game.Hash);
        }

        [Theory]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN2Q1p/PPPBBPPP/1R2K2R w Kkq - 1 2", "O-O",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN2Q1p/PPPBBPPP/1R3RK1 b kq - 2 2")]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/P1N2Q1p/1PPBBPPP/R3K2R b KQkq - 0 1", "O-O",
            "r4rk1/p1ppqpb1/bn2pnp1/3PN3/1p2P3/P1N2Q1p/1PPBBPPP/R3K2R w KQ - 1 2")]
        public void Zobrist_Castling(string fenStart, string move, string fenTargetPos)
        {
            var gameSource = new Chessgame(fenStart);
            gameSource.TryMakeMove(move);
            var gameTarget = new Chessgame(fenTargetPos);
            Assert.Equal(gameTarget.Hash, gameSource.Hash);
        }

        [Theory]
        [InlineData("r1bqkb1r/pppp1ppp/2n2n2/1B2p3/4P3/5N2/PPPP1PPP/RNBQK2R w KQkq - 4 4", "O-O")]
        [InlineData("r1bqk2r/pppp1ppp/2n2n2/1Bb1p3/4P3/2P2N2/PP1P1PPP/RNBQ1RK1 b kq - 0 5", "O-O")]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R w KQkq - 0 1", "O-O-O")]
        [InlineData("r3kbnr/pppppppp/8/3BN3/3Bn3/8/PPPPPPPP/R3KB1R b KQkq - 0 1", "O-O-O")]
        [InlineData("1r2k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/P1N2Q1p/1PPBBPPP/R3K2R w KQk - 0 2", "O-O")]
        public void Zobrist_CastlingUndo(string fen, string move)
        {
            var game = new Chessgame(fen);
            var hash = game.Hash;
            game.TryMakeMove(move);
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Theory]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN4Q/PPPBBPPP/R3K2R w KQkq - 0 2", "Qxh8+",
            "r3k2Q/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN5/PPPBBPPP/R3K2R b KQq - 0 2")]
        [InlineData("rnbq1k1r/pp1Pbppp/8/2p5/2B5/P7/1PP1NnPP/RNBQK2R w KQ - 0 9", "dxc8=Q",
            "rnQq1k1r/pp2bppp/8/2p5/2B5/P7/1PP1NnPP/RNBQK2R b KQ - 0 9")]
        [InlineData("rnbq1k1r/pp1Pbppp/8/2p5/2B5/P7/1PP1NnPP/RNBQK2R w KQ - 0 9", "Kxf2",
            "rnbq1k1r/pp1Pbppp/8/2p5/2B5/P7/1PP1NKPP/RNBQ3R b - - 0 9")]
        public void Zobrist_Captures(string fenStart, string move, string fenTargetPos)
        {
            var gameSource = new Chessgame(fenStart);
            gameSource.TryMakeMove(move);
            var gameTarget = new Chessgame(fenTargetPos);
            Assert.Equal(gameTarget.Hash, gameSource.Hash);
        }

        [Theory]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN4Q/PPPBBPPP/R3K2R w KQkq - 0 2", "Qxh8+")]
        [InlineData("rnbq1k1r/pp1Pbppp/8/2p5/2B5/P7/1PP1NnPP/RNBQK2R w KQ - 0 9", "dxc8=Q")]
        public void Zobrist_CapturesUndo(string fen, string move)
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
        public void Zobrist_EnPassantUndo(string fen, string move)
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
        public void Zobrist_PromotionUndo(string fen, string move)
        {
            var game = new Chessgame(fen);
            var hash = game.Hash;
            game.TryMakeMove(move);
            Assert.NotEqual(hash, game.Hash);
            game.UndoLastMove();
            Assert.Equal(hash, game.Hash);
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "e3",
            "rnbqkbnr/pppppppp/8/8/8/4P3/PPPP1PPP/RNBQKBNR b KQkq - 0 1")]
        [InlineData("r1bqkb1r/ppp2ppp/3p1n2/8/3QP3/2N5/PPP2PPP/R1B1KB1R w KQkq - 2 7", "Bg5",
            "r1bqkb1r/ppp2ppp/3p1n2/6B1/3QP3/2N5/PPP2PPP/R3KB1R b KQkq - 3 7")]
        [InlineData("r1bqkb1r/ppp2ppp/3p1n2/6B1/3QP3/2N5/PPP2PPP/R3KB1R b KQkq - 3 7", "Be7",
            "r1bqk2r/ppp1bppp/3p1n2/6B1/3QP3/2N5/PPP2PPP/R3KB1R w KQkq - 4 8")]
        [InlineData("rnbqkb1r/ppp1pppp/5n2/3p4/3P4/2N2P2/PPP1P1PP/R1BQKBNR b KQkq - 0 3", "c6",
            "rnbqkb1r/pp2pppp/2p2n2/3p4/3P4/2N2P2/PPP1P1PP/R1BQKBNR w KQkq - 0 4")]
        [InlineData("rnbqkb1r/pp2pppp/2p2n2/3p4/3P4/2N2P2/PPP1P1PP/R1BQKBNR w KQkq - 0 4", "Nxd5",
            "rnbqkb1r/pp2pppp/2p2n2/3N4/3P4/5P2/PPP1P1PP/R1BQKBNR b KQkq - 0 4")]
        [InlineData("r1bqk2r/ppp1bppp/3p1n2/6B1/3QP3/2N5/PPP2PPP/R3KB1R w KQkq - 4 8", "O-O-O",
            "r1bqk2r/ppp1bppp/3p1n2/6B1/3QP3/2N5/PPP2PPP/2KR1B1R b kq - 5 8")]
        [InlineData("r1bqk2r/ppp1bppp/3p1n2/6B1/2BQP3/2N5/PPP2PPP/R3K2R b KQkq - 5 8", "O-O",
            "r1bq1rk1/ppp1bppp/3p1n2/6B1/2BQP3/2N5/PPP2PPP/R3K2R w KQ - 6 9")]
        [InlineData("r1bq1rk1/ppp1bppp/3p1n2/6B1/2BQP3/2N5/PPP2PPP/R3K2R w KQ - 6 9", "O-O",
            "r1bq1rk1/ppp1bppp/3p1n2/6B1/2BQP3/2N5/PPP2PPP/R4RK1 b - - 7 9")]
        [InlineData("rnbqkbnr/1ppppppp/p7/8/8/PP6/2PPPPPP/RNBQKBNR b KQk - 0 3", "Ra7",
            "1nbqkbnr/rppppppp/p7/8/8/PP6/2PPPPPP/RNBQKBNR w KQk - 1 4")]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/PpN2Q1p/1PPBBPPP/R3K2R w KQkq - 0 2", "Rb1",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/PpN2Q1p/1PPBBPPP/1R2K2R b Kkq - 1 2")]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/PpN2Q1p/1PPBBPPP/R3K2R w KQkq - 0 2", "Kd1",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/PpN2Q1p/1PPBBPPP/R2K3R b kq - 1 2")]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN2Q1p/PPPBBPPP/1R2K2R w Kkq - 1 2", "Kd1",
            "r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1pN2Q1p/PPPBBPPP/1R1K3R b kq - 2 2")]
        public void Zobrist_Generic(string fenStart, string move, string fenTargetPos)
        {
            var gameSource = new Chessgame(fenStart);
            gameSource.TryMakeMove(move);
            var gameTarget = new Chessgame(fenTargetPos);
            Assert.Equal(gameTarget.Hash, gameSource.Hash);
        }
    }
}
