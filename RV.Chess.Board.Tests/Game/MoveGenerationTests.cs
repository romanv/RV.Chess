using RV.Chess.PGN;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class MoveGenerationTests
    {
        [Theory]
        [InlineData("k7/8/2P1p3/8/3N4/8/8/7K w - - 0 1", "d4", new string[] { "e6", "f5", "f3", "e2", "c2", "b3", "b5" })]
        [InlineData("N6k/8/1P2p3/8/8/8/8/K7 w - - 0 1", "a8", new string[] { "c7" })]
        [InlineData("7k/8/1P6/8/8/8/5p2/K6N w - - 0 1", "h1", new string[] { "f2", "g3" })]
        public void Moves_Knight_Correct(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var knightMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Knight).ToList();
            Assert.Equal(allowedTargetSquares.Length, knightMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => knightMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Fact]
        public void Moves_Pawn_Basic_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("k7/8/3p3p/3P1P2/8/P1p5/8/7K w - - 0 1");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("k7/8/3p3p/3P1P2/8/P1p5/8/7K b - - 0 1");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Equal(5, movesWhite.Length);
            Assert.Single(movesWhite.Where(m => m.From == "a3" && m.To == "a4"));
            Assert.Single(movesWhite.Where(m => m.From == "f5" && m.To == "f6"));
            Assert.Equal(5, movesBlack.Length);
            Assert.Single(movesBlack.Where(m => m.From == "c3" && m.To == "c2"));
            Assert.Single(movesBlack.Where(m => m.From == "h6" && m.To == "h5"));
        }

        [Fact]
        public void Moves_Pawn_Double_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("k7/7p/3p4/3P1P2/8/2p5/P7/7K w - - 0 1");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("k7/7p/3p4/3P1P2/8/2p5/P7/7K b - - 0 1");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Single(movesWhite.Where(m => m.From == "a2" && m.To == "a4"));
            Assert.Single(movesBlack.Where(m => m.From == "h7" && m.To == "h5"));
        }

        [Fact]
        public void Moves_Pawn_CantMoveIfBlocked()
        {
            var game = new Chessgame();

            // all moves are blocked
            game.SetFen("6k1/8/8/8/8/Q7/P7/6K1 w - - 4 18");
            Assert.Empty(game.GenerateMoves().Where(m => m.Piece.Type == PieceType.Pawn));

            // only double move is blocked
            game.SetFen("6k1/8/8/8/Q7/8/P7/6K1 w - - 4 18");
            Assert.Single(game.GenerateMoves().Where(m => m.Piece.Type == PieceType.Pawn));
        }

        [Fact]
        public void Moves_PawnPromotion_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("k7/5P1p/3p4/3P4/8/8/P1p5/7K w - - 0 1");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("k7/5P1p/3p4/3P4/8/8/P1p5/7K b - - 0 1");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Equal(9, movesWhite.Length);
            Assert.Equal(4, movesWhite.Where(m => m.PromoteTo is not PieceType.None).Count());
            Assert.Equal(9, movesBlack.Length);
            Assert.Equal(4, movesBlack.Where(m => m.PromoteTo is not PieceType.None).Count());
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { "d2", "f2" })]
        [InlineData("8/1k6/8/8/8/8/8/7K b - - 0 1", "b7", new string[] { "a8", "b8", "c8", "a7", "c7", "a6", "b6", "c6" })]
        public void Moves_King_Safe(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var kingMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.King).ToList();
            Assert.Equal(allowedTargetSquares.Length, kingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => kingMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { "f2" })]
        [InlineData("rnbqk2r/pppppppp/8/6b1/8/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { "f2" })]
        [InlineData("rnbb2nr/ppq1pppp/3k4/2pp4/Q7/2P5/PP1PPPPP/RNB1KBNR b KQ - 0 1", "d6", new string[] { "e6", "e5" })]
        [InlineData("rnbb2nr/ppq1pppp/3k4/2p5/4P3/2P5/PP1P1PPP/RNBQKBNR b KQ - 0 1", "d6", new string[] { "d7", "e6", "c6", "e5" })]
        [InlineData("rnbb4/p1qnrppp/2pk4/2p5/4pP2/2P2Q2/PP1P1PP1/RN2KBNR b KQ - 0 1", "d6", new string[] { "d5", "e6" })]
        public void Moves_King_CantMoveIntoCheck(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var kingMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.King).ToList();
            Assert.Equal(allowedTargetSquares.Length, kingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => kingMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQK2R w KQkq - 0 1", "e1", new string[] { "g1" })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3KBNR w KQkq - 0 1", "e1", new string[] { "c1" })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3K2R w KQkq - 0 1", "e1", new string[] { "c1", "g1" })]
        [InlineData("rnbqk2r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3K2R b KQkq - 0 1", "e8", new string[] { "g8" })]
        public void Moves_King_Castling(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var castlingMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.King && m.IsCastling).ToList();
            Assert.Equal(allowedTargetSquares.Length, castlingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => castlingMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/8/3Pn3/PPP1P1PP/RNBQK2R w KQkq - 0 1")]
        [InlineData("r3kbnr/ppp1pppp/8/8/6Q1/8/PPPPPPPP/RNB1KBNR b KQkq - 0 1")]
        [InlineData("r3kbnr/pppp1ppp/8/8/4Q3/8/PPPPPPPP/RNB1KBNR b KQkq - 0 1")]
        public void Moves_King_CantCastleUnderAttack(string fen)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var castlingMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.King && m.IsCastling).ToList();
            Assert.Empty(castlingMoves);
        }

        [Fact]
        public void Captures_PawnBasic_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("k7/8/4p1p1/3P1P2/8/2p5/P2N4/7K w - - 0 1");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("k7/8/4p1p1/3P1P2/8/2p5/P2N4/7K b - - 0 1");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Single(movesWhite.Where(m => m.From == "d5" && m.To == "e6" && m.IsCapture));
            Assert.Single(movesWhite.Where(m => m.From == "f5" && m.To == "e6" && m.IsCapture));
            Assert.Single(movesWhite.Where(m => m.From == "f5" && m.To == "g6" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => m.From == "e6" && m.To == "d5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => m.From == "e6" && m.To == "f5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => m.From == "g6" && m.To == "f5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => m.From == "c3" && m.To == "d2" && m.IsCapture));
        }

        [Fact]
        public void Captures_PawnWithPromotion_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("k5r1/5P2/6p1/8/8/1P6/2p5/3N3K w - - 0 1");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("k5r1/5P2/6p1/8/8/1P6/2p5/3N3K b - - 0 1");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Equal(4, movesWhite.Where(m => m.From == "f7" && m.To == "g8"
                && m.IsCapture && m.PromoteTo is not PieceType.None).Count());
            Assert.Equal(4, movesBlack.Where(m => m.From == "c2" && m.To == "d1"
                && m.IsCapture && m.PromoteTo is not PieceType.None).Count());
        }

        [Fact]
        public void Captures_PawnEnPassant_CorrectForBothSides()
        {
            var gameWhite = new Chessgame();
            gameWhite.SetFen("4k3/4p1p1/8/4Pp2/8/8/8/3QK3 w - f6 0 3");
            var movesWhite = gameWhite.GenerateMoves();
            var gameBlack = new Chessgame();
            gameBlack.SetFen("rnbqkbnr/pp1ppppp/8/8/1Pp1P3/P7/2PP1PPP/RNBQKBNR b KQkq b3 0 3");
            var movesBlack = gameBlack.GenerateMoves();

            Assert.Single(movesWhite.Where(m => m.From == "e5" && m.To == "f6" && m.IsCapture && m.IsEnPassant));
            Assert.Single(movesBlack.Where(m => m.From == "c4" && m.To == "b3" && m.IsCapture && m.IsEnPassant));
        }

        [Theory]
        [InlineData("7k/8/3q4/8/3R4/3K4/8/8 w - - 0 1", "d4", new string[] { "d5", "d6" })]
        [InlineData("7k/8/8/8/8/3KR1q1/8/8 w - - 0 1", "e3", new string[] { "f3", "g3" })]
        [InlineData("7k/8/8/4K3/4R3/8/4q3/8 w - - 0 1", "e4", new string[] { "e3", "e2" })]
        [InlineData("7k/8/8/q2RK3/8/8/8/8 w - - 0 1", "d5", new string[] { "a5", "b5", "c5" })]
        [InlineData("6kq/8/8/4R3/3K4/8/8/8 w - - 0 1", "e5", new string[] {})]
        [InlineData("7k/8/1q6/2R5/3K4/8/8/8 w - - 0 1", "c5", new string[] {})]
        [InlineData("7k/8/8/8/3K4/2R5/8/q7 w - - 0 1", "c3", new string[] {})]
        [InlineData("7k/8/1K6/2R5/8/8/8/6q1 w - - 0 1", "c5", new string[] {})]
        public void Pins_Rook_MovesAlongPinnedAxisOnly(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var rookMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Rook).ToList();
            Assert.Equal(allowedTargetSquares.Length, rookMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => rookMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("k7/6r1/8/4B3/3K4/8/8/8 w - - 0 1", "e5", new string[] { "b8", "c7", "d6", "f6", "g7", "f4", "g3", "h2" })]
        [InlineData("k7/6b1/8/4B3/3K4/8/8/8 w - - 0 1", "e5", new string[] { "f6", "g7" })]
        [InlineData("k7/8/1b6/2B5/3K4/8/8/8 w - - 0 1", "c5", new string[] { "b6" })]
        [InlineData("k7/6K1/5B2/8/3b4/8/8/8 w - - 0 1", "f6", new string[] { "e5", "d4" })]
        [InlineData("k7/8/1K6/2B5/3b4/8/8/8 w - - 0 1", "c5", new string[] { "d4" })]
        public void Pins_Bishop_MovesAlongPinnedAxisOnly(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var bishopMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Bishop).ToList();
            Assert.Equal(allowedTargetSquares.Length, bishopMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => bishopMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("3K4/3Q4/8/3q4/8/8/8/7k w - - 0 1", "d7", new string[] { "d6", "d5" })]
        [InlineData("k7/8/2K5/3Q4/4b3/8/8/8 w - - 0 1", "d5", new string[] { "e4" })]
        [InlineData("k7/8/8/r2QK3/8/8/8/8 w - - 0 1", "d5", new string[] { "a5", "b5", "c5" })]
        [InlineData("k7/8/8/4K3/4Q3/4r3/8/8 w - - 0 1", "e4", new string[] { "e3" })]
        public void Pins_Queen_MovesAlongPinnedAxisOnly(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var queenMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Queen).ToList();
            Assert.Equal(allowedTargetSquares.Length, queenMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => queenMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("7k/8/8/4K3/3N4/2b5/8/8 w - - 0 1")]
        [InlineData("k3r3/8/4N3/4K3/8/8/8/8 w - - 0 1")]
        [InlineData("k7/8/5q2/8/8/2N5/1K6/8 w - - 0 1")]
        [InlineData("3K4/8/8/8/8/3N4/8/k2q4 w - - 0 1")]
        public void Pins_Knight_CantMoveIfPinned(string fen)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var knightMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Knight).ToList();
            Assert.Empty(knightMoves);
        }

        [Theory]
        [InlineData("k7/8/b7/8/2P5/8/4K3/8 w - - 0 1")]
        [InlineData("K7/8/3k4/4p3/8/8/7Q/8 b - - 0 1")]
        [InlineData("K7/8/3k4/3p4/3RP3/8/8/8 b - - 0 1")]
        public void Pins_PinnedPawns_CantMove(string fen)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            Assert.Empty(game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Pawn));
        }

        [Theory]
        [InlineData("8/8/3k4/3p4/8/8/3R4/7K b - - 0 1", "d5", new string[] { "d4" })]
        public void Pins_PinnedPawns_CanMoveAlongPinAxis(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var pawnMoves = game.GenerateMoves().Where(m => m.Piece.Type is PieceType.Pawn).ToList();
            Assert.Equal(allowedTargetSquares.Length, pawnMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => pawnMoves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("8/8/3k4/8/8/4N3/8/7K w - - 0 1", "f5")]
        [InlineData("8/8/3kp3/2pp4/8/8/7K/Q7 w - - 0 1", "e5")]
        [InlineData("7B/8/3kp3/2pp4/8/8/8/K7 w - - 0 1", "e5")]
        [InlineData("7B/8/3kp3/2ppp3/8/8/1R6/7K w - - 0 1", "b6")]
        [InlineData("7B/6p1/3kp3/2pp4/4P3/8/8/7K w - - 0 1", "e5")]
        public void Checks_BasicCheckFlagSetCorrectly(string fen, string checkingMoveTargetSquare)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var checkingMove = game.GenerateMoves().Single(m => m.To == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("8/6k1/8/4Pp2/8/8/8/7K w - f6 0 1", "f6")]
        [InlineData("rn1qkbnr/pppbpppp/8/8/2p5/3PP3/PPP1KPPP/RNBQ1BNR b - - 0 1", "d3")]
        public void Checks_PawnAfterCapture(string fen, string checkingMoveTargetSquare)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var bb = game.GenerateMoves();
            var checkingMove = game.GenerateMoves().Single(m => m.To == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("8/8/3kp3/2pp4/5P2/8/7Q/7K w - - 0 1", "f5")]
        [InlineData("k7/3r4/2p1p3/1p6/2PnP3/1p1K1p2/2p1p3/8 b - - 0 1", "f5")]
        [InlineData("k7/3r4/b1p1p3/1p6/3PP3/1p1K1p2/2p1p3/8 b - - 0 1", "b4")]
        public void Checks_DiscoveryCheckFlagSetCorrectly(string fen, string checkingMoveTargetSquare)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var checkingMove = game.GenerateMoves().Single(m => m.To == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("3rk3/8/8/8/1q6/8/5P2/4K3 w - - 0 1", "e1", new string[] { "e2", "f1" })]
        [InlineData("4k3/8/8/7b/8/3n3q/5P2/4K1R1 w - - 0 1", "e1", new string[] { "d2" })]
        public void Checks_Evasions_KingToSafeSquareOnly(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var moves = game.GenerateMoves().ToList();
            Assert.Equal(allowedTargetSquares.Length, moves.Count);
            Assert.True(allowedTargetSquares.All(ts => moves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("4k3/8/8/7b/8/8/2PP1P2/2QKB3 w - - 0 1", "f2", new string[] { "f3" })]
        public void Checks_Blocks_CanBlockSlidingAttack(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var moves = game.GenerateMoves().ToList();
            Assert.Equal(allowedTargetSquares.Length, moves.Count);
            Assert.True(allowedTargetSquares.All(ts => moves.Find(rm => rm.From == sourceSquare && rm.To == ts) != null));
        }

        [Theory]
        [InlineData("r2k1b1r/ppp2pp1/2n1b2p/2p1P3/4NP2/2N5/PPP3PP/R3KB1R w KQ - 2 13", "O-O-O+")]
        public void Checks_Castling_Sets_Check(string fen, string expectedCheck)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var moves = game.GenerateMoves().ToList();
            Assert.Single(moves.Where(m => m.San == expectedCheck));
        }

        [Theory]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "Kf1")]
        [InlineData("2r1k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/1K1R3R w k - 5 13", "Nxc6")]
        [InlineData("r1bqkbnr/ppp2ppp/2np4/1B6/3NP3/8/PPP2PPP/RNBQK2R b KQkq - 0 5", "Ne7")]
        public void Moves_Misc(string fen, string san)
        {
            var game = new Chessgame();
            game.SetFen(fen);
            var mm = game.GenerateMoves().Select(m => m.San).ToList();
            var move = game.GenerateMoves().SingleOrDefault(m => m.San == san);
            Assert.NotNull(move);
        }
    }
}
