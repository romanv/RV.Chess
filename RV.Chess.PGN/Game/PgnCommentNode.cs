namespace RV.Chess.PGN;

public class PgnCommentNode(string comment) : PgnNode
{
    public override PgnNodeKind Kind => PgnNodeKind.Comment;

    public string Comment { get; } = comment;

    public override string ToString()
    {
        return $"{{ {Comment} }}";
    }
}
