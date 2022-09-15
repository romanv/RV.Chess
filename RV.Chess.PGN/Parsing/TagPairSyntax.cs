namespace RV.Chess.PGN
{
    internal class TagPairSyntax : SyntaxNode
    {
        public TagPairSyntax(string key, string value, string text)
            : base(text)
        {
            Key = key;
            Value = value;
        }

        public override SyntaxNodeKind Kind => SyntaxNodeKind.TagPair;

        public string Key { get; }
        public string Value { get; }
    }
}
