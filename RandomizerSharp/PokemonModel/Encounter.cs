namespace RandomizerSharp.PokemonModel
{
    /*----------------------------------------------------------------------------*/
    /*--  Encounter.java - contains one wild Pokemon slot                       --*/
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

    public class Encounter
    {
        public int Level;
        public int MaxLevel;
        public Pokemon Pokemon;

        public override string ToString()
        {
            if (Pokemon == null)
                return "ERROR";
            if (MaxLevel == 0)
                return Pokemon.Name + " Lv" + Level;
            return Pokemon.Name + " Lvs " + Level + "-" + MaxLevel;
        }
    }
}