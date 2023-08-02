using RV.Chess.Shared.Types;

namespace RV.Chess.PGN.Tree
{
    public class ChessTreeMove : IEquatable<ChessTreeMove>
    {
        public int Id { get; set; }

        public int MoveNumber { get; set; }

        public Side Side { get; set; }

        public bool IsNullMove { get; set; }

        public string San { get; set; } = string.Empty;

        public string Annotation { get; set; } = string.Empty;

        public List<string> Comments { get; set; } = new();

        public bool Equals(ChessTreeMove? other)
        {
            return San == other?.San && Side == other?.Side && MoveNumber == other?.MoveNumber;
        }

        public override string ToString() => Side == Side.White ? $"{MoveNumber}.{San}" : $"{MoveNumber}...{San}";

        public override bool Equals(object? obj)
        {
            return Equals(obj as ChessTreeMove);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, MoveNumber, Side, IsNullMove, San);
        }

        public static bool operator ==(ChessTreeMove? m1, ChessTreeMove? m2)
        {
            if (m1 is null || m2 is null)
            {
                return false;
            }

            return m1.Equals(m2);
        }

        public static bool operator !=(ChessTreeMove? m1, ChessTreeMove? m2)
        {
            if (m1 is null || m2 is null)
            {
                return true;
            }

            return m1.Equals(m2);
        }
    }
}
