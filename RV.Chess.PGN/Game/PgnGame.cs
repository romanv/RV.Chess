namespace RV.Chess.PGN
{
    public class PgnGame : PgnMovetextNode
    {
        public static PgnGame FromString(string pgn)
        {
            var lexer = new Lexer(pgn);
            var tokens = lexer.ParseTokens();
            var parser = new Parser(pgn, tokens);
            var nodes = parser.Parse();

            var tags = nodes.Where(n => n is TagPairSyntax)
                .ToDictionary(n => ((TagPairSyntax)n).Key, n => ((TagPairSyntax)n).Value);
            var moves = ParseMoveSection(nodes.Where(n => n is not TagPairSyntax));

            if (!tags.Any() || !moves.Any())
            {
                throw new InvalidDataException("Pgn contains no tags or moves\n" + pgn);
            }

            if (moves.Last() is not PgnTerminatorNode)
            {
                throw new InvalidDataException("Wrong game terminator\n" + pgn);
            }

            return new PgnGame(tags, moves);
        }

        public override PgnNodeKind Kind => PgnNodeKind.Game;

        public Dictionary<string, string> Tags { get; private set; } = new Dictionary<string, string>();

        private PgnGame(Dictionary<string, string> tags, List<PgnNode> moves) : base(moves)
        {
            Tags = tags;
        }

        private static List<PgnNode> ParseMoveSection(IEnumerable<SyntaxNode> nodes,
            int startingMoveNumber = 1,
            Side startingSide = Side.White)
        {
            var result = new List<PgnNode>();
            var currentMoveNumber = startingMoveNumber;
            var currentSideToMove = startingSide;

            foreach (var node in nodes)
            {
                switch (node)
                {
                    case MoveNumberSyntax num:
                        currentMoveNumber = num.Number;
                        break;
                    case SANSyntax san:
                        result.Add(new PgnMoveNode(currentMoveNumber, currentSideToMove, san.Value, san.Annotation));
                        currentSideToMove = currentSideToMove == Side.White ? Side.Black : Side.White;
                        break;
                    case RAVSyntax rav:
                        var startAtSide = currentSideToMove == Side.White ? Side.Black : Side.White;
                        var items = ParseMoveSection(rav.Nodes, currentMoveNumber, startAtSide);
                        result.Add(new PgnVariationNode(items));
                        break;
                    case GameTerminatorSyntax t:
                        result.Add(new PgnTerminatorNode(t.Result));
                        break;
                    case CommentSyntax c:
                        result.Add(new PgnCommentNode(c.Value));
                        break;
                    default:
                        throw new InvalidDataException($"Invalid node type: {node.GetType()}");
                }
            }

            return result;
        }
    }
}
