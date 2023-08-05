using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Entities.Annotations
{
    public class AfterMoveTextAnnotation : AnnotationBase
    {
        public TextLanguage Language { get; private set; } = TextLanguage.Unset;

        public string Text { get; private set; } = string.Empty;

        public override AnnotationBase Decode(BinaryReader reader, uint length, int posNo)
        {
            reader.ReadByte();
            var lang = (reader.ReadByte()) switch
            {
                0x2a => TextLanguage.English,
                0x35 => TextLanguage.German,
                0x31 => TextLanguage.French,
                0x2b => TextLanguage.Spanish,
                0x46 => TextLanguage.Italian,
                0x67 => TextLanguage.Dutch,
                0x75 => TextLanguage.Portuguese,
                _ => TextLanguage.Unset,
            };
            var text = reader.ReadBytes((int)length - 2).AsSpan().ToCBString();

            return new AfterMoveTextAnnotation
            {
                Language = lang,
                Text = text,
                Position = posNo,
            };
        }
    }
}
