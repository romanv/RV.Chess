using RV.Chess.Shared.Types;

namespace RV.Chess.PGN;

internal class PgnParserState
{
    private readonly Stack<List<PgnNode>> _moves = [];
    private List<PgnError> _errors = [];
    private Dictionary<string, string> _tags = [];

    public PgnParserState()
    {
        _moves.Push([]);
    }

    public IReadOnlyList<PgnError> Errors => _errors;

    public IReadOnlyDictionary<string, string> Tags => _tags;

    public List<PgnNode> Moves => _moves.Peek();

    public int MoveNo { get; set; } = 1;

    public Side Side { get; set; } = Side.White;

    public void ResetGame()
    {
        _moves.Clear();
        _moves.Push([]);
        _tags = [];
        _errors = [];
        Side = Side.White;
        MoveNo = 1;
    }

    public PgnGame GetGame() => new(_tags, Moves, _errors);

    public void StartVariation() => _moves.Push([]);

    public List<PgnNode> EndVariation() => _moves.Pop();

    public PgnMoveNode? LastMove => Moves.FindLast(m => m is PgnMoveNode) as PgnMoveNode;

    public void AddTag(string key, string value) => _tags[key] = value;

    public void AddNode(PgnNode node) => Moves.Add(node);

    public void AddMove(string san, bool isNullMove)
    {
        Moves.Add(new PgnMoveNode(MoveNo, Side, san, nullMove: isNullMove));
        Side = Side.Opposite();
    }

    public void AddError(PgnErrorType type, string message)
    {
        _errors.Add(new PgnError(type, message));
    }

    public void AddSuffix(ReadOnlySpan<char> suffix)
    {
        if (LastMove == null)
        {
            throw new InvalidOperationException("Can't add the suffix - last node is not a move node");
        }

        LastMove.Annotation = suffix.ToString();
    }
}
