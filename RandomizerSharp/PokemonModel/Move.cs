namespace RandomizerSharp.PokemonModel
{
    public class Move
    {
        public const int StruggleId = 165;
        public int EffectIndex { get; }

        public MoveCategory Category { get; set; }
        public double HitCount { get; set; } = 1;
        public double Hitratio { get; set; }
        public int InternalId { get; set; }

        public string Name { get; set; }
        public int Number { get; set; }
        public int Power { get; set; }
        public int Pp { get; set; }
        public Typing Type { get; set; }

        public override string ToString() =>
            $"#{Number} {Name} - Power: {Power}, Base PP: {Pp}, Type: {Type}, Hit%: {Hitratio}, Effect: {EffectIndex}";
    }
}