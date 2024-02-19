using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class TournamentReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbt";

        internal TournamentReader(string dbPath) : base(dbPath)
        {
        }

        const int FILE_HEADER_SIZE = 32;
        const int RECORD_SIZE = 90;
        const int RECORD_METADATA_SIZE = 9;

        internal IEnumerable<Result<TournamentRecord>> GetAll()
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            var tournamentId = 0;

            _fs.Seek(FILE_HEADER_SIZE, SeekOrigin.Begin);
            while (_reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                Result<TournamentRecord> result;

                try
                {
                    var record = _reader.ReadBytes(RECORD_METADATA_SIZE + RECORD_SIZE).AsSpan();
                    result = new TournamentRecord(tournamentId,
                        record.Slice(RECORD_METADATA_SIZE, 40).ToCBZeroTerminatedString(),
                        record.Slice(RECORD_METADATA_SIZE + 40, 30).ToCBZeroTerminatedString(),
                        record.Slice(RECORD_METADATA_SIZE + 70, 3).ToDateFromLittleEndian());
                    tournamentId++;
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
