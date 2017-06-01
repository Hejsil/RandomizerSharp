using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp.NDS
{
    public class Machine
    {
        public Machine(int id, Kind type)
        {
            Id = id;
            Type = type;
        }

        public Move Move { get; set; }
        public int Id { get; }
        public Kind Type { get; }

        public override string ToString() =>
            $"Machine: Id: {Id}, Move: {Move}";

        public enum Kind
        {
            Hidden,
            Technical
        }
    }
}
