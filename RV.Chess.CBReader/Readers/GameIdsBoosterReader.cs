using FluentResults;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class GameIdsBoosterReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cib";

        const int FILE_HEADER_SIZE = 12;
        const int RECORD_SIZE = 64;

        public GameIdsBoosterReader(string fileName) : base(fileName)
        {
        }

        /*
            0 	4 	Id of the next block in the linked list (-1 if there is no next block).
                    If this is an unused block, this points to the next unused block,
                    or 0 if there are no more unused blocks.
            4 	4 	Unknown. Always 0?
            8 	4 	Number of game references in this block
            12 	4 	Id of a game
            16 	4 	Id of a game
            ...
        */
        internal IEnumerable<Result<uint>> GetGames(int firstBlockIdx)
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            var seenBlocks = new HashSet<int>();
            var seenGames = new HashSet<uint>();
            var currBlockIdx = firstBlockIdx;

            while (currBlockIdx > -1)
            {
                if (seenBlocks.Contains(currBlockIdx))
                {
                    throw new InvalidOperationException("Loop detected in the game ids booster");
                }


                _fs.Seek(FILE_HEADER_SIZE + currBlockIdx * RECORD_SIZE, SeekOrigin.Begin);
                var block = _reader.ReadBytes(RECORD_SIZE).AsSpan();
                var nextBlockIdx = block.Slice(0, 4).ToIntLittleEndian();
                var gamesInBlockCount = block.Slice(8, 4).ToIntLittleEndian();
                var gamesInBlock = new List<uint>();

                for (var i = 0; i < gamesInBlockCount; i++)
                {
                    var gameId = block.Slice(12 + i * 4, 4).ToUIntLittleEndian();

                    if (!seenGames.Contains(gameId))
                    {
                        seenGames.Add(gameId);
                        gamesInBlock.Add(gameId);
                    }
                }

                // can't return id's directly due to the Span inside the iterator
                foreach (var g in gamesInBlock)
                {
                    yield return g;
                }

                seenBlocks.Add(currBlockIdx);
                currBlockIdx = nextBlockIdx;
            }
        }
    }
}
