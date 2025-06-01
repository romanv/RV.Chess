namespace RV.Chess.Board.Utils
{
    public static class Coordinates
    {
        private static readonly char[] RANKS = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

        private static readonly string[] SQUARES =
        {
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
        };

        public static string IdxToSquare(int idx) => (idx >= 0 && idx < 64)
            ? SQUARES[idx]
            : throw new InvalidDataException($"Bad square index ({idx}). Must be between 0 and 63");

        public static int SquareToIdx(string square)
        {
            var file = square[0] - 97;
            var rank = square[1] - 49;
            return rank * 8 + file;
        }

        public static char SquareIdxToFile(int idx)
        {
            var fileIdx = idx % 8;
            return fileIdx < 8 ? RANKS[fileIdx] : ' ';
        }

        public static int SquareIdxToRank(int idx) => (idx / 8) + 1;

        public static bool IsValidSquare(string square) => SquareToIdx(square) > -1;
    }
}
