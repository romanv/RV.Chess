namespace RV.Chess.CBReader.Types
{
    [Flags]
    public enum AnnotationMagnitude
    {
        None,
        Variations50,
        Variations300,
        Variations1000,
        Commentaries,
        Symbols,
        Squares,
        Arrows,
        TimeSpent,
        Annotations,
    }
}
