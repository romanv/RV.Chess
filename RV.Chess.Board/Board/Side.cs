namespace RV.Chess.Board
{
    public enum Side
    {
        Black,
        White,
        None,
    }

    public static class SideExtensions
    {
        public static Side Opposite(this Side c) => c == Side.White ? Side.Black : Side.White;
    }
}
