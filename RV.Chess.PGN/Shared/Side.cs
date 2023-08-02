namespace RV.Chess.PGN
{
    public enum Side
    {
        White,
        Black,
    }

    public static class SideExtensions
    {
        public static Side Opposite(this Side s) => s == Side.White ? Side.Black : Side.White;
    }
}
