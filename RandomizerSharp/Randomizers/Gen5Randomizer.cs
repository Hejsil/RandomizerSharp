using System;
using RandomizerSharp.Constants;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class Gen5Randomizer
    {
        private readonly Gen5RomHandler _gen5Rom;
        private readonly Random _random;

        // ReSharper disable once SuggestBaseTypeForParameter
        public Gen5Randomizer(Gen5RomHandler romHandler)
            : this(romHandler, (int) DateTime.Now.Ticks)
        { }

        // ReSharper disable once SuggestBaseTypeForParameter
        public Gen5Randomizer(Gen5RomHandler romHandler, int seed)
        {
            _gen5Rom = romHandler;
            _random = new Random(seed);
        }

        public void BanLuckyEgg()
        {
            _gen5Rom.AllowedItems.BanSingles(Gen5Constants.LuckyEggIndex);
            _gen5Rom.NonBadItems.BanSingles(Gen5Constants.LuckyEggIndex);
        }
    }
}
