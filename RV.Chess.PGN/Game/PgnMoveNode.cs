namespace RV.Chess.PGN
{
    public class PgnMoveNode : PgnNode
    {
        public PgnMoveNode(int moveNumber, Side side, string san, string annotation = "")
        {
            MoveNumber = moveNumber;
            Side = side;
            San = san;
            Annotation = annotation;
        }

        public int MoveNumber { get; }

        public Side Side { get; }

        public string San { get; }

        public string Annotation { get; }

        public override PgnNodeKind Kind => PgnNodeKind.Move;

        public override string ToString()
        {
            return $"{MoveNumber}{(Side == Side.White ? ". " : "... ")}{San}{Annotation}";
        }
    }
}
