using FluentResults;

namespace RV.Chess.CBReader.Readers
{
    internal class TopGamesReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "flags";

        const uint FILE_HEADER_SIZE = 12;

        public TopGamesReader(string fileName) : base(fileName)
        {
        }

        // https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/games.md#flags
        internal IEnumerable<Result<uint>> Read(uint count)
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            _fs.Seek(FILE_HEADER_SIZE, SeekOrigin.Begin);

            var currGameId = -1;
            var topGamesFound = 0;

            while (_reader.BaseStream.Position < _reader.BaseStream.Length && topGamesFound < count)
            {
                var gamesBlock = _reader.ReadByte();

                while (gamesBlock > 0)
                {
                    var isRated = (gamesBlock & 0b10) > 0;

                    if (isRated && (gamesBlock & 0b1) > 0)
                    {
                        topGamesFound++;
                        yield return (uint)currGameId;
                    }

                    gamesBlock >>= 2;
                    currGameId += 1;
                }
            }
        }
    }
}
