using System.Text;
using RV.Chess.Shared.Types;

namespace RV.Chess.PGN;

public class PgnGame : PgnMovetextNode
{
    private readonly List<PgnError> _errors = [];

    public PgnGame(IEnumerable<PgnNode> moves) : base(moves)
    {
    }

    public PgnGame(Dictionary<string, string> tags, List<PgnNode> moves) : base(moves)
    {
        Tags = tags;
    }

    public PgnGame(Dictionary<string, string> tags, List<PgnNode> moves, List<PgnError> errors) : base(moves)
    {
        Tags = tags;
        _errors = errors;
    }

    public override PgnNodeKind Kind => PgnNodeKind.Game;

    public IReadOnlyList<PgnError> Errors => _errors;

    public Dictionary<string, string> Tags { get; private set; } = [];

    public bool IsSuccess => _errors.Count == 0;

    public bool IsFailed => _errors.Count != 0;

    public PgnGame Value => this;

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var tag in Tags)
        {
            sb.Append($"[{tag.Key} \"{tag.Value}\"]");
            sb.Append(Environment.NewLine);
        }

        sb.Append(Environment.NewLine);
        PgnNode? prv = null;

        foreach (var move in Moves)
        {
            if (move is PgnMoveNode mn)
            {
                if (prv is PgnMoveNode prvMoveNode && prvMoveNode.Side == Side.White)
                {
                    sb.Append(mn.San);
                }
                else
                {
                    sb.Append(mn.ToString());
                }
            }
            else
            {
                sb.Append(move.ToString());
            }

            sb.Append(' ');
            prv = move;
        }

        return sb.ToString().Trim();
    }

    internal void AddError(PgnErrorType type, string message)
    {
        _errors.Add(new PgnError(type, message));
    }
}
