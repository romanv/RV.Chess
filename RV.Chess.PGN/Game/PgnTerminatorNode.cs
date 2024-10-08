﻿using RV.Chess.Shared.Types;

namespace RV.Chess.PGN;

public class PgnTerminatorNode(GameResult terminator) : PgnNode
{
    public GameResult Terminator { get; } = terminator;

    public override PgnNodeKind Kind => PgnNodeKind.Terminator;

    public override string ToString()
    {
        return Terminator switch
        {
            GameResult.White => "1-0",
            GameResult.Black => "0-1",
            GameResult.Tie => "1/2-1/2",
            _ => "*",
        };
    }
}
