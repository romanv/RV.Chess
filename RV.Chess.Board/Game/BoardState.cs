using System.Diagnostics;
using System.Numerics;
using System.Text;
using RV.Chess.Board.Utils;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Game
{
    internal sealed class BoardState
    {
        private const ulong DEFAULT_BLACK = 0xffff000000000000;
        private const ulong DEFAULT_ALL = 0xffff00000000ffff;
        private const ulong DEFAULT_WHITE = 0x000000000000ffff;

        private static readonly Dictionary<PieceType, ulong> DEFAULT_PIECE_BITBOARDS = new()
        {
            { PieceType.Bishop, 0x2400000000000024 },
            { PieceType.King, 0x1000000000000010 },
            { PieceType.Knight, 0x4200000000000042 },
            { PieceType.Pawn, 0xff00000000ff00 },
            { PieceType.Queen, 0x800000000000008 },
            { PieceType.Rook, 0x8100000000000081 },
        };

        public BoardState() { }

        internal void CopyFrom(BoardState toMirror)
        {
            PieceBoards = (ulong[])toMirror.PieceBoards.Clone();
            Occupied = (ulong[])toMirror.Occupied.Clone();
        }

        internal ulong[] Occupied = new ulong[]
        {
            DEFAULT_WHITE,
            DEFAULT_BLACK,
            DEFAULT_ALL,
        };


        internal ulong[] PieceBoards { get; private set; } = new ulong[]
        {
            0,
            DEFAULT_PIECE_BITBOARDS[PieceType.King],
            DEFAULT_PIECE_BITBOARDS[PieceType.Queen],
            DEFAULT_PIECE_BITBOARDS[PieceType.Rook],
            DEFAULT_PIECE_BITBOARDS[PieceType.Bishop],
            DEFAULT_PIECE_BITBOARDS[PieceType.Knight],
            DEFAULT_PIECE_BITBOARDS[PieceType.Pawn],
        };

        internal ulong GetPieceBoard(PieceType type, Side side) => PieceBoards[(int)type] & OwnBlockers(side);

        internal ulong OwnBlockers(Side sideToMove) => Occupied[(int)sideToMove];

        internal ulong EnemyBlockers(Side sideToMove) => Occupied[(int)sideToMove ^ 1];

        internal void AddPiece(PieceType type, Side side, int square)
        {
            Debug.Assert(square >= 0 && square <= 63, "Square number must be between 0 and 63");
            RemovePieceAt(square);
            Occupied[2] = Occupied[2].SetAt(square);
            PieceBoards[(int)type] = PieceBoards[(int)type].SetAt(square);
            Occupied[(int)side] = Occupied[(int)side].SetAt(square);
        }

        internal void Clear()
        {
            Occupied = new ulong[] { 0, 0, 0 };
            PieceBoards = new ulong[] { 0, 0, 0, 0, 0, 0, 0, };
        }

        internal int GetOwnKingSquare(Side side) => BitOperations.TrailingZeroCount(PieceBoards[1] & Occupied[(int)side]);

        internal int GetEnemyKingSquare(Side side) => BitOperations.TrailingZeroCount(PieceBoards[1] & Occupied[(int)side ^ 1]);

        internal PieceType GetPieceTypeAt(int square)
        {
            Debug.Assert(square >= 0 && square <= 64);

            if (PieceBoards[6].OccupiedAt(square)) return PieceType.Pawn;
            if (PieceBoards[5].OccupiedAt(square)) return PieceType.Knight;
            if (PieceBoards[4].OccupiedAt(square)) return PieceType.Bishop;
            if (PieceBoards[3].OccupiedAt(square)) return PieceType.Rook;
            if (PieceBoards[2].OccupiedAt(square)) return PieceType.Queen;
            if (PieceBoards[1].OccupiedAt(square)) return PieceType.King;

            return PieceType.None;
        }

        internal Side GetPieceSideAt(int square)
        {
            Debug.Assert(square >= 0 && square <= 64);

            if (Occupied[0].OccupiedAt(square)) return Side.White;
            if (Occupied[1].OccupiedAt(square)) return Side.Black;

            return Side.None;
        }

        internal bool IsOccupied(int square) => Occupied[2].OccupiedAt(square);

        internal bool IsOccupied(string square)
        {
            var idx = Coordinates.SquareToIdx(square);
            Debug.Assert(idx >= 0 && idx <= 63, $"Invalid square: {square}");
            return IsOccupied(idx);
        }

#if DEBUG
        public void Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine("  ┌───┬───┬───┬───┬───┬───┬───┬───┐");

            for (var rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} ");

                for (var file = 0; file < 8; file++)
                {
                    var isEmpty = true;
                    var squareIdx = rank * 8 + file;

                    if (Occupied[2].OccupiedAt(squareIdx))
                    {
                        var character = GetPieceTypeAt(squareIdx) switch
                        {
                            PieceType.Bishop => Occupied[0].OccupiedAt(squareIdx) ? "│ B " : "│ b ",
                            PieceType.King => Occupied[0].OccupiedAt(squareIdx) ? "│ K " : "│ k ",
                            PieceType.Knight => Occupied[0].OccupiedAt(squareIdx) ? "│ N " : "│ n ",
                            PieceType.Pawn => Occupied[0].OccupiedAt(squareIdx) ? "│ P " : "│ p ",
                            PieceType.Queen => Occupied[0].OccupiedAt(squareIdx) ? "│ Q " : "│ q ",
                            PieceType.Rook => Occupied[0].OccupiedAt(squareIdx) ? "│ R " : "│ r ",
                            _ => "",
                        };
                        sb.Append(character);
                        isEmpty = false;
                    }

                    if (isEmpty)
                    {
                        sb.Append("│   ");
                    }
                }

                sb.Append("|\n");

                if (rank > 0)
                {
                    sb.AppendLine("  ├───┼───┼───┼───┼───┼───┼───┼───┤");
                }
                else
                {
                    sb.AppendLine("  └───┴───┴───┴───┴───┴───┴───┴───┘");
                    sb.AppendLine("    a   b   c   d   e   f   g   h");
                }
            }

            Debug.WriteLine(sb.ToString());
        }
#endif

        internal void RemovePieceAt(string square)
        {
            var idx = Coordinates.SquareToIdx(square);
            Debug.Assert(idx >= 0 && idx <= 63, $"Invalid square: {square}");
            RemovePieceAt(idx);
        }

        internal void RemovePieceAt(int square)
        {
            var mask = 1UL << square;
            Occupied[0] &= ~mask;
            Occupied[1] &= ~mask;
            Occupied[2] &= ~mask;
            PieceBoards[1] &= ~mask;
            PieceBoards[2] &= ~mask;
            PieceBoards[3] &= ~mask;
            PieceBoards[4] &= ~mask;
            PieceBoards[5] &= ~mask;
            PieceBoards[6] &= ~mask;
        }

        internal void Reset()
        {
            Occupied = new ulong[]
            {
                DEFAULT_WHITE,
                DEFAULT_BLACK,
                DEFAULT_ALL,
            };

            PieceBoards = new ulong[]
            {
                0,
                DEFAULT_PIECE_BITBOARDS[PieceType.King],
                DEFAULT_PIECE_BITBOARDS[PieceType.Queen],
                DEFAULT_PIECE_BITBOARDS[PieceType.Rook],
                DEFAULT_PIECE_BITBOARDS[PieceType.Bishop],
                DEFAULT_PIECE_BITBOARDS[PieceType.Knight],
                DEFAULT_PIECE_BITBOARDS[PieceType.Pawn],
            };
        }

        internal void AddPieceUnsafe(PieceType type, Side side, int square)
        {
            RemovePieceAt(square);
            Occupied[2] = Occupied[2].SetAt(square);
            PieceBoards[(int)type] = PieceBoards[(int)type].SetAt(square);
            Occupied[(int)side] = Occupied[(int)side].SetAt(square);
        }
    }
}
