using System.Text;

namespace RV.Chess.Board
{
    internal static class SanGenerator
    {
        /// <summary>
        /// Fills the moves with SAN notation
        /// </summary>
        /// <param name="moves">List of legal moves (objects are mutated during processing)</param>
        internal static void Generate(IEnumerable<Move> moves)
        {
            var sb = new StringBuilder();

            foreach (var m in moves)
            {
                if (m.IsCastling)
                {
                    // castling moves
                    if (m.ToIdx > m.FromIdx)
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
                            sb.Append("#");
                        }
                        else
                        {
                            sb.Append("+");
                        }
                    }
                }
                else
                {
                    // normal moves
                    if (m.Piece.Type != PieceType.Pawn)
                    {
                        sb.Append(m.Piece.TypeChar);

                        var alternativeAttackers = 0UL;

                        foreach (var mb in moves)
                        {
                            if (m.Piece.Type == mb.Piece.Type && m.ToIdx == mb.ToIdx && m.FromIdx != mb.FromIdx)
                            {
                                alternativeAttackers |= mb.FromMask;
                            }
                        }

                        var disambiguateByFile = false;
                        var disambiguateByRank = false;

                        if (alternativeAttackers > 0)
                        {
                            var fileMask = Movement.FILE_MASKS[m.FromIdx % 8];
                            var rankMask = Movement.RANK_MASKS[m.FromIdx / 8];

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
                            sb.Append(Chessboard.SquareIdxToFile(m.FromIdx));
                        }

                        if (disambiguateByRank)
                        {
                            sb.Append(Chessboard.SquareIdxToRank(m.FromIdx));
                        }

                        if (m.IsCapture)
                        {
                            sb.Append('x');
                        }
                    }
                    else if (m.IsCapture)
                    {
                        sb.Append(m.From[0]);
                        sb.Append('x');
                    }

                    sb.Append(Chessboard.SquareIdxToFile(m.ToIdx));
                    sb.Append(Chessboard.SquareIdxToRank(m.ToIdx));

                    if (m.PromoteTo != PieceType.None)
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
                }

                m.SetSan(sb.ToString());
                sb.Clear();
            }
        }
    }
}
