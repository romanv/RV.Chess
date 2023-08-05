using RV.Chess.CBReader.Types;
using RV.Chess.CBReader.Utils;

namespace RV.Chess.CBReader.Entities
{
    public record CbGameMetadata : CbhRecord
    {
        public uint MovesDataStartOffset { get; set; }

        public uint AnnotationsDataStartOffset { get; set; }

        public required PlayerRecord WhitePlayer { get; set; }

        public required PlayerRecord BlackPlayer { get; set; }

        public uint TournamentId { get; set; }

        public uint AnnotatorId { get; set; }

        public uint SourceId { get; set; }

        public DateOnly GameDate { get; set; }

        public GameResult GameResult { get; set; }

        public byte LineEvalNAG { get; set; }

        public byte Round { get; set; }

        public byte SubRound { get; set; }

        public uint WhiteRating { get; set; }

        public uint BlackRating { get; set; }

        public required Eco ECO { get; set; }

        public Medals Medals { get; set; }

        public AnnotationFlags AnnotationFlags { get; set; }

        public AnnotationMagnitude AnnotationMagnitude { get; set; }

        public byte MoveCount { get; set; }
    }
}
