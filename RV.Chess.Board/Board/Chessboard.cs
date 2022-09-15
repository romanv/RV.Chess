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

        private readonly Dictionary<PieceType, Bitboard> _pieceBoards = new();

        private readonly Dictionary<Side, Dictionary<PieceType, Bitboard>> _pieceBoardsByColor = new();

#pragma warning disable CS8618 // Fields are initialed inside the Reset method
        public Chessboard()
#pragma warning restore CS8618
        {
            Reset();
        }

        public Chessboard(Chessboard boardToClone)
        {
            OccupiedBoard = boardToClone.OccupiedBoard;
            OccupiedWhiteBoard = boardToClone.OccupiedWhiteBoard;
            OccupiedBlackBoard = boardToClone.OccupiedBlackBoard;

            _pieceBoardsByColor[Side.White] = new();
            _pieceBoardsByColor[Side.Black] = new();

            foreach (var (type, board) in boardToClone.PieceBoards)
            {
                _pieceBoards[type] = new Bitboard(board.Board);
                _pieceBoardsByColor[Side.White][type] = board & OccupiedWhiteBoard;
                _pieceBoardsByColor[Side.Black][type] = board & OccupiedBlackBoard;
            }
        }

        public Bitboard OccupiedBlackBoard { get; private set; }

        public Bitboard OccupiedBoard { get; private set; }

        public Bitboard OccupiedWhiteBoard { get; private set; }

        public ulong GetPieceBoard(PieceType type, Side color) => _pieceBoards[type].Board & OwnBlockers(color).Board;

        public static string IdxToSquare(int idx) => (idx >= 0 && idx < 64)
            ? SQUARES[idx]
            : throw new InvalidDataException($"Bad square index ({idx}). Must be between 0 and 63");

        public static bool IsValidSquare(string square) => SquareToIdx(square) > -1;

        public static int SquareToIdx(string square) => Array.IndexOf(SQUARES, square);

        internal Bitboard OwnBlockers(Side sideToMove) => sideToMove == Side.White ? OccupiedWhiteBoard : OccupiedBlackBoard;

        internal Bitboard EnemyBlockers(Side sideToMove) => sideToMove == Side.White ? OccupiedBlackBoard : OccupiedWhiteBoard;

        internal Dictionary<PieceType, Bitboard> PieceBoards => _pieceBoards;

        internal Bitboard GetPieceBoardByColor(Side color, PieceType type) => _pieceBoardsByColor[color][type];

        internal Bitboard GetOccupiedByColor(Side color) => color == Side.White
            ? OccupiedWhiteBoard
            : OccupiedBlackBoard;

        public void AddPiece(Piece piece, string square) => AddPiece(piece.Type, piece.Side, square);

        public void AddPiece(PieceType type, Side color, string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                AddPiece(type, color, idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public void AddPiece(Piece piece, int square) => AddPiece(piece.Type, piece.Side, square);

        public void AddPiece(PieceType type, Side color, int square)
        {
            if (square < 0 || square > 63)
            {
                throw new InvalidDataException("Square number must be between 0 and 63");
            }

            RemovePieceAt(square);

            OccupiedBoard.SetAt(square);
            _pieceBoards[type].SetAt(square);
            _pieceBoardsByColor[color][type].SetAt(square);

            if (color == Side.White)
            {
                OccupiedWhiteBoard.SetAt(square);
            }
            else
            {
                OccupiedBlackBoard.SetAt(square);
            }
        }

        public void Clear()
        {
            OccupiedBlackBoard.Clear();
            OccupiedWhiteBoard.Clear();
            OccupiedBoard.Clear();

            foreach (var (type, _) in DEFAULT_PIECE_BITBOARDS)
            {
                _pieceBoards[type] = new Bitboard(0);
                _pieceBoardsByColor[Side.White][type].Clear();
                _pieceBoardsByColor[Side.Black][type].Clear();
            }
        }

        public List<Piece> GetAllPieces()
        {
            var pieces = new List<Piece>();

            for (var i = 0; i < 64; i++)
            {
                var piece = GetPieceAt(i);

                if (piece.Type != PieceType.None)
                {
                    pieces.Add(piece);
                }
            }

            return pieces;
        }

        public int GetOwnKingSquare(Side ownColor)
            => ownColor == Side.White ? GetKingSquare(Side.White) : GetKingSquare(Side.Black);

        public int GetEnemyKingSquare(Side ownColor)
            => ownColor == Side.White ? GetKingSquare(Side.Black) : GetKingSquare(Side.White);

        public int GetKingSquare(Side color)
        {
            var kingBb = _pieceBoards[PieceType.King].Board
                & (color == Side.White ? OccupiedWhiteBoard.Board : OccupiedBlackBoard.Board);

            return 63 - BitOperations.LeadingZeroCount(kingBb);
        }

        public Piece GetPieceAt(string square)
        {
            var idx = SquareToIdx(square);

            if (idx >= 0)
            {
                return GetPieceAt(idx);
            }
            else
            {
                throw new InvalidDataException($"Invalid square: {square}");
            }
        }

        public Piece GetPieceAt(int square)
        {
            Debug.Assert(square >= 0 && square <= 64);

            // ugly, but faster then iterating trough dictionary keys
            if (_pieceBoards[PieceType.Rook].OccupiedAt(square))
            {
                return new Piece(PieceType.Rook, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }
            else if (_pieceBoards[PieceType.Knight].OccupiedAt(square))
            {
                return new Piece(PieceType.Knight, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }
            else if (_pieceBoards[PieceType.Bishop].OccupiedAt(square))
            {
                return new Piece(PieceType.Bishop, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }
            else if (_pieceBoards[PieceType.Queen].OccupiedAt(square))
            {
                return new Piece(PieceType.Queen, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }
            else if (_pieceBoards[PieceType.King].OccupiedAt(square))
            {
                return new Piece(PieceType.King, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }
            else if (_pieceBoards[PieceType.Pawn].OccupiedAt(square))
            {
                return new Piece(PieceType.Pawn, OccupiedWhiteBoard.OccupiedAt(square) ? Side.White : Side.Black);
            }

            return new Piece(PieceType.None, Side.White);
        }

        public bool IsOccupied(int square) => OccupiedBoard.OccupiedAt(square);

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

                    foreach (var (type, board) in _pieceBoards)
                    {
                        var squareIdx = rank * 8 + file;
                        if (board.OccupiedAt(squareIdx))
                        {
                            var character = type switch
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
                            break;
                        }
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
            if (!IsOccupied(square))
            {
                return;
            }

            var (type, color) = GetPieceAt(square);

            if (type == PieceType.None)
            {
                return;
            }

            OccupiedBoard.RemoveAt(square);
            _pieceBoards[type].RemoveAt(square);
            _pieceBoardsByColor[color][type].RemoveAt(square);

            if (color == Side.White)
            {
                OccupiedWhiteBoard.RemoveAt(square);
            }
            else
            {
                OccupiedBlackBoard.RemoveAt(square);
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
            OccupiedBlackBoard = new Bitboard(BLACK);
            OccupiedWhiteBoard = new Bitboard(WHITE);
            OccupiedBoard = new Bitboard(OCCUPIED);

            _pieceBoardsByColor[Side.White] = new();
            _pieceBoardsByColor[Side.Black] = new();

            foreach (var (type, bitboard) in DEFAULT_PIECE_BITBOARDS)
            {
                _pieceBoards[type] = new Bitboard(bitboard);
                _pieceBoardsByColor[Side.White][type] = _pieceBoards[type] & OccupiedWhiteBoard;
                _pieceBoardsByColor[Side.Black][type] = _pieceBoards[type] & OccupiedBlackBoard;
            }
        }
    }
}
