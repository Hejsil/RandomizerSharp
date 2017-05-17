namespace RandomizerSharp.PokemonModel
{
    public class IngameTrade
    {
        public int Id;

        public int Item = 0;

        public int[] Ivs = new int[0];

        public string Nickname, OtName;

        public int OtId;

        public Pokemon RequestedPokemon, GivenPokemon;
    }
}