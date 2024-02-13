using System.Numerics;
using System.Text;
using RV.Chess.Board.Game;
using RV.Chess.Board.Types;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Utils
{
    internal static class FenGenerator
    {
        internal static string BuildFen(Chessgame game, bool rebuildhalfMoveClock = true)
        {
            var fen = new StringBuilder();

            for (var rank = 7; rank >= 0; rank--)
            {
                var emptyFilesCount = 0;

                for (var file = 0; file < 8; file++)
                {
                    var pieceType = game.Board.GetPieceTypeAt(rank * 8 + file);

                    if (pieceType == PieceType.None)
                    {
                        emptyFilesCount++;
                    }
                    else
                    {
                        if (emptyFilesCount > 0)
                        {
                            fen.Append(emptyFilesCount);
                            emptyFilesCount = 0;
                        }

                        var pieceSide = game.Board.GetPieceSideAt(rank * 8 + file);
                        fen.Append(pieceType.ToChar(pieceSide));
                    }
                }

                if (emptyFilesCount > 0)
                {
                    fen.Append(emptyFilesCount);
                }

                if (rank > 0)
                {
                    fen.Append('/');
                }
            }

            fen.Append($" {(game.SideToMove == Side.White ? 'w' : 'b')}");
            fen.Append($" {game.CastlingRights.AsString()}");
            fen.Append($" {(game.EpSquareMask > 0 ? Coordinates.IdxToSquare(BitOperations.TrailingZeroCount(game.EpSquareMask)) : '-')}");

            // build half-move clock from moves
            if (rebuildhalfMoveClock)
            {
                var clock = game.HalfMoveClockStart;

                foreach (var move in game._moveList)
                {
                    if (move.Type == MoveType.Pawn || move.IsPromotion || move.IsCapture)
                    {
                        clock = 0;
                    }
                    else
                    {
                        clock++;
                    }
                }

                fen.Append($" {clock}");
            }
            else
                fen.Append($" 0");
            {
            }

            fen.Append($" {game.CurrentMoveNumber}");

            return fen.ToString();
        }

        internal static bool ReadFen(Chessgame game, string fen)
        {
            game.Board.Clear();
            var rank = 7;
            var file = 0;
            var pos = 0;

            while (rank >= 0)
            {
                switch (fen[pos])
                {
                    case 'r': case 'n': case 'b': case 'q': case 'k': case 'p':
                    case 'R': case 'N': case 'B': case 'Q': case 'K': case 'P':
                        var type = PieceTypeFromChar(fen[pos]);
                        var side = char.IsLower(fen[pos]) ? Side.Black : Side.White;
                        game.Board.AddPiece(type, side, rank * 8 + file);
                        file++;
                        break;
                    case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8':
                        file += (int)char.GetNumericValue(fen[pos]);
                        break;
                    case '/':
                        if (file != 8)
                        {
                            return false;
                        }

                        rank--;
                        file = 0;
                        break;
                    case ' ':
                        if (file < 8 && rank != 0)
                        {
                            return false;
                        }
                        rank--;
                        break;
                    default:
                        return false;
                };

                pos++;
            }

            var parts = fen.Trim().Split(' ');

            if (parts.Length != 6)
            {
                return false;
            }

            // side to move
            switch (parts[1])
            {
                case "w":
                    game.SideToMove = Side.White;
                    break;
                case "b":
                    game.SideToMove = Side.Black;
                    break;
                default:
                    return false;
            }

            if (TryParseFenCastling(parts[2], out var rights))
            {
                game.CastlingRights = rights;
            }
            else
            {
                return false;
            }

            if (TryParseFenEnPassant(parts[3], out var mask))
            {
                game.EpSquareMask = mask;
            }
            else
            {
                return false;
            }

            if (int.TryParse(parts[4], out var halfMoveClockStart))
            {
                game.HalfMoveClockStart = halfMoveClockStart;
            }
            else
            {
                return false;
            }

            if (int.TryParse(parts[5], out var fullMoveNumberPart))
            {
                if (fullMoveNumberPart < 1)
                {
                    return false;
                }

                game.CurrentMoveNumber = fullMoveNumberPart;
            }
            else
            {
                return false;
            }

            game._moveList.Clear();
            game._incrementalHash = Zobrist.GetIncrementalHash(game);

            return true;
        }

        private static PieceType PieceTypeFromChar(char t)
        {
            return t switch
            {
                'r' or 'R' => PieceType.Rook,
                'n' or 'N' => PieceType.Knight,
                'b' or 'B' => PieceType.Bishop,
                'q' or 'Q' => PieceType.Queen,
                'k' or 'K' => PieceType.King,
                'p' or 'P' => PieceType.Pawn,
                _ => PieceType.None,
            };
        }

        private static bool TryParseFenCastling(string castlingSegment, out CastlingRights result)
        {
            result = CastlingRights.None;
            var pos = 0;

            if (castlingSegment.Length < 1 || castlingSegment.Length > 4)
            {
                return false;
            }

            while (pos < castlingSegment.Length)
            {
                var c = castlingSegment[pos];

                if (c == '-')
                {
                    break;
                }
                else if (c == 'K')
                {
                    result |= CastlingRights.WhiteKingside;
                    pos++;
                }
                else if (c == 'Q')
                {
                    result |= CastlingRights.WhiteQueenside;
                    pos++;
                }
                else if (c == 'k')
                {
                    result |= CastlingRights.BlackKingside;
                    pos++;
                }
                else if (c == 'q')
                {
                    result |= CastlingRights.BlackQueenside;
                    pos++;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseFenEnPassant(string enPassantSegment, out ulong result)
        {
            result = 0;

            if (enPassantSegment == "-")
            {
                return true;
            }
            else if (Coordinates.IsValidSquare(enPassantSegment))
            {
                result = 1UL << Coordinates.SquareToIdx(enPassantSegment);
                return true;
            }

            return false;
        }
    }
}
