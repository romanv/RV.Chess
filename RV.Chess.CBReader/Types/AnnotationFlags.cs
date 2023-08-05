namespace RV.Chess.CBReader.Types
{
    [Flags]
    public enum AnnotationFlags
    {
        None = 0,
        StartingPosition = 1,
        Variations = 2,
        Commentary = 4,
        Symbols = 8,
        GraphicalSquares = 32,
        GraphicalArrows = 64,
        TimeSpent = 256,
        Unknown = 512,
        Training = 1024,
        EmbeddedAudio = 131072,
        EmbeddedPicture = 262144,
        EmbeddedVideo = 524288,
        GameQuotation = 1048576,
        PathStructure = 2097152,
        PiecePath = 4194304,
        WhiteClock = 8388608,
        BlackClock = 16777216,
        CriticalPosition = 33554432,
        Correspondence = 67108864,
        MediaAnnotation = 134217728,
        Unorthodox = 268435456,
        WebLink = 536870912,
    }
}
