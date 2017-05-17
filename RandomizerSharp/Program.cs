using System.IO;
using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp
{
    internal class Program
    {
        private static void Main()
        {
            var rom = @"A:\Mega\ProgramDataDump\RandomizerSettings\6149 - Pokemon Black Version 2 (DSi Enhanced)(U)(frieNDS).nds";
            var radomized = @"A:\Programs\desmume-0.9.11-win64\roms\random.nds";
            
            var romHandler = new Gen5RomHandler(rom);
            var randomizer = new Gen5Randomizer(romHandler, StreamWriter.Null);
            randomizer.RandomizeMovePowers();
            randomizer.RandomizeTrainerPokes(true, false, false);
            randomizer.RandomizeHiddenHollowPokemon();

            romHandler.SaveRom(radomized);
        }
    }
}