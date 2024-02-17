using System.Text;
using RV.Chess.Shared.Types;

namespace RV.Chess.PGN
{
    public class PgnGame : PgnMovetextNode
    {
        public PgnGame(IEnumerable<PgnNode> moves) : base(moves)
        {
        }

        public PgnGame(Dictionary<string, string> tags, List<PgnNode> moves) : base(moves)
        {
            Tags = tags;
        }

        public override PgnNodeKind Kind => PgnNodeKind.Game;

        public Dictionary<string, string> Tags { get; private set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var tag in Tags)
            {
                sb.Append($"[{tag.Key} \"{tag.Value}\"]");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine);
            PgnNode? prv = null;

            foreach (var move in Moves)
            {
                if (move is PgnMoveNode mn)
                {
                    if (prv is PgnMoveNode prvMoveNode && prvMoveNode.Side == Side.White)
                    {
                        sb.Append(mn.San);
                    }
                    else
                    {
                        sb.Append(mn.ToString());
                    }
                }
                else
                {
                    sb.Append(move.ToString());
                }

                sb.Append(' ');
                prv = move;
            }

            return sb.ToString().Trim();
        }
    }
}
