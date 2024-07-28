namespace RV.Chess.PGN;

public record PgnError
{
    public PgnError(PgnErrorType type, string message)
    {
        Type = type;
        Message = message;
    }

    public PgnErrorType Type { get; }

    public string Message { get; }
}
