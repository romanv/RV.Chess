using System.Text;
using RV.Chess.Shared.Types;

namespace RV.Chess.PGN
{
    public abstract class PgnMovetextNode : PgnNode
    {
        protected PgnMovetextNode(IEnumerable<PgnNode> moves)
        {
            Moves = moves.ToList();
        }

        public List<PgnNode> Moves { get; private set; } = new List<PgnNode>();

        public string Movetext
        {
            get
            {
                var sb = new StringBuilder();

                for (var i = 0; i < Moves.Count; i++)
                {
                    if (Moves[i] is PgnMoveNode m)
                    {
                        // print number for all white's moves, first move in the game / variation and first move after the variation
                        var shouldPrintNumber = i == 0 || m.Side == Side.White || i > 0 && Moves[i - 1] is PgnVariationNode;

                        if (shouldPrintNumber)
                        {
                            sb.Append(m.MoveNumber);
                            sb.Append(m.Side == Side.White ? "." : "...");
                        }

                        sb.Append(m.San);
                    }
                    else if (Moves[i] is PgnCommentNode c)
                    {
                        sb.Append(c.ToString());
                    }
                    else if (Moves[i] is PgnTerminatorNode t)
                    {
                        sb.Append(t.ToString());
                    }
                    else if (Moves[i] is PgnVariationNode v)
                    {
                        sb.Append($"({ v.Movetext })");
                    }

                    if (i < Moves.Count - 1)
                    {
                        sb.Append(' ');
                    }
                }

                return sb.ToString();
            }
        }
    }
}
