namespace RV.Chess.PGN;

public class PgnVariationNode(IEnumerable<PgnNode> moves) : PgnMovetextNode(moves)
{
    public override PgnNodeKind Kind => PgnNodeKind.Variation;

    public override string ToString()
    {
        return $"({string.Join(' ', Moves.Select(m => m.ToString()))})";
    }
}
