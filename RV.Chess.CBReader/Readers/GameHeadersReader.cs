using System.Collections.Immutable;
using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Types;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Readers
{
    internal class GameHeadersReader : ReaderBase
    {
        internal override string FILE_EXTENSION => "cbh";

        const int FILE_HEADER_SIZE = 46;
        const int RECORD_SIZE = 46;

        private readonly PlayerDataReader _playerReader;
        private readonly MovesReader _movesReader;

        internal GameHeadersReader(
            string dbPath,
            PlayerDataReader playerReader,
            MovesReader movesReader) : base(dbPath)
        {
            _playerReader = playerReader;
            _movesReader = movesReader;
        }

        /*
           https://github.com/Yarin78/morphy/blob/master/morphy-cbh/docs/cbh-format/games.md
           Offset  Length
                1       4   Position in .cbg file where the game data starts (or guiding text)
                5       4   Position in .cba file where annotations start (0 = no annotations)
                9       3   White player number in .cbp file
                12      3   Black player number in .cbp file
                15      3   Tournament number in .cbt file
                18      3   Annotator number in .cbc file
                21      3   Source number in .cbs file
                24      3   Date
                27      1   Game result
                28      1   Line evaluation (only set if game result is 3 ("line"))
                29      1   Round
                30      1   Subround
                31      2   White rating
                33      2   Black rating
                35      2   ECO code
                37      2   Medals
                39      4   Annotation flags
                43      2   Annotation magnitude
                45      1   Number of moves in main variant. If 255, the actual number of moves is checked in the game data.
        */
        internal IEnumerable<Result<CbhRecord>> Read(uint skip, uint count)
        {
            if (IsError)
            {
                throw new InvalidOperationException(ErrorMessage);
            }

            var readRecords = 0;

            _fs.Seek(FILE_HEADER_SIZE + skip * RECORD_SIZE, SeekOrigin.Begin);
            while (readRecords < count && _reader.BaseStream.Position != _reader.BaseStream.Length)
            {
                Result<CbhRecord> result;

                try
                {
                    var record = _reader.ReadBytes(RECORD_SIZE).AsSpan();
                    var isGameRecord = (record[0] >> 1 & 1) == 0;

                    if (isGameRecord)
                    {
                        var game = new CbGameMetadata
                        {
                            Id = (uint)(skip + readRecords),
                            MovesDataStartOffset = record.Slice(1, 4).ToUIntBigEndian(),
                            AnnotationsDataStartOffset = record.Slice(5, 4).ToUIntBigEndian(),
                            WhitePlayer = _playerReader.GetSingle((int)record.Slice(9, 3).ToUIntBigEndian()),
                            BlackPlayer = _playerReader.GetSingle((int)record.Slice(12, 3).ToUIntBigEndian()),
                            TournamentId = record.Slice(15, 3).ToUIntBigEndian(),
                            AnnotatorId = record.Slice(18, 3).ToUIntBigEndian(),
                            SourceId = record.Slice(21, 3).ToUIntBigEndian(),
                            GameDate = record.Slice(24, 3).ToDateFromBigEndian(),
                            GameResult = (GameResult)record[27],
                            LineEvalNAG = record[28],
                            Round = record[29],
                            SubRound = record[30],
                            WhiteRating = record.Slice(31, 2).ToUIntBigEndian(),
                            BlackRating = record.Slice(33, 2).ToUIntBigEndian(),
                            ECO = EcoDecoder.Decode(record.Slice(35, 2).ToUIntBigEndian()),
                            Medals = (Medals)record.Slice(37, 2).ToUIntBigEndian(),
                            AnnotationFlags = (AnnotationFlags)record.Slice(39, 4).ToUIntBigEndian(),
                            AnnotationMagnitude = GetAnnotationMagnitude(record.Slice(43, 2).ToUIntBigEndian()),
                            MoveCount = record[45],
                        };

                        result = game;
                    }
                    else
                    {
                        var offset = record.Slice(1, 4).ToUIntBigEndian();
                        var text = new GuidingText
                        {
                            Id = (uint)(skip + readRecords),
                            TextOffset = offset,
                            TournamentId = record.Slice(7, 3).ToUIntBigEndian(),
                            SourceId = record.Slice(10, 3).ToUIntBigEndian(),
                            AnnotatorId = record.Slice(13, 3).ToUIntBigEndian(),
                            Round = record[16],
                            SubRound = record[17],
                            AnnotationFlags = record.Slice(18, 4).ToUIntBigEndian(),
                            Sections = _movesReader.GetText(offset) switch
                            {
                                { IsSuccess: true } success => success.Value,
                                _ => ImmutableList<GuidingTextSection>.Empty,
                            },
                        };

                        result = text;
                    }
                }
                catch (Exception ex)
                {
                    result = Result.Fail($"Game #{skip + readRecords} parsing error ({ex.Message})");
                }

                readRecords++;

                yield return result;
            }
        }

        private static AnnotationMagnitude GetAnnotationMagnitude(uint code)
        {
            var result = AnnotationMagnitude.None;

            switch (code & 0b11)
            {
                case 1:
                    result |= AnnotationMagnitude.Variations50;
                    break;
                case 2:
                    result |= AnnotationMagnitude.Variations300;
                    break;
                case 3:
                    result |= AnnotationMagnitude.Variations1000;
                    break;
            }

            if ((code & 0b100) > 0)
                result |= AnnotationMagnitude.Commentaries;

            if ((code & 0b1000) > 0)
                result |= AnnotationMagnitude.Symbols;

            if ((code & 0b10000) > 0)
                result |= AnnotationMagnitude.Squares;

            if ((code & 0b100000) > 0)
                result |= AnnotationMagnitude.Arrows;

            if ((code & 0b1000000) > 0)
                result |= AnnotationMagnitude.TimeSpent;

            if ((code & 0b10000000) > 0)
                result |= AnnotationMagnitude.Annotations;

            return result;
        }
    }
}
