using RV.Chess.PGN.Utils;

namespace RV.Chess.PGN.Tree;

public static class CompactChessTreeExtensions
{
    public delegate T ChessTreeMoveBuilder<T>(PgnMoveNode pgnMove, T? prev, int id)
        where T : ChessTreeMove;

    private static ChessTreeMove DefaultMoveBuilder(PgnMoveNode pgnMove, ChessTreeMove? prev, int id)
    {
        return new ChessTreeMove
        {
            Annotation = pgnMove.Annotation,
            Id = id,
            IsNullMove = pgnMove.NullMove,
            MoveNumber = pgnMove.MoveNumber,
            San = pgnMove.San,
            Side = pgnMove.Side,
        };
    }

    public static CompactChessTreeNode<ChessTreeMove> ToCompactTree(this PgnGame game)
    {
        return ToCompactTree<ChessTreeMove>([game], DefaultMoveBuilder);
    }

    public static CompactChessTreeNode<T> ToCompactTree<T>(
        this PgnGame game,
        ChessTreeMoveBuilder<T> builder) where T : ChessTreeMove
    {
        return ToCompactTree([game], builder);
    }

    public static CompactChessTreeNode<ChessTreeMove> ToCompactTree(this IEnumerable<PgnGame> games)
    {
        return ToCompactTree<ChessTreeMove>(games, DefaultMoveBuilder);
    }

    public static CompactChessTreeNode<T> ToCompactTree<T>(
        this IEnumerable<PgnGame> games,
        ChessTreeMoveBuilder<T> builder) where T : ChessTreeMove
    {
        var root = new CompactChessTreeNode<T>();
        var id = 0;

        foreach (var game in games)
        {
            var (lastId, newBranches) = BuildBranches(game.Moves, builder, id, null);

            foreach (var branch in newBranches)
            {
                MergeInto(root, branch);
            }

            id = lastId + 1;
        }

        if (root.Moves.Count == 0 && root.Next.Count == 0)
        {
            throw new InvalidDataException("Tree is empty");
        }

        return root;
    }

    private static void MergeInto<T>(
        CompactChessTreeNode<T> root,
        IList<T> branch) where T : ChessTreeMove
    {
        if (root.Moves.Count == 0 && root.Next.Count == 0)
        {
            root.Moves = [.. branch];
            return;
        }

        var i = 0;

        while (i < root.Moves.Count && i < branch.Count && root.Moves[i] == branch[i])
        {
            if (branch[i].Comments.Count != 0)
            {
                foreach (var c in branch[i].Comments)
                {
                    root.Moves[i].Comments = CommentMerger.MergeInto(root.Moves[i].Comments, c);
                }
            }

            i++;
        }

        if (i < root.Moves.Count && i < branch.Count)
        {
            // both branches still have moves in them, so we need to create a fork
            var newVariationFromRoot = new CompactChessTreeNode<T>
            {
                Moves = root.Moves.Skip(i).ToList(),
                Next = root.Next,
            };

            var newVariationFromBranch = new CompactChessTreeNode<T>
            {
                Moves = branch.Skip(i).ToList(),
            };

            root.Next =
            [
                newVariationFromRoot,
                newVariationFromBranch,
            ];

            root.Moves.RemoveRange(i, root.Moves.Count - i);
        }
        else if (i < branch.Count)
        {
            // root moves are exhausted, but there are still moves in the branch
            // we have to check them against all existing branches to see if there is a match
            var matchingBranch = root.Next.Find(n => n.Moves.FirstOrDefault() == branch[i]);

            if (matchingBranch != null)
            {
                MergeInto(matchingBranch, branch.Skip(i).ToList());
            }
            else
            {
                root.Next.Add(new CompactChessTreeNode<T>
                {
                    Moves = branch.Skip(i).ToList(),
                });
            }
        }
    }

    private static (int, List<List<T>>) BuildBranches<T>(
        List<PgnNode> moves,
        ChessTreeMoveBuilder<T> builder,
        int startingId,
        T? prev) where T : ChessTreeMove
    {
        var id = startingId + 1;
        var result = new List<List<T>>() { new() };

        foreach (var move in moves)
        {
            if (move is PgnMoveNode mn)
            {
                var prevNew = result[^1].Count != 0 ? result[^1][^1] : prev;
                result[^1].Add(builder(mn, prevNew, id));
                id++;
            }
            else if (move is PgnCommentNode cn)
            {
                // do not need to check for comment doubles because in this function we go one game at a time
                // doubles would be checked during the merging process in the calling function
                if (result.Count != 0 && result[^1].Count != 0)
                {
                    result[^1][^1].Comments.Add(cn.Comment);
                }
            }
            else if (move is PgnVariationNode vn)
            {
                var prevNew = result[^1].Count > 1 ? result[^1][^2] : prev;
                var (lastId, subBranchRoots) = BuildBranches(vn.Moves, builder, id, prevNew);

                foreach (var sbr in subBranchRoots)
                {
                    // for each new sub-branch we duplicate whole sequence of moves to create parallel branches
                    // original branch before the variation had started should always become the last in the list
                    // so the main line continuation moves could be simply added to it without tracking its index
                    var newBranch = new List<T>(result[^1].Take(result[^1].Count - 1));
                    newBranch.AddRange(sbr);
                    result.Add(newBranch);
                    (result[^2], result[^1]) = (result[^1], result[^2]);
                    id = lastId + 1;
                }
            }
        }

        return (id, result);
    }
}
