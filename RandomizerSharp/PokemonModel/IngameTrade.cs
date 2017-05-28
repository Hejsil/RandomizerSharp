namespace RandomizerSharp.PokemonModel
{
    public class IngameTrade
    {
        public Pokemon GivenPokemon { get; set; }

        public Item Item { get; set; }

        public int[] Ivs { get; set; } = new int[0];

        public string Nickname { get; set; }

        public int OtId { get; set; }
        public string OtName { get; set; }

        public Pokemon RequestedPokemon { get; set; }
        public int TradeId { get; set; }
    }
}