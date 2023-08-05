using RV.Chess.CBReader.Entities;
using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.MoveDecoding
{
    internal record VariationStartState(CbMove Root, MoveDecodingState State, Side PrecedingSide);
}
