using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.Entities
{
    public sealed class GameMoves
    {
        public CbMove Root { get; } = new CbMove();

        public string StartingFen { get; init; } = string.Empty;

        public int NextMoveNo { get; init; } = 1;

        public Side StartingSide { get; init; } = Side.White;

        public CastlingRights StartingCastling { get; init; } = CastlingRights.All;
    }
}
