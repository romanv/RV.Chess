namespace RV.Chess.PGN
{
    internal class MoveNumberSyntax : SyntaxNode
    {
        public MoveNumberSyntax(int number, string text)
            : base(text)
        {
            Number = number;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.MoveNumber;

        public int Number { get; }
    }
}
