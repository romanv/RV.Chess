namespace RV.Chess.PGN
{
    internal class Token
    {
        internal Token(string text, TokenKind kind, int start, int end, object? value)
        {
            Kind = kind;
            Start = start;
            End = end;
            Value = value;
            Text = text;
        }

        internal TokenKind Kind { get; }
        public int Start { get; }
        public int End { get; }
        internal object? Value { get; }
        internal string Text { get; }
    }
}
