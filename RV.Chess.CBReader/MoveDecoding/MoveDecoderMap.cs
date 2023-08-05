﻿namespace RV.Chess.CBReader.MoveDecoding
{
    internal static class MoveDecoderMap
    {
        internal static Dictionary<byte, MoveOperation> Operations = new()
        {
            { 0, new MoveOperation(MoveType.Null, 0, 0, 0) },

            // King moves
            { 1, new MoveOperation(MoveType.King, 0, 0, 1) },
            { 2, new MoveOperation(MoveType.King, 0, 1, 1) },
            { 3, new MoveOperation(MoveType.King, 0, 1, 0) },
            { 4, new MoveOperation(MoveType.King, 0, 1, -1) },
            { 5, new MoveOperation(MoveType.King, 0, 0, -1) },
            { 6, new MoveOperation(MoveType.King, 0, -1, -1) },
            { 7, new MoveOperation(MoveType.King, 0, -1, 0) },
            { 8, new MoveOperation(MoveType.King, 0, -1, 1) },
            { 9, new MoveOperation(MoveType.King, 0, 2, 0) },
            { 10, new MoveOperation(MoveType.King, 0, -2, 0) },

            // Queen moves
            { 11, new MoveOperation(MoveType.Queen, 0, 0, 1) },
            { 12, new MoveOperation(MoveType.Queen, 0, 0, 2) },
            { 13, new MoveOperation(MoveType.Queen, 0, 0, 3) },
            { 14, new MoveOperation(MoveType.Queen, 0, 0, 4) },
            { 15, new MoveOperation(MoveType.Queen, 0, 0, 5) },
            { 16, new MoveOperation(MoveType.Queen, 0, 0, 6) },
            { 17, new MoveOperation(MoveType.Queen, 0, 0, 7) },
            { 18, new MoveOperation(MoveType.Queen, 0, 1, 0) },
            { 19, new MoveOperation(MoveType.Queen, 0, 2, 0) },
            { 20, new MoveOperation(MoveType.Queen, 0, 3, 0) },
            { 21, new MoveOperation(MoveType.Queen, 0, 4, 0) },
            { 22, new MoveOperation(MoveType.Queen, 0, 5, 0) },
            { 23, new MoveOperation(MoveType.Queen, 0, 6, 0) },
            { 24, new MoveOperation(MoveType.Queen, 0, 7, 0) },
            { 25, new MoveOperation(MoveType.Queen, 0, 1, 1) },
            { 26, new MoveOperation(MoveType.Queen, 0, 2, 2) },
            { 27, new MoveOperation(MoveType.Queen, 0, 3, 3) },
            { 28, new MoveOperation(MoveType.Queen, 0, 4, 4) },
            { 29, new MoveOperation(MoveType.Queen, 0, 5, 5) },
            { 30, new MoveOperation(MoveType.Queen, 0, 6, 6) },
            { 31, new MoveOperation(MoveType.Queen, 0, 7, 7) },
            { 32, new MoveOperation(MoveType.Queen, 0, 1, -1) },
            { 33, new MoveOperation(MoveType.Queen, 0, 2, -2) },
            { 34, new MoveOperation(MoveType.Queen, 0, 3, -3) },
            { 35, new MoveOperation(MoveType.Queen, 0, 4, -4) },
            { 36, new MoveOperation(MoveType.Queen, 0, 5, -5) },
            { 37, new MoveOperation(MoveType.Queen, 0, 6, -6) },
            { 38, new MoveOperation(MoveType.Queen, 0, 7, -7) },

            // Rook moves - first
            { 39, new MoveOperation(MoveType.Rook, 0, 0, 1) },
            { 40, new MoveOperation(MoveType.Rook, 0, 0, 2) },
            { 41, new MoveOperation(MoveType.Rook, 0, 0, 3) },
            { 42, new MoveOperation(MoveType.Rook, 0, 0, 4) },
            { 43, new MoveOperation(MoveType.Rook, 0, 0, 5) },
            { 44, new MoveOperation(MoveType.Rook, 0, 0, 6) },
            { 45, new MoveOperation(MoveType.Rook, 0, 0, 7) },
            { 46, new MoveOperation(MoveType.Rook, 0, 1, 0) },
            { 47, new MoveOperation(MoveType.Rook, 0, 2, 0) },
            { 48, new MoveOperation(MoveType.Rook, 0, 3, 0) },
            { 49, new MoveOperation(MoveType.Rook, 0, 4, 0) },
            { 50, new MoveOperation(MoveType.Rook, 0, 5, 0) },
            { 51, new MoveOperation(MoveType.Rook, 0, 6, 0) },
            { 52, new MoveOperation(MoveType.Rook, 0, 7, 0) },

            // Rook moves - second
            { 53, new MoveOperation(MoveType.Rook, 1, 0, 1) },
            { 54, new MoveOperation(MoveType.Rook, 1, 0, 2) },
            { 55, new MoveOperation(MoveType.Rook, 1, 0, 3) },
            { 56, new MoveOperation(MoveType.Rook, 1, 0, 4) },
            { 57, new MoveOperation(MoveType.Rook, 1, 0, 5) },
            { 58, new MoveOperation(MoveType.Rook, 1, 0, 6) },
            { 59, new MoveOperation(MoveType.Rook, 1, 0, 7) },
            { 60, new MoveOperation(MoveType.Rook, 1, 1, 0) },
            { 61, new MoveOperation(MoveType.Rook, 1, 2, 0) },
            { 62, new MoveOperation(MoveType.Rook, 1, 3, 0) },
            { 63, new MoveOperation(MoveType.Rook, 1, 4, 0) },
            { 64, new MoveOperation(MoveType.Rook, 1, 5, 0) },
            { 65, new MoveOperation(MoveType.Rook, 1, 6, 0) },
            { 66, new MoveOperation(MoveType.Rook, 1, 7, 0) },

            // Bishop moves - first
            { 67, new MoveOperation(MoveType.Bishop, 0, 1, 1) },
            { 68, new MoveOperation(MoveType.Bishop, 0, 2, 2) },
            { 69, new MoveOperation(MoveType.Bishop, 0, 3, 3) },
            { 70, new MoveOperation(MoveType.Bishop, 0, 4, 4) },
            { 71, new MoveOperation(MoveType.Bishop, 0, 5, 5) },
            { 72, new MoveOperation(MoveType.Bishop, 0, 6, 6) },
            { 73, new MoveOperation(MoveType.Bishop, 0, 7, 7) },
            { 74, new MoveOperation(MoveType.Bishop, 0, 1, -1) },
            { 75, new MoveOperation(MoveType.Bishop, 0, 2, -2) },
            { 76, new MoveOperation(MoveType.Bishop, 0, 3, -3) },
            { 77, new MoveOperation(MoveType.Bishop, 0, 4, -4) },
            { 78, new MoveOperation(MoveType.Bishop, 0, 5, -5) },
            { 79, new MoveOperation(MoveType.Bishop, 0, 6, -6) },
            { 80, new MoveOperation(MoveType.Bishop, 0, 7, -7) },

            // Bishop moves - second
            { 81, new MoveOperation(MoveType.Bishop, 1, 1, 1) },
            { 82, new MoveOperation(MoveType.Bishop, 1, 2, 2) },
            { 83, new MoveOperation(MoveType.Bishop, 1, 3, 3) },
            { 84, new MoveOperation(MoveType.Bishop, 1, 4, 4) },
            { 85, new MoveOperation(MoveType.Bishop, 1, 5, 5) },
            { 86, new MoveOperation(MoveType.Bishop, 1, 6, 6) },
            { 87, new MoveOperation(MoveType.Bishop, 1, 7, 7) },
            { 88, new MoveOperation(MoveType.Bishop, 1, 1, -1) },
            { 89, new MoveOperation(MoveType.Bishop, 1, 2, -2) },
            { 90, new MoveOperation(MoveType.Bishop, 1, 3, -3) },
            { 91, new MoveOperation(MoveType.Bishop, 1, 4, -4) },
            { 92, new MoveOperation(MoveType.Bishop, 1, 5, -5) },
            { 93, new MoveOperation(MoveType.Bishop, 1, 6, -6) },
            { 94, new MoveOperation(MoveType.Bishop, 1, 7, -7) },

            // Knight moves - first
            { 95, new MoveOperation(MoveType.Knight, 0, 2, 1) },
            { 96, new MoveOperation(MoveType.Knight, 0, 1, 2) },
            { 97, new MoveOperation(MoveType.Knight, 0, -1, 2) },
            { 98, new MoveOperation(MoveType.Knight, 0, -2, 1) },
            { 99, new MoveOperation(MoveType.Knight, 0, -2, -1) },
            { 100, new MoveOperation(MoveType.Knight, 0, -1, -2) },
            { 101, new MoveOperation(MoveType.Knight, 0, 1, -2) },
            { 102, new MoveOperation(MoveType.Knight, 0, 2, -1) },

            // Knight moves - second
            { 103, new MoveOperation(MoveType.Knight, 1, 2, 1) },
            { 104, new MoveOperation(MoveType.Knight, 1, 1, 2) },
            { 105, new MoveOperation(MoveType.Knight, 1, -1, 2) },
            { 106, new MoveOperation(MoveType.Knight, 1, -2, 1) },
            { 107, new MoveOperation(MoveType.Knight, 1, -2, -1) },
            { 108, new MoveOperation(MoveType.Knight, 1, -1, -2) },
            { 109, new MoveOperation(MoveType.Knight, 1, 1, -2) },
            { 110, new MoveOperation(MoveType.Knight, 1, 2, -1) },

            // Pawn moves
            { 111, new MoveOperation(MoveType.Pawn, 0, 0, 1) },
            { 112, new MoveOperation(MoveType.Pawn, 0, 0, 2) },
            { 113, new MoveOperation(MoveType.Pawn, 0, 1, 1) },
            { 114, new MoveOperation(MoveType.Pawn, 0, -1, 1) },
            { 115, new MoveOperation(MoveType.Pawn, 1, 0, 1) },
            { 116, new MoveOperation(MoveType.Pawn, 1, 0, 2) },
            { 117, new MoveOperation(MoveType.Pawn, 1, 1, 1) },
            { 118, new MoveOperation(MoveType.Pawn, 1, -1, 1) },
            { 119, new MoveOperation(MoveType.Pawn, 2, 0, 1) },
            { 120, new MoveOperation(MoveType.Pawn, 2, 0, 2) },
            { 121, new MoveOperation(MoveType.Pawn, 2, 1, 1) },
            { 122, new MoveOperation(MoveType.Pawn, 2, -1, 1) },
            { 123, new MoveOperation(MoveType.Pawn, 3, 0, 1) },
            { 124, new MoveOperation(MoveType.Pawn, 3, 0, 2) },
            { 125, new MoveOperation(MoveType.Pawn, 3, 1, 1) },
            { 126, new MoveOperation(MoveType.Pawn, 3, -1, 1) },
            { 127, new MoveOperation(MoveType.Pawn, 4, 0, 1) },
            { 128, new MoveOperation(MoveType.Pawn, 4, 0, 2) },
            { 129, new MoveOperation(MoveType.Pawn, 4, 1, 1) },
            { 130, new MoveOperation(MoveType.Pawn, 4, -1, 1) },
            { 131, new MoveOperation(MoveType.Pawn, 5, 0, 1) },
            { 132, new MoveOperation(MoveType.Pawn, 5, 0, 2) },
            { 133, new MoveOperation(MoveType.Pawn, 5, 1, 1) },
            { 134, new MoveOperation(MoveType.Pawn, 5, -1, 1) },
            { 135, new MoveOperation(MoveType.Pawn, 6, 0, 1) },
            { 136, new MoveOperation(MoveType.Pawn, 6, 0, 2) },
            { 137, new MoveOperation(MoveType.Pawn, 6, 1, 1) },
            { 138, new MoveOperation(MoveType.Pawn, 6, -1, 1) },
            { 139, new MoveOperation(MoveType.Pawn, 7, 0, 1) },
            { 140, new MoveOperation(MoveType.Pawn, 7, 0, 2) },
            { 141, new MoveOperation(MoveType.Pawn, 7, 1, 1) },
            { 142, new MoveOperation(MoveType.Pawn, 7, -1, 1) },

            // Queen moves - second
            { 143, new MoveOperation(MoveType.Queen, 1, 0, 1) },
            { 144, new MoveOperation(MoveType.Queen, 1, 0, 2) },
            { 145, new MoveOperation(MoveType.Queen, 1, 0, 3) },
            { 146, new MoveOperation(MoveType.Queen, 1, 0, 4) },
            { 147, new MoveOperation(MoveType.Queen, 1, 0, 5) },
            { 148, new MoveOperation(MoveType.Queen, 1, 0, 6) },
            { 149, new MoveOperation(MoveType.Queen, 1, 0, 7) },
            { 150, new MoveOperation(MoveType.Queen, 1, 1, 0) },
            { 151, new MoveOperation(MoveType.Queen, 1, 2, 0) },
            { 152, new MoveOperation(MoveType.Queen, 1, 3, 0) },
            { 153, new MoveOperation(MoveType.Queen, 1, 4, 0) },
            { 154, new MoveOperation(MoveType.Queen, 1, 5, 0) },
            { 155, new MoveOperation(MoveType.Queen, 1, 6, 0) },
            { 156, new MoveOperation(MoveType.Queen, 1, 7, 0) },
            { 157, new MoveOperation(MoveType.Queen, 1, 1, 1) },
            { 158, new MoveOperation(MoveType.Queen, 1, 2, 2) },
            { 159, new MoveOperation(MoveType.Queen, 1, 3, 3) },
            { 160, new MoveOperation(MoveType.Queen, 1, 4, 4) },
            { 161, new MoveOperation(MoveType.Queen, 1, 5, 5) },
            { 162, new MoveOperation(MoveType.Queen, 1, 6, 6) },
            { 163, new MoveOperation(MoveType.Queen, 1, 7, 7) },
            { 164, new MoveOperation(MoveType.Queen, 1, 1, -1) },
            { 165, new MoveOperation(MoveType.Queen, 1, 2, -2) },
            { 166, new MoveOperation(MoveType.Queen, 1, 3, -3) },
            { 167, new MoveOperation(MoveType.Queen, 1, 4, -4) },
            { 168, new MoveOperation(MoveType.Queen, 1, 5, -5) },
            { 169, new MoveOperation(MoveType.Queen, 1, 6, -6) },
            { 170, new MoveOperation(MoveType.Queen, 1, 7, -7) },

            // Queen moves - third
            { 171, new MoveOperation(MoveType.Queen, 2, 0, 1) },
            { 172, new MoveOperation(MoveType.Queen, 2, 0, 2) },
            { 173, new MoveOperation(MoveType.Queen, 2, 0, 3) },
            { 174, new MoveOperation(MoveType.Queen, 2, 0, 4) },
            { 175, new MoveOperation(MoveType.Queen, 2, 0, 5) },
            { 176, new MoveOperation(MoveType.Queen, 2, 0, 6) },
            { 177, new MoveOperation(MoveType.Queen, 2, 0, 7) },
            { 178, new MoveOperation(MoveType.Queen, 2, 1, 0) },
            { 179, new MoveOperation(MoveType.Queen, 2, 2, 0) },
            { 180, new MoveOperation(MoveType.Queen, 2, 3, 0) },
            { 181, new MoveOperation(MoveType.Queen, 2, 4, 0) },
            { 182, new MoveOperation(MoveType.Queen, 2, 5, 0) },
            { 183, new MoveOperation(MoveType.Queen, 2, 6, 0) },
            { 184, new MoveOperation(MoveType.Queen, 2, 7, 0) },
            { 185, new MoveOperation(MoveType.Queen, 2, 1, 1) },
            { 186, new MoveOperation(MoveType.Queen, 2, 2, 2) },
            { 187, new MoveOperation(MoveType.Queen, 2, 3, 3) },
            { 188, new MoveOperation(MoveType.Queen, 2, 4, 4) },
            { 189, new MoveOperation(MoveType.Queen, 2, 5, 5) },
            { 190, new MoveOperation(MoveType.Queen, 2, 6, 6) },
            { 191, new MoveOperation(MoveType.Queen, 2, 7, 7) },
            { 192, new MoveOperation(MoveType.Queen, 2, 1, -1) },
            { 193, new MoveOperation(MoveType.Queen, 2, 2, -2) },
            { 194, new MoveOperation(MoveType.Queen, 2, 3, -3) },
            { 195, new MoveOperation(MoveType.Queen, 2, 4, -4) },
            { 196, new MoveOperation(MoveType.Queen, 2, 5, -5) },
            { 197, new MoveOperation(MoveType.Queen, 2, 6, -6) },
            { 198, new MoveOperation(MoveType.Queen, 2, 7, -7) },

            // Rook moves - third
            { 199, new MoveOperation(MoveType.Rook, 2, 0, 1) },
            { 200, new MoveOperation(MoveType.Rook, 2, 0, 2) },
            { 201, new MoveOperation(MoveType.Rook, 2, 0, 3) },
            { 202, new MoveOperation(MoveType.Rook, 2, 0, 4) },
            { 203, new MoveOperation(MoveType.Rook, 2, 0, 5) },
            { 204, new MoveOperation(MoveType.Rook, 2, 0, 6) },
            { 205, new MoveOperation(MoveType.Rook, 2, 0, 7) },
            { 206, new MoveOperation(MoveType.Rook, 2, 1, 0) },
            { 207, new MoveOperation(MoveType.Rook, 2, 2, 0) },
            { 208, new MoveOperation(MoveType.Rook, 2, 3, 0) },
            { 209, new MoveOperation(MoveType.Rook, 2, 4, 0) },
            { 210, new MoveOperation(MoveType.Rook, 2, 5, 0) },
            { 211, new MoveOperation(MoveType.Rook, 2, 6, 0) },
            { 212, new MoveOperation(MoveType.Rook, 2, 7, 0) },

            // Bishop moves - third
            { 213, new MoveOperation(MoveType.Bishop, 2, 1, 1) },
            { 214, new MoveOperation(MoveType.Bishop, 2, 2, 2) },
            { 215, new MoveOperation(MoveType.Bishop, 2, 3, 3) },
            { 216, new MoveOperation(MoveType.Bishop, 2, 4, 4) },
            { 217, new MoveOperation(MoveType.Bishop, 2, 5, 5) },
            { 218, new MoveOperation(MoveType.Bishop, 2, 6, 6) },
            { 219, new MoveOperation(MoveType.Bishop, 2, 7, 7) },
            { 220, new MoveOperation(MoveType.Bishop, 2, 1, -1) },
            { 221, new MoveOperation(MoveType.Bishop, 2, 2, -2) },
            { 222, new MoveOperation(MoveType.Bishop, 2, 3, -3) },
            { 223, new MoveOperation(MoveType.Bishop, 2, 4, -4) },
            { 224, new MoveOperation(MoveType.Bishop, 2, 5, -5) },
            { 225, new MoveOperation(MoveType.Bishop, 2, 6, -6) },
            { 226, new MoveOperation(MoveType.Bishop, 2, 7, -7) },

            // Knight moves - third
            { 227, new MoveOperation(MoveType.Knight, 2, 2, 1) },
            { 228, new MoveOperation(MoveType.Knight, 2, 1, 2) },
            { 229, new MoveOperation(MoveType.Knight, 2, -1, 2) },
            { 230, new MoveOperation(MoveType.Knight, 2, -2, 1) },
            { 231, new MoveOperation(MoveType.Knight, 2, -2, -1) },
            { 232, new MoveOperation(MoveType.Knight, 2, -1, -2) },
            { 233, new MoveOperation(MoveType.Knight, 2, 1, -2) },
            { 234, new MoveOperation(MoveType.Knight, 2, 2, -1) },

            // Utility
            { 235, new MoveOperation(MoveType.Multi, 0, 0, 0) },
            { 236, new MoveOperation(MoveType.Ignore, 0, 0, 0) },
            { 254, new MoveOperation(MoveType.VariationStart, 0, 0, 0) },
            { 255, new MoveOperation(MoveType.VariationEnd, 0, 0, 0) },

            /*
             *             { 11, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 1) },
            { 12, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 2) },
            { 13, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 3) },
            { 14, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 4) },
            { 15, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 5) },
            { 16, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 6) },
            { 17, new MoveOperation(MoveType.QUEEN_ONE, 0, 0, 7) },
            { 18, new MoveOperation(MoveType.QUEEN_ONE, 0, 1, 0) },
            { 19, new MoveOperation(MoveType.QUEEN_ONE, 0, 2, 0) },
            { 20, new MoveOperation(MoveType.QUEEN_ONE, 0, 3, 0) },
            { 21, new MoveOperation(MoveType.QUEEN_ONE, 0, 4, 0) },
            { 22, new MoveOperation(MoveType.QUEEN_ONE, 0, 5, 0) },
            { 23, new MoveOperation(MoveType.QUEEN_ONE, 0, 6, 0) },
            { 24, new MoveOperation(MoveType.QUEEN_ONE, 0, 7, 0) },
            { 25, new MoveOperation(MoveType.QUEEN_ONE, 0, 1, 1) },
            { 26, new MoveOperation(MoveType.QUEEN_ONE, 0, 2, 2) },
            { 27, new MoveOperation(MoveType.QUEEN_ONE, 0, 3, 3) },
            { 28, new MoveOperation(MoveType.QUEEN_ONE, 0, 4, 4) },
            { 29, new MoveOperation(MoveType.QUEEN_ONE, 0, 5, 5) },
            { 30, new MoveOperation(MoveType.QUEEN_ONE, 0, 6, 6) },
            { 31, new MoveOperation(MoveType.QUEEN_ONE, 0, 7, 7) },
            { 32, new MoveOperation(MoveType.QUEEN_ONE, 0, 1, -1) },
            { 33, new MoveOperation(MoveType.QUEEN_ONE, 0, 2, -2) },
            { 34, new MoveOperation(MoveType.QUEEN_ONE, 0, 3, -3) },
            { 35, new MoveOperation(MoveType.QUEEN_ONE, 0, 4, -4) },
            { 36, new MoveOperation(MoveType.QUEEN_ONE, 0, 5, -5) },
            { 37, new MoveOperation(MoveType.QUEEN_ONE, 0, 6, -6) },
            { 38, new MoveOperation(MoveType.QUEEN_ONE, 0, 7, -7) },

            // Rook moves - first
            { 39, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 1) },
            { 40, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 2) },
            { 41, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 3) },
            { 42, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 4) },
            { 43, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 5) },
            { 44, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 6) },
            { 45, new MoveOperation(MoveType.ROOK_ONE, 0, 0, 7) },
            { 46, new MoveOperation(MoveType.ROOK_ONE, 0, 1, 0) },
            { 47, new MoveOperation(MoveType.ROOK_ONE, 0, 2, 0) },
            { 48, new MoveOperation(MoveType.ROOK_ONE, 0, 3, 0) },
            { 49, new MoveOperation(MoveType.ROOK_ONE, 0, 4, 0) },
            { 50, new MoveOperation(MoveType.ROOK_ONE, 0, 5, 0) },
            { 51, new MoveOperation(MoveType.ROOK_ONE, 0, 6, 0) },
            { 52, new MoveOperation(MoveType.ROOK_ONE, 0, 7, 0) },

            // Rook moves - second
            { 53, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 1) },
            { 54, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 2) },
            { 55, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 3) },
            { 56, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 4) },
            { 57, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 5) },
            { 58, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 6) },
            { 59, new MoveOperation(MoveType.ROOK_TWO, 1, 0, 7) },
            { 60, new MoveOperation(MoveType.ROOK_TWO, 1, 1, 0) },
            { 61, new MoveOperation(MoveType.ROOK_TWO, 1, 2, 0) },
            { 62, new MoveOperation(MoveType.ROOK_TWO, 1, 3, 0) },
            { 63, new MoveOperation(MoveType.ROOK_TWO, 1, 4, 0) },
            { 64, new MoveOperation(MoveType.ROOK_TWO, 1, 5, 0) },
            { 65, new MoveOperation(MoveType.ROOK_TWO, 1, 6, 0) },
            { 66, new MoveOperation(MoveType.ROOK_TWO, 1, 7, 0) },

            // Bishop moves - first
            { 67, new MoveOperation(MoveType.BISHOP_ONE, 0, 1, 1) },
            { 68, new MoveOperation(MoveType.BISHOP_ONE, 0, 2, 2) },
            { 69, new MoveOperation(MoveType.BISHOP_ONE, 0, 3, 3) },
            { 70, new MoveOperation(MoveType.BISHOP_ONE, 0, 4, 4) },
            { 71, new MoveOperation(MoveType.BISHOP_ONE, 0, 5, 5) },
            { 72, new MoveOperation(MoveType.BISHOP_ONE, 0, 6, 6) },
            { 73, new MoveOperation(MoveType.BISHOP_ONE, 0, 7, 7) },
            { 74, new MoveOperation(MoveType.BISHOP_ONE, 0, 1, -1) },
            { 75, new MoveOperation(MoveType.BISHOP_ONE, 0, 2, -2) },
            { 76, new MoveOperation(MoveType.BISHOP_ONE, 0, 3, -3) },
            { 77, new MoveOperation(MoveType.BISHOP_ONE, 0, 4, -4) },
            { 78, new MoveOperation(MoveType.BISHOP_ONE, 0, 5, -5) },
            { 79, new MoveOperation(MoveType.BISHOP_ONE, 0, 6, -6) },
            { 80, new MoveOperation(MoveType.BISHOP_ONE, 0, 7, -7) },

            // Bishop moves - second
            { 81, new MoveOperation(MoveType.BISHOP_TWO, 1, 1, 1) },
            { 82, new MoveOperation(MoveType.BISHOP_TWO, 1, 2, 2) },
            { 83, new MoveOperation(MoveType.BISHOP_TWO, 1, 3, 3) },
            { 84, new MoveOperation(MoveType.BISHOP_TWO, 1, 4, 4) },
            { 85, new MoveOperation(MoveType.BISHOP_TWO, 1, 5, 5) },
            { 86, new MoveOperation(MoveType.BISHOP_TWO, 1, 6, 6) },
            { 87, new MoveOperation(MoveType.BISHOP_TWO, 1, 7, 7) },
            { 88, new MoveOperation(MoveType.BISHOP_TWO, 1, 1, -1) },
            { 89, new MoveOperation(MoveType.BISHOP_TWO, 1, 2, -2) },
            { 90, new MoveOperation(MoveType.BISHOP_TWO, 1, 3, -3) },
            { 91, new MoveOperation(MoveType.BISHOP_TWO, 1, 4, -4) },
            { 92, new MoveOperation(MoveType.BISHOP_TWO, 1, 5, -5) },
            { 93, new MoveOperation(MoveType.BISHOP_TWO, 1, 6, -6) },
            { 94, new MoveOperation(MoveType.BISHOP_TWO, 1, 7, -7) },

            // Knight moves - first
            { 95, new MoveOperation(MoveType.KNIGHT_ONE, 0, 2, 1) },
            { 96, new MoveOperation(MoveType.KNIGHT_ONE, 0, 1, 2) },
            { 97, new MoveOperation(MoveType.KNIGHT_ONE, 0, -1, 2) },
            { 98, new MoveOperation(MoveType.KNIGHT_ONE, 0, -2, 1) },
            { 99, new MoveOperation(MoveType.KNIGHT_ONE, 0, -2, -1) },
            { 100, new MoveOperation(MoveType.KNIGHT_ONE, 0, -1, -2) },
            { 101, new MoveOperation(MoveType.KNIGHT_ONE, 0, 1, -2) },
            { 102, new MoveOperation(MoveType.KNIGHT_ONE, 0, 2, -1) },

            // Knight moves - second
            { 103, new MoveOperation(MoveType.KNIGHT_TWO, 1, 2, 1) },
            { 104, new MoveOperation(MoveType.KNIGHT_TWO, 1, 1, 2) },
            { 105, new MoveOperation(MoveType.KNIGHT_TWO, 1, -1, 2) },
            { 106, new MoveOperation(MoveType.KNIGHT_TWO, 1, -2, 1) },
            { 107, new MoveOperation(MoveType.KNIGHT_TWO, 1, -2, -1) },
            { 108, new MoveOperation(MoveType.KNIGHT_TWO, 1, -1, -2) },
            { 109, new MoveOperation(MoveType.KNIGHT_TWO, 1, 1, -2) },
            { 110, new MoveOperation(MoveType.KNIGHT_TWO, 1, 2, -1) },

            // Pawn moves
            { 111, new MoveOperation(MoveType.PAWN_A, 0, 0, 1) },
            { 112, new MoveOperation(MoveType.PAWN_A, 0, 0, 2) },
            { 113, new MoveOperation(MoveType.PAWN_A, 0, 1, 1) },
            { 114, new MoveOperation(MoveType.PAWN_A, 0, -1, 1) },
            { 115, new MoveOperation(MoveType.PAWN_B, 0, 0, 1) },
            { 116, new MoveOperation(MoveType.PAWN_B, 0, 0, 2) },
            { 117, new MoveOperation(MoveType.PAWN_B, 0, 1, 1) },
            { 118, new MoveOperation(MoveType.PAWN_B, 0, -1, 1) },
            { 119, new MoveOperation(MoveType.PAWN_C, 0, 0, 1) },
            { 120, new MoveOperation(MoveType.PAWN_C, 0, 0, 2) },
            { 121, new MoveOperation(MoveType.PAWN_C, 0, 1, 1) },
            { 122, new MoveOperation(MoveType.PAWN_C, 0, -1, 1) },
            { 123, new MoveOperation(MoveType.PAWN_D, 0, 0, 1) },
            { 124, new MoveOperation(MoveType.PAWN_D, 0, 0, 2) },
            { 125, new MoveOperation(MoveType.PAWN_D, 0, 1, 1) },
            { 126, new MoveOperation(MoveType.PAWN_D, 0, -1, 1) },
            { 127, new MoveOperation(MoveType.PAWN_E, 0, 0, 1) },
            { 128, new MoveOperation(MoveType.PAWN_E, 0, 0, 2) },
            { 129, new MoveOperation(MoveType.PAWN_E, 0, 1, 1) },
            { 130, new MoveOperation(MoveType.PAWN_E, 0, -1, 1) },
            { 131, new MoveOperation(MoveType.PAWN_F, 0, 0, 1) },
            { 132, new MoveOperation(MoveType.PAWN_F, 0, 0, 2) },
            { 133, new MoveOperation(MoveType.PAWN_F, 0, 1, 1) },
            { 134, new MoveOperation(MoveType.PAWN_F, 0, -1, 1) },
            { 135, new MoveOperation(MoveType.PAWN_G, 0, 0, 1) },
            { 136, new MoveOperation(MoveType.PAWN_G, 0, 0, 2) },
            { 137, new MoveOperation(MoveType.PAWN_G, 0, 1, 1) },
            { 138, new MoveOperation(MoveType.PAWN_G, 0, -1, 1) },
            { 139, new MoveOperation(MoveType.PAWN_H, 0, 0, 1) },
            { 140, new MoveOperation(MoveType.PAWN_H, 0, 0, 2) },
            { 141, new MoveOperation(MoveType.PAWN_H, 0, 1, 1) },
            { 142, new MoveOperation(MoveType.PAWN_H, 0, -1, 1) },

            // Queen moves - second
            { 143, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 1) },
            { 144, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 2) },
            { 145, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 3) },
            { 146, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 4) },
            { 147, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 5) },
            { 148, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 6) },
            { 149, new MoveOperation(MoveType.QUEEN_TWO, 1, 0, 7) },
            { 150, new MoveOperation(MoveType.QUEEN_TWO, 1, 1, 0) },
            { 151, new MoveOperation(MoveType.QUEEN_TWO, 1, 2, 0) },
            { 152, new MoveOperation(MoveType.QUEEN_TWO, 1, 3, 0) },
            { 153, new MoveOperation(MoveType.QUEEN_TWO, 1, 4, 0) },
            { 154, new MoveOperation(MoveType.QUEEN_TWO, 1, 5, 0) },
            { 155, new MoveOperation(MoveType.QUEEN_TWO, 1, 6, 0) },
            { 156, new MoveOperation(MoveType.QUEEN_TWO, 1, 7, 0) },
            { 157, new MoveOperation(MoveType.QUEEN_TWO, 1, 1, 1) },
            { 158, new MoveOperation(MoveType.QUEEN_TWO, 1, 2, 2) },
            { 159, new MoveOperation(MoveType.QUEEN_TWO, 1, 3, 3) },
            { 160, new MoveOperation(MoveType.QUEEN_TWO, 1, 4, 4) },
            { 161, new MoveOperation(MoveType.QUEEN_TWO, 1, 5, 5) },
            { 162, new MoveOperation(MoveType.QUEEN_TWO, 1, 6, 6) },
            { 163, new MoveOperation(MoveType.QUEEN_TWO, 1, 7, 7) },
            { 164, new MoveOperation(MoveType.QUEEN_TWO, 1, 1, -1) },
            { 165, new MoveOperation(MoveType.QUEEN_TWO, 1, 2, -2) },
            { 166, new MoveOperation(MoveType.QUEEN_TWO, 1, 3, -3) },
            { 167, new MoveOperation(MoveType.QUEEN_TWO, 1, 4, -4) },
            { 168, new MoveOperation(MoveType.QUEEN_TWO, 1, 5, -5) },
            { 169, new MoveOperation(MoveType.QUEEN_TWO, 1, 6, -6) },
            { 170, new MoveOperation(MoveType.QUEEN_TWO, 1, 7, -7) },

            // Queen moves - third
            { 171, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 1) },
            { 172, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 2) },
            { 173, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 3) },
            { 174, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 4) },
            { 175, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 5) },
            { 176, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 6) },
            { 177, new MoveOperation(MoveType.QUEEN_THREE, 2, 0, 7) },
            { 178, new MoveOperation(MoveType.QUEEN_THREE, 2, 1, 0) },
            { 179, new MoveOperation(MoveType.QUEEN_THREE, 2, 2, 0) },
            { 180, new MoveOperation(MoveType.QUEEN_THREE, 2, 3, 0) },
            { 181, new MoveOperation(MoveType.QUEEN_THREE, 2, 4, 0) },
            { 182, new MoveOperation(MoveType.QUEEN_THREE, 2, 5, 0) },
            { 183, new MoveOperation(MoveType.QUEEN_THREE, 2, 6, 0) },
            { 184, new MoveOperation(MoveType.QUEEN_THREE, 2, 7, 0) },
            { 185, new MoveOperation(MoveType.QUEEN_THREE, 2, 1, 1) },
            { 186, new MoveOperation(MoveType.QUEEN_THREE, 2, 2, 2) },
            { 187, new MoveOperation(MoveType.QUEEN_THREE, 2, 3, 3) },
            { 188, new MoveOperation(MoveType.QUEEN_THREE, 2, 4, 4) },
            { 189, new MoveOperation(MoveType.QUEEN_THREE, 2, 5, 5) },
            { 190, new MoveOperation(MoveType.QUEEN_THREE, 2, 6, 6) },
            { 191, new MoveOperation(MoveType.QUEEN_THREE, 2, 7, 7) },
            { 192, new MoveOperation(MoveType.QUEEN_THREE, 2, 1, -1) },
            { 193, new MoveOperation(MoveType.QUEEN_THREE, 2, 2, -2) },
            { 194, new MoveOperation(MoveType.QUEEN_THREE, 2, 3, -3) },
            { 195, new MoveOperation(MoveType.QUEEN_THREE, 2, 4, -4) },
            { 196, new MoveOperation(MoveType.QUEEN_THREE, 2, 5, -5) },
            { 197, new MoveOperation(MoveType.QUEEN_THREE, 2, 6, -6) },
            { 198, new MoveOperation(MoveType.QUEEN_THREE, 2, 7, -7) },

            // Rook moves - third
            { 199, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 1) },
            { 200, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 2) },
            { 201, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 3) },
            { 202, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 4) },
            { 203, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 5) },
            { 204, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 6) },
            { 205, new MoveOperation(MoveType.ROOK_THREE, 2, 0, 7) },
            { 206, new MoveOperation(MoveType.ROOK_THREE, 2, 1, 0) },
            { 207, new MoveOperation(MoveType.ROOK_THREE, 2, 2, 0) },
            { 208, new MoveOperation(MoveType.ROOK_THREE, 2, 3, 0) },
            { 209, new MoveOperation(MoveType.ROOK_THREE, 2, 4, 0) },
            { 210, new MoveOperation(MoveType.ROOK_THREE, 2, 5, 0) },
            { 211, new MoveOperation(MoveType.ROOK_THREE, 2, 6, 0) },
            { 212, new MoveOperation(MoveType.ROOK_THREE, 2, 7, 0) },

            // Bishop moves - third
            { 213, new MoveOperation(MoveType.BISHOP_THREE, 2, 1, 1) },
            { 214, new MoveOperation(MoveType.BISHOP_THREE, 2, 2, 2) },
            { 215, new MoveOperation(MoveType.BISHOP_THREE, 2, 3, 3) },
            { 216, new MoveOperation(MoveType.BISHOP_THREE, 2, 4, 4) },
            { 217, new MoveOperation(MoveType.BISHOP_THREE, 2, 5, 5) },
            { 218, new MoveOperation(MoveType.BISHOP_THREE, 2, 6, 6) },
            { 219, new MoveOperation(MoveType.BISHOP_THREE, 2, 7, 7) },
            { 220, new MoveOperation(MoveType.BISHOP_THREE, 2, 1, -1) },
            { 221, new MoveOperation(MoveType.BISHOP_THREE, 2, 2, -2) },
            { 222, new MoveOperation(MoveType.BISHOP_THREE, 2, 3, -3) },
            { 223, new MoveOperation(MoveType.BISHOP_THREE, 2, 4, -4) },
            { 224, new MoveOperation(MoveType.BISHOP_THREE, 2, 5, -5) },
            { 225, new MoveOperation(MoveType.BISHOP_THREE, 2, 6, -6) },
            { 226, new MoveOperation(MoveType.BISHOP_THREE, 2, 7, -7) },

            // Knight moves - third
            { 227, new MoveOperation(MoveType.KNIGHT_THREE, 2, 2, 1) },
            { 228, new MoveOperation(MoveType.KNIGHT_THREE, 2, 1, 2) },
            { 229, new MoveOperation(MoveType.KNIGHT_THREE, 2, -1, 2) },
            { 230, new MoveOperation(MoveType.KNIGHT_THREE, 2, -2, 1) },
            { 231, new MoveOperation(MoveType.KNIGHT_THREE, 2, -2, -1) },
            { 232, new MoveOperation(MoveType.KNIGHT_THREE, 2, -1, -2) },
            { 233, new MoveOperation(MoveType.KNIGHT_THREE, 2, 1, -2) },
            { 234, new MoveOperation(MoveType.KNIGHT_THREE, 2, 2, -1) }, */
        };
    }
}
