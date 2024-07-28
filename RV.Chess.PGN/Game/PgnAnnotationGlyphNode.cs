namespace RV.Chess.PGN;

public class PgnAnnotationGlyphNode(string nag) : PgnNode
{
    public override PgnNodeKind Kind => PgnNodeKind.NAG;

    public string NAG { get; } = nag;

    public override string ToString()
    {
        return NAG;
    }
}
