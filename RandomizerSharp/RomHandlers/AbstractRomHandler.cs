using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public int AbilitiesPerPokemon { get; protected set; }
        public string[] AbilityNames { get; protected set; } = Array.Empty<string>();
        public IReadOnlyList<Move> AllMoves { get; protected set; } = Array.Empty<Move>();
        public ItemList AllowedItems { get; protected set; }
        public IReadOnlyList<Pokemon> AllPokemons { get; protected set; } = Array.Empty<Pokemon>();

        public IReadOnlyList<Pokemon> BannedForStaticPokemon { get; protected set; } = Array.Empty<Pokemon>();

        public IReadOnlyList<Pokemon> BannedForWildEncounters { get; protected set; } = Array.Empty<Pokemon>();

        public bool CanChangeStarters { get; protected set; } = true;

        public bool CanChangeStaticPokemon { get; protected set; }

        public bool CanChangeTrainerText { get; protected set; }

        public int[] CurrentFieldTMs { get; protected set; } = Array.Empty<int>();


        public string DefaultExtension { get; protected set; } = "";


        public IReadOnlyList<int> DoublesTrainerClasses { get; protected set; } = Array.Empty<int>();

        public IReadOnlyList<int> EarlyRequiredHmMoves { get; protected set; } = Array.Empty<int>();


        public IReadOnlyList<EncounterSet> Encounters { get; protected set; } = Array.Empty<EncounterSet>();
        public int[] FieldItems { get; protected set; } = Array.Empty<int>();

        public IReadOnlyList<int> FieldMoves { get; protected set; } = Array.Empty<int>();

        public bool FixedTrainerClassNamesLength { get; protected set; }

        public HashSet<int> GameBreakingMoves { get; } = new HashSet<int> { 49, 82 };

        public Game Game { get; set; }

        public bool HasMoveTutors { get; protected set; }

        public bool HasPhysicalSpecialSplit { get; protected set; }

        public bool HasTimeBasedEncounters { get; protected set; }

        public int HighestAbilityIndex { get; protected set; }

        public IReadOnlyList<int> HmMoves { get; protected set; } = Array.Empty<int>();


        public IReadOnlyList<IngameTrade> IngameTrades { get; protected set; } = Array.Empty<IngameTrade>();

        public bool IsRomHack { get; protected set; }

        public bool IsYellow { get; protected set; }


        public IReadOnlyList<string> ItemNames { get; protected set; } = Array.Empty<string>();
        public IReadOnlyList<Pokemon> LegendaryPokemons => ValidPokemons.Where(p => p.Legendary).ToArray();
        public string LoadedFilename { get; protected set; } = "";

        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;

        public int MaxTradeNicknameLength => 10;

        public int MaxTradeOtNameLength => 7;
        public int MaxTrainerClassNameLength { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }

        public int MiscTweaksAvailable { get; protected set; }

        public IReadOnlyList<int> MovesBannedFromLevelup { get; protected set; } = Array.Empty<int>();

        public Dictionary<Pokemon, bool[]> MoveTutorCompatibility { get; protected set; } =
            new Dictionary<Pokemon, bool[]>();

        public int[] MoveTutorMoves { get; protected set; } = Array.Empty<int>();
        public ItemList NonBadItems { get; protected set; }

        public IReadOnlyList<Pokemon> NonLegendaryPokemons => ValidPokemons.Where(p => !p.Legendary).ToArray();
        public IReadOnlyList<Bitmap> PokemonSprites { get; protected set; } = Array.Empty<Bitmap>();
        public int[] RegularFieldItems { get; protected set; } = Array.Empty<int>();

        public IReadOnlyList<int> RequiredFieldTMs { get; protected set; } = Array.Empty<int>();

        public IReadOnlyList<StarterPokemon> Starters { get; protected set; } = Array.Empty<StarterPokemon>();
        public Pokemon[] StaticPokemon { get; protected set; } = Array.Empty<Pokemon>();

        public bool SupportsFourStartingMoves { get; protected set; }

        public int[] TcNameLengthsByTrainer { get; protected set; } = Array.Empty<int>();

        public int[] TmMoves { get; protected set; } = Array.Empty<int>();


        public IReadOnlyList<string> TrainerClassNames { get; protected set; } = Array.Empty<string>();

        public TrainerNameMode TrainerNameMode { get; protected set; }

        public string[] TrainerNames { get; protected set; } = Array.Empty<string>();
        public IReadOnlyList<Trainer> Trainers { get; protected set; } = Array.Empty<Trainer>();
        public IReadOnlyList<Move> ValidMoves { get; protected set; } = Array.Empty<Move>();
        public IReadOnlyList<Pokemon> ValidPokemons { get; protected set; } = Array.Empty<Pokemon>();

        public abstract int InternalStringLength(string @string);

        public virtual bool TypeInGame(Typing type) => type.IsHackOnly == false;

        public abstract bool SaveRom(string filename);
    }
}