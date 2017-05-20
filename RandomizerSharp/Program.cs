using System.IO;
using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rom = @"A:\Mega\ProgramDataDump\RandomizerSettings\5584 - Pokemon - White Version (DSi Enhanced)(USA) (E)(SweeTnDs).nds";
            var radomized = @"A:\Programs\desmume-0.9.11-win64\roms\random.nds";
            
            var romHandler = new Gen5RomHandler(rom);
            var randomizer = new Randomizer(romHandler, StreamWriter.Null);
            var gen5Randomizer = new Gen5Randomizer(romHandler, StreamWriter.Null);

            randomizer.RandomizeStarters(true);
            randomizer.RandomizeStaticPokemon(true);
            randomizer.RandomizeIngameTrades(true, true, true);
            randomizer.RandomizeTrainerPokes(true, false, true, true);
            randomizer.RandomEncounters(Randomizer.Encounters.CatchEmAll, false);
            randomizer.RandomizeTmMoves(true, false, true, 1.0);
            randomizer.RandomizeTmhmCompatibility(Randomizer.TmsHmsCompatibility.RandomPreferType);
            randomizer.RandomizeMoveTutorMoves(true, false, 1.0);
            randomizer.RandomizeMoveTutorCompatibility(true);
            randomizer.RandomizeFieldItems(true);

            gen5Randomizer.ApplyFastestText();
            gen5Randomizer.RandomizeHiddenHollowPokemon();

            romHandler.SaveRom(radomized);
        }
    }
}