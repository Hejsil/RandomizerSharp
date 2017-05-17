using System.Collections.Generic;

namespace RandomizerSharp.PokemonModel
{
    /*----------------------------------------------------------------------------*/
    /*--  EncounterSet.java - contains a group of wild Pokemon                  --*/
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


    public class EncounterSet
    {
        public ISet<Pokemon> BannedPokemon = new HashSet<Pokemon>();
        public string DisplayName;
        public Encounter[] Encounters;
        public int Offset;

        public int Rate;

        public override string ToString()
        {
            return "Encounter [Rate = " + Rate + ", Encounters = " + Encounters + "]";
        }
    }
}