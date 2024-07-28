using RV.Chess.Shared.Types;

namespace RV.Chess.PGN;

public class PgnMoveNode(
    int moveNumber,
    Side side,
    string san,
    string annotation = "",
    bool nullMove = false) : PgnNode
{
    public int MoveNumber { get; } = moveNumber;

    public Side Side { get; } = side;

    public string San { get; internal set; } = san;

    public bool NullMove { get; internal set; } = nullMove;

    public string Annotation { get; internal set; } = annotation;

    public override PgnNodeKind Kind => PgnNodeKind.Move;

    public override string ToString()
    {
        return $"{MoveNumber}{(Side == Side.White ? "." : "...")}{San}{Annotation}";
    }
}
