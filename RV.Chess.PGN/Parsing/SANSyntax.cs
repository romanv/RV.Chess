namespace RV.Chess.PGN
{
    internal class SANSyntax : SyntaxNode
    {
        public SANSyntax(string text, string value, string annotation = "") : base(text)
        {
            Value = value;
            Annotation = annotation;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.SANSyntax;

        public string Value { get; }

        public string Annotation { get; }
    }
}
