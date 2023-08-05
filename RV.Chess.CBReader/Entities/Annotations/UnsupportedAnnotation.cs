namespace RV.Chess.CBReader.Entities.Annotations
{
    public class UnsupportedAnnotation : AnnotationBase
    {
        public int Type { get; init; }

        public override AnnotationBase Decode(BinaryReader reader, uint length, int posNo)
            => throw new NotImplementedException();
    }
}
