using System.Text;
using System.Runtime.CompilerServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace RV.Chess.Board
{
    public class Bitboard
    {
        private ulong _board;

        public Bitboard(ulong board)
        {
            _board = board;
        }

        public ulong Board => _board;

        internal void SetAt(int bitIdx)
        {
            _board |= (1UL << bitIdx);
        }

        internal void RemoveAt(int bitIdx)
        {
            _board &= ~(1UL << bitIdx);
        }

        internal bool OccupiedAt(int square) => (_board & (1UL << square)) != 0;

        internal static bool OccupiedAt(ulong board, int square) => (board & (1UL << square)) != 0;

        internal void Clear()
        {
            _board = 0;
        }

#if DEBUG
        public void Print()
        {
            Print(this);
        }

        public static void Print(Bitboard bitboard)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  +---+---+---+---+---+---+---+---+");

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");

                for (var file = 0; file < 8; file++)
                {
                    var squareIdx = rank * 8 + file;

                    if (bitboard.OccupiedAt(squareIdx))
                    {
                        sb.Append("| + ");

                    }
                    else
                    {
                        sb.Append("|   ");
                    }
                }

                sb.Append("|\n");

                if (rank > 0)
                {
                    sb.AppendLine("  |---|---|---|---|---|---|---|---|");
                }
                else
                {
                    sb.AppendLine("  +---+---+---+---+---+---+---+---+");
                    sb.AppendLine("    a   b   c   d   e   f   g   h");
                }
            }

            Debug.WriteLine(sb.ToString());
        }
#endif


        public static Bitboard operator &(Bitboard a, Bitboard b) => new(a.Board & b.Board);

        public static ulong operator &(Bitboard a, ulong b) => a.Board & b;

        public static ulong operator &(ulong a, Bitboard b) => a & b.Board;
    }
}
