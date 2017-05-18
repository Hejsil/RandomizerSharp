using System;
using System.Collections.Generic;
using System.Text;

namespace RandomizerSharp.PokemonModel
{
    /*----------------------------------------------------------------------------*/
    /*--  Trainer.java - represents a Trainer's pokemon set/other details.      --*/
    /*--                                                                        --*/
    /*--  Part of "Universal Pokemon Randomizer" by Dabomstew                   --*/
    /*--  Pokemon and any associated names and the like are                     --*/
    /*--  trademark and (C) Nintendo 1996-2012.                                 --*/
    /*--                                                                        --*/
    /*--  The custom code written here is licensed under the terms of the GPL:  --*/
    /*--                                                                        --*/
    /*--  This program is free software: you can redistribute it and/or modify  --*/
    /*--  it under the terms of the GNU General Public License as published by  --*/
    /*--  the Free Software Foundation, either version 3 of the License, or     --*/
    /*--  (at your option) any later version.                                   --*/
    /*--                                                                        --*/
    /*--  This program is distributed in the hope that it will be useful,       --*/
    /*--  but WITHOUT ANY WARRANTY; without even the implied warranty of        --*/
    /*--  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the          --*/
    /*--  GNU General Public License for more details.                          --*/
    /*--                                                                        --*/
    /*--  You should have received a copy of the GNU General Public License     --*/
    /*--  along with this program. If not, see <http://www.gnu.org/licenses/>.  --*/
    /*----------------------------------------------------------------------------*/


    public class Trainer : IComparable<Trainer>
    {
        public string FullDisplayName;
        public bool ImportantTrainer;
        public string Name;
        public int Offset;
        public TrainerPokemon[] Pokemon;
        public int Poketype;
        public string Tag;
        public int Trainerclass;

        public virtual int CompareTo(Trainer o)
        {
            return Offset - o.Offset;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            if (!ReferenceEquals(FullDisplayName, null))
                sb.Append(FullDisplayName + " ");
            else if (!ReferenceEquals(Name, null))
                sb.Append(Name + " ");
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

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            var other = (Trainer) obj;

            if (Offset != other.Offset)
                return false;

            return true;
        }
    }
}