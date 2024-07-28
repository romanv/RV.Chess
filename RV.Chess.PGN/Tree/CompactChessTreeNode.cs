namespace RV.Chess.PGN.Tree;

public class CompactChessTreeNode<T> where T : ChessTreeMove
{
    public List<T> Moves { get; set; } = [];

    public List<CompactChessTreeNode<T>> Next { get; set; } = [];

    public override string ToString()
    {
        return $"{string.Join(' ', Moves.Take(3).Select(m => m.ToString()))} -";
    }
}
