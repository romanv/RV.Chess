namespace RV.Chess.Board.Utils
{
    internal class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message) : base(message) { }

        public InvalidMoveException(string from, string to, string position) : base()
        {
            From = from;
            To = to;
            Position = position;
        }

        public InvalidMoveException(int from, int to, string position) : base()
        {
            From = Coordinates.IdxToSquare(from);
            To = Coordinates.IdxToSquare(to);
            Position = position;
        }

        public string From { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;
    }
}
