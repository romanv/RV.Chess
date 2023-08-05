using System.Collections.Immutable;

namespace RV.Chess.CBReader.Entities
{
    public record GuidingText : CbhRecord
    {
        public uint TextOffset { get; init; }

        public uint TournamentId { get; init; }

        public uint AnnotatorId { get; init; }

        public uint SourceId { get; init; }

        public byte Round { get; init; }

        public byte SubRound { get; init; }

        public uint AnnotationFlags { get; init; }

        public required IImmutableList<GuidingTextSection> Sections { get; init; }
    }
}
