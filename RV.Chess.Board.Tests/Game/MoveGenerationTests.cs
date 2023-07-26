using RV.Chess.Board.Game;
using RV.Chess.Board.Tests.Utils;
using RV.Chess.Board.Types;
using RV.Chess.Board.Utils;
using Xunit;

namespace RV.Chess.Board.Tests
{
    public class MoveGenerationTests
    {
        [Theory]
        [InlineData("k7/8/2P1p3/8/3N4/8/8/7K w - - 0 1", "d4", new string[] { "e6", "f5", "f3", "e2", "c2", "b3", "b5" })]
        [InlineData("N6k/8/1P2p3/8/8/8/8/K7 w - - 0 1", "a8", new string[] { "c7" })]
        [InlineData("7k/8/1P6/8/8/8/5p2/K6N w - - 0 1", "h1", new string[] { "f2", "g3" })]
        public void Moves_Knight_Basic(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var knightMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Knight).ToList();
            Assert.Equal(allowedTargetSquares.Length, knightMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => knightMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.From) == sourceSquare
                    && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Fact]
        public void Moves_Pawn_Basic()
        {
            var movesWhite = new Chessgame("k7/8/3p3p/3P1P2/8/P1p5/8/7K w - - 0 1").GenerateMoves();
            var movesBlack = new Chessgame("k7/8/3p3p/3P1P2/8/P1p5/8/7K b - - 0 1").GenerateMoves();
            Assert.Equal(5, movesWhite.Length);
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "a3"
                && Coordinates.IdxToSquare(m.To) == "a4"));
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "f5"
                && Coordinates.IdxToSquare(m.To) == "f6"));
            Assert.Equal(5, movesBlack.Length);
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "c3"
                && Coordinates.IdxToSquare(m.To) == "c2"));
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "h6"
                && Coordinates.IdxToSquare(m.To) == "h5"));
        }

        [Fact]
        public void Moves_Pawn_DoublePushes()
        {
            var movesWhite = new Chessgame("k7/7p/3p4/3P1P2/8/2p5/P7/7K w - - 0 1").GenerateMoves();
            var movesBlack = new Chessgame("k7/7p/3p4/3P1P2/8/2p5/P7/7K b - - 0 1").GenerateMoves();
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "a2"
                && Coordinates.IdxToSquare(m.To) == "a4"));
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "h7"
                && Coordinates.IdxToSquare(m.To) == "h5"));
        }

        [Fact]
        public void Moves_Pawn_CantMoveIfBlocked()
        {
            var game = new Chessgame("6k1/8/8/8/8/Q7/P7/6K1 w - - 4 18");
            Assert.Empty(game.GenerateMoves().Where(m => m.Type == MoveType.Pawn));
            game.SetFen("6k1/8/8/8/Q7/8/P7/6K1 w - - 4 18");
            Assert.Single(game.GenerateMoves().Where(m => m.Type == MoveType.Pawn));
        }

        [Fact]
        public void Moves_PawnPromotion_CorrectForBothSides()
        {
            var movesWhite = new Chessgame("k7/5P1p/3p4/3P4/8/8/P1p5/7K w - - 0 1").GenerateMoves();
            var movesBlack = new Chessgame("k7/5P1p/3p4/3P4/8/8/P1p5/7K b - - 0 1").GenerateMoves();
            Assert.Equal(9, movesWhite.Length);
            Assert.Equal(4, movesWhite.Where(m => m.IsPromotion).Count());
            Assert.Equal(9, movesBlack.Length);
            Assert.Equal(4, movesBlack.Where(m => m.IsPromotion).Count());
        }

        [Theory]
        [InlineData("rnbqkbnr/pppppppp/8/8/8/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", new string[] { "d2", "f2" })]
        [InlineData("8/1k6/8/8/8/8/8/7K b - - 0 1", new string[] { "a8", "b8", "c8", "a7", "c7", "a6", "b6", "c6" })]
        public void Moves_King_Basic(string fen, string[] allowedTargetSquares)
        {
            var kingMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.King).ToList();
            Assert.Equal(allowedTargetSquares.Length, kingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => kingMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { "f2" })]
        [InlineData("rnbqk2r/pppppppp/8/6b1/8/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { "f2" })]
        [InlineData("rnbb2nr/ppq1pppp/3k4/2pp4/Q7/2P5/PP1PPPPP/RNB1KBNR b KQ - 0 1", "d6", new string[] { "e6", "e5" })]
        [InlineData("rnbb2nr/ppq1pppp/3k4/2p5/4P3/2P5/PP1P1PPP/RNBQKBNR b KQ - 0 1", "d6", new string[] { "d7", "e6", "c6", "e5" })]
        [InlineData("rnbb4/p1qnrppp/2pk4/2p5/4pP2/2P2Q2/PP1P1PP1/RN2KBNR b KQ - 0 1", "d6", new string[] { "d5", "e6" })]
        public void Chesks_KingCantMoveIntoCheck(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var kingMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.King).ToList();
            Assert.Equal(allowedTargetSquares.Length, kingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => kingMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.From) == sourceSquare
                    && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQKBNR w KQkq - 0 1", "e1", new string[] { })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/RNBQK2R w KQkq - 0 1", "e1", new string[] { "g1" })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3KBNR w KQkq - 0 1", "e1", new string[] { "c1" })]
        [InlineData("rnbqkb1r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3K2R w KQkq - 0 1", "e1", new string[] { "c1", "g1" })]
        [InlineData("rnbqk2r/pppppppp/8/8/2n5/3P1P2/PPP1P1PP/R3K2R b KQkq - 0 1", "e8", new string[] { "g8" })]
        [InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/4P3/1p3Q1p/PPPBBPPP/RN2K2R w KQkq - 0 2", "e1", new string[] { "g1" })]
        public void Castling_CanCastle(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var castlingMoves = new Chessgame(fen).GenerateMoves()
                .Where(m => m.Type == MoveType.CastleShort || m.Type == MoveType.CastleLong).ToList();
            Assert.Equal(allowedTargetSquares.Length, castlingMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => castlingMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.From) == sourceSquare
                    && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("rnbqkb1r/pppppppp/8/8/8/3Pn3/PPP1P1PP/RNBQK2R w KQkq - 0 1")]
        [InlineData("r3kbnr/ppp1pppp/8/8/6Q1/8/PPPPPPPP/RNB1KBNR b KQkq - 0 1")]
        [InlineData("r3kbnr/pppp1ppp/8/8/4Q3/8/PPPPPPPP/RNB1KBNR b KQkq - 0 1")]
        public void Castling_CantCastleUnderAttack(string fen)
        {
            var castlingMoves = new Chessgame(fen).GenerateMoves()
                .Where(m => m.Type is MoveType.King && m.IsCastling).ToList();
            Assert.Empty(castlingMoves);
        }

        [Fact]
        public void Captures_PawnBasic_CorrectForWhite()
        {
            var movesWhite = new Chessgame("k7/8/4p1p1/3P1P2/8/2p5/P2N4/7K w - - 0 1").GenerateMoves();
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "d5"
                && Coordinates.IdxToSquare(m.To) == "e6" && m.IsCapture));
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "f5"
                && Coordinates.IdxToSquare(m.To) == "e6" && m.IsCapture));
            Assert.Single(movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "f5"
                && Coordinates.IdxToSquare(m.To) == "g6" && m.IsCapture));
        }

        [Fact]
        public void Captures_PawnBasic_CorrectForBlack()
        {
            var movesBlack = new Chessgame("k7/8/4p1p1/3P1P2/8/2p5/P2N4/7K b - - 0 1").GenerateMoves();
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "e6"
                && Coordinates.IdxToSquare(m.To) == "d5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "e6"
                && Coordinates.IdxToSquare(m.To) == "f5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "g6"
                && Coordinates.IdxToSquare(m.To) == "f5" && m.IsCapture));
            Assert.Single(movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "c3"
                && Coordinates.IdxToSquare(m.To) == "d2" && m.IsCapture));
        }

        [Fact]
        public void Captures_PawnWithPromotion_CorrectForWhite()
        {
            var movesWhite = new Chessgame("k5r1/5P2/6p1/8/8/1P6/2p5/3N3K w - - 0 1").GenerateMoves();
            Assert.Equal(4, movesWhite.Where(m => Coordinates.IdxToSquare(m.From) == "f7"
                && Coordinates.IdxToSquare(m.To) == "g8" && m.IsCapture && m.IsPromotion).Count());
        }

        [Fact]
        public void Captures_PawnWithPromotion_CorrectForBlack()
        {
            var movesBlack = new Chessgame("k5r1/5P2/6p1/8/8/1P6/2p5/3N3K b - - 0 1").GenerateMoves();
            Assert.Equal(4, movesBlack.Where(m => Coordinates.IdxToSquare(m.From) == "c2"
                && Coordinates.IdxToSquare(m.To) == "d1" && m.IsCapture && m.IsPromotion).Count());
        }

        [Fact]
        public void Captures_PawnEnPassant_CorrectForWhite()
        {
            var moves = new Chessgame("4k3/4p1p1/8/4Pp2/8/8/8/3QK3 w - f6 0 3").GenerateMoves();
            Assert.Single(moves.Where(m => Coordinates.IdxToSquare(m.From) == "e5"
                && Coordinates.IdxToSquare(m.To) == "f6" && m.IsCapture && m.IsEnPassant));
        }

        [Fact]
        public void Captures_PawnEnPassant_CorrectForBlack()
        {
            var moves = new Chessgame("rnbqkbnr/pp1ppppp/8/8/1Pp1P3/P7/2PP1PPP/RNBQKBNR b KQkq b3 0 3")
                .GenerateMoves();
            Assert.Single(moves.Where(m => Coordinates.IdxToSquare(m.From) == "c4"
                && Coordinates.IdxToSquare(m.To) == "b3" && m.IsCapture && m.IsEnPassant));
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
            var rookMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Rook).ToList();
            Assert.Equal(allowedTargetSquares.Length, rookMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => rookMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.From) == sourceSquare
                    && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("8/3k4/8/5r2/8/3Q3B/8/3K4 b - - 0 1")]
        [InlineData("8/8/1Rb3k1/8/8/3Q4/8/3K4 b - - 0 1")]
        public void Pins_PinnedPieceCantBlockCheck(string fen)
        {
            var moves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is not MoveType.King).ToList();
            Assert.Empty(moves);
        }

        [Theory]
        [InlineData("k7/6r1/8/4B3/3K4/8/8/8 w - - 0 1", new string[] { "b8", "c7", "d6", "f6", "g7", "f4", "g3", "h2" })]
        [InlineData("k7/6b1/8/4B3/3K4/8/8/8 w - - 0 1", new string[] { "f6", "g7" })]
        [InlineData("k7/8/1b6/2B5/3K4/8/8/8 w - - 0 1", new string[] { "b6" })]
        [InlineData("k7/6K1/5B2/8/3b4/8/8/8 w - - 0 1", new string[] { "e5", "d4" })]
        [InlineData("k7/8/1K6/2B5/3b4/8/8/8 w - - 0 1", new string[] { "d4" })]
        public void Pins_Bishop_MovesAlongPinnedAxisOnly(string fen, string[] allowedTargetSquares)
        {
            var bishopMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Bishop).ToList();
            Assert.Equal(allowedTargetSquares.Length, bishopMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => bishopMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("3K4/3Q4/8/3q4/8/8/8/7k w - - 0 1", new string[] { "d6", "d5" })]
        [InlineData("k7/8/2K5/3Q4/4b3/8/8/8 w - - 0 1", new string[] { "e4" })]
        [InlineData("7k/8/8/r2QK3/8/8/8/8 w - - 0 1", new string[] { "a5", "b5", "c5" })]
        [InlineData("7k/8/8/4K3/4Q3/4r3/8/8 w - - 0 1", new string[] { "e3" })]
        public void Pins_Queen_MovesAlongPinnedAxisOnly(string fen, string[] allowedTargetSquares)
        {
            var queenMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Queen).ToList();
            Assert.Equal(allowedTargetSquares.Length, queenMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => queenMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("7k/8/8/4K3/3N4/2b5/8/8 w - - 0 1")]
        [InlineData("k3r3/8/4N3/4K3/8/8/8/8 w - - 0 1")]
        [InlineData("k7/8/5q2/8/8/2N5/1K6/8 w - - 0 1")]
        [InlineData("3K4/8/8/8/8/3N4/8/k2q4 w - - 0 1")]
        public void Pins_Knight_CantMoveIfPinned(string fen)
        {
            var knightMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Knight).ToList();
            Assert.Empty(knightMoves);
        }

        [Fact]
        public void Pins_Knight_CantMoveIfPinned_TwoKnights()
        {
            var knightMoves = new Chessgame("r1bqk2r/ppp2ppp/2nb4/1B3n2/4Q3/2N2N2/PPP2PPP/R1B1K2R b KQkq - 7 10")
                .GenerateMoves().Where(m => m.Type is MoveType.Knight).ToList();
            Assert.Single(knightMoves);
            Assert.Equal("f5", Coordinates.IdxToSquare(knightMoves.First().From));
            Assert.Equal("e7", Coordinates.IdxToSquare(knightMoves.First().To));
        }

        [Theory]
        [InlineData("k7/8/b7/8/2P5/8/4K3/8 w - - 0 1")]
        [InlineData("K7/8/3k4/4p3/8/8/7Q/8 b - - 0 1")]
        [InlineData("K7/8/3k4/3p4/3RP3/8/8/8 b - - 0 1")]
        public void Pins_Pawns_CantMoveFromPin(string fen)
        {
            var game = new Chessgame(fen);
            Assert.Empty(game.GenerateMoves().Where(m => m.Type is MoveType.Pawn));
        }

        [Theory]
        [InlineData("8/8/3k4/3p4/8/8/3R4/7K b - - 0 1", new string[] { "d4" })]
        [InlineData("8/8/3k4/4p3/5B2/8/8/7K b - - 0 1", new string[] { "f4" })]
        [InlineData("k7/8/2b5/3P4/4K3/8/8/8 w - - 0 1", new string[] { "c6" })]
        public void Pins_Pawns_CanMoveAlongPinAxis(string fen, string[] allowedTargetSquares)
        {
            var pawnMoves = new Chessgame(fen).GenerateMoves().Where(m => m.Type is MoveType.Pawn).ToList();
            Assert.Equal(allowedTargetSquares.Length, pawnMoves.Count);
            Assert.True(allowedTargetSquares.All(ts => pawnMoves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Fact]
        public void Pins_Pawns_CantTakeEnPassantCheckBlocker()
        {
            var moves = new Chessgame("8/8/3p4/KPp4r/1R3p1k/4P3/6P1/8 w - c6 0 1").GenerateMoves().ToList();
            Assert.DoesNotContain(moves, m => m.ToString() == "b5c6");
        }

        [Theory]
        [InlineData("8/8/3k4/8/8/4N3/8/7K w - - 0 1", "f5")]
        [InlineData("8/8/3kp3/2pp4/8/8/7K/Q7 w - - 0 1", "e5")]
        [InlineData("7B/8/3kp3/2pp4/8/8/8/K7 w - - 0 1", "e5")]
        [InlineData("7B/8/3kp3/2ppp3/8/8/1R6/7K w - - 0 1", "b6")]
        [InlineData("7B/6p1/3kp3/2pp4/4P3/8/8/7K w - - 0 1", "e5")]
        public void Checks_BasicCheckFlagSetCorrectly(string fen, string checkingMoveTargetSquare)
        {
            var checkingMove = new Chessgame(fen).GenerateMoves()
                .Single(m => Coordinates.IdxToSquare(m.To) == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("8/6k1/8/4Pp2/8/8/8/7K w - f6 0 1", "f6")]
        [InlineData("rn1qkbnr/pppbpppp/8/8/2p5/3PP3/PPP1KPPP/RNBQ1BNR b - - 0 1", "d3")]
        public void Checks_PawnAfterCapture(string fen, string checkingMoveTargetSquare)
        {
            var checkingMove = new Chessgame(fen).GenerateMoves()
                .Single(m => Coordinates.IdxToSquare(m.To) == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("8/8/3kp3/2pp4/5P2/8/7Q/7K w - - 0 1", "f5")]
        [InlineData("k7/3r4/2p1p3/1p6/2PnP3/1p1K1p2/2p1p3/8 b - - 0 1", "f5")]
        [InlineData("k7/3r4/b1p1p3/1p6/3PP3/1p1K1p2/2p1p3/8 b - - 0 1", "b4")]
        public void Checks_DiscoveryCheckFlagSetCorrectly(string fen, string checkingMoveTargetSquare)
        {
            var checkingMove = new Chessgame(fen).GenerateMoves()
                .Single(m => Coordinates.IdxToSquare(m.To) == checkingMoveTargetSquare);
            Assert.True(checkingMove.IsCheck);
        }

        [Theory]
        [InlineData("3k4/8/3q4/8/8/1n2n3/r7/3K4 w - - 0 1", new string[] { "e1" })]
        [InlineData("3k4/5P2/8/3R4/8/2q1r3/8/3K4 b - - 0 1", new string[] { "c7", "e7", "c8" })]
        public void Checks_Evasions_DoubleCheck(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Count);
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.King && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Fact]
        public void Checks_Evasions_CanEvadeWithCapture()
        {
            var moves = new Chessgame("rnb1kbnr/ppppp1pp/8/8/7q/8/PPPPp1PP/RNBQKBNR w KQkq - 0 1")
                .GenerateMoves().Where(m => m.Type == MoveType.King).ToList();
            Assert.Single(moves);
            Assert.True(moves[0].IsCapture);
            Assert.Equal("e2", Coordinates.IdxToSquare(moves[0].To));
        }

        [Theory]
        [InlineData("3k4/8/8/3r3R/2q5/8/8/3K4 w - - 0 1", new string[] { "d5" })]
        [InlineData("3k4/8/8/3r4/2q4R/8/8/3K4 w - - 0 1", new string[] { "d4" })]
        public void Checks_Rook_CanBlockOrCapture(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Rook).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Rook && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("8/2k5/8/8/5Q2/2b5/8/3K4 b - - 0 1", new string[] { "e5" })]
        [InlineData("8/8/2b3k1/8/4Q3/8/8/3K4 b - - 0 1", new string[] { "e4" })]
        public void Checks_Bishop_CanBlockOrCapture(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Bishop).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Bishop && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("3k4/8/8/8/8/3NN3/1n3N2/3K4 w - - 0 1", new string[] { "b2" })]
        [InlineData("3k4/8/8/4b3/8/5K2/7n/5N2 w - - 0 1", new string[] { "h2" })]
        public void Checks_Knight_CanBlockOrCapture(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Knight).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Knight && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("rnb1kbnr/pppp1ppp/8/8/7q/8/PPPPP1PP/RNBQKBNR w KQkq - 0 1", new string[] { "g3" })]
        [InlineData("rnb1kbnr/pppp1ppp/8/8/8/6q1/PPPPP1PP/RNBQKBNR w KQkq - 0 1", new string[] { "g3" })]
        [InlineData("rnbqkbnr/ppp3pp/3p1p2/1B6/8/8/PPPP2PP/RNBQK1NR b KQkq - 0 1", new string[] { "c6" })]
        [InlineData("rnbqkbnr/1p4pp/p2p1p2/1B6/8/8/PPPP2PP/RNBQK1NR b KQkq - 0 1", new string[] { "b5" })]
        public void Checks_Pawns_CanBlockOrCapture(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Pawn).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Pawn && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("8/8/8/3k4/3pP3/8/6K1/8 b - e3 0 1", new string[] { "e3" })]
        [InlineData("8/8/8/3kPp2/6K1/8/8/8 w - f6 0 2", new string[] { "f6" })]
        public void Checks_Pawns_CanCaptureCheckerPawnEnPassant(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Pawn).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Pawn && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("3k4/8/q2Q4/3r4/3b4/8/8/3K4 b - - 0 1", new string[] { "d6" })]
        [InlineData("3k4/8/2q5/3r4/3b3Q/8/8/3K4 b - - 0 1", new string[] { "f6" })]
        public void Checks_Queen_CanBlockOrCapture(string fen, string[] targets)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(targets.Length, moves.Where(m => m.Type == MoveType.Queen).Count());
            Assert.True(targets.All(ts => moves
                .FindIndex(rm => rm.Type == MoveType.Queen && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Theory]
        [InlineData("3rk3/8/8/8/1q6/8/5P2/4K3 w - - 0 1", "e1", new string[] { "e2", "f1" })]
        [InlineData("4k3/8/8/7b/8/3n3q/5P2/4K1R1 w - - 0 1", "e1", new string[] { "d2" })]
        public void Checks_Evasions_KingToSafeSquareOnly(string fen, string sourceSquare, string[] allowedTargetSquares)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Equal(allowedTargetSquares.Length, moves.Count);
            Assert.True(allowedTargetSquares.All(ts => moves
                .FindIndex(rm => Coordinates.IdxToSquare(rm.From) == sourceSquare
                    && Coordinates.IdxToSquare(rm.To) == ts) > -1));
        }

        [Fact]
        public void Checks_Evasions_KingCantMoveBehindItself()
        {
            var moves = new Chessgame("8/4k3/8/8/4R3/8/8/4K3 b - - 0 1").GenerateMoves().ToList();
            Assert.DoesNotContain(moves, m => m.To == Coordinates.SquareToIdx("e8"));
        }

        [Theory]
        [InlineData("r2k1b1r/ppp2pp1/2n1b2p/2p1P3/4NP2/2N5/PPP3PP/R3KB1R w KQ - 2 13", "e1", "c1")]
        public void Checks_Castling_Sets_Check(string fen, string from, string to)
        {
            var moves = new Chessgame(fen).GenerateMoves().ToList();
            Assert.Single(moves.Where(m => Coordinates.IdxToSquare(m.From) == from
                && Coordinates.IdxToSquare(m.To) == to));
        }

        [Theory]
        [InlineData("r1bqk2r/pp1pnpbp/2n1p1p1/2p5/4PP2/2PP1NP1/PP4BP/RNBQK2R w KQkq - 0 8", "e1", "f1")]
        [InlineData("2r1k2r/pp2ppbp/2npb1p1/q7/3NP3/2N1BP2/PPPQ2PP/1K1R3R w k - 5 13", "d4", "c6")]
        [InlineData("r1bqkbnr/ppp2ppp/2np4/1B6/3NP3/8/PPP2PPP/RNBQK2R b KQkq - 0 5", "g8", "e7")]
        [InlineData("2R5/3kp2R/3p4/8/7K/8/8/3Q4 w - - 0 1", "d1", "g4")]
        [InlineData("1B6/3p4/2p2Kp1/P1k3p1/P1p2np1/2P1N1N1/2bP4/8 w - - 0 1", "d2", "d4")]
        [InlineData("3r4/4pp2/R2b1k1K/2np4/4PPB1/2P3n1/8/8 w - - 0 2", "e4", "e5")]
        public void Moves_Misc(string fen, string from, string to)
        {
            var move = new Chessgame(fen).GenerateMoves().SingleOrDefault(m =>
                Coordinates.IdxToSquare(m.From) == from && Coordinates.IdxToSquare(m.To) == to);
            Assert.NotEqual(0UL, move.Move);
        }
    }
}
