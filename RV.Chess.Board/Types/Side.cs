namespace RV.Chess.Board.Types
{
    public enum Side
    {
        White = 0,
        Black = 1,
        None = 2,
    }

    public static class SideExtensions
    {
        public static Side Opposite(this Side s) => (Side)((int)s ^ 1);
    }
}
