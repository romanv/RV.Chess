using System.Numerics;

namespace RV.Chess.Board
{
    internal static class BitExtensions
    {
        // https://matthewarcus.wordpress.com/2012/11/18/reversing-a-64-bit-word/
        internal static ulong Reverse(this ulong value)
        {
            const ulong M0 = 0x5555555555555555UL;
            const ulong M1 = 0x0300c0303030c303UL;
            const ulong M2 = 0x00c0300c03f0003fUL;
            const ulong M3 = 0x00000ffc00003fffUL;

            var n = ((value >> 1) & M0) | (value & M0) << 1;
            n = SwapBits(n, M1, 4);
            n = SwapBits(n, M2, 8);
            n = SwapBits(n, M3, 20);
            n = (n >> 34) | (n << 30);

            return n;
        }

        internal static bool HasSingleBitSet(this ulong value)
        {
            return BitOperations.PopCount(value) == 1;
        }

        internal static int LastSignificantBitIndex(this ulong value)
        {
            return 63 - BitOperations.LeadingZeroCount(value);
        }

        private static ulong SwapBits(ulong num, ulong mask, int shift)
        {
            var q = ((num >> shift) ^ num) & mask;
            return num ^ q ^ (q << shift);
        }
    }
}
