using FluentResults;
using RV.Chess.CBReader.Entities.Annotations;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class AnnotationsReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cba";

        private const int GAME_RECORD_HEADER_SIZE = 14;

        private readonly Dictionary<int, Type> _decoders;
        private readonly Dictionary<int, List<IAnnotation>> _emptyAnnotations = new();

        public AnnotationsReader(string fileName) : base(fileName)
        {
            _decoders = new Dictionary<int, Type>
            {
                { 2, typeof(AfterMoveTextAnnotation) },
                { 3, typeof(SymbolAnnotation) },
            };
        }

        // https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/annotations.md
        internal Result<Dictionary<int, List<IAnnotation>>> GetAnnotations(uint offset)
        {
            if (offset == 0)
            {
                return _emptyAnnotations;
            }

            if (IsError)
            {
                return Result.Fail(ErrorMessage);
            }

            /*
                Ofs 0, Len 3 - The game ID this is the annotation for (big-endian)
                Ofs 3, Len 4 - ? 01 00 0E 0E ?
                Ofs 7, Len 3 - Number of annotations in this game + 1
                Ofs 10, Len 4 - The number of bytes used for annotations in this game, starting at ofs 0 (big-endian)
                Ofs 14 - start of annotation data
            */

            _fs.Seek(offset, SeekOrigin.Begin);

            var recordHeader = _reader.ReadBytes(GAME_RECORD_HEADER_SIZE).AsSpan();
            var count = recordHeader.Slice(7, 3).ToUIntBigEndian() - 1;
            var annotations = new Dictionary<int, List<IAnnotation>>();

            for (var i = 0; i < count; i++)
            {
                var aHeader = _reader.ReadBytes(6).AsSpan();
                var posNo = aHeader[..3].ToIntBigEndian();
                var type = aHeader.Slice(3, 1)[0];
                var length = aHeader.Slice(4, 2).ToUIntBigEndian() - 6;

                if (!annotations.ContainsKey(posNo))
                {
                    annotations[posNo] = new List<IAnnotation>();
                }

                if (_decoders.TryGetValue(type, out var decoderType))
                {
                    annotations[posNo].Add(DecodeAnnotation(decoderType, _reader, length, posNo));
                }
                else
                {
                    _reader.ReadBytes((int)length);
                    annotations[posNo].Add(new UnsupportedAnnotation() { Type = type, Position = posNo });
                }
            }

            return annotations;
        }

        private static IAnnotation DecodeAnnotation(Type annotationType, BinaryReader reader, uint length, int posNo)
        {
            var decoder = Activator.CreateInstance(annotationType);

            if (decoder is IAnnotation ad)
            {
                return ad.Decode(reader, length, posNo);
            }

            return new UnsupportedAnnotation() { Position = posNo };
        }
    }
}
