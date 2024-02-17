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

        public override string ToString() => string.Join(" ", Nags.Select(n => NagToString(n)));

        private static string NagToString(Nag nag)
        {
            var name = Enum.GetName(typeof(Nag), nag);

            if (!string.IsNullOrEmpty(name))
            {
                var attr = nag.GetType()
                    .GetField(name)
                    ?.GetCustomAttributes(false)
                    .OfType<NagTypeAttribute>()
                    .SingleOrDefault();

                if (attr != null)
                {
                    return attr.StringValue;
                }
            }

            return string.Empty;
        }
    }
}
