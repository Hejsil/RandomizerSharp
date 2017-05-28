namespace RandomizerSharp.PokemonModel
{
    public class Item
    {
        public Item(int id) => Id = id;

        public int Id { get; }
        public string Name { get; set; }
    }
}
