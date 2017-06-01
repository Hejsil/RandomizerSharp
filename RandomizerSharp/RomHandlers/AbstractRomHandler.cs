using System;
using System.Collections.Generic;
using System.IO;
using RandomizerSharp.NDS;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.RomHandlers
{
    public abstract class AbstractRomHandler
    {
        public IReadOnlyList<Pokemon> Pokemons { get; protected set; } = Array.Empty<Pokemon>();
        public IReadOnlyList<Move> Moves { get; protected set; } = Array.Empty<Move>();
        public IReadOnlyList<Trainer> Trainers { get; protected set; } = Array.Empty<Trainer>();
        public IReadOnlyList<EncounterSet> Encounters { get; protected set; } = Array.Empty<EncounterSet>();
        public IReadOnlyList<IngameTrade> IngameTrades { get; protected set; } = Array.Empty<IngameTrade>();
        public IReadOnlyList<Item> Items { get; protected set; } = Array.Empty<Item>();
        public IReadOnlyList<Ability> Abilities { get; protected set; } = Array.Empty<Ability>();
        public IReadOnlyList<Typing> Types { get; protected set; } = Array.Empty<Typing>();
        public IReadOnlyList<Machine> Machines { get; protected set; } = Array.Empty<Machine>();

        public StarterPokemon[] Starters { get; protected set; } = Array.Empty<StarterPokemon>();
        public Pokemon[] StaticPokemon { get; protected set; } = Array.Empty<Pokemon>();
        
        public Move[] MoveTutorMoves { get; protected set; } = Array.Empty<Move>();
        public Item[] FieldItems { get; protected set; } = Array.Empty<Item>();


        public string[] TrainerClassNames { get; protected set; } = Array.Empty<string>();
        public string[] TrainerNames { get; protected set; } = Array.Empty<string>();
        
        public Game Game { get; protected set; }

        //public ItemList AllowedItems { get; protected set; }
        //public ItemList NonBadItems { get; protected set; }
        public bool CanChangeTrainerText { get; protected set; }
        public bool FixedTrainerClassNamesLength { get; protected set; }
        public TrainerNameMode TrainerNameMode { get; protected set; }
        public int MaxTrainerNameLength { get; protected set; }
        public int MaxTrainerClassNameLength { get; protected set; }
        public int MaxSumOfTrainerNameLengths { get; protected set; } = int.MaxValue;
        public int[] DoublesTrainerClasses { get; protected set; } = Array.Empty<int>();

        public abstract int InternalStringLength(string @string);
        public virtual bool TypeInGame(Typing type) => type.IsHackOnly == false;
        public abstract void SaveRom(Stream stream);
        public abstract void SaveRom(string filename);
    }
}