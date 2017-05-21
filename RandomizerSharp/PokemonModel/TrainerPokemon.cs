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

        public bool ResetMoves { get; set; } = false;

        public override string ToString() => $"{Pokemon.Name} Lv{Level}";
    }
}