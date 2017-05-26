using System;
using System.Text;

namespace RandomizerSharp.PokemonModel
{
    public class Trainer : IEquatable<Trainer>
    {
        public TrainerPokemon[] Pokemon { get; set; }
        public int Poketype { get; set; }
        public string Tag { get; set; }
        public int Trainerclass { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            
            if (Trainerclass != 0)
                sb.Append("(" + Trainerclass + ")");

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
            result = prime * result;

            return result;
        }

        public bool Equals(Trainer trainer)
        {
            if (ReferenceEquals(this, trainer))
                return true;

            return false;
        }

        public override bool Equals(object obj) => Equals(obj as Trainer);
    }
}