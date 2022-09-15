namespace RV.Chess.PGN
{
    internal partial class GameTerminatorSyntax : SyntaxNode
    {
        public GameTerminatorSyntax(string text, GameResult result) : base(text)
        {
            Result = result;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.GameResultSyntax;

        public GameResult Result { get; }
    }
}
