namespace RV.Chess.PGN.Utils
{
    public static class Extensions
    {
        internal static char PeekAt(this ReadOnlySpan<char> text, int pos) => pos < text.Length ? text[pos] : '\0';
    }
}
