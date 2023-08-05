namespace RV.Chess.CBReader.Entities.Annotations
{
    public abstract class AnnotationBase : IAnnotation
    {
        public required int Position { get; init; }

        public abstract AnnotationBase Decode(BinaryReader reader, uint length, int posNo);
    }
}
