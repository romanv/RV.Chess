namespace RV.Chess.PGN;

[Serializable]
public class PgnParsingException : Exception
{
    public PgnParsingException(PgnErrorType type)
    {
        Type = type;
    }

    public PgnParsingException(PgnErrorType type, string message) : base(message)
    {
        Type = type;
    }

    public PgnErrorType Type { get; }
}
