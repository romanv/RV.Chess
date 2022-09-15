namespace RV.Chess.PGN
{
    internal abstract class SyntaxNode
    {
        public SyntaxNode(string text)
        {
            Text = text;
        }

        public abstract SyntaxNodeKind Kind { get; }

        public string Text { get; }
    }
}
