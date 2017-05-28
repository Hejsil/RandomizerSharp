using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public abstract class BaseRandomizer
    {
        protected ArraySlice<Pokemon> ValidPokemons { get; }
        protected ArraySlice<Move> ValidMoves { get; }
        protected IEnumerable<Pokemon> NonLegendaryPokemon => ValidPokemons.Where(p => !p.Legendary);
        protected IEnumerable<Pokemon> LegendaryPokemon => ValidPokemons.Where(p => p.Legendary);

        protected Random Random { get; }
        protected AbstractRomHandler RomHandler { get; }

        protected BaseRandomizer(AbstractRomHandler romHandler)
            : this(romHandler, new Random())
        {
        }

        protected BaseRandomizer(AbstractRomHandler romHandler, Random random)
        {
            RomHandler = romHandler;
            Random = random;

            switch (RomHandler.Game.GameKind)
            {
                case GameEnum.Red:
                case GameEnum.Blue:
                case GameEnum.Green:
                case GameEnum.Yellow:
                case GameEnum.Silver:
                case GameEnum.Gold:
                case GameEnum.Crystal:
                case GameEnum.Ruby:
                case GameEnum.Sapphire:
                case GameEnum.Emerald:
                case GameEnum.FireRed:
                case GameEnum.LeafGreen:
                case GameEnum.Diamond:
                case GameEnum.Pearl:
                case GameEnum.Platinum:
                case GameEnum.HeartGold:
                case GameEnum.SoulSilver:
                case GameEnum.X:
                case GameEnum.Y:
                case GameEnum.OmegaRuby:
                case GameEnum.AlphaSapphire:
                case GameEnum.Sun:
                case GameEnum.Moon:
                default:
                    throw new NotImplementedException();

                case GameEnum.Black:
                case GameEnum.White:
                case GameEnum.Black2:
                case GameEnum.White2:
                    ValidPokemons = RomHandler.Pokemons.SliceFrom(1, Gen5Constants.PokemonCount);
                    ValidMoves = RomHandler.Moves.SliceFrom(1, Gen5Constants.MoveCount);
                    break;
            }
        }

        protected Item RandomItem()
        {
            var items = RomHandler.Items;
            return items[Random.Next(items.Count)];
        }

        protected Pokemon RandomPokemon()
        {
            var pokemons = ValidPokemons;
            return pokemons[Random.Next(pokemons.Count)];
        }

        protected Pokemon RandomNonLegendaryPokemon()
        {
            var nonLegendaries = NonLegendaryPokemon;
            var enumerable = nonLegendaries as Pokemon[] ?? nonLegendaries.ToArray();
            return enumerable[Random.Next(enumerable.Length)];
        }

        protected Pokemon RandomLegendaryPokemon()
        {
            var legendaries = LegendaryPokemon;
            var enumerable = legendaries as Pokemon[] ?? legendaries.ToArray();
            return enumerable[Random.Next(enumerable.Length)];
        }

        protected Typing RandomType()
        {
            var t = Typing.RandomType(Random);
            while (!RomHandler.TypeInGame(t))
                t = Typing.RandomType(Random);

            return t;
        }

        protected void CopyUpEvolutionsHelper(Action<Pokemon> bpAction, Action<Pokemon, Pokemon, bool> epAction)
        {
            foreach (var pk in ValidPokemons)
            {
                if (pk != null)
                    pk.TemporaryFlag = false;
            }

            //  Get evolution data.
            var dontCopyPokes = RomFunctions.GetBasicOrNoCopyPokemon(ValidPokemons);
            var middleEvos = RomFunctions.GetMiddleEvolutions(ValidPokemons);
            foreach (var pk in dontCopyPokes)
            {
                bpAction(pk);
                pk.TemporaryFlag = true;
            }

            //  go "up" evolutions looking for pre-evos to do first
            foreach (var pk in ValidPokemons)
            {
                if (pk == null || pk.TemporaryFlag)
                    continue;

                //  Non-randomized pokes at this point must have
                //  a linear chain of single evolutions down to
                //  a randomized poke.
                var currentStack = new Stack<Evolution>();
                var ev = pk.EvolutionsTo[0];
                while (!ev.From.TemporaryFlag)
                {
                    currentStack.Push(ev);
                    ev = ev.From.EvolutionsTo[0];
                }

                //  Now "ev" is set to an evolution from a Pokemon that has had
                //  the base action done on it to one that hasn't.
                //  Do the evolution action for everything left on the stack.
                epAction(ev.From, ev.To, !middleEvos.Contains(ev.To));
                ev.To.TemporaryFlag = true;
                while (currentStack.Count != 0)
                {
                    ev = currentStack.Pop();
                    epAction(ev.From, ev.To, !middleEvos.Contains(ev.To));
                    ev.To.TemporaryFlag = true;
                }
            }
        }
    }
}