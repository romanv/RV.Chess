using System.Numerics;
using System.Diagnostics;
using System.Text;

namespace RV.Chess.Board
{
    public sealed class Chessboard
    {
        private const ulong BLACK = 0xffff000000000000;
        private const ulong OCCUPIED = 0xffff00000000ffff;
        private const ulong WHITE = 0x000000000000ffff;

        private static readonly string[] SQUARES =
        {
            "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1",
            "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2",
            "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3",
            "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4",
            "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5",
            "a6", "b6", "c6", "d6", "e6", "f6", "g6", "h6",
            "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7",
            "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8",
        };

        private static readonly char[] RANKS = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };

        private static readonly Dictionary<PieceType, ulong> DEFAULT_PIECE_BITBOARDS = new()
        {
            { PieceType.Bishop, 0x2400000000000024 },
            { PieceType.King, 0x1000000000000010 },
            { PieceType.Knight, 0x4200000000000042 },
            { PieceType.Pawn, 0xff00000000ff00 },
            { PieceType.Queen, 0x800000000000008 },
            { PieceType.Rook, 0x8100000000000081 },
        };

        private ulong _bishops = DEFAULT_PIECE_BITBOARDS[PieceType.Bishop];
        private ulong _kings = DEFAULT_PIECE_BITBOARDS[PieceType.King];
        private ulong _knights = DEFAULT_PIECE_BITBOARDS[PieceType.Knight];
        private ulong _pawns = DEFAULT_PIECE_BITBOARDS[PieceType.Pawn];
        private ulong _queens = DEFAULT_PIECE_BITBOARDS[PieceType.Queen];
        private ulong _rooks = DEFAULT_PIECE_BITBOARDS[PieceType.Rook];

        private ulong _occupied = OCCUPIED;
        private ulong _occupiedWhite = WHITE;
        private ulong _occupiedBlack = BLACK;

        public Chessboard() {}

        public Chessboard(Chessboard boardToClone)
        {
            _occupied = boardToClone.OccupiedBoard;
            _occupiedWhite = boardToClone.OccupiedWhiteBoard;
            _occupiedBlack = boardToClone.OccupiedBlackBoard;

            _bishops = boardToClone._bishops;
            _kings = boardToClone._kings;
            _knights = boardToClone._knights;
            _pawns = boardToClone._pawns;
            _queens = boardToClone._queens;
            _rooks = boardToClone._rooks;
        }

        public ulong OccupiedBoard => _occupied;

        public ulong OccupiedWhiteBoard => _occupiedWhite;

        public ulong OccupiedBlackBoard => _occupiedBlack;

        public ulong GetPieceBoard(PieceType type, Side side)
        {
            ulong board = 0;

            switch (type)
            {
                case PieceType.Bishop:
                    board = _bishops;
                    break;
                case PieceType.King:
                    board = _kings;
                    break;
                case PieceType.Knight:
                    board = _knights;
                    break;
                case PieceType.Pawn:
                    board = _pawns;
                    break;
                case PieceType.Queen:
                    board = _queens;
                    break;
                case PieceType.Rook:
                    board = _rooks;
                    break;
            }

            return board & OwnBlockers(side);
        }

        public static string IdxToSquare(int idx) => (idx >= 0 && idx < 64)
            ? SQUARES[idx]
            : throw new InvalidDataException($"Bad square index ({idx}). Must be between 0 and 63");

        public static bool IsValidSquare(string square) => SquareToIdx(square) > -1;

        public static int SquareToIdx(string square) => Array.IndexOf(SQUARES, square);

        internal ulong OwnBlockers(Side sideToMove) => sideToMove == Side.White ? OccupiedWhiteBoard : OccupiedBlackBoard;

        internal ulong EnemyBlockers(Side sideToMove) => sideToMove == Side.White ? OccupiedBlackBoard : OccupiedWhiteBoard;

        internal ulong GetOccupiedBySide(Side side) => side == Side.White ? OccupiedWhiteBoard : OccupiedBlackBoard;

        public void AddPiece(PieceType type, Side side, string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                AddPiece(type, side, idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public void AddPiece(PieceType type, Side side, int square)
        {
            if (square < 0 || square > 63)
            {
                throw new InvalidDataException("Square number must be between 0 and 63");
            }

            RemovePieceAt(square);
            _occupied = OccupiedBoard.SetAt(square);

            switch (type)
            {
                case PieceType.Bishop:
                    _bishops = _bishops.SetAt(square);
                    break;
                case PieceType.King:
                    _kings = _kings.SetAt(square);
                    break;
                case PieceType.Knight:
                    _knights = _knights.SetAt(square);
                    break;
                case PieceType.Pawn:
                    _pawns = _pawns.SetAt(square);
                    break;
                case PieceType.Queen:
                    _queens = _queens.SetAt(square);
                    break;
                case PieceType.Rook:
                    _rooks = _rooks.SetAt(square);
                    break;
            }

            if (side == Side.White)
            {
                _occupiedWhite = _occupiedWhite.SetAt(square);
            }
            else
            {
                _occupiedBlack = _occupiedBlack.SetAt(square);
            }
        }

        public void Clear()
        {
            _occupied = 0;
            _occupiedWhite = 0;
            _occupiedBlack = 0;
            _bishops = 0;
            _kings = 0;
            _knights = 0;
            _pawns = 0;
            _queens = 0;
            _rooks = 0;
        }

        public int GetOwnKingSquare(Side ownSide)
            => ownSide == Side.White ? GetKingSquare(Side.White) : GetKingSquare(Side.Black);

        public int GetEnemyKingSquare(Side ownSide)
            => ownSide == Side.White ? GetKingSquare(Side.Black) : GetKingSquare(Side.White);

        public int GetKingSquare(Side side)
        {
            var kingBb = _kings & (side == Side.White ? OccupiedWhiteBoard : OccupiedBlackBoard);
            return 63 - BitOperations.LeadingZeroCount(kingBb);
        }

        public PieceType GetPieceTypeAt(string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                return GetPieceTypeAt(idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public PieceType GetPieceTypeAt(int square)
        {
            Debug.Assert(square >= 0 && square <= 64);

            if (_pawns.OccupiedAt(square)) return PieceType.Pawn;
            if (_knights.OccupiedAt(square)) return PieceType.Knight;
            if (_bishops.OccupiedAt(square)) return PieceType.Bishop;
            if (_rooks.OccupiedAt(square)) return PieceType.Rook;
            if (_queens.OccupiedAt(square)) return PieceType.Queen;
            if (_kings.OccupiedAt(square)) return PieceType.King;
            return PieceType.None;
        }

        public Side GetPieceSideAt(string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                return GetPieceSideAt(idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public Side GetPieceSideAt(int square)
        {
            Debug.Assert(square >= 0 && square <= 64);

            if (_occupiedWhite.OccupiedAt(square)) return Side.White;
            if (_occupiedBlack.OccupiedAt(square)) return Side.Black;
            return Side.None;
        }

        public bool IsOccupied(int square) => _occupied.OccupiedAt(square);

        public bool IsOccupied(string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                return IsOccupied(idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

#if DEBUG
        public void Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  +---+---+---+---+---+---+---+---+");

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");

                for (var file = 0; file < 8; file++)
                {
                    var isEmpty = true;
                    var squareIdx = rank * 8 + file;

                    if (_occupied.OccupiedAt(squareIdx))
                    {
                        var character = GetPieceTypeAt(squareIdx) switch
                        {
                            PieceType.Bishop => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| B " : "| b ",
                            PieceType.King => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| K " : "| k ",
                            PieceType.Knight => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| N " : "| n ",
                            PieceType.Pawn => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| P " : "| p ",
                            PieceType.Queen => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| Q " : "| q ",
                            PieceType.Rook => OccupiedWhiteBoard.OccupiedAt(squareIdx) ? "| R " : "| r ",
                            _ => "",
                        };
                        sb.Append(character);
                        isEmpty = false;
                    }

                    if (isEmpty)
                    {
                        sb.Append("|   ");
                    }
                }

                sb.Append("|\n");

                if (rank > 0)
                {
                    sb.AppendLine("  |---|---|---|---|---|---|---|---|");
                }
                else
                {
                    sb.AppendLine("  +---+---+---+---+---+---+---+---+");
                    sb.AppendLine("    a   b   c   d   e   f   g   h");
                }
            }

            Debug.WriteLine(sb.ToString());
        }
#endif

        public void RemovePieceAt(string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                RemovePieceAt(idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public void RemovePieceAt(int square)
        {
            if (!_occupied.OccupiedAt(square))
            {
                return;
            }

            var type = GetPieceTypeAt(square);

            if (type == PieceType.None)
            {
                return;
            }

            var side = GetPieceSideAt(square);

            switch (type)
            {
                case PieceType.Bishop:
                    _bishops = _bishops.RemoveAt(square);
                    break;
                case PieceType.King:
                    _kings = _kings.RemoveAt(square);
                    break;
                case PieceType.Knight:
                    _knights = _knights.RemoveAt(square);
                    break;
                case PieceType.Pawn:
                    _pawns = _pawns.RemoveAt(square);
                    break;
                case PieceType.Queen:
                    _queens = _queens.RemoveAt(square);
                    break;
                case PieceType.Rook:
                    _rooks = _rooks.RemoveAt(square);
                    break;
            }

            _occupied = _occupied.RemoveAt(square);

            if (side == Side.White)
            {
                _occupiedWhite = _occupiedWhite.RemoveAt(square);
            }
            else
            {
                _occupiedBlack = _occupiedBlack.RemoveAt(square);
            }
        }

        public static char SquareIdxToFile(int idx)
        {
            var fileIdx = idx % 8;
            return fileIdx < 8 ? RANKS[fileIdx] : ' ';
        }

        public static int SquareIdxToRank(int idx) => (idx / 8) + 1;

        internal void Reset()
        {
            _occupied = OCCUPIED;
            _occupiedWhite = WHITE;
            _occupiedBlack = BLACK;

            _bishops = DEFAULT_PIECE_BITBOARDS[PieceType.Bishop];
            _kings = DEFAULT_PIECE_BITBOARDS[PieceType.King];
            _knights = DEFAULT_PIECE_BITBOARDS[PieceType.Knight];
            _pawns = DEFAULT_PIECE_BITBOARDS[PieceType.Pawn];
            _queens = DEFAULT_PIECE_BITBOARDS[PieceType.Queen];
            _rooks = DEFAULT_PIECE_BITBOARDS[PieceType.Rook];
        }
    }
}
