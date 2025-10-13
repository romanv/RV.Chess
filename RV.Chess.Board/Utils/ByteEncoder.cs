using System.Numerics;
using RV.Chess.Board.Game;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Utils;

internal static class ByteEncoder
{
    internal static int WriteBytes(Chessgame game, Span<byte> destination)
    {
        if (destination.Length < 192)
            throw new ArgumentException(
                $"Insufficient destination buffer capacity: required 24 bytes, actual {destination.Length}");

        if (!BitConverter.TryWriteBytes(destination, game.Board.Occupied[2]))
            return 0;

        var occupied = game.Board.Occupied[2];
        var destinationIdx = 8;
        var isFirstByteHalf = true;
        var epSquare = game.EpSquare;
        var setEnPassant = false;
        var written = 8;

        // Iterate through all set bits of the occupancy board
        while (occupied > 0)
        {
            /*
            0    0000  white king, white to move
            1    0001  white queen
            2    0010  white rook no castle
            3    0011  white bishop
            4    0100  white knight
            5    0101  white pawn
            6    0110  white rook can castle
            7    0111  white king, black to move
            8    1000  black king
            9    1001  black queen
            10   1010  black rook no castle
            11   1011  black bishop
            12   1100  black knight
            13   1101  black pawn
            14   1110  black rook can castle
            15   1111  if last was an en passant pawn, this would be set
            */

            var squareIdx = BitOperations.TrailingZeroCount(occupied);
            // Encode the piece at the square
            // Start encoding at 0000 for white, 1000 for black piece at square
            var encoded = (int)(((game.Board.Occupied[1] >> squareIdx) & 1) << 3);
            var piece = (int)game.Board.GetPieceTypeAt(squareIdx) - 1;
            // Side to move is encoded in the white king - 000 for white to move and 111 for black
            if (piece == 0 && game.SideToMove == Side.Black)
                piece = 7;

            if (piece == 2
                && (
                    (squareIdx == 0 && game.CastlingRights.HasFlag(CastlingRights.WhiteQueenside))
                    || (squareIdx == 7 && game.CastlingRights.HasFlag(CastlingRights.WhiteKingside))
                    || (squareIdx == 56 && game.CastlingRights.HasFlag(CastlingRights.BlackQueenside))
                    || (squareIdx == 63 && game.CastlingRights.HasFlag(CastlingRights.BlackKingside))
                    )
            )
            {
                piece = 0b110;
            }
            // If piece is a pawn, next 4 bits could mark whether this pawn set an en passant square
            else if (piece == 5 && epSquare > 0)
            {
                // This pawn is the one that had moved two squares on the previous move
                if ((game.SideToMove == Side.White && squareIdx == epSquare - 8)
                    || (game.SideToMove == Side.Black && squareIdx == epSquare + 8))
                {
                    setEnPassant = true;
                }
            }

            encoded |= piece;
            if (isFirstByteHalf)
            {
                if (setEnPassant)
                {
                    destination[destinationIdx] = (byte)((encoded << 4) | 0b1111);
                    setEnPassant = false;
                    destinationIdx++;
                }
                else
                {
                    destination[destinationIdx] = (byte)(encoded << 4);
                    isFirstByteHalf = false;
                }

                written++;
            }
            else
            {
                destination[destinationIdx] |= (byte)encoded;
                destinationIdx++;

                if (setEnPassant)
                {
                    destination[destinationIdx] = 0b1111 << 4;
                    written++;
                    setEnPassant = false;
                }
                else
                {
                    isFirstByteHalf = true;
                }
            }

            occupied &= occupied - 1;
        }

        return written;
    }

    internal static void ReadBytes(ReadOnlySpan<byte> bytes, Chessgame game)
    {
        if (bytes.Length < 9)
            throw new InvalidDataException("Data is too short to contain a meaningful position");

        game.ClearBoard();
        game.CastlingRights = CastlingRights.None;
        var occupied = BitConverter.ToUInt64(bytes[..8]);
        var sourceIdx = 8;
        var isFirstByteHalf = true;
        var prevPawn = 0;
        var prevPawnSquareIdx = 0;

        while (occupied > 0)
        {
            var squareIdx = BitOperations.TrailingZeroCount(occupied);
            var data = isFirstByteHalf ? bytes[sourceIdx] >> 4 : bytes[sourceIdx] & 0b1111;

            if (data == 0b1111)
            {
                // Square that was jumped by the previously encoded pawn must be marked as en passant.
                var jumpedSquare = prevPawn >> 3 == 1
                    ? prevPawnSquareIdx + 8 // last encoded pawn was black
                    : prevPawnSquareIdx - 8; // last encoded pawn was white
                game.SetEnPassant(jumpedSquare);
                // Should not clear the occupied bit because we did not process an actual piece information,
                // but rather an en passant marker.
                isFirstByteHalf = !isFirstByteHalf;
                if (isFirstByteHalf)
                    sourceIdx++;

                continue;
            }

            if (data == 0b0111)
            {
                // White king, black to move
                game.Board.AddPieceUnsafe(PieceType.King, Side.White, squareIdx);
                game.SideToMove = Side.Black;
            }
            else
            {
                var pieceSide = (Side)(data >> 3);
                var piece = data & 0b111;
                var pieceType = (PieceType)(piece + 1);

                if (piece == 0b110)
                {
                    // Rook that can castle
                    game.Board.AddPieceUnsafe(PieceType.Rook, pieceSide, squareIdx);
                    switch (squareIdx)
                    {
                        case 0:
                            game.CastlingRights |= CastlingRights.WhiteQueenside;
                            break;
                        case 7:
                            game.CastlingRights |= CastlingRights.WhiteKingside;
                            break;
                        case 56:
                            game.CastlingRights |= CastlingRights.BlackQueenside;
                            break;
                        case 63:
                            game.CastlingRights |= CastlingRights.BlackKingside;
                            break;
                    }
                }
                else
                {
                    game.Board.AddPieceUnsafe(pieceType, pieceSide, squareIdx);
                    if (piece == 0b0101)
                    {
                        prevPawn = data;
                        prevPawnSquareIdx = squareIdx;
                    }
                }
            }

            isFirstByteHalf = !isFirstByteHalf;
            if (isFirstByteHalf)
                sourceIdx++;

            occupied &= occupied - 1;
        }
    }
}
