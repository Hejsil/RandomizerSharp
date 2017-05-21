using System;

namespace RandomizerSharp.PokemonModel
{
    public class Evolution : IComparable<Evolution>, IEquatable<Evolution>
    {
        public Pokemon From { get; }
        public Pokemon To { get; }
        public bool CarryStats { get; set; }
        public int ExtraInfo { get; set; }
        public EvolutionType Type1 { get; set; }

        public Evolution(Pokemon from, Pokemon to, bool carryStats, EvolutionType type, int extra)
        {
            From = from;
            To = to;
            CarryStats = carryStats;
            Type1 = type;
            ExtraInfo = extra;
        }

        public int CompareTo(Evolution o)
        {
            if (From.Id < o.From.Id)
                return -1;
            if (From.Id > o.From.Id)
                return 1;
            if (To.Id < o.To.Id)
                return -1;
            if (To.Id > o.To.Id)
                return 1;
            if (Type1.Ordinal() < o.Type1.Ordinal())
                return -1;

            return Type1.Ordinal() > o.Type1.Ordinal() ? 1 : 0;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = prime * result + From.Id;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = prime * result + To.Id;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = prime * result + Type1.Ordinal();
            return result;
        }

        public bool Equals(Evolution evo)
        {
            if (ReferenceEquals(this, evo))
                return true;
            if (evo == null)
                return false;
            if (!Equals(From, evo.From))
                return false;
            if (!Equals(To, evo.To))
                return false;

            return Type1 == evo.Type1;
        }

        public override bool Equals(object obj) => Equals(obj as Evolution);
    }
}