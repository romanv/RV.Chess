namespace RV.Chess.CBReader.Entities.Annotations
{
    public interface IAnnotation
    {
        public int Position { get; }

        public AnnotationBase Decode(BinaryReader reader, uint length, int posNo);
    }
}
