using System.Diagnostics;
using FluentResults;

namespace RV.Chess.CBReader.Readers
{
    internal class PositionSearchBoosterReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbb";

        const int RECORD_SIZE = 52;

        public PositionSearchBoosterReader(string fileName) : base(fileName)
        {
        }

        internal IEnumerable<Result<uint>> Find(byte[] searchMask)
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            _fs.Seek(RECORD_SIZE, SeekOrigin.Begin);

            uint currGameId = 0;

            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var gameMask = _reader.ReadBytes(RECORD_SIZE);
                var matches = true;

                for (var i = 0; i < 52; i++)
                {
                    var mask = searchMask[i];
                    var data = gameMask[i];

                    if ((data & mask) != mask)
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    yield return currGameId;
                }

                currGameId++;
            }
        }
    }
}
