using System.Collections.Generic;

namespace RandomizerSharp.Constants
{
    public class GlobalConstants
    {
        public static IReadOnlyList<bool> BannedForDamagingMove { get; }

        public static IReadOnlyList<bool> BannedRandomMoves { get; }

        public static IReadOnlyList<int> BattleTrappingAbilities { get; } = new[] { 23, 42, 71 };

        public static readonly IReadOnlyList<int> DoubleHitMoves = new[] { 155, 458, 24, 530, 544, 41 }
            ; // Twineedle -  Gear Grind -  Dual Chop -  Double Kick -  Double Hit -  Bonemerang

        public const int MetronomeMove = 118;

        public const int MinDamagingMovePower = 50;

        public static IReadOnlyList<int> NegativeAbilities { get; } = new[] { 129, 112, 54, 59, 161 };

        public static IReadOnlyList<int> NormalMultihitMoves { get; } =
            new[] { 292, 140, 198, 331, 4, 3, 31, 154, 333, 42, 350, 131, 541 };

        public const int TripleKickIndex = 167;
        // Defeatist, Slow Start, Truant, Forecast, Zen Mode
        // To test: Illusion, Imposter

        public const int WonderGuardIndex = 25;

        static GlobalConstants()
        {
            var bannedRandomMoves = new bool[560];
            var bannedForDamagingMove = new bool[560];

            bannedRandomMoves[144] = true; // Transform, glitched in RBY
            bannedRandomMoves[165] = true; // Struggle, self explanatory

            bannedForDamagingMove[120] = true; // SelfDestruct
            bannedForDamagingMove[138] = true; // Dream Eater
            bannedForDamagingMove[153] = true; // Explosion
            bannedForDamagingMove[173] = true; // Snore
            bannedForDamagingMove[206] = true; // False Swipe
            bannedForDamagingMove[248] = true; // Future Sight
            bannedForDamagingMove[252] = true; // Fake Out
            bannedForDamagingMove[264] = true; // Focus Punch
            bannedForDamagingMove[353] = true; // Doom Desire
            bannedForDamagingMove[364] = true; // Feint
            bannedForDamagingMove[387] = true; // Last Resort
            bannedForDamagingMove[389] = true; // Sucker Punch

            // new 160
            bannedForDamagingMove[132] = true; // Constrict, overly weak
            bannedForDamagingMove[99] = true; // Rage, lock-in in gen1
            bannedForDamagingMove[205] = true; // Rollout, lock-in
            bannedForDamagingMove[301] = true; // Ice Ball, Rollout clone

            // make sure these cant roll
            bannedForDamagingMove[39] = true; // Sonicboom
            bannedForDamagingMove[82] = true; // Dragon Rage
            bannedForDamagingMove[32] = true; // Horn Drill
            bannedForDamagingMove[12] = true; // Guillotine
            bannedForDamagingMove[90] = true; // Fissure
            bannedForDamagingMove[329] = true; // Sheer Cold

            BannedRandomMoves = bannedRandomMoves;
            BannedForDamagingMove = bannedForDamagingMove;
        }
    }
}