namespace RV.Chess.PGN
{
    public class PgnTerminatorNode : PgnNode
    {
        public PgnTerminatorNode(GameResult terminator)
        {
            Terminator = terminator;
        }

        public GameResult Terminator { get; }

        public override PgnNodeKind Kind => PgnNodeKind.Terminator;

        public override string ToString()
        {
            return Terminator switch
            {
                GameResult.White => "1-0",
                GameResult.Black => "0-1",
                GameResult.Tie => "1/2-1/2",
                _ => "*",
            };
        }
    }
}
