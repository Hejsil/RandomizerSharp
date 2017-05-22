using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class WorldRandomizer : BaseRandomizer
    {
        public WorldRandomizer(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public WorldRandomizer(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
        }

        public void RandomizeStaticPokemon(bool legendForLegend)
        {
            //  Load
            var currentStaticPokemon = RomHandler.StaticPokemon;
            var banned = RomHandler.BannedForStaticPokemon;
            if (legendForLegend)
            {
                var legendariesLeft = LegendaryPokemon.ToList();
                var nonlegsLeft = NonLegendaryPokemon.ToList();
                legendariesLeft.RemoveAll(banned.Contains);
                nonlegsLeft.RemoveAll(banned.Contains);

                for (var i = 0; i < currentStaticPokemon.Length; ++i)
                {
                    if (currentStaticPokemon[i].Legendary)
                    {
                        var num = Random.Next(legendariesLeft.Count);
                        currentStaticPokemon[i] = legendariesLeft[num];
                        legendariesLeft.RemoveAt(num);

                        if (legendariesLeft.Count != 0)
                            continue;

                        legendariesLeft.AddRange(LegendaryPokemon);
                        legendariesLeft.RemoveAll(banned.Contains);
                    }
                    else
                    {
                        var num = Random.Next(nonlegsLeft.Count);
                        currentStaticPokemon[i] = nonlegsLeft[num];
                        nonlegsLeft.RemoveAt(num);

                        if (nonlegsLeft.Count != 0)
                            continue;

                        nonlegsLeft.AddRange(NonLegendaryPokemon);
                        nonlegsLeft.RemoveAll(banned.Contains);
                    }
                }
            }
            else
            {
                var pokemonLeft = ValidPokemons.ToList();
                pokemonLeft.RemoveAll(banned.Contains);
                for (var i = 0; i < currentStaticPokemon.Length; i++)
                {
                    var num = Random.Next(pokemonLeft.Count);

                    currentStaticPokemon[i] = pokemonLeft[num];
                    pokemonLeft.RemoveAt(num);

                    if (pokemonLeft.Count == 0)
                    {
                        pokemonLeft.AddRange(ValidPokemons);
                        pokemonLeft.RemoveAll(banned.Contains);
                    }
                }
            }
        }

        public void RandomizeIngameTrades(bool randomizeRequest, bool randomIVs, bool randomItem)
        {
            // get old trades
            var trades = RomHandler.IngameTrades;
            var usedRequests = new List<Pokemon>();
            var usedGivens = new List<Pokemon>();
            var possibleItems = RomHandler.AllowedItems;

            foreach (var trade in trades)
            {
                // pick new given pokemon
                var oldgiven = trade.GivenPokemon;
                Pokemon given;


                do
                {
                    given = RandomPokemon();
                } while (usedGivens.Contains(given));

                usedGivens.Add(given);
                trade.GivenPokemon = given;

                // requested pokemon?
                if (Equals(oldgiven, trade.RequestedPokemon))
                {
                    // preserve trades for the same pokemon
                    trade.RequestedPokemon = given;
                }
                else if (randomizeRequest)
                {
                    Pokemon request;

                    do
                    {
                        request = RandomPokemon();
                    } while (usedRequests.Contains(request) || Equals(request, given));

                    usedRequests.Add(request);
                    trade.RequestedPokemon = request;
                }

                if (randomIVs)
                {
                    var maxIv = RomHandler.Game.Generation() == 1 ? 16 : 32;

                    for (var i = 0; i < trade.Ivs.Length; i++)
                    {
                        trade.Ivs[i] = Random.Next(maxIv);
                    }
                }

                if (randomItem)
                {
                    trade.Item = possibleItems.RandomItem(Random);
                }
            }
        }

        public void RandomizeStarters(bool withTwoEvos)
        {
            // Randomise
            var starterCount = 3;

            if (RomHandler.Game == Game.Yellow)
                starterCount = 2;

            foreach (var starter in RomHandler.Starters)
                starter.Pokemon = null;

            for (var i = 0; i < starterCount; i++)
            {
                Pokemon pkmn;
                var selectFrom = ValidPokemons;

                if (withTwoEvos)
                {
                    selectFrom = selectFrom
                        .Where(pk => pk.EvolutionsTo.Count == 0 && pk.EvolutionsFrom.Count > 0)
                        .Where(pk => pk.EvolutionsFrom.Any(ev => ev.To.EvolutionsFrom.Count > 0))
                        .ToArray();
                }

                do
                {
                    pkmn = selectFrom[Random.Next(selectFrom.Count)];
                } while (RomHandler.Starters.Any(p => pkmn.Equals(p.Pokemon)));

                RomHandler.Starters[i].Pokemon = pkmn;
            }
        }

        public void RandomizeStarterHeldItems(bool banBadItems)
        {
            var oldHeldItems = RomHandler.Starters;
            var possibleItems = banBadItems ? RomHandler.NonBadItems : RomHandler.AllowedItems;

            foreach (var t in oldHeldItems)
                t.HeldItem = possibleItems.RandomItem(Random);
        }

        public void ShuffleFieldItems()
        {
            var currentItems = RomHandler.RegularFieldItems;
            var currentTMs = RomHandler.CurrentFieldTMs;
            currentItems.Shuffle(Random);
            currentTMs.Shuffle(Random);
        }

        public void RandomizeFieldItems(bool banBadItems)
        {
            var possibleItems = banBadItems ? RomHandler.NonBadItems : RomHandler.AllowedItems;

            var currentItems = RomHandler.RegularFieldItems;
            var currentTMs = RomHandler.CurrentFieldTMs;
            var requiredTMs = RomHandler.RequiredFieldTMs;
            var fieldItemCount = currentItems.Length;
            var fieldTmCount = currentTMs.Length;
            var reqTmCount = requiredTMs.Count;
            var totalTmCount = RomHandler.TmMoves.Length;
            var newItems = new List<int>();
            var newTMs = new List<int>();

            for (var i = 0; i < fieldItemCount; i++)
                newItems.Add(possibleItems.RandomNonTm(Random));

            newTMs.AddRange(requiredTMs);

            for (var i = reqTmCount; i < fieldTmCount; i++)
            {
                while (true)
                {
                    var tm = Random.Next(totalTmCount) + 1;
                    if (newTMs.Contains(tm))
                        continue;

                    newTMs.Add(tm);
                    break;
                }
            }

            newItems.Shuffle(Random);
            newTMs.Shuffle(Random);

            for (var i = 0; i < newItems.Count; ++i)
                currentItems[i] = newItems[i];
            for (var i = 0; i < newTMs.Count; ++i)
                currentTMs[i] = newTMs[i];
        }

        public void RandomizeHiddenHollowPokemon()
        {
            switch (RomHandler)
            {
                case Gen5RomHandler gen5:
                    var allowedUnovaPokemon = Gen5Constants.Bw2HiddenHollowUnovaPokemon;
                    var randomSize = Gen5Constants.NonUnovaPokemonCount + allowedUnovaPokemon.Count;

                    foreach (var hollow in gen5.HiddenHollows)
                    {
                        var pokeChoice = Random.Next(randomSize) + 1;
                        var genderRatio = Random.Next(101);

                        if (pokeChoice > Gen5Constants.NonUnovaPokemonCount)
                            pokeChoice = allowedUnovaPokemon[pokeChoice - (Gen5Constants.NonUnovaPokemonCount + 1)];

                        hollow.Pokemon = pokeChoice;
                        hollow.Form = 0;
                        hollow.GenderRatio = (byte) genderRatio;
                    }

                    break;
            }
        }
    }
}