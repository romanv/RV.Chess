using System.Text;

namespace RV.Chess.Board
{
    internal static class FEN
    {
        internal static string BuildFEN(Chessgame game)
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
            fen.Append($" {game.CastlingRights.ToString()}");
            fen.Append($" {(game.EnPassantSquareIdx > -1 ? Chessboard.IdxToSquare(game.EnPassantSquareIdx) : '-')}");
            fen.Append($" {game.HalfMoveClock}");
            fen.Append($" {game.CurrentMoveNumber}");

            return fen.ToString();
        }

        /// <summary>
        /// Fills corresponding game data with data parsed from the fen
        /// </summary>
        /// <param name="game">Mutated board</param>
        internal static void PutFENDataIntoChessgame(Chessgame game, string fen)
        {
            game.Board.Clear();

            var parts = fen.Trim().Split(' ');

            if (parts.Length != 6)
            {
                throw new InvalidDataException($"Invalid fen: {fen}");
            }

            FillBoard(parts[0], game.Board);

            // side to move
            var sideToMovePart = parts[1];

            if (sideToMovePart == "w" || sideToMovePart == "b")
            {
                game.SideToMove = sideToMovePart == "w" ? Side.White : Side.Black;
            }
            else
            {
                throw new InvalidDataException($"Invalid side to move segment: {fen}");
            }

            if (TryParseFenCastling(parts[2], out var rights))
            {
                game.CastlingRights = rights;
            }
            else
            {
                throw new InvalidDataException($"Invalid castling segment: {fen}");
            }

            if (TryParseFenEnPassant(parts[3], out var square))
            {
                game.EnPassantSquareIdx = square;
            }
            else
            {
                throw new InvalidDataException($"Invalid en passant segment: {fen}");
            }

            if (int.TryParse(parts[4], out var halfMoveClock))
            {
                game.HalfMoveClock = halfMoveClock;
            }
            else
            {
                throw new InvalidDataException($"Invalid halfmove clock segment: {fen}");
            }

            if (int.TryParse(parts[5], out var fullMoveNumberPart))
            {
                if (fullMoveNumberPart < 1)
                {
                    throw new InvalidDataException($"Invalid move number segment: {fen}");
                }

                game.CurrentMoveNumber = fullMoveNumberPart;
            }
            else
            {
                throw new InvalidDataException($"Invalid move number segment: {fen}");
            }

            game.Moves.Clear();
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

        private static void FillBoard(string piecePlacementPart, Chessboard board)
        {
            // fen starts from the upper left square (index 56) and goes goes 56-63, then 48-55 etc
            var offset = 0;
            var rankSize = 0;
            var pos = 0;
            var rankCount = 0;

            while (pos < piecePlacementPart.Length)
            {
                var squareIdx = 56 - ((offset / 8) * 8) + (offset % 8);
                var c = piecePlacementPart[pos];

                if (char.IsDigit(c))
                {
                    var shift = c - '0';
                    offset += shift;
                    rankSize += shift;
                }
                else if (char.IsLetter(c))
                {
                    var type = PieceTypeFromChar(c);

                    if (type == PieceType.None)
                    {
                        throw new InvalidDataException($"Bad piece placement: {piecePlacementPart}");
                    }

                    var side = char.IsLower(c) ? Side.Black : Side.White;
                    board.AddPiece(type, side, squareIdx);
                    offset++;
                    rankSize++;
                }
                else if (c == '/')
                {
                    if (rankSize != 8)
                    {
                        throw new InvalidDataException($"Invalid number of pieces on the rank: {piecePlacementPart}");
                    }

                    rankSize = 0;
                    rankCount++;
                }

                if (rankSize > 8)
                {
                    throw new InvalidDataException($"Invalid number of pieces on the rank: {piecePlacementPart}");
                }

                pos++;
            }

            if (!board.GetPieceBoard(PieceType.King, Side.White).HasSingleBitSet()
                || !board.GetPieceBoard(PieceType.King, Side.Black).HasSingleBitSet())
            {
                throw new InvalidDataException($"Board must have single king for each side: {piecePlacementPart}");
            }

            if (rankCount != 7)
            {
                throw new InvalidDataException($"Invalid number of ranks: {piecePlacementPart}");
            }
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
                    result.Add(CastlingDirection.WhiteKingside);
                    pos++;
                }
                else if (c == 'Q')
                {
                    result.Add(CastlingDirection.WhiteQueenside);
                    pos++;
                }
                else if (c == 'k')
                {
                    result.Add(CastlingDirection.BlackKingside);
                    pos++;
                }
                else if (c == 'q')
                {
                    result.Add(CastlingDirection.BlackQueenside);
                    pos++;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseFenEnPassant(string enPassantSegment, out int result)
        {
            result = -1;

            if (enPassantSegment == "-")
            {
                return true;
            }
            else if (Chessboard.IsValidSquare(enPassantSegment))
            {
                result = Chessboard.SquareToIdx(enPassantSegment);
                return true;
            }

            return false;
        }
    }
}
