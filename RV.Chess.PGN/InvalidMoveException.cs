namespace RV.Chess.PGN;

public class InvalidMoveException(string san, string position, string[] prevMoves) : Exception
{
    public string San { get; set; } = san;

    public string Position { get; set; } = position;

    public string[] PrevMoves { get; set; } = prevMoves;
}
