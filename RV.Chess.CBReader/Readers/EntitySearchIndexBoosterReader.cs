using System.Buffers.Binary;
using FluentResults;

namespace RV.Chess.CBReader.Readers
{
    internal class EntitySearchIndexBoosterReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cit";

        const int FILE_HEADER_SIZE = 12;
        const int RECORD_SIZE = 40;
        private readonly GameIdsBoosterReader _gibReader;

        public EntitySearchIndexBoosterReader(string fileName, GameIdsBoosterReader gibReader) : base(fileName)
        {
            _gibReader = gibReader;
        }

        internal IEnumerable<Result<uint>> GetGamesByPlayer(uint playerId)
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            try
            {
                _fs.Seek(FILE_HEADER_SIZE + playerId * RECORD_SIZE, SeekOrigin.Begin);
                var record = _reader.ReadBytes(RECORD_SIZE).AsSpan();
                var playerHeadBlockIndex = BinaryPrimitives.ReadInt32LittleEndian(record.Slice(0, 4));
                var games = _gibReader.GetGames(playerHeadBlockIndex);
                return games;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
