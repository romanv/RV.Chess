namespace RV.Chess.PGN
{
    public class PgnTreeNode
    {
        public int Id { get; set; }

        public int MoveNumber { get; set; }

        public Side Side { get; set; }

        public string San { get; set; } = string.Empty;

        public string Annotation { get; set; } = string.Empty;

        public List<string> Comments { get; set; } = new();

        public PgnTreeNode? Parent { get; set; }

        public List<PgnTreeNode> Children { get; set; } = new();
    }
}
