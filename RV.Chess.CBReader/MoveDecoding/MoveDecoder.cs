using FluentResults;
using RV.Chess.CBReader.Entities;
using RV.Chess.CBReader.Entities.Annotations;
using RV.Chess.CBReader.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.CBReader.MoveDecoding
{
    internal static class MoveDecoder
    {
        private static readonly byte[] MoveDecryptionMap =
        {
            0xa2, 0x95, 0x43, 0xf5, 0xc1, 0x3d, 0x4a, 0x6c,	//   0 -   7
	        0x53, 0x83, 0xcc, 0x7c, 0xff, 0xae, 0x68, 0xad,	//   8 -  15
	        0xd1, 0x92, 0x8b, 0x8d, 0x35, 0x81, 0x5e, 0x74,	//  16 -  23
	        0x26, 0x8e, 0xab, 0xca, 0xfd, 0x9a, 0xf3, 0xa0,	//  24 -  31
	        0xa5, 0x15, 0xfc, 0xb1, 0x1e, 0xed, 0x30, 0xea,	//  32 -  39
	        0x22, 0xeb, 0xa7, 0xcd, 0x4e, 0x6f, 0x2e, 0x24,	//  40 -  47
	        0x32, 0x94, 0x41, 0x8c, 0x6e, 0x58, 0x82, 0x50,	//  48 -  55
	        0xbb, 0x02, 0x8a, 0xd8, 0xfa, 0x60, 0xde, 0x52,	//  56 -  63
	        0xba, 0x46, 0xac, 0x29, 0x9d, 0xd7, 0xdf, 0x08,	//  64 -  71
	        0x21, 0x01, 0x66, 0xa3, 0xf1, 0x19, 0x27, 0xb5,	//  72 -  79
	        0x91, 0xd5, 0x42, 0x0e, 0xb4, 0x4c, 0xd9, 0x18,	//  80 -  87
	        0x5f, 0xbc, 0x25, 0xa6, 0x96, 0x04, 0x56, 0x6a,	//  88 -  95
	        0xaa, 0x33, 0x1c, 0x2b, 0x73, 0xf0, 0xdd, 0xa4,	//  96 - 103
            0x37, 0xd3, 0xc5, 0x10, 0xbf, 0x5a, 0x23, 0x34,	// 104 - 111
	        0x75, 0x5b, 0xb8, 0x55, 0xd2, 0x6b, 0x09, 0x3a,	// 112 - 119
	        0x57, 0x12, 0xb3, 0x77, 0x48, 0x85, 0x9b, 0x0f,	// 120 - 127
	        0x9e, 0xc7, 0xc8, 0xa1, 0x7f, 0x7a, 0xc0, 0xbd,	// 128 - 135
	        0x31, 0x6d, 0xf6, 0x3e, 0xc3, 0x11, 0x71, 0xce,	// 136 - 143
	        0x7d, 0xda, 0xa8, 0x54, 0x90, 0x97, 0x1f, 0x44,	// 144 - 151
	        0x40, 0x16, 0xc9, 0xe3, 0x2c, 0xcb, 0x84, 0xec,	// 152 - 159
	        0x9f, 0x3f, 0x5c, 0xe6, 0x76, 0x0b, 0x3c, 0x20,	// 160 - 167
	        0xb7, 0x36, 0x00, 0xdc, 0xe7, 0xf9, 0x4f, 0xf7,	// 168 - 175
	        0xaf, 0x06, 0x07, 0xe0, 0x1a, 0x0a, 0xa9, 0x4b,	// 176 - 183
            0x0c, 0xd6, 0x63, 0x87, 0x89, 0x1d, 0x13, 0x1b,	// 184 - 191
	        0xe4, 0x70, 0x05, 0x47, 0x67, 0x7b, 0x2f, 0xee,	// 192 - 199
	        0xe2, 0xe8, 0x98, 0x0d, 0xef, 0xcf, 0xc4, 0xf4,	// 200 - 207
	        0xfb, 0xb0, 0x17, 0x99, 0x64, 0xf2, 0xd4, 0x2a,	// 208 - 215
	        0x03, 0x4d, 0x78, 0xc6, 0xfe, 0x65, 0x86, 0x88,	// 216 - 223
	        0x79, 0x45, 0x3b, 0xe5, 0x49, 0x8f, 0x2d, 0xb9,	// 224 - 231
	        0xbe, 0x62, 0x93, 0x14, 0xe9, 0xd0, 0x38, 0x9c,	// 232 - 239
	        0xb2, 0xc2, 0x59, 0x5d, 0xb6, 0x72, 0x51, 0xf8,	// 240 - 247
	        0x28, 0x7e, 0x61, 0x39, 0xe1, 0xdb, 0x69, 0x80,	// 248 - 255
        };

        internal static Result<GameMoves> Decode(
            Span<byte> moves,
            GameMoves game,
            MoveDecodingState? state,
            Dictionary<int, List<IAnnotation>> annotations)
        {
            var piecesState = state ?? new MoveDecodingState();
            var variationStates = new Stack<VariationStartState>(256);
            var moveCount = 0;
            var isMultiMove = false;
            byte? multiCodeByteOne = null;
            var curr = game.Root;
            var sideToMove = game.StartingSide;

            foreach (var moveCode in moves)
            {
                var idx = (byte)(moveCode - moveCount);

                if (idx < 0 || idx >= MoveDecryptionMap.Length)
                {
                    return Result.Fail($"Invalid move code index ({idx})");
                }

                var operationCode = MoveDecryptionMap[idx];

                if (isMultiMove)
                {
                    if (multiCodeByteOne == null)
                    {
                        multiCodeByteOne = operationCode;
                        continue;
                    }
                    else
                    {
                        var multiCodeByte = (multiCodeByteOne << 8) | operationCode;
                        var moveResult = MakeMultiMove(multiCodeByte.Value, piecesState);
                        if (moveResult.IsFailed)
                        {
                            return Result.Fail(moveResult.Errors.FirstOrDefault()?.Message);
                        }

                        if (annotations.TryGetValue(moveCount, out var an))
                        {
                            moveResult.Value.Annotations = an.ToArray();
                        }

                        curr = curr.AppendNextMove(moveResult.Value);
                        multiCodeByteOne = null;
                        isMultiMove = false;
                        moveCount++;
                        sideToMove = sideToMove.Opposite();
                    }
                }
                else if (MoveDecoderMap.Operations.TryGetValue(operationCode, out var op))
                {
                    if (op.Type == MoveType.Multi)
                    {
                        isMultiMove = true;
                        continue;
                    }
                    else if (op.Type == MoveType.VariationStart)
                    {
                        variationStates.Push(new VariationStartState(curr, piecesState.Clone(), sideToMove));
                    }
                    else if (op.Type == MoveType.VariationEnd)
                    {
                        // last move of each game is also VARIATION_END
                        if (variationStates.Count == 0)
                        {
                            return game;
                        }
                        else
                        {
                            (curr, piecesState, sideToMove) = variationStates.Pop();
                        }
                    }
                    else if (op.Type == MoveType.Ignore)
                    {
                        continue;
                    }
                    else if (op.Type == MoveType.Null)
                    {
                        curr = curr.AppendNextMove(new CbMove { IsNullMove = true });
                        sideToMove = sideToMove.Opposite();
                        moveCount++;
                        continue;
                    }
                    else
                    {
                        var moveResult = piecesState.MakeMove(op, sideToMove);
                        if (moveResult.IsFailed)
                        {
                            return Result.Fail(moveResult.Errors.FirstOrDefault()?.Message);
                        }

                        if (annotations.TryGetValue(moveCount, out var an))
                        {
                            moveResult.Value.Annotations = an.ToArray();
                        }

                        curr = curr.AppendNextMove(moveResult.Value);
                        sideToMove = sideToMove.Opposite();
                        moveCount++;
                    }
                }
            }

            return game;
        }

        private static Result<CbMove> MakeMultiMove(int code, MoveDecodingState state)
        {
            var sourceSquare = ((byte)(code & 0b111111)).RotatePartLeft(6, 3);
            var targetSquare = ((byte)((code >> 6) & 0b111111)).RotatePartLeft(6, 3);
            var promoteTo = ((code >> 12) & 0b11) switch
            {
                0 => PieceType.Queen,
                1 => PieceType.Rook,
                2 => PieceType.Bishop,
                3 => PieceType.Knight,
                _ => PieceType.None,
            };

            // if we dont check for this, multi moves for any piece would set promoteTo to a Queen
            if (!state.IsPawnAt(sourceSquare))
            {
                promoteTo = PieceType.None;
            }

            return state.MakeMove(sourceSquare, targetSquare, promoteTo);
        }
    }
}
