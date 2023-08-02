using RV.Chess.PGN.Tree;

namespace RV.Chess.PGN
{
    public class ChessTreeNode : ChessTreeMove
    {
        public ChessTreeNode? Parent { get; set; }

        public List<ChessTreeNode> Children { get; set; } = new();
    }
}
