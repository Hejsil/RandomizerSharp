using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public Trainer[] Trainers { get; protected set; } = Array<Trainer>.Empty;

        public Pokemon[] NonLegendaryPokemons => ValidPokemons.Where(p => !p.Legendary).ToArray();
        public Pokemon[] LegendaryPokemons => ValidPokemons.Where(p => p.Legendary).ToArray();
        public ArraySlice<Move> ValidMoves { get; protected set; } = Array<Move>.Empty;
        public ArraySlice<Pokemon> ValidPokemons { get; protected set; } = Array<Pokemon>.Empty;

        public Move[] AllMoves { get; protected set; } = Array<Move>.Empty;
        public Pokemon[] AllPokemons { get; protected set; } = Array<Pokemon>.Empty;


        public EncounterSet[] Encounters { get; protected set; } = Array<EncounterSet>.Empty;


        public int AbilitiesPerPokemon { get; protected set; }

        public int HighestAbilityIndex { get; protected set; }

        public bool HasPhysicalSpecialSplit { get; protected set; }

        public bool SupportsFourStartingMoves { get; protected set; }
        public Pokemon[] StaticPokemon { get; protected set; } = Array<Pokemon>.Empty;

        public bool CanChangeStaticPokemon { get; protected set; }

        public bool HasMoveTutors { get; protected set; }

        public int[] MoveTutorMoves { get; protected set; } = Array<int>.Empty;

        public Dictionary<Pokemon, bool[]> MoveTutorCompatibility { get; protected set; } = new Dictionary<Pokemon, bool[]>();

        public bool CanChangeTrainerText { get; protected set; }

        public string[] TrainerNames { get; protected set; } = Array<string>.Empty;

        public TrainerNameMode TrainerNameMode { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }

        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;

        public int[] TcNameLengthsByTrainer { get; protected set; } = Array<int>.Empty;


        public string[] TrainerClassNames { get; protected set; } = Array<string>.Empty;
        public int MaxTrainerClassNameLength { get; protected set; }


        public int[] DoublesTrainerClasses { get; protected set; } = Array<int>.Empty;
        public ItemList AllowedItems { get; protected set; }
        public ItemList NonBadItems { get; protected set; }


        public string[] ItemNames { get; protected set; } = Array<string>.Empty;
        public int[] StarterHeldItems { get; protected set; } = Array<int>.Empty;


        public int[] RequiredFieldTMs { get; protected set; } = Array<int>.Empty;

        public int[] CurrentFieldTMs { get; protected set; } = Array<int>.Empty;
        public int[] RegularFieldItems { get; protected set; } = Array<int>.Empty;


        public IngameTrade[] IngameTrades { get; protected set; } = Array<IngameTrade>.Empty;


        public bool HasDVs { get; protected set; }


        public string DefaultExtension { get; protected set; } = "";
        public Bitmap[] PokemonSprites { get; protected set; } = Array<Bitmap>.Empty;

        public bool IsRomHack { get; protected set; }

        public int GenerationOfPokemon { get; protected set; }

        public int MiscTweaksAvailable { get; protected set; }

        public bool CanChangeStarters { get; protected set; } = true;

        public bool HasTimeBasedEncounters { get; protected set; }

        public Pokemon[] BannedForWildEncounters { get; protected set; } = Array<Pokemon>.Empty;

        public Pokemon[] BannedForStaticPokemon { get; protected set; } = Array<Pokemon>.Empty;

        public int[] TmMoves { get; protected set; } = Array<int>.Empty;

        public int[] HmMoves { get; protected set; } = Array<int>.Empty;

        public int MaxTradeNicknameLength => 10;

        public int MaxTradeOtNameLength => 7;
        public string LoadedFilename { get; protected set; } = "";
        public Pokemon[] Starters { get; protected set; } = Array<Pokemon>.Empty;

        public int[] MovesBannedFromLevelup { get; protected set; } = Array<int>.Empty;

        public HashSet<int> GameBreakingMoves { get; } = new HashSet<int> { 49, 82 };

        public int[] FieldMoves { get; protected set; } = Array<int>.Empty;

        public int[] EarlyRequiredHmMoves { get; protected set; } = Array<int>.Empty;

        public bool IsYellow { get; protected set; }

        public string RomName { get; protected set; } = "";

        public string RomCode { get; protected set; } = "";

        public string SupportLevel { get; protected set; } = "";

        public bool FixedTrainerClassNamesLength { get; protected set; }
        public int[] FieldItems { get; protected set; } = Array<int>.Empty;


        public string[] AbilityNames { get; protected set; } = Array<string>.Empty;

        public abstract int InternalStringLength(string @string);

        public virtual bool TypeInGame(Typing type)
        {
            return type.IsHackOnly == false;
        }

        public abstract bool SaveRom(string filename);
    }
}