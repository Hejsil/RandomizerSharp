namespace RandomizerSharp.PokemonModel
{
    public class Move
    {
        public const int StruggleId = 165;

        public MoveCategory Category;
        public int EffectIndex;
        public double HitCount = 1; // not saved, only used in randomized move powers.
        public double Hitratio;
        public int InternalId;

        public string Name;
        public int Number;
        public int Power;
        public int Pp;
        public Typing Type;

        public override string ToString()
        {
            return "#" + Number + " " + Name + " - Power: " + Power + ", Base PP: " + Pp + ", Type: " + Type +
                   ", Hit%: " + Hitratio + ", Effect: " + EffectIndex;
        }
    }
}