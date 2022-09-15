namespace RV.Chess.PGN
{
    public class PgnVariationNode : PgnMovetextNode
    {
        public PgnVariationNode(IEnumerable<PgnNode> moves) : base(moves)
        {
        }

        public override PgnNodeKind Kind => PgnNodeKind.Variation;

        public override string ToString()
        {
            return "(" + string.Join(' ', Moves.Select(m => m.ToString())) + ")";
        }
    }
}
