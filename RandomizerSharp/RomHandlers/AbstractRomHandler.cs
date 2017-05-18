using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public ArraySlice<Trainer> Trainers { get; protected set; } = Slice<Trainer>.Empty;
        public ArraySlice<Pokemon> NonLegendaryPokemons => ValidPokemons.Where(p => !p.Legendary).Slice();
        public ArraySlice<Pokemon> LegendaryPokemons => ValidPokemons.Where(p => p.Legendary).Slice();
        public ArraySlice<Move> ValidMoves { get; protected set; } = Slice<Move>.Empty;
        public ArraySlice<Pokemon> ValidPokemons { get; protected set; } = Slice<Pokemon>.Empty;
        public ArraySlice<Move> AllMoves { get; protected set; } = Slice<Move>.Empty;
        public ArraySlice<Pokemon> AllPokemons { get; protected set; } = Slice<Pokemon>.Empty;

        public ArraySlice<EncounterSet> Encounters { get; protected set; } = Slice<EncounterSet>.Empty;


        public int AbilitiesPerPokemon { get; protected set; }

        public int HighestAbilityIndex { get; protected set; }

        public bool HasPhysicalSpecialSplit { get; protected set; }

        public bool SupportsFourStartingMoves { get; protected set; }
        public ArraySlice<Pokemon> StaticPokemon { get; protected set; } = Slice<Pokemon>.Empty;

        public bool CanChangeStaticPokemon { get; protected set; }

        public bool HasMoveTutors { get; protected set; }

        public ArraySlice<int> MoveTutorMoves { get; protected set; } = Slice<int>.Empty;

        public IDictionary<Pokemon, ArraySlice<bool>> MoveTutorCompatibility { get; protected set; }

        public bool CanChangeTrainerText { get; protected set; }

        public ArraySlice<string> TrainerNames { get; protected set; } = Slice<string>.Empty;

        public TrainerNameMode TrainerNameMode { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }

        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;

        public ArraySlice<int> TcNameLengthsByTrainer { get; protected set; } = Slice<int>.Empty;


        public ArraySlice<string> TrainerClassNames { get; protected set; } = Slice<string>.Empty;
        public int MaxTrainerClassNameLength { get; protected set; }


        public ArraySlice<int> DoublesTrainerClasses { get; protected set; } = Slice<int>.Empty;
        public ItemList AllowedItems { get; protected set; }
        public ItemList NonBadItems { get; protected set; }


        public ArraySlice<string> ItemNames { get; protected set; } = Slice<string>.Empty;
        public ArraySlice<int> StarterHeldItems { get; protected set; } = Slice<int>.Empty;


        public ArraySlice<int> RequiredFieldTMs { get; protected set; } = Slice<int>.Empty;

        public ArraySlice<int> CurrentFieldTMs { get; protected set; } = Slice<int>.Empty;
        public ArraySlice<int> RegularFieldItems { get; protected set; } = Slice<int>.Empty;


        public ArraySlice<IngameTrade> IngameTrades { get; protected set; } = Slice<IngameTrade>.Empty;


        public bool HasDVs { get; protected set; }


        public string DefaultExtension { get; protected set; }
        public ArraySlice<Bitmap> PokemonSprites { get; protected set; } = Slice<Bitmap>.Empty;

        public bool IsRomHack { get; protected set; }

        public int GenerationOfPokemon { get; protected set; }

        public int MiscTweaksAvailable { get; protected set; }

        public bool CanChangeStarters { get; protected set; } = true;

        public bool HasTimeBasedEncounters { get; protected set; }

        public ArraySlice<Pokemon> BannedForWildEncounters { get; protected set; } = Slice<Pokemon>.Empty;

        public ArraySlice<Pokemon> BannedForStaticPokemon { get; protected set; } = Slice<Pokemon>.Empty;

        public ArraySlice<int> TmMoves { get; protected set; } = Slice<int>.Empty;

        public ArraySlice<int> HmMoves { get; protected set; } = Slice<int>.Empty;

        public int MaxTradeNicknameLength => 10;

        public int MaxTradeOtNameLength => 7;
        public string LoadedFilename { get; protected set; }
        public ArraySlice<Pokemon> Starters { get; protected set; } = Slice<Pokemon>.Empty;

        public ArraySlice<int> MovesBannedFromLevelup { get; protected set; } = Slice<int>.Empty;

        public ISet<int> GameBreakingMoves { get; } = new HashSet<int> { 49, 82 };

        public ArraySlice<int> FieldMoves { get; protected set; } = Slice<int>.Empty;

        public ArraySlice<int> EarlyRequiredHmMoves { get; protected set; } = Slice<int>.Empty;

        public bool IsYellow { get; protected set; }

        public string RomName { get; protected set; }

        public string RomCode { get; protected set; }

        public string SupportLevel { get; protected set; }

        public bool FixedTrainerClassNamesLength { get; protected set; }
        public ArraySlice<int> FieldItems { get; protected set; } = Slice<int>.Empty;


        public ArraySlice<string> AbilityNames { get; protected set; } = Slice<string>.Empty;

        public abstract int InternalStringLength(string @string);

        public virtual bool TypeInGame(Typing type)
        {
            return type.IsHackOnly == false;
        }

        public abstract bool SaveRom(string filename);
    }
}