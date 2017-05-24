using System;
using System.Text;

namespace RandomizerSharp.PokemonModel
{
    public class Trainer : IComparable<Trainer>, IEquatable<Trainer>
    {
        public bool ImportantTrainer { get; }
        public int Offset { get; set; }
        public TrainerPokemon[] Pokemon { get; set; }
        public int Poketype { get; set; }
        public string Tag { get; set; }
        public int Trainerclass { get; set; }

        public virtual int CompareTo(Trainer o) => Offset - o.Offset;

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            
            if (Trainerclass != 0)
                sb.Append("(" + Trainerclass + ") - ");

            sb.Append($"{Offset:x}");
            sb.Append(" => ");

            var first = true;
            foreach (var p in Pokemon)
            {
                if (!first)
                    sb.Append(',');
                sb.Append(p.Pokemon.Name + " Lv" + p.Level);
                first = false;
            }

            sb.Append(']');

            if (!ReferenceEquals(Tag, null))
                sb.Append(" (" + Tag + ")");

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = prime * result + Offset;

            return result;
        }

        public bool Equals(Trainer trainer)
        {
            if (ReferenceEquals(this, trainer))
                return true;

            return Offset == trainer?.Offset;
        }

        public override bool Equals(object obj) => Equals(obj as Trainer);
    }
}