using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RV.Chess.Board
{
    internal static class BitExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasSingleBitSet(this ulong value)
        {
            return BitOperations.PopCount(value) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong SetAt(this ulong value, int bitIdx)
        {
            return value | (1UL << bitIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong RemoveAt(this ulong value, int bitIdx)
        {
            return value & ~(1UL << bitIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool OccupiedAt(this ulong value, int square) => (value & (1UL << square)) != 0;

        internal static string ToBinString(this ulong value) =>
            $"{Convert.ToString((long)value, toBase: 2)}";

#if DEBUG
        public static void Print(this ulong value)
        {
            var sb = new StringBuilder();
            sb.AppendLine("  ┌───┬───┬───┬───┬───┬───┬───┬───┐");

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");

                for (var file = 0; file < 8; file++)
                {
                    var isEmpty = true;
                    var squareIdx = rank * 8 + file;

                    if ((value & (1UL << squareIdx)) > 0)
                    {
                        sb.Append("│ X ");
                        isEmpty = false;
                    }

                    if (isEmpty)
                    {
                        sb.Append("│   ");
                    }
                }

                sb.Append("|\n");

                if (rank > 0)
                {
                    sb.AppendLine("  ├───┼───┼───┼───┼───┼───┼───┼───┤");
                }
                else
                {
                    sb.AppendLine("  └───┴───┴───┴───┴───┴───┴───┴───┘");
                    sb.AppendLine("    a   b   c   d   e   f   g   h");
                }
            }

            Debug.WriteLine(sb.ToString());
        }
#endif
    }
}
