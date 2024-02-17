namespace RV.Chess.CBReader.Types
{
    [Flags]
    public enum AnnotationMagnitude
    {
        None = 0,
        Variations50 = 1,
        Variations300 = 2,
        Variations1000 = 4,
        Commentaries = 8,
        Symbols = 16,
        Squares = 32,
        Arrows = 64,
        TimeSpent = 128,
        Annotations = 256,
    }
}
