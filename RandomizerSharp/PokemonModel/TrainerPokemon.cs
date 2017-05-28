using System.Collections.Generic;
using RandomizerSharp.Properties;

namespace RandomizerSharp.PokemonModel
{
    public class TrainerPokemon
    {
        public int Ability { get; set; }

        public int AiLevel { get; set; }
        public int HeldItem { get; set; }
        public int Level { get; set; }
        
        public Move Move1 { get; set; }
        public Move Move2 { get; set; }
        public Move Move3 { get; set; }
        public Move Move4 { get; set; }

        public Pokemon Pokemon { get; set; }
        
        public void ResetMoves(Move defaultMove)
        {
            var moves = RomFunctions.GetMovesAtLevel(Pokemon, Level, defaultMove);
            
            Move1 = moves[0];
            Move2 = moves[1];
            Move3 = moves[2];
            Move4 = moves[3];
        }

        public override string ToString() => $"{Pokemon.Name} Lv{Level}";
    }
}