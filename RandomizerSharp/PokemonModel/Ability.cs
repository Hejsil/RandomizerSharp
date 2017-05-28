using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerSharp.PokemonModel
{
    public class Ability
    {
        public int Id { get; }
        public string Name { get; set; }

        public Ability(int id) => Id = id;
    }
}
