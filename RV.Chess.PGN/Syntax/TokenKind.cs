namespace RV.Chess.PGN
{
    // https://github.com/fsmosca/PGN-Standard/blob/master/PGN-Standard.txt

    internal enum TokenKind
    {
        String,
        Integer,
        Period,
        Asterisk,
        GameTerminator,
        SquareBracketOpen,
        SquareBracketClose,
        ParenthesisOpen,
        ParenthesisClose,
        AngleBracketOpen,
        AngleBracketClose,
        Semicolon,
        NewLine,
        Comment,
        NumericAnnotationGlyph,
        Symbol,
        Invalid,
        EndOfFile,
        CurlyBracketOpen,
        CurlyBracketClose,
        Annotation,
        Whitespace,
    }
}
