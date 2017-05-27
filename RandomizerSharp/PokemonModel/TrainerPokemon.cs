namespace RandomizerSharp.PokemonModel
{
    public class TrainerPokemon
    {
        public int Ability { get; set; }

        public int AiLevel { get; set; }
        public int HeldItem { get; set; }
        public int Level { get; set; }

        public int Move1 { get; set; }
        public int Move2 { get; set; }
        public int Move3 { get; set; }
        public int Move4 { get; set; }

        public Pokemon Pokemon { get; set; }
        
        public void ResetMoves()
        {
            var moves = RomFunctions.GetMovesAtLevel(Pokemon, Level);

            Move1 = moves[0];
            Move2 = moves[1];
            Move3 = moves[2];
            Move4 = moves[3];
        }

        public override string ToString() => $"{Pokemon.Name} Lv{Level}";
    }
}