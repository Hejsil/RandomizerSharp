using System.IO;
using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var rom =
                @"A:\Mega\ProgramDataDump\RandomizerSettings\PokemonBlack2.nds";
            var radomized = @"A:\Programs\desmume-0.9.11-win64\roms\random.nds";

            var romHandler = new Gen5RomHandler(rom);

            // TODO: Given pokemons were not randomized 
            // TODO: Field items not randomized 
            var world = new WorldRandomizer(romHandler);
            world.RandomizeStarters(true);
            world.RandomizeStaticPokemon(true);
            world.RandomizeIngameTrades(true, true, true);
            world.RandomizeFieldItems(true);
            world.RandomizeHiddenHollowPokemon();

            // TODO: Only Shedinjas 
            var trainer = new TrainerRandomizer(romHandler);
            trainer.TypeThemeTrainerPokes(true, false, true, true);

            var wild = new WildRandomizer(romHandler);
            wild.RandomEncounters(EncountersRandomization.CatchEmAll, false);

            // TODO: TMs were not randomized 
            var move = new MoveRandomizer(romHandler);
            move.RandomizeTmMoves(true, false, true, 1.0);
            move.RandomizeTmhmCompatibility(TmsHmsCompatibility.RandomPreferType);
            move.RandomizeMoveTutorMoves(true, false, 1.0);
            move.RandomizeMoveTutorCompatibility(true);

            var util = new UtilityTweacker(romHandler);
            util.ApplyFastestText();
            util.RemoveBrokenMoves();
            util.RemoveTradeEvolutions(false);

            if (File.Exists(radomized))
                File.Delete(radomized);

            romHandler.SaveRom(radomized);
        }
    }
}