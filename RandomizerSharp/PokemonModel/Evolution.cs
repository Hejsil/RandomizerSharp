using System;

namespace RandomizerSharp.PokemonModel
{
    public class Evolution : IComparable<Evolution>
    {
        public bool CarryStats;
        public int ExtraInfo;

        public Pokemon From;
        public Pokemon To;
        public EvolutionType Type;

        public Evolution(Pokemon from, Pokemon to, bool carryStats, EvolutionType type, int extra)
        {
            From = from;
            To = to;
            CarryStats = carryStats;
            Type = type;
            ExtraInfo = extra;
        }

        public virtual int CompareTo(Evolution o)
        {
            if (From.Id < o.From.Id)
                return -1;
            if (From.Id > o.From.Id)
                return 1;
            if (To.Id < o.To.Id)
                return -1;
            if (To.Id > o.To.Id)
                return 1;
            if (Type.Ordinal() < o.Type.Ordinal())
                return -1;
            if (Type.Ordinal() > o.Type.Ordinal())
                return 1;
            return 0;
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
            result = prime * result + Type.Ordinal();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            var other = (Evolution) obj;
            if (From != other.From)
                return false;
            if (To != other.To)
                return false;
            if (Type != other.Type)
                return false;
            return true;
        }
    }
}