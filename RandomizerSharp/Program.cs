using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var rom = @"A:\Mega\ProgramDataDump\RandomizerSettings\5584 - Pokemon - White Version (DSi Enhanced)(USA) (E)(SweeTnDs).nds";
            var radomized = @"A:\Programs\desmume-0.9.11-win64\roms\random.nds";
            
            var romHandler = new Gen5RomHandler(rom);

            var world = new WorldRandomizer(romHandler);
            world.RandomizeStarters(true);
            world.RandomizeStaticPokemon(true);
            world.RandomizeIngameTrades(true, true, true);
            world.RandomizeFieldItems(true);
            world.RandomizeHiddenHollowPokemon();

            var trainer = new TrainerRandomizer(romHandler);
            trainer.RandomizeTrainerPokes(true, false, true, true);

            var wild = new WildRandomizer(romHandler);
            wild.RandomEncounters(WildRandomizer.Encounters.CatchEmAll, false);

            var move = new MoveRandomizer(romHandler);
            move.RandomizeTmMoves(true, false, true, 1.0);
            move.RandomizeTmhmCompatibility(MoveRandomizer.TmsHmsCompatibility.RandomPreferType);
            move.RandomizeMoveTutorMoves(true, false, 1.0);
            move.RandomizeMoveTutorCompatibility(true);

            var util = new UtilityTweacker(romHandler);
            util.ApplyFastestText();

            romHandler.SaveRom(radomized);
        }
    }
}