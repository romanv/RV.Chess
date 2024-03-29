﻿namespace RV.Chess.CBReader.Entities.Annotations
{
    enum NagType
    {
        Comment,
        Prefix,
        Eval,
    }

    class NagTypeAttribute : Attribute
    {
        public NagTypeAttribute(NagType type, string stringValue = "")
        {
            Type = type;
            StringValue = stringValue;
        }

        public NagType Type { get; }
        public string StringValue { get; }
    }

    // https://en.wikipedia.org/wiki/Numeric_Annotation_Glyphs
    public enum Nag
    {
        Null,
        [NagTypeAttribute(NagType.Comment, "!")]
        GoodMove,
        [NagTypeAttribute(NagType.Comment, "?")]
        BadMove,
        [NagTypeAttribute(NagType.Comment, "!!")]
        BrilliantMove,
        [NagTypeAttribute(NagType.Comment, "??")]
        Blunder,
        [NagTypeAttribute(NagType.Comment, "!?")]
        InterestingMove,
        [NagTypeAttribute(NagType.Comment, "?!")]
        DubiousMove,
        [NagTypeAttribute(NagType.Comment, "□")]
        ForcedMove,
        [NagTypeAttribute(NagType.Comment)]
        SingularMove,
        [NagTypeAttribute(NagType.Comment)]
        WorstMove,
        [NagTypeAttribute(NagType.Eval, "=")]
        PositionEven,
        [NagTypeAttribute(NagType.Eval)]
        PositionQuietEqual,
        [NagTypeAttribute(NagType.Eval)]
        PositionActiveEqual,
        [NagTypeAttribute(NagType.Eval, "∞")]
        PositionUnclear,
        [NagTypeAttribute(NagType.Eval, "⩲")]
        WhiteSlightAdvantage,
        [NagTypeAttribute(NagType.Eval, "⩱")]
        BlackSlightAdvantage,
        [NagTypeAttribute(NagType.Eval, "±")]
        WhiteModerateAdvantage,
        [NagTypeAttribute(NagType.Eval, "∓")]
        BlackModerateAdvantage,
        [NagTypeAttribute(NagType.Eval, "+−")]
        WhiteDecisiveAdvantage,
        [NagTypeAttribute(NagType.Eval, "−+")]
        BlackDecisiveAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        WhiteCrushingAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        BlackCrushingAdvantage,
        [NagTypeAttribute(NagType.Comment, "⨀")]
        WhiteZugzwang,
        [NagTypeAttribute(NagType.Comment, "⨀")]
        BlackZugzwang,
        WhiteSlightSpaceAdvantage,
        BlackSlightSpaceAdvantage,
        [NagTypeAttribute(NagType.Eval, "○")]
        WhiteModerateSpaceAdvantage,
        [NagTypeAttribute(NagType.Eval, "○")]
        BlackModerateSpaceAdvantage,
        WhiteDecisiveSpaceAdvantage,
        BlackDecisiveSpaceAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        WhiteSlightDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        BlackSlightDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval, "⟳")]
        WhiteModerateDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval, "⟳")]
        BlackModerateDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        WhiteDecisiveDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval)]
        BlackDecisiveDevelopmentAdvantage,
        [NagTypeAttribute(NagType.Eval, "↑")]
        WhiteInitiative,
        [NagTypeAttribute(NagType.Eval, "↑")]
        BlackInitiative,
        [NagTypeAttribute(NagType.Eval)]
        WhiteLastingInitiative,
        [NagTypeAttribute(NagType.Eval)]
        BlackLastingInitiative,
        [NagTypeAttribute(NagType.Eval, "→")]
        WhiteAttack,
        [NagTypeAttribute(NagType.Eval, "→")]
        BlackAttack,
        [NagTypeAttribute(NagType.Eval)]
        WhiteInsufficientCompensation,
        [NagTypeAttribute(NagType.Eval)]
        BlackInsufficientCompensation,
        [NagTypeAttribute(NagType.Eval)]
        WhiteSufficientCompensation,
        [NagTypeAttribute(NagType.Eval)]
        BlackSufficientCompensation,
        [NagTypeAttribute(NagType.Eval)]
        WhiteGoodCompensation,
        [NagTypeAttribute(NagType.Eval)]
        BlackGoodCompensation,
        WhiteSlightCenterControlAdvantage,
        BlackSlightCenterControlAdvantage,
        WhiteModerateCenterControlAdvantage,
        BlackModerateCenterControlAdvantage,
        WhiteDecisiveCenterControlAdvantage,
        BlackDecisiveCenterControlAdvantage,
        WhiteSlightKingsideControlAdvantage,
        BlackSlightKingsideControlAdvantage,
        WhiteModerateKingsideControlAdvantage,
        BlackModerateKingsideControlAdvantage,
        WhiteDecisiveKingsideControlAdvantage,
        BlackDecisiveKingsideControlAdvantage,
        WhiteSlightQueensideControlAdvantage,
        BlackSlightQueensideControlAdvantage,
        WhiteModerateQueensideControlAdvantage,
        BlackModerateQueensideControlAdvantage,
        WhiteDecisiveQueensideControlAdvantage,
        BlackDecisiveQueensideControlAdvantage,
        WhiteVulnerableFirstRank,
        BlackVulnerableFirstRank,
        WhiteWellProtectedFirstRank,
        BlackWellProtectedFirstRank,
        WhitePoorlyProtectedKing,
        BlackPoorlyProtectedKing,
        WhiteWellProtectedKing,
        BlackWellProtectedKing,
        WhitePoorlyPlacedKing,
        BlackPoorlyPlacedKing,
        WhiteWellPlacedKing,
        BlackWellPlacedKing,
        WhiteVeryWeakPawnStructure,
        BlackVeryWeakPawnStructure,
        WhiteModeratelyWeakPawnStructure,
        BlackModeratelyWeakPawnStructure,
        WhiteModeratelyStrongPawnStructure,
        BlackModeratelyStrongPawnStructure,
        WhiteVeryStrongPawnStructure,
        BlackVeryStrongPawnStructure,
        WhitePoorKnightPlacement,
        BlackPoorKnightPlacement,
        WhiteGoodKnightPlacement,
        BlackGoodKnightPlacement,
        WhitePoorBishopPlacement,
        BlackPoorBishopPlacement,
        WhiteGoodBishopPlacement,
        BlackGoodBishopPlacement,
        WhitePoorRookPlacement,
        BlackPoorRookPlacement,
        WhiteGoodRookPlacement,
        BlackGoodRookPlacement,
        WhitePoorQueenPlacement,
        BlackPoorQueenPlacement,
        WhiteGoodQueenPlacement,
        BlackGoodQueenPlacement,
        WhitePoorPieceCoordination,
        BlackPoorPieceCoordination,
        WhiteGoodPieceCoordination,
        BlackGoodPieceCoordination,
        WhitePlayedOpeningVeryPoorly,
        BlackPlayedOpeningVeryPoorly,
        WhitePlayedOpeningPoorly,
        BlackPlayedOpeningPoorly,
        WhitePlayedOpeningWell,
        BlackPlayedOpeningWell,
        WhitePlayedOpeningVeryWell,
        BlackPlayedOpeningVeryWell,
        WhitePlayedMiddlegameVeryPoorly,
        BlackPlayedMiddlegameVeryPoorly,
        WhitePlayedMiddlegamePoorly,
        BlackPlayedMiddlegamePoorly,
        WhitePlayedMiddlegameWell,
        BlackPlayedMiddlegameWell,
        WhitePlayedMiddlegameVeryWell,
        BlackPlayedMiddlegameVeryWell,
        WhitePlayedEndingVeryPoorly,
        BlackPlayedEndingVeryPoorly,
        WhitePlayedEndingPoorly,
        BlackPlayedEndingPoorly,
        WhitePlayedEndingWell,
        BlackPlayedEndingWell,
        WhitePlayedEndingVeryWell,
        BlackPlayedEndingVeryWell,
        [NagTypeAttribute(NagType.Eval)]
        WhiteSlightCounterplay,
        [NagTypeAttribute(NagType.Eval)]
        BlackSlightCounterplay,
        [NagTypeAttribute(NagType.Eval, "⇆")]
        WhiteModerateCounterplay,
        [NagTypeAttribute(NagType.Eval, "⇆")]
        BlackModerateCounterplay,
        [NagTypeAttribute(NagType.Eval)]
        WhiteDecisiveCounterplay,
        [NagTypeAttribute(NagType.Eval)]
        BlackDecisiveCounterplay,
        WhiteModerateTimePressure,
        BlackModerateTimePressure,
        WhiteZeitnot,
        BlackZeitnot,
    }
}
