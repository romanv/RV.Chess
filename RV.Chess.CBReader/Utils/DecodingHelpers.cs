using System.Buffers.Binary;
using System.Text;

namespace RV.Chess.CBReader.Utils
{
    internal static class DecodingHelpers
    {
        internal static Encoding _encoding = Encoding.GetEncoding("ISO-8859-1");

        internal static uint ToUIntBigEndian(this Span<byte> span)
        {
            return span.Length switch
            {
                1 => span[0],
                2 => BinaryPrimitives.ReadUInt16BigEndian(span),
                3 => (uint)(span[0] << 16 | span[1] << 8 | span[2]),
                _ => BinaryPrimitives.ReadUInt32BigEndian(span),
            };
        }

        internal static int ToIntBigEndian(this Span<byte> span)
        {
            return span.Length switch
            {
                1 => span[0],
                2 => BinaryPrimitives.ReadInt16BigEndian(span),
                3 => span[0] << 16 | span[1] << 8 | span[2],
                _ => BinaryPrimitives.ReadInt32BigEndian(span),
            };
        }

        internal static uint ToUIntLittleEndian(this Span<byte> span)
        {
            return span.Length switch
            {
                1 => span[0],
                2 => BinaryPrimitives.ReadUInt16LittleEndian(span),
                3 => (uint)(span[2] << 16 | span[1] << 8 | span[0]),
                _ => BinaryPrimitives.ReadUInt32LittleEndian(span),
            };
        }

        internal static int ToIntLittleEndian(this Span<byte> span)
        {
            return span.Length switch
            {
                1 => span[0],
                2 => BinaryPrimitives.ReadInt16LittleEndian(span),
                3 => span[2] << 16 | span[1] << 8 | span[0],
                _ => BinaryPrimitives.ReadInt32LittleEndian(span),
            };
        }

        internal static DateOnly ToDate(this Span<byte> span)
        {
            var b = (int)span.ToUIntBigEndian();
            var d = Math.Max(1, b & 0b11111);
            var m = Math.Max(1, (b >> 5) & 0b1111);
            var y = Math.Max(1, b >> 9);

            return new DateOnly(y, m, d);
        }

        internal static string ToCBString(this Span<byte> bytes)
        {
            return _encoding.GetString(bytes);
        }

        internal static string ToCBZeroTerminatedString(this Span<byte> bytes)
        {
            var idx = bytes.IndexOf<byte>(0b0);
            var nullIdx = idx == -1 ? bytes.Length : idx;
            return _encoding.GetString(bytes[..nullIdx]);
        }

        internal static byte RotatePartLeft(this byte value, int bitCount, int shift)
        {
            var mask = 0b11111111 >> (8 - bitCount);
            return (byte)((value << shift | value >> (bitCount - shift)) & mask);
        }
    }
}
