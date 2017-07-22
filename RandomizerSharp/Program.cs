using System.IO;
using RandomizerSharp.Randomizers;
using RandomizerSharp.RomHandlers;
using System;

namespace RandomizerSharp
{
    public class Program
    {
        private static void Main(string[] args)
        {
            string inPath = null, outPath = $"{DateTime.Now.Ticks}.nds";

            if (args.Length > 0)
                inPath = args[0];

            if (args.Length > 1)
                outPath = args[1];

            if (inPath == null)
            {
                Console.WriteLine(@"Input path was not specified");
                return;
            }

            var romHandler = new Gen5RomHandler(inPath);

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

            if (File.Exists(outPath))
            {
                for (;;)
                {
                    Console.Write(@"{0} allready exists. Want to replace it? [Y/n]: ");
                    var readLine = Console.ReadLine();
                    if (readLine == null) continue;

                    var answer = readLine.Trim().ToLower();
                    Console.WriteLine();

                    if (answer == "" || answer == "y")
                    {
                        File.Delete(outPath);
                        romHandler.SaveRom(outPath);
                        break;
                    }

                    if (answer == "n") break;
                }
            }
            else
            {
                romHandler.SaveRom(outPath);
            }
        }
    }
}