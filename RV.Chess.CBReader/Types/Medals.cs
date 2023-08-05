namespace RV.Chess.CBReader.Types
{
    [Flags]
    public enum Medals
    {
        None = 0,
        BestGame = 1,
        DecidedTournament = 2,
        ModelGame = 4,
        Novelty = 8,
        PawnStructure = 16,
        Strategy = 32,
        Tactics = 64,
        WithAttack = 128,
        Sacrifice = 256,
        Defense = 512,
        Material = 1024,
        PiecePlay = 2048,
        Endgame = 4096,
        TacticalBlunder = 8192,
        StrategicalBlunder = 16384,
        User = 32768,
    }
}
