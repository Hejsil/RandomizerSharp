using System.Collections.Generic;

namespace RandomizerSharp.Constants
{
    public class GlobalConstants
    {
        // Defeatist, Slow Start, Truant, Forecast, Zen Mode
        // To test: Illusion, Imposter

        public const int WonderGuardIndex = 25;

        public const int MinDamagingMovePower = 50;

        public const int MetronomeMove = 118;

        public const int TripleKickIndex = 167;

        public static readonly bool[] BannedRandomMoves = new bool[560], BannedForDamagingMove = new bool[560];

        /* @formatter:off */
        public static readonly IList<int> NormalMultihitMoves =
                new List<int> {292, 140, 198, 331, 4, 3, 31, 154, 333, 42, 350, 131, 541}
            ; // Tail Slap -  Spike Cannon -  Rock Blast -  Pin Missile -  Icicle Spear -  Fury Swipes -  Fury Attack -  DoubleSlap -  Comet Punch -  Bullet Seed -  Bone Rush -  Barrage -  Arm Thrust

        public static readonly IList<int> DoubleHitMoves = new List<int> {155, 458, 24, 530, 544, 41}
            ; // Twineedle -  Gear Grind -  Dual Chop -  Double Kick -  Double Hit -  Bonemerang


        /* @formatter:on */

        public static readonly IList<int> BattleTrappingAbilities = new List<int> {23, 42, 71};

        public static readonly IList<int> NegativeAbilities = new List<int> {129, 112, 54, 59, 161};

        static GlobalConstants()
        {
            BannedRandomMoves[144] = true; // Transform, glitched in RBY
            BannedRandomMoves[165] = true; // Struggle, self explanatory

            BannedForDamagingMove[120] = true; // SelfDestruct
            BannedForDamagingMove[138] = true; // Dream Eater
            BannedForDamagingMove[153] = true; // Explosion
            BannedForDamagingMove[173] = true; // Snore
            BannedForDamagingMove[206] = true; // False Swipe
            BannedForDamagingMove[248] = true; // Future Sight
            BannedForDamagingMove[252] = true; // Fake Out
            BannedForDamagingMove[264] = true; // Focus Punch
            BannedForDamagingMove[353] = true; // Doom Desire
            BannedForDamagingMove[364] = true; // Feint
            BannedForDamagingMove[387] = true; // Last Resort
            BannedForDamagingMove[389] = true; // Sucker Punch

            // new 160
            BannedForDamagingMove[132] = true; // Constrict, overly weak
            BannedForDamagingMove[99] = true; // Rage, lock-in in gen1
            BannedForDamagingMove[205] = true; // Rollout, lock-in
            BannedForDamagingMove[301] = true; // Ice Ball, Rollout clone

            // make sure these cant roll
            BannedForDamagingMove[39] = true; // Sonicboom
            BannedForDamagingMove[82] = true; // Dragon Rage
            BannedForDamagingMove[32] = true; // Horn Drill
            BannedForDamagingMove[12] = true; // Guillotine
            BannedForDamagingMove[90] = true; // Fissure
            BannedForDamagingMove[329] = true; // Sheer Cold
        }
    }
}