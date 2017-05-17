namespace RandomizerSharp.PokemonModel
{
    public class TrainerPokemon
    {
        public int Ability;

        public int AiLevel;
        public int HeldItem;
        public int Level;

        public int Move1;
        public int Move2;
        public int Move3;
        public int Move4;

        public Pokemon Pokemon;

        public bool ResetMoves = false;

        public override string ToString()
        {
            return Pokemon.Name + " Lv" + Level;
        }
    }
}