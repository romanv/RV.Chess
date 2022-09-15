namespace RV.Chess.PGN
{
    public class InvalidPgnException : Exception
    {
        public InvalidPgnException(string error, int position, string text)
        {
            Error = error;
            Position = position;
            Text = text;
        }

        public string Error { get; }

        public int Position { get; }

        public string Text { get; }

        public string ErrorSpan
        {
            get
            {
                return Position + 16 < Text.Length
                    ? Text.Substring(Position, 16)
                    : Text[Position..];
            }
        }
    }
}
