using System.Collections.Generic;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.Constants
{
    public class Gen5Constants
    {
        public static ItemList AllowedItems => AllowedItems1.Copy();

        public const int B2Route4EncounterFile = 104,
            B2VrExclusiveRoom1 = 71,
            B2VrExclusiveRoom2 = 73,
            B2ReversalMountainStart = 49,
            B2ReversalMountainEnd = 54;

        public const int BsHpOffset = 0,
            BsAttackOffset = 1,
            BsDefenseOffset = 2,
            BsSpeedOffset = 3,
            BsSpAtkOffset = 4,
            BsSpDefOffset = 5,
            BsPrimaryTypeOffset = 6,
            BsSecondaryTypeOffset = 7,
            BsCatchRateOffset = 8,
            BsCommonHeldItemOffset = 12,
            BsRareHeldItemOffset = 14,
            BsDarkGrassHeldItemOffset = 16,
            BsGrowthCurveOffset = 21,
            BsAbility1Offset = 24,
            BsAbility2Offset = 25,
            BsAbility3Offset = 26,
            BsTmhmCompatOffset = 40,
            BsMtCompatOffset = 60;

        public static readonly int[] Bw1EarlyRequiredHmMoves = new[] { 15 };

        public const string Bw1ItemPalettesPrefix = "E903EA03020003000400050006000700",
            Bw2ItemPalettesPrefix = "FD03FE03020003000400050006000700";

        public static readonly byte[] Bw1NewStarterScript = new byte[]
        {
            0x24,
            0x00,
            0xA7,
            0x02,
            0xE7,
            0x00,
            0x00,
            0x00,
            0xDE,
            0x00,
            0x00,
            0x00,
            0xF8,
            0x01,
            0x05,
            0x00
        };

        public static readonly int[] Bw1RequiredFieldTMs = new[]
        {
            2,
            3,
            5,
            6,
            9,
            12,
            13,
            19,
            22,
            24,
            26,
            29,
            30,
            35,
            36,
            39,
            41,
            46,
            47,
            50,
            52,
            53,
            55,
            58,
            61,
            63,
            65,
            66,
            71,
            80,
            81,
            84,
            85,
            86,
            90,
            91,
            92,
            93
        };

        public const string Bw1StarterScriptMagic = "2400A702";

        public const int Bw1StarterTextOffset = 18, Bw1CherenText1Offset = 26, Bw1CherenText2Offset = 53;
        public static readonly int[] Bw2EarlyRequiredHmMoves = new int[0];

        public static readonly int[] Bw2HiddenHollowUnovaPokemon = {
            505,
            507,
            510,
            511,
            513,
            515,
            519,
            523,
            525,
            527,
            529,
            531,
            533,
            535,
            538,
            539,
            542,
            545,
            546,
            548,
            550,
            553,
            556,
            558,
            559,
            561,
            564,
            569,
            572,
            575,
            578,
            580,
            583,
            587,
            588,
            594,
            596,
            601,
            605,
            607,
            610,
            613,
            616,
            618,
            619,
            621,
            622,
            624,
            626,
            628,
            630,
            631,
            632
        };

        public const int Bw2MoveTutorCount = 60, Bw2MoveTutorBytesPerEntry = 12;

        public static readonly byte[] Bw2NewStarterScript = new byte[]
        {
            0x28,
            0x00,
            0xA1,
            0x40,
            0x04,
            0x00,
            0xDE,
            0x00,
            0x00,
            0x00,
            0xFD,
            0x01,
            0x05,
            0x00
        };

        public static readonly int[] Bw2RequiredFieldTMs = new[]
        {
            1,
            2,
            3,
            5,
            6,
            12,
            13,
            19,
            22,
            26,
            28,
            29,
            30,
            36,
            39,
            41,
            46,
            47,
            50,
            52,
            53,
            56,
            58,
            61,
            63,
            65,
            66,
            67,
            69,
            71,
            80,
            81,
            84,
            85,
            86,
            90,
            91,
            92,
            93
        };

        public const int Bw2Route4AreaIndex = 40,
            Bw2VictoryRoadAreaIndex = 76,
            Bw2ReversalMountainAreaIndex = 73;

        public const string Bw2StarterScriptMagic = "2800A1400400";

        public const int Bw2StarterTextOffset = 37, Bw2RivalTextOffset = 60;

        public static readonly int[] EncountersOfEachType = new[] { 12, 12, 12, 5, 5, 5, 5 };

        public static readonly string[] EncounterTypeNames = new[]
        {
            "Grass/Cave",
            "Doubles Grass",
            "Shaking Spots",
            "Surfing",
            "Surfing Spots",
            "Fishing",
            "Fishing Spots"
        };

        public const int EvolutionMethodCount = 27;
        public static readonly int[] FieldMoves = new[] { 15, 19, 57, 70, 148, 91, 100, 127, 230, 291 };

        public static readonly int[] HabitatClassificationOfEachType = new[] { 0, 0, 0, 1, 1, 2, 2 };

        public const int HighestAbilityIndex = 123;

        public const int LuckyEggIndex = 0xE7;

        public static readonly MoveCategory[] MoveCategoryIndices = new[]
        {
            MoveCategory.Status,
            MoveCategory.Physical,
            MoveCategory.Special
        };

        public static ItemList NonBadItems => NonBadItems1.Copy();

        public const int NormalItemSetVarCommand = 0x28,
            HiddenItemSetVarCommand = 0x2A,
            NormalItemVarSet = 0x800C,
            HiddenItemVarSet = 0x8000;

        public const int PerSeasonEncounterDataLength = 232,
            Bw2AreaDataEntryLength = 345,
            Bw2EncounterAreaCount = 85;

        public const int PokemonCount = 649, MoveCount = 559, NonUnovaPokemonCount = 493;

        public const int ScriptListTerminator = 0xFD13;

        public const int SlowpokeIndex = 79, KarrablastIndex = 588, ShelmetIndex = 616;

        public const int TmCount = 95,
            HmCount = 6,
            TmBlockOneCount = 92,
            TmBlockOneOffset = 328,
            TmBlockTwoOffset = 618;

        public const string TmDataPrefix = "87038803";
        public const int TypeBw = 0;
        public const int TypeBw2 = 1;

        public static readonly Typing[] TypeTable = new[]
        {
            Typing.Normal,
            Typing.Fighting,
            Typing.Flying,
            Typing.Poison,
            Typing.Ground,
            Typing.Rock,
            Typing.Bug,
            Typing.Ghost,
            Typing.Steel,
            Typing.Fire,
            Typing.Water,
            Typing.Grass,
            Typing.Electric,
            Typing.Psychic,
            Typing.Ice,
            Typing.Dragon,
            Typing.Dark
        };

        public const int W2Route4EncounterFile = 105,
            W2VrExclusiveRoom1 = 78,
            W2VrExclusiveRoom2 = 79,
            W2ReversalMountainStart = 55,
            W2ReversalMountainEnd = 60;

        public const int WaterStoneIndex = 84;

        public static readonly int[] WildFileToAreaMap = new[]
        {
            2,
            4,
            8,
            59,
            61,
            63,
            19,
            19,
            20,
            20,
            21,
            21,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            24,
            24,
            24,
            25,
            25,
            25,
            25,
            26,
            26,
            26,
            26,
            76,
            27,
            27,
            27,
            27,
            27,
            70,
            70,
            70,
            70,
            70,
            29,
            35,
            71,
            71,
            72,
            72,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            73,
            74,
            74,
            74,
            74,
            74,
            74,
            74,
            74,
            74,
            74,
            76,
            76,
            76,
            76,
            76,
            76,
            76,
            76,
            76,
            76,
            77,
            77,
            77,
            79,
            79,
            79,
            79,
            79,
            79,
            79,
            79,
            79,
            78,
            78,
            -1, // Nature Preserve (not on map)
            55,
            57,
            58,
            37,
            38,
            39,
            30,
            30,
            40,
            40,
            41,
            42,
            31,
            31,
            31,
            43,
            32,
            32,
            32,
            32,
            44,
            33,
            45,
            46,
            47,
            48,
            49,
            34,
            50,
            51,
            36,
            53,
            66,
            67,
            69,
            75,
            12,
            52,
            68
        };

        private static readonly ItemList AllowedItems1;

        // ReSharper disable once UnusedMember.Local
        private static readonly int[][] HabitatListEntries = new[]
        {
            new[] { 104, 105 }, // Route 4
            new[] { 124 }, // Route 15
            new[] { 134 }, // Route 21
            new[] { 84, 85, 86 }, // Clay Tunnel
            new[] { 23, 24, 25, 26 }, // Twist Mountain
            new[] { 97 }, // Village Bridge
            new[] { 27, 28, 29, 30 }, // Dragonspiral Tower
            new[] { 81, 82, 83 }, // Relic Passage
            new[] { 106 }, // Route 5*
            new[] { 125 }, // Route 16*
            new[] { 98 }, // Marvelous Bridge
            new[] { 123 }, // Abundant Shrine
            new[] { 132 }, // Undella Town
            new[] { 107 }, // Route 6
            new[] { 43 }, // Undella Bay
            new[] { 102, 103 }, // Wellspring Cave
            new[] { 95 }, // Nature Preserve
            new[] { 127 }, // Route 18
            new[] { 32, 33, 34, 35, 36 }, // Giant Chasm
            new[] { 111 }, // Route 7
            new[] { 31, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80 }, // Victory Road
            new[] { 12, 13, 14, 15, 16, 17, 18, 19 }, // Relic Castle
            new[] { 0 }, // Striation City
            new[] { 128 }, // Route 19
            new[] { 3 }, // Aspertia City
            new[] { 116 }, // Route 8*
            new[] { 44, 45 }, // Floccesy Ranch
            new[] { 61, 62, 63, 64, 65, 66, 67, 68, 69, 70 }, // Strange House
            new[] { 129 }, // Route 20
            new[] { 4 }, // Virbank City
            new[] { 37, 38, 39, 40, 41 }, // Castelia Sewers
            new[] { 118 }, // Route 9
            new[] { 46, 47 }, // Virbank Complex
            new[] { 42 }, // P2 Laboratory
            new[] { 1 }, // Castelia City
            new[] { 8, 9 }, // Pinwheel Forest
            new[] { 5 }, // Humilau City
            new[] { 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60 }, // Reversal Mountain
            new[] { 6, 7 }, // Dreamyard
            new[] { 112, 113, 114, 115 }, // Celestial Tower
            new[] { 130 }, // Route 22
            new[] { 10, 11 }, // Desert Resort
            new[] { 119 }, // Route 11
            new[] { 133 }, // Route 17
            new[] { 99 }, // Route 1
            new[] { 131 }, // Route 23
            new[] { 2 }, // Icirrus City*
            new[] { 120 }, // Route 12
            new[] { 100 }, // Route 2
            new[] { 108, 109 }, // Mistralton Cave
            new[] { 121 }, // Route 13
            new[] { 101 }, // Route 3
            new[] { 117 }, // Moor of Icirrus*
            new[] { 96 }, // Driftveil Drawbridge
            new[] { 93, 94 }, // Seaside Cave
            new[] { 126 }, // Lostlorn Forest
            new[] { 122 }, // Route 14
            new[] { 20, 21, 22 } // Chargestone Cave
        };

        private static readonly ItemList NonBadItems1;

        static Gen5Constants()
        {
            AllowedItems1 = new ItemList(638);

            // Key items + version exclusives
            AllowedItems1.BanRange(428, 109);
            AllowedItems1.BanRange(621, 18);
            AllowedItems1.BanSingles(574, 578, 579, 616, 617);

            // Unknown blank items or version exclusives
            AllowedItems1.BanRange(113, 3);
            AllowedItems1.BanRange(120, 14);

            // TMs & HMs - tms cant be held in gen5
            AllowedItems1.TmRange(328, 92);
            AllowedItems1.TmRange(618, 3);
            AllowedItems1.BanRange(328, 100);
            AllowedItems1.BanRange(618, 3);

            // Battle Launcher exclusives
            AllowedItems1.BanRange(592, 24);

            // non-bad items
            // ban specific pokemon hold items, berries, apricorns, mail
            NonBadItems1 = AllowedItems.Copy();
            NonBadItems1.BanSingles(0x6F, 0x70, 0xEC, 0x9B);
            NonBadItems1.BanRange(0x5F, 4); // mulch
            NonBadItems1.BanRange(0x87, 2); // orbs
            NonBadItems1.BanRange(0x89, 12); // mails
            NonBadItems1.BanRange(0x9F, 54); // berries DansGame
            NonBadItems1.BanRange(0x100, 4); // pokemon specific
            NonBadItems1.BanRange(0x104, 5); // contest scarves
        }

        public static byte MoveCategoryToByte(MoveCategory cat)
        {
            switch (cat)
            {
                case MoveCategory.Physical:
                    return 1;
                case MoveCategory.Special:
                    return 2;
                case MoveCategory.Status:
                    return 0;
                default:
                    return 0;
            }
        }

        public static byte TypeToByte(Typing type)
        {
            switch (type.InnerEnumValue)
            {
                case Typing.InnerEnum.Normal:
                    return 0x00;
                case Typing.InnerEnum.Fighting:
                    return 0x01;
                case Typing.InnerEnum.Flying:
                    return 0x02;
                case Typing.InnerEnum.Poison:
                    return 0x03;
                case Typing.InnerEnum.Ground:
                    return 0x04;
                case Typing.InnerEnum.Rock:
                    return 0x05;
                case Typing.InnerEnum.Bug:
                    return 0x06;
                case Typing.InnerEnum.Ghost:
                    return 0x07;
                case Typing.InnerEnum.Fire:
                    return 0x09;
                case Typing.InnerEnum.Water:
                    return 0x0A;
                case Typing.InnerEnum.Grass:
                    return 0x0B;
                case Typing.InnerEnum.Electric:
                    return 0x0C;
                case Typing.InnerEnum.Psychic:
                    return 0x0D;
                case Typing.InnerEnum.Ice:
                    return 0x0E;
                case Typing.InnerEnum.Dragon:
                    return 0x0F;
                case Typing.InnerEnum.Steel:
                    return 0x08;
                case Typing.InnerEnum.Dark:
                    return 0x10;
                default:
                    return 0; // normal by default
            }
        }

        /* @formatter:on */
        public static void TagTrainersBw(IList<Trainer> trs)
        {
            // We use different Gym IDs to cheat the system for the 3 n00bs
            // Chili, Cress, and Cilan
            // Cilan can be GYM1, then Chili is GYM9 and Cress GYM10
            // Also their *trainers* are GYM11 lol

            // Gym Trainers
            Tag(trs, "GYM11", 0x09, 0x0A);
            Tag(trs, "GYM2", 0x56, 0x57, 0x58);
            Tag(trs, "GYM3", 0xC4, 0xC6, 0xC7, 0xC8);
            Tag(trs, "GYM4", 0x42, 0x43, 0x44, 0x45);
            Tag(trs, "GYM5", 0xC9, 0xCA, 0xCB, 0x5F, 0xA8);
            Tag(trs, "GYM6", 0x7D, 0x7F, 0x80, 0x46, 0x47);
            Tag(trs, "GYM7", 0xD7, 0xD8, 0xD9, 0xD4, 0xD5, 0xD6);
            Tag(trs, "GYM8", 0x109, 0x10A, 0x10F, 0x10E, 0x110, 0x10B, 0x113, 0x112);

            // Gym Leaders
            Tag(trs, 0x0C, "GYM1"); // Cilan
            Tag(trs, 0x0B, "GYM9"); // Chili
            Tag(trs, 0x0D, "GYM10"); // Cress
            Tag(trs, 0x15, "GYM2"); // Lenora
            Tag(trs, 0x16, "GYM3"); // Burgh
            Tag(trs, 0x17, "GYM4"); // Elesa
            Tag(trs, 0x18, "GYM5"); // Clay
            Tag(trs, 0x19, "GYM6"); // Skyla
            Tag(trs, 0x83, "GYM7"); // Brycen
            Tag(trs, 0x84, "GYM8"); // Iris or Drayden
            Tag(trs, 0x85, "GYM8"); // Iris or Drayden

            // Elite 4
            Tag(trs, 0xE4, "ELITE1"); // Shauntal
            Tag(trs, 0xE6, "ELITE2"); // Grimsley
            Tag(trs, 0xE7, "ELITE3"); // Caitlin
            Tag(trs, 0xE5, "ELITE4"); // Marshal

            // Elite 4 R2
            Tag(trs, 0x233, "ELITE1"); // Shauntal
            Tag(trs, 0x235, "ELITE2"); // Grimsley
            Tag(trs, 0x236, "ELITE3"); // Caitlin
            Tag(trs, 0x234, "ELITE4"); // Marshal
            Tag(trs, 0x197, "CHAMPION"); // Alder

            // Ubers?
            Tag(trs, 0x21E, "UBER"); // Game Freak Guy
            Tag(trs, 0x237, "UBER"); // Cynthia
            Tag(trs, 0xE8, "UBER"); // Ghetsis
            Tag(trs, 0x24A, "UBER"); // N-White
            Tag(trs, 0x24B, "UBER"); // N-Black

            // Rival - Cheren
            TagRivalBw(trs, "RIVAL1", 0x35);
            TagRivalBw(trs, "RIVAL2", 0x11F);
            TagRivalBw(trs, "RIVAL3", 0x38); // used for 3rd battle AND tag battle
            TagRivalBw(trs, "RIVAL4", 0x193);
            TagRivalBw(trs, "RIVAL5", 0x5A); // 5th battle & 2nd tag battle
            TagRivalBw(trs, "RIVAL6", 0x21B);
            TagRivalBw(trs, "RIVAL7", 0x24C);
            TagRivalBw(trs, "RIVAL8", 0x24F);

            // Rival - Bianca
            TagRivalBw(trs, "FRIEND1", 0x3B);
            TagRivalBw(trs, "FRIEND2", 0x1F2);
            TagRivalBw(trs, "FRIEND3", 0x1FB);
            TagRivalBw(trs, "FRIEND4", 0x1EB);
            TagRivalBw(trs, "FRIEND5", 0x1EE);
            TagRivalBw(trs, "FRIEND6", 0x252);
        }

        public static void TagTrainersBw2(IList<Trainer> trs)
        {
            // Use GYM9/10/11 for the retired Chili/Cress/Cilan.
            // Lenora doesn't have a team, or she'd be 12.
            // Likewise for Brycen

            // Some trainers have TWO teams because of Challenge Mode
            // I believe this is limited to Gym Leaders, E4, Champ...
            // The "Challenge Mode" teams have levels at similar to regular,
            // but have the normal boost applied too.

            // Gym Trainers
            Tag(trs, "GYM1", 0xab, 0xac);
            Tag(trs, "GYM2", 0xb2, 0xb3);
            Tag(trs, "GYM3", 0x2de, 0x2df, 0x2e0, 0x2e1);
            // GYM4: old gym site included to give the city a theme
            Tag(trs, "GYM4", 0x26d, 0x94, 0xcf, 0xd0, 0xd1); // 0x94 might be 0x324
            Tag(trs, "GYM5", 0x13f, 0x140, 0x141, 0x142, 0x143, 0x144, 0x145);
            Tag(trs, "GYM6", 0x95, 0x96, 0x97, 0x98, 0x14c);
            Tag(trs, "GYM7", 0x17d, 0x17e, 0x17f, 0x180, 0x181);
            Tag(trs, "GYM8", 0x15e, 0x15f, 0x160, 0x161, 0x162, 0x163);

            // Gym Leaders
            // Order: Normal, Challenge Mode
            // All the challenge mode teams are near the end of the ROM
            // which makes things a bit easier.
            Tag(trs, "GYM1", 0x9c, 0x2fc); // Cheren
            Tag(trs, "GYM2", 0x9d, 0x2fd); // Roxie
            Tag(trs, "GYM3", 0x9a, 0x2fe); // Burgh
            Tag(trs, "GYM4", 0x99, 0x2ff); // Elesa
            Tag(trs, "GYM5", 0x9e, 0x300); // Clay
            Tag(trs, "GYM6", 0x9b, 0x301); // Skyla
            Tag(trs, "GYM7", 0x9f, 0x302); // Drayden
            Tag(trs, "GYM8", 0xa0, 0x303); // Marlon

            // Elite 4 / Champion
            // Order: Normal, Challenge Mode, Rematch, Rematch Challenge Mode
            Tag(trs, "ELITE1", 0x26, 0x304, 0x8f, 0x309);
            Tag(trs, "ELITE2", 0x28, 0x305, 0x91, 0x30a);
            Tag(trs, "ELITE3", 0x29, 0x307, 0x92, 0x30c);
            Tag(trs, "ELITE4", 0x27, 0x306, 0x90, 0x30b);
            Tag(trs, "CHAMPION", 0x155, 0x308, 0x218, 0x30d);

            // Rival - Hugh
            TagRivalBw(trs, "RIVAL1", 0xa1); // Start
            TagRivalBw(trs, "RIVAL2", 0xa6); // Floccessy Ranch
            TagRivalBw(trs, "RIVAL3", 0x24c); // Tag Battles in the sewers
            TagRivalBw(trs, "RIVAL4", 0x170); // Tag Battle on the Plasma Frigate
            TagRivalBw(trs, "RIVAL5", 0x17a); // Undella Town 1st visit
            TagRivalBw(trs, "RIVAL6", 0x2bd); // Lacunosa Town Tag Battle
            TagRivalBw(trs, "RIVAL7", 0x31a); // 2nd Plasma Frigate Tag Battle
            TagRivalBw(trs, "RIVAL8", 0x2ac); // Victory Road
            TagRivalBw(trs, "RIVAL9", 0x2b5); // Undella Town Post-E4
            TagRivalBw(trs, "RIVAL10", 0x2b8); // Driftveil Post-Undella-Battle

            // Tag Battle with Opposite Gender Hero
            TagRivalBw(trs, "FRIEND1", 0x168);
            TagRivalBw(trs, "FRIEND1", 0x16b);

            // Tag/PWT Battles with Cheren
            Tag(trs, "GYM1", 0x173, 0x278, 0x32E);

            // The Restaurant Brothers
            Tag(trs, "GYM9", 0x1f0); // Cilan
            Tag(trs, "GYM10", 0x1ee); // Chili
            Tag(trs, "GYM11", 0x1ef); // Cress

            // Themed Trainers
            Tag(trs, "THEMED:ZINZOLIN", 0x2c0, 0x248, 0x15b);
            Tag(trs, "THEMED:COLRESS", 0x166, 0x158, 0x32d, 0x32f);
            Tag(trs, "THEMED:SHADOW1", 0x247, 0x15c, 0x2af);
            Tag(trs, "THEMED:SHADOW2", 0x1f2, 0x2b0);
            Tag(trs, "THEMED:SHADOW3", 0x1f3, 0x2b1);

            // Uber-Trainers
            // There are *fourteen* ubers of 17 allowed (incl. the champion)
            // It's a rather stacked game...
            Tag(trs, 0x246, "UBER"); // Alder
            Tag(trs, 0x1c8, "UBER"); // Cynthia
            Tag(trs, 0xca, "UBER"); // Benga/BlackTower
            Tag(trs, 0xc9, "UBER"); // Benga/WhiteTreehollow
            Tag(trs, 0x5, "UBER"); // N/Zekrom
            Tag(trs, 0x6, "UBER"); // N/Reshiram
            Tag(trs, 0x30e, "UBER"); // N/Spring
            Tag(trs, 0x30f, "UBER"); // N/Summer
            Tag(trs, 0x310, "UBER"); // N/Autumn
            Tag(trs, 0x311, "UBER"); // N/Winter
            Tag(trs, 0x159, "UBER"); // Ghetsis
            Tag(trs, 0x8c, "UBER"); // Game Freak Guy
            Tag(trs, 0x24f, "UBER"); // Game Freak Leftovers Guy
        }

        private static void TagRivalBw(IList<Trainer> allTrainers, string tag, int offset)
        {
            allTrainers[offset - 1].Tag = tag + "-0";
            allTrainers[offset].Tag = tag + "-1";
            allTrainers[offset + 1].Tag = tag + "-2";
        }

        private static void Tag(IList<Trainer> allTrainers, int number, string tag)
        {
            if (allTrainers.Count > number - 1)
                allTrainers[number - 1].Tag = tag;
        }

        private static void Tag(IList<Trainer> allTrainers, string tag, params int[] numbers)
        {
            foreach (var num in numbers)
                if (allTrainers.Count > num - 1)
                    allTrainers[num - 1].Tag = tag;
        }
    }
}