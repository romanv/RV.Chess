using RV.Chess.Shared.Types;

namespace RV.Chess.PGN
{
    public class PgnMoveNode : PgnNode
    {
        public PgnMoveNode(int moveNumber, Side side, string san, string annotation = "", bool nullMove = false)
        {
            MoveNumber = moveNumber;
            Side = side;
            San = san;
            Annotation = annotation;
            NullMove = nullMove;
        }

        public int MoveNumber { get; }

        public Side Side { get; }

        public string San { get; }

        public bool NullMove { get; set; }

        public string Annotation { get; }

        public override PgnNodeKind Kind => PgnNodeKind.Move;

        public override string ToString()
        {
            return $"{MoveNumber}{(Side == Side.White ? "." : "...")}{San}{Annotation}";
        }
    }
}
