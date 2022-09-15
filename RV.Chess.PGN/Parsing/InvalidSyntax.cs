namespace RV.Chess.PGN
{
    internal class InvalidSyntax : SyntaxNode
    {
        public InvalidSyntax() : base(string.Empty)
        {
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.Invalid;
    }
}
