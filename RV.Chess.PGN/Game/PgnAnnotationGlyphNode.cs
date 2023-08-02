namespace RV.Chess.PGN
{
    public class PgnAnnotationGlyphNode : PgnNode
    {
        public PgnAnnotationGlyphNode(string nag)
        {
            NAG = nag;
        }

        public override PgnNodeKind Kind => PgnNodeKind.NAG;

        public string NAG { get; }

        public override string ToString()
        {
            return NAG;
        }
    }
}
