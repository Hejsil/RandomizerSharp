using System;
using System.IO;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class Gen5Randomizer
    {
        private readonly Gen5RomHandler _gen5Rom;
        private readonly Random _random;
        private readonly StreamWriter _log;

        // ReSharper disable once SuggestBaseTypeForParameter
        public Gen5Randomizer(Gen5RomHandler romHandler, StreamWriter log)
        {
            _gen5Rom = romHandler;
            _log = log;
            _random = new Random();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public Gen5Randomizer(Gen5RomHandler romHandler, StreamWriter log, int seed)
        {
            _gen5Rom = romHandler;
            _log = log;
            _random = new Random(seed);
        }

        public void RandomizeHiddenHollowPokemon()
        {
            var allowedUnovaPokemon = Gen5Constants.Bw2HiddenHollowUnovaPokemon;
            var randomSize = Gen5Constants.NonUnovaPokemonCount + allowedUnovaPokemon.Count;

            foreach (var hollow in _gen5Rom.HiddenHollows)
            {
                var pokeChoice = _random.Next(randomSize) + 1;
                var genderRatio = _random.Next(101);

                if (pokeChoice > Gen5Constants.NonUnovaPokemonCount)
                    pokeChoice = allowedUnovaPokemon[pokeChoice - (Gen5Constants.NonUnovaPokemonCount + 1)];

                hollow.Pokemon = pokeChoice;
                hollow.Form = 0;
                hollow.GenderRatio = (byte)genderRatio;
            }
        }

        public void ApplyFastestText()
        {
            _gen5Rom.GenericIpsPatch(_gen5Rom.Arm9, "FastestTextTweak");
        }

        public void BanLuckyEgg()
        {
            _gen5Rom.AllowedItems.BanSingles(Gen5Constants.LuckyEggIndex);
            _gen5Rom.NonBadItems.BanSingles(Gen5Constants.LuckyEggIndex);
        }
    }
}
