using System;
using RandomizerSharp.Constants;

namespace RandomizerSharp.PokemonModel
{
    public sealed class Game
    {
        // TODO: For some reason, the old randomizer claims that some gen2 games can't change static pokemon. Gotta check if this is true
        public static readonly Game Red = new Game(
            gameKind: GameEnum.Red, 
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 1,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Blue = new Game(
            gameKind: GameEnum.Blue,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 1,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Green = new Game(
            gameKind: GameEnum.Green,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 1,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Yellow = new Game(
            gameKind: GameEnum.Yellow,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 1,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Silver = new Game(
            gameKind: GameEnum.Silver,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: false,
            hasMoveTutors: true,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: false,
            generation: 2,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Gold = new Game(
            gameKind: GameEnum.Gold,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: false,
            hasMoveTutors: true,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: false,
            generation: 2,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Crystal = new Game(
            gameKind: GameEnum.Crystal,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: false,
            hasMoveTutors: true,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: false,
            generation: 2,
            abilitiesPerPokemon: 0,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Ruby = new Game(
            gameKind: GameEnum.Ruby,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 3,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Sapphire = new Game(
            gameKind: GameEnum.Sapphire,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 3,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Emerald = new Game(
            gameKind: GameEnum.Emerald,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 3,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game LeafGreen = new Game(
            gameKind: GameEnum.LeafGreen,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 3,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game FireRed = new Game(
            gameKind: GameEnum.FireRed,
            hasPhysicalSpecialSplit: false,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 3,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        // dppt technically do have hasTimeBasedEncounters but we ignore them completely
        public static readonly Game Diamond = new Game(
            gameKind: GameEnum.Diamond,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 4,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Pearl = new Game(
            gameKind: GameEnum.Pearl,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 4,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Platinum = new Game(
            gameKind: GameEnum.Platinum,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 4,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game HeartGold = new Game(
            gameKind: GameEnum.HeartGold,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 4,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game SoulSilver = new Game(
            gameKind: GameEnum.SoulSilver,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 4,
            abilitiesPerPokemon: 2,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Black = new Game(
            gameKind: GameEnum.Black,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 5,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Gen5Constants.Bw1RequiredFieldTMs,
            fieldMoves: Gen5Constants.FieldMoves,
            earlyRequiredHmMoves: Gen5Constants.Bw1EarlyRequiredHmMoves);

        public static readonly Game White = new Game(
            gameKind: GameEnum.White,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 5,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Gen5Constants.Bw1RequiredFieldTMs,
            fieldMoves: Gen5Constants.FieldMoves,
            earlyRequiredHmMoves: Gen5Constants.Bw1EarlyRequiredHmMoves);

        public static readonly Game Black2 = new Game(
            gameKind: GameEnum.Black2,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 5,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Gen5Constants.Bw2RequiredFieldTMs,
            fieldMoves: Gen5Constants.FieldMoves,
            earlyRequiredHmMoves: Gen5Constants.Bw2EarlyRequiredHmMoves);

        public static readonly Game White2 = new Game(
            gameKind: GameEnum.White2,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: true,
            hasTimeBasedEncounters: true,
            supportsFoarStartingMoves: true,
            generation: 5,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Gen5Constants.Bw2RequiredFieldTMs,
            fieldMoves: Gen5Constants.FieldMoves,
            earlyRequiredHmMoves: Gen5Constants.Bw2EarlyRequiredHmMoves);

        public static readonly Game X = new Game(
            gameKind: GameEnum.X,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 6,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Y = new Game(
            gameKind: GameEnum.Y,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 6,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game OmegaRuby = new Game(
            gameKind: GameEnum.OmegaRuby,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 6,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game AlphaSapphire = new Game(
            gameKind: GameEnum.AlphaSapphire,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 6,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Sun = new Game(
            gameKind: GameEnum.Sun,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 7,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        public static readonly Game Moon = new Game(
            gameKind: GameEnum.Moon,
            hasPhysicalSpecialSplit: true,
            canChangeStaticPokemon: true,
            hasMoveTutors: false,
            hasTimeBasedEncounters: false,
            supportsFoarStartingMoves: true,
            generation: 7,
            abilitiesPerPokemon: 3,
            requiredFieldTMs: Array.Empty<int>(),
            fieldMoves: Array.Empty<int>(),
            earlyRequiredHmMoves: Array.Empty<int>());

        private Game(
            GameEnum gameKind,
            bool hasPhysicalSpecialSplit, 
            bool canChangeStaticPokemon, 
            bool hasMoveTutors, 
            bool hasTimeBasedEncounters, 
            bool supportsFoarStartingMoves, 
            int generation, 
            int abilitiesPerPokemon, 
            int[] requiredFieldTMs, 
            int[] fieldMoves, 
            int[] earlyRequiredHmMoves)
        {
            GameKind = gameKind;
            HasPhysicalSpecialSplit = hasPhysicalSpecialSplit;
            CanChangeStaticPokemon = canChangeStaticPokemon;
            HasMoveTutors = hasMoveTutors;
            HasTimeBasedEncounters = hasTimeBasedEncounters;
            SupportsFoarStartingMoves = supportsFoarStartingMoves;
            Generation = generation;
            AbilitiesPerPokemon = abilitiesPerPokemon;
            RequiredFieldTMs = requiredFieldTMs;
            FieldMoves = fieldMoves;
            EarlyRequiredHmMoves = earlyRequiredHmMoves;
        }

        public GameEnum GameKind { get; }

        public bool HasPhysicalSpecialSplit { get; }
        public bool CanChangeStaticPokemon { get; }
        public bool HasMoveTutors { get; }
        public bool HasTimeBasedEncounters { get; }
        public bool SupportsFoarStartingMoves { get; }

        public int Generation { get; }
        public int AbilitiesPerPokemon { get; }

        public int[] RequiredFieldTMs { get; }
        public int[] FieldMoves { get; }
        public int[] EarlyRequiredHmMoves { get; }
    }

    public enum GameEnum
    {
        // Gen 1
        Red,
        Blue,
        Green,
        Yellow,

        // Gen 2
        Silver,
        Gold,
        Crystal,

        // Gen 3
        Ruby,
        Sapphire,
        Emerald,
        FireRed,
        LeafGreen,

        // Gen 4
        Diamond,
        Pearl,
        Platinum,
        HeartGold,
        SoulSilver,

        // Gen 5
        Black,
        White,
        Black2,
        White2,

        // Gen 6
        X,
        Y,
        OmegaRuby,
        AlphaSapphire,

        // Gen 7
        Sun,
        Moon
    }
}