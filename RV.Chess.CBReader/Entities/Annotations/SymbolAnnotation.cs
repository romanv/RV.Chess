namespace RV.Chess.CBReader.Entities.Annotations
{
    public class SymbolAnnotation : AnnotationBase
    {
        public Nag[] Nags { get; private set; } = Array.Empty<Nag>();

        public override AnnotationBase Decode(BinaryReader reader, uint length, int posNo)
        {
            var nags = new List<Nag>((int)length);

            for (var i = 0; i < length; i++)
            {
                var s = (int)reader.ReadByte();

                if (s > 0 && Enum.IsDefined(typeof(Nag), s))
                {
                    nags.Add((Nag)s);
                }
            }

            return new SymbolAnnotation()
            {
                Nags = nags.ToArray(),
                Position = posNo,
            };
        }
    }
}
