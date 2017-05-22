namespace RandomizerSharp.PokemonModel
{
    public class Item
    {
        public Item(int id, Kind type)
        {
            Id = id;
            Type = type;
        }

        public int Id { get; }
        public Kind Type { get; }
        
        public enum Kind
        {
            Technical,
            Hidden,
            Other
        }
    }
}
