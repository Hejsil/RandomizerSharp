using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerSharp.NDS;

namespace RandomizerSharp.PokemonModel
{
    public class MachineLearnt
    {
        public MachineLearnt(Machine machine) => Machine = machine;

        public Machine Machine { get; }
        public bool Learns { get; set; }

        public override string ToString() =>
            $"MachineLearnt: Learns: {Learns}, Machine: {Machine}";
    }
}
