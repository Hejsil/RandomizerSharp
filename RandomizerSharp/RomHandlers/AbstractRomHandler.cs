using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public IReadOnlyList<Pokemon> AllPokemons { get; protected set; }
        public IReadOnlyList<StarterPokemon> Starters { get; protected set; }
        public Pokemon[] StaticPokemon { get; protected set; }

        public int HighestAbilityIndex { get; protected set; }
        public int AbilitiesPerPokemon { get; protected set; }
        public string[] AbilityNames { get; protected set; }

        public int MaxTradeNicknameLength => 10;
        public int MaxTradeOtNameLength => 7;
        
        public IReadOnlyList<int> RequiredFieldTMs { get; protected set; }
        public IReadOnlyList<int> MovesBannedFromLevelup { get; protected set; }
        public bool HasMoveTutors { get; protected set; }
        public IReadOnlyList<Move> AllMoves { get; protected set; }

        public HashSet<int> GameBreakingMoves { get; } = new HashSet<int> { 49, 82 };
        public IReadOnlyList<int> FieldMoves { get; protected set; } 
        public IReadOnlyList<int> EarlyRequiredHmMoves { get; protected set; }
        public bool SupportsFourStartingMoves { get; protected set; }
        public bool HasPhysicalSpecialSplit { get; protected set; }
        public int[] MoveTutorMoves { get; protected set; } = Array.Empty<int>();

        public int[] CurrentFieldTMs { get; protected set; } = Array.Empty<int>();

        public IReadOnlyList<Pokemon> BannedForStaticPokemon { get; protected set; } = Array.Empty<Pokemon>();
        public IReadOnlyList<Pokemon> BannedForWildEncounters { get; protected set; } = Array.Empty<Pokemon>();

        public bool CanChangeStarters { get; protected set; } = true;
        public bool CanChangeStaticPokemon { get; protected set; }

        public ItemList AllowedItems { get; protected set; }
        
        public int[] FieldItems { get; protected set; } = Array.Empty<int>();
        public IReadOnlyList<int> HmMoves { get; protected set; }
        public int[] TmMoves { get; protected set; }

        public IReadOnlyList<string> ItemNames { get; protected set; } = Array.Empty<string>();
        public ItemList NonBadItems { get; protected set; }
        public int[] RegularFieldItems { get; protected set; } = Array.Empty<int>();


        public IReadOnlyList<int> DoublesTrainerClasses { get; protected set; } = Array.Empty<int>();
        public bool CanChangeTrainerText { get; protected set; }
        public bool FixedTrainerClassNamesLength { get; protected set; }
        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;
        public int MaxTrainerClassNameLength { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }
        public int[] TcNameLengthsByTrainer { get; protected set; } = Array.Empty<int>();
        public IReadOnlyList<string> TrainerClassNames { get; protected set; } = Array.Empty<string>();
        public string[] TrainerNames { get; protected set; } = Array.Empty<string>();
        public IReadOnlyList<Trainer> Trainers { get; protected set; } = Array.Empty<Trainer>();
        public TrainerNameMode TrainerNameMode { get; protected set; }
        
        public Game Game { get; set; }
        public string DefaultExtension { get; protected set; }
        public string LoadedFilename { get; protected set; } = "";
        
        public bool HasTimeBasedEncounters { get; protected set; }
        public IReadOnlyList<EncounterSet> Encounters { get; protected set; } = Array.Empty<EncounterSet>();
        
        public IReadOnlyList<IngameTrade> IngameTrades { get; protected set; } = Array.Empty<IngameTrade>();
        
        public int MiscTweaksAvailable { get; protected set; }


        public abstract int InternalStringLength(string @string);
        public virtual bool TypeInGame(Typing type) => type.IsHackOnly == false;
        public abstract bool SaveRom(string filename);
    }
}