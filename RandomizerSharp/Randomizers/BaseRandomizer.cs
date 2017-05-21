using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public abstract class BaseRandomizer
    {
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
        }

        protected Pokemon RandomPokemon()
        {
            var pokemons = RomHandler.ValidPokemons;
            return pokemons[Random.Next(pokemons.Count)];
        }

        protected Pokemon RandomNonLegendaryPokemon()
        {
            var nonLegendaries = RomHandler.NonLegendaryPokemons;
            return nonLegendaries[Random.Next(nonLegendaries.Count)];
        }

        protected Pokemon RandomLegendaryPokemon()
        {
            var legendaries = RomHandler.LegendaryPokemons;
            return legendaries[Random.Next(legendaries.Count)];
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
            var allPokes = RomHandler.ValidPokemons;
            foreach (var pk in allPokes)
            {
                if (pk != null)
                    pk.TemporaryFlag = false;
            }

            //  Get evolution data.
            var dontCopyPokes = RomFunctions.GetBasicOrNoCopyPokemon(RomHandler);
            var middleEvos = RomFunctions.GetMiddleEvolutions(RomHandler);
            foreach (var pk in dontCopyPokes)
            {
                bpAction(pk);
                pk.TemporaryFlag = true;
            }

            //  go "up" evolutions looking for pre-evos to do first
            foreach (var pk in allPokes)
            {
                if (pk != null && !pk.TemporaryFlag)
                {
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
}
