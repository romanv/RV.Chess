using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class AnnotatorsReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbc";

        internal AnnotatorsReader(string dbPath) : base(dbPath)
        {
        }

        const int FILE_HEADER_SIZE = 32;
        const int RECORD_SIZE = 53;
        const int RECORD_METADATA_SIZE = 9;

        internal IEnumerable<Result<AnnotatorRecord>> GetAll()
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            var annotatorId = 0;

            _fs.Seek(FILE_HEADER_SIZE, SeekOrigin.Begin);
            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                Result<AnnotatorRecord> result;

                try
                {
                    var record = _reader.ReadBytes(RECORD_METADATA_SIZE + RECORD_SIZE).AsSpan();
                    result = new AnnotatorRecord(annotatorId,
                        record.Slice(RECORD_METADATA_SIZE, 45).ToCBZeroTerminatedString(),
                        record.Slice(RECORD_METADATA_SIZE + 45, 4).ToIntLittleEndian());
                    annotatorId++;
                }
                catch (Exception ex)
                {
                    result = Result.Fail(ex.Message);
                }

                yield return result;
            }
        }
    }
}
