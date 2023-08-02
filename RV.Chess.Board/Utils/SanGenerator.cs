using System.Text;
using RV.Chess.Board.Game;
using RV.Chess.Board.Types;
using RV.Chess.Shared.Types;

namespace RV.Chess.Board.Utils
{
    internal static class SanGenerator
    {
        internal static string Generate(FastMove m, Span<FastMove> allLegal)
        {
            var sb = new StringBuilder();

            if (m.IsCastling)
            {
                if (m.To > m.From)
                {
                    sb.Append("O-O");
                }
                else
                {
                    sb.Append("O-O-O");
                }

                if (m.IsCheck)
                {
                    if (m.IsMate)
                    {
                        sb.Append('#');
                    }
                    else
                    {
                        sb.Append('+');
                    }
                }

                return sb.ToString();
            }
            else if (!m.IsPawn)
            {
                sb.Append(m.Type.ToPieceType().TypeChar());

                var alternativeAttackers = 0UL;

                foreach (var fm in allLegal)
                {
                    if (m.Type == fm.Type && m.To == fm.To && m.From != fm.From)
                    {
                        alternativeAttackers |= fm.FromMask;
                    }
                }

                var disambiguateByFile = false;
                var disambiguateByRank = false;

                if (alternativeAttackers > 0)
                {
                    var fileMask = Movement.FileMasks[m.From % 8];
                    var rankMask = Movement.RanksMasks[m.From / 8];

                    if ((rankMask & alternativeAttackers) > 0)
                    {
                        disambiguateByFile = true;
                    }

                    if ((fileMask & alternativeAttackers) > 0)
                    {
                        disambiguateByRank = true;
                    }
                    else
                    {
                        disambiguateByFile = true;
                    }
                }

                if (disambiguateByFile)
                {
                    sb.Append(Coordinates.SquareIdxToFile(m.From));
                }

                if (disambiguateByRank)
                {
                    sb.Append(Coordinates.SquareIdxToRank(m.From));
                }

                if (m.IsCapture)
                {
                    sb.Append('x');
                }
            }
            else if (m.IsCapture)
            {
                // pawn captures do not use source rank in the san
                sb.Append(Coordinates.IdxToSquare(m.From)[0]);
                sb.Append('x');
            }

            sb.Append(Coordinates.SquareIdxToFile(m.To));
            sb.Append(Coordinates.SquareIdxToRank(m.To));

            if (m.IsPromotion)
            {
                sb.Append('=');
                sb.Append(m.PromotionChar);
            }

            if (m.IsCheck)
            {
                if (m.IsMate)
                {
                    sb.Append('#');
                }
                else
                {
                    sb.Append('+');
                }
            }

            return sb.ToString();
        }
    }
}
