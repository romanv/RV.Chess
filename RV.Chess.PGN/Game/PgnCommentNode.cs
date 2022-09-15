namespace RV.Chess.PGN
{
    public class PgnCommentNode : PgnNode
    {
        public PgnCommentNode(string comment)
        {
            Comment = comment;
        }

        public override PgnNodeKind Kind => PgnNodeKind.Comment;

        public string Comment { get; }

        public override string ToString()
        {
            return $"{{ { Comment } }}";
        }
    }
}
