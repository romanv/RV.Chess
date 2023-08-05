using RV.Chess.CBReader.Entities.Annotations;
using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.Entities
{
    public sealed record CbMove
    {
        public byte From { get; init; } = 255;

        public byte To { get; init; } = 255;

        public PieceType PromoteTo { get; init; } = PieceType.None;

        public bool IsNullMove { get; init; }

        public IAnnotation[] Annotations { get; set; } = Array.Empty<IAnnotation>();

        public List<CbMove> Next { get; } = new List<CbMove>();

        public CbMove AppendNextMove(CbMove nextMove)
        {
            Next.Add(nextMove);
            return nextMove;
        }
    }
}
