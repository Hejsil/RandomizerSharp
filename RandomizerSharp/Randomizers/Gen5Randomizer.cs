using System.IO;
using RandomizerSharp.Constants;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class Gen5Randomizer : Randomizer
    {
        private Gen5RomHandler Gen5Rom => (Gen5RomHandler) RomHandler;

        public Gen5Randomizer(Gen5RomHandler romHandler, StreamWriter log) 
            : base(romHandler, log)
        { }

        public Gen5Randomizer(Gen5RomHandler romHandler, StreamWriter log, int seed) 
            : base(romHandler, log, seed)
        { }
        public void RandomizeHiddenHollowPokemon()
        {
            var allowedUnovaPokemon = Gen5Constants.Bw2HiddenHollowUnovaPokemon;
            var randomSize = Gen5Constants.NonUnovaPokemonCount + allowedUnovaPokemon.Length;

            foreach (var hollow in Gen5Rom.HiddenHollows)
            {
                var pokeChoice = Random.Next(randomSize) + 1;
                var genderRatio = Random.Next(101);

                if (pokeChoice > Gen5Constants.NonUnovaPokemonCount)
                    pokeChoice = allowedUnovaPokemon[pokeChoice - (Gen5Constants.NonUnovaPokemonCount + 1)];

                hollow.Pokemon = pokeChoice;
                hollow.Form = 0;
                hollow.GenderRatio = (byte)genderRatio;
            }
        }

        public void ApplyFastestText()
        {
            Gen5Rom.GenericIpsPatch(Gen5Rom.Arm9, "FastestTextTweak");
        }

        public void BanLuckyEgg()
        {
            Gen5Rom.AllowedItems.BanSingles(Gen5Constants.LuckyEggIndex);
            Gen5Rom.NonBadItems.BanSingles(Gen5Constants.LuckyEggIndex);
        }
    }
}
