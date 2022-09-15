namespace RV.Chess.PGN
{
    internal class CommentSyntax : SyntaxNode
    {
        public enum CommentType
        {
            RestOfLine,
            Brace,
        }

        public CommentSyntax(string text, string value, CommentType type) : base(text)
        {
            Value = value;
            CommentaryType = type;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.Comment;

        public string Value { get; }

        public CommentType CommentaryType { get; }
    }
}
