using FluentResults;
using RV.Chess.PGN.Utils;

namespace RV.Chess.PGN.Tree
{
    public static class ChessTreeExtensions
    {
        public delegate T ChessTreeNodeBuilder<T>(PgnMoveNode pgnMove, T? parent, int id) where T : ChessTreeNode;

        private static ChessTreeNode DefaultNodeBuilder(
            PgnMoveNode pgnMove,
            ChessTreeNode? parent,
            int id)
        {
            return new ChessTreeNode
            {
                Annotation = pgnMove.Annotation,
                Children = new List<ChessTreeNode>(),
                Id = id,
                IsNullMove = pgnMove.NullMove,
                MoveNumber = pgnMove.MoveNumber,
                Parent = parent,
                San = pgnMove.San,
                Side = pgnMove.Side,
            };
        }

        public static List<ChessTreeNode> ToTree(this PgnGame game)
        {
            return ToTree<ChessTreeNode>(new List<PgnGame> { game }, DefaultNodeBuilder);
        }

        public static List<ChessTreeNode> ToTree(this IEnumerable<PgnGame> games)
        {
            return ToTree<ChessTreeNode>(games, DefaultNodeBuilder);
        }

        public static List<ChessTreeNode> ToTree(this IEnumerable<Result<PgnGame>> games)
        {
            return ToTree<ChessTreeNode>(games.Where(r => r.IsSuccess).Select(r => r.Value), DefaultNodeBuilder);
        }

        public static List<T> ToTree<T>(
            this IEnumerable<Result<PgnGame>> games,
            ChessTreeNodeBuilder<T> builder) where T : ChessTreeNode
        {
            return ToTree(games.Where(r => r.IsSuccess).Select(r => r.Value), builder);
        }

        public static List<T> ToTree<T>(
            this IEnumerable<PgnGame> games,
            ChessTreeNodeBuilder<T> builder) where T : ChessTreeNode
        {
            var rootNodes = new List<T>();
            var id = 0;

            foreach (var game in games)
            {
                var (lastId, newBranchRoot) = BuildBranch(game.Moves, builder, id, null);

                if (newBranchRoot != null)
                {
                    MergeInto(rootNodes, new List<T> { newBranchRoot });
                }

                id = lastId + 1;
            }

            if (rootNodes == null)
            {
                throw new InvalidDataException("Tree is empty");
            }

            return rootNodes;
        }

        private static (int, T?) BuildBranch<T>(
            List<PgnNode> moves,
            ChessTreeNodeBuilder<T> builder,
            int startingId,
            T? prev) where T : ChessTreeNode
        {
            var id = startingId + 1;
            T? root = null;
            T? curr = prev;

            foreach (var move in moves)
            {
                if (move is PgnMoveNode mn)
                {
                    var node = builder(mn, curr, id);
                    node.Parent = curr;
                    curr?.Children.Add(node);
                    root ??= node;
                    curr = node;
                    id++;
                }
                else if (move is PgnCommentNode cn)
                {
                    // do not need to check for comment doubles because in this function we go one game at a time
                    // doubles would be checked during the merging process in the calling function
                    curr?.Comments.Add(cn.Comment);
                }
                else if (move is PgnVariationNode vn)
                {
                    var (lastId, branchRoot) = BuildBranch(vn.Moves, builder, id, curr);

                    if (branchRoot != null)
                    {
                        curr?.Parent?.Children.Add(branchRoot);
                        id = lastId + 1;
                    }
                }
            }

            return (id, root);
        }

        private static void MergeInto<T>(List<T> target, List<T> branches) where T : ChessTreeNode
        {
            foreach (var newRoot in branches)
            {
                var currNewBranchNode = newRoot;
                var sameMove = target.FirstOrDefault(m => m.San == newRoot.San);

                if (sameMove != null)
                {
                    if (sameMove.MoveNumber != newRoot.MoveNumber)
                    {
                        throw new InvalidDataException(
                            $"Move number mismatch ({sameMove.MoveNumber} / {currNewBranchNode.MoveNumber})");
                    }

                    if (currNewBranchNode.Comments.Any())
                    {
                        foreach (var c in currNewBranchNode.Comments)
                        {
                            sameMove.Comments = CommentMerger.MergeInto(sameMove.Comments, c);
                        }
                    }

                    MergeInto(sameMove.Children, currNewBranchNode.Children);
                }
                else
                {
                    target.Add(currNewBranchNode);
                }
            }
        }
    }
}
