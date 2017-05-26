using System;
using System.IO;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public StarterPokemon[] Starters { get; protected set; }
        public Pokemon[] AllPokemons { get; protected set; }
        public Pokemon[] StaticPokemon { get; protected set; }

        public string[] AbilityNames { get; protected set; }
        
        public Move[] AllMoves { get; protected set; }
        
        public int[] MoveTutorMoves { get; protected set; } = Array.Empty<int>();

        public int[] FieldItems { get; protected set; } = Array.Empty<int>();
        public int[] HmMoves { get; protected set; }
        public int[] TmMoves { get; protected set; }

        public string[] ItemNames { get; protected set; } = Array.Empty<string>();
        
        public int[] DoublesTrainerClasses { get; protected set; } = Array.Empty<int>();
        public string[] TrainerClassNames { get; protected set; } = Array.Empty<string>();
        public string[] TrainerNames { get; protected set; } = Array.Empty<string>();
        public Trainer[] Trainers { get; protected set; } = Array.Empty<Trainer>();
        
        public EncounterSet[] Encounters { get; protected set; } = Array.Empty<EncounterSet>();
        
        public IngameTrade[] IngameTrades { get; protected set; } = Array.Empty<IngameTrade>();

        public Game Game { get; set; }


        // TODO: Is not read from rom, so should come from somewhere else
        public int[] CurrentFieldTMs { get; protected set; } = Array.Empty<int>();
        public int[] RegularFieldItems { get; protected set; } = Array.Empty<int>();
        public ItemList AllowedItems { get; protected set; }
        public ItemList NonBadItems { get; protected set; }
        public bool CanChangeTrainerText { get; protected set; }
        public bool FixedTrainerClassNamesLength { get; protected set; }
        public TrainerNameMode TrainerNameMode { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }
        public int MaxTrainerClassNameLength { get; protected set; }
        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;

        public abstract int InternalStringLength(string @string);
        public virtual bool TypeInGame(Typing type) => type.IsHackOnly == false;
        public abstract void SaveRom(Stream stream);
        public abstract void SaveRom(string filename);
    }
}