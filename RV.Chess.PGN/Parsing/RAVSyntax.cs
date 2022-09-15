using System.Collections.Immutable;

namespace RV.Chess.PGN
{
    internal class RAVSyntax : SyntaxNode
    {
        public RAVSyntax(string text, IEnumerable<SyntaxNode> nodes) : base(text)
        {
            Nodes = nodes.ToImmutableArray();
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.RAVSyntax;

        public ImmutableArray<SyntaxNode> Nodes { get; }
    }
}
