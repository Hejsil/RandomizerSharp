using System.Collections.Generic;

namespace RandomizerSharp.PokemonModel
{
    public class Move
    {
        public Move(int id)
        {
            Id = id;
        }

        public static IReadOnlyList<int> GameBreaking { get; } = new [] { 49, 82 };

        public const int StruggleId = 165;
        public MoveCategory Category { get; set; }
        public double Hitratio { get; set; }
        public int Id { get; }

        public string Name { get; set; }
        public int Power { get; set; }
        public int Pp { get; set; }
        public Typing Type { get; set; }

        public override string ToString() =>
            $"#{Name} - Power: {Power}, Base PP: {Pp}, Type: {Type}, Hit%: {Hitratio}";
    }
}