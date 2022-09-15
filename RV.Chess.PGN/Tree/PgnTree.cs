using System.Text;

namespace RV.Chess.PGN
{
    public class PgnTree
    {
        public PgnTree(IEnumerable<PgnGame> games)
        {
            var id = 0;

            foreach (var game in games)
            {
                PgnTreeNode? currRoot = null;

                foreach (var node in game.Moves)
                {
                    if (node is PgnMoveNode move)
                    {
                        var existingNode = currRoot != null
                            ? currRoot.Children.SingleOrDefault(n => n.San == move.San)
                            : RootNodes.SingleOrDefault(n => n.San == move.San);

                        if (existingNode != null)
                        {
                            // go down to the next level
                            if (move.MoveNumber != existingNode.MoveNumber)
                            {
                                throw new InvalidDataException($"Move number mismatch ({move.MoveNumber} / {existingNode.MoveNumber})");
                            }

                            currRoot = existingNode;
                        }
                        else
                        {
                            // create new branch
                            var leaf = new PgnTreeNode
                            {
                                San = move.San,
                                Children = new List<PgnTreeNode>(),
                                Annotation = move.Annotation,
                                MoveNumber = move.MoveNumber,
                                Parent = currRoot,
                                Side = move.Side,
                                Id = id,
                            };

                            id++;
                            var siblingsList = currRoot?.Children ?? RootNodes;
                            siblingsList.Add(leaf);
                            currRoot = leaf;
                        }
                    }
                    else if (node is PgnCommentNode cn && currRoot != null)
                    {
                        if (currRoot.Comments.Any())
                        {
                            currRoot.Comments = MergeComments(currRoot.Comments, cn.Comment);
                        }
                        else
                        {
                            currRoot.Comments.Add(cn.Comment);
                        }

                    }
                }

                currRoot = null;
            }
        }

        private static List<string> MergeComments(List<string> existingComments, string newComment)
        {
            var merged = new List<string>(existingComments);

            var useComment = true;

            for (var i = 0; i < merged.Count; i++)
            {
                if (string.Equals(newComment, merged[i]))
                {
                    useComment = false;
                    break;
                }

                var (commentToExisting, existingToComment) = GetPairwiseStringSimilarity(newComment, merged[i]);

                if (commentToExisting > 0.8 && existingToComment < 0.8)
                {
                    // comment is a substring of existing one, just ignore it
                    useComment = false;
                    break;
                }
                else if (existingToComment > 0.8 && commentToExisting < 0.8)
                {
                    // existing one is a substring of the comment, replace it
                    merged[i] = newComment;
                    useComment = false;
                    break;
                }
                else if (existingToComment > 0.8 && commentToExisting > 0.8)
                {
                    useComment = false;
                }
            }

            if (useComment)
            {
                merged.Add(newComment);
            }

            return merged;
        }

        private static (float, float) GetPairwiseStringSimilarity(string sA, string sB)
        {
            if (string.IsNullOrEmpty(sA) || string.IsNullOrEmpty(sB))
            {
                return (0, 0);
            }

            var wordsA = PreprocessCommentString(sA);
            var wordsB = PreprocessCommentString(sB);
            var similarAtoB = wordsA.Where(w => wordsB.Contains(w)).Count();
            var similarBtoA = wordsB.Where(w => wordsA.Contains(w)).Count();

            return (
                (float)similarAtoB / wordsA.Count,
                (float)similarBtoA / wordsB.Count
            );
        }

        private static List<string> PreprocessCommentString(string comment)
        {
            var words = new List<string>();
            var word = new StringBuilder();

            foreach (var c in comment)
            {
                if (char.IsLetterOrDigit(c))
                {
                    word.Append(c);
                }
                else if (word.Length > 0)
                {
                    words.Add(word.ToString());
                    word.Clear();
                }
            }

            return words;
        }

        public List<PgnTreeNode> RootNodes { get; set; } = new();
    }
}
