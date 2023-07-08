namespace RV.Chess.Board
{
    public enum Side
    {
        White = 0,
        Black = 1,
        None = 2,
    }

    public static class SideExtensions
    {
        public static Side Opposite(this Side c) => c == Side.White ? Side.Black : Side.White;
    }
}
