﻿using System;

namespace RandomizerSharp.Constants
{
    /*----------------------------------------------------------------------------*/
    /*--  SysConstants.java - contains constants not related to the             --*/
    /*--                      randomization process itself, such as those       --*/
    /*--                      relating to file I/O and the updating system.     --*/
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

    public class SysConstants
    {
        public const string AutoupdateUrl = "http://pokehacks.dabomstew.com/randomizer/autoupdate/";
        public const string CustomNamesFile = "customnames.rncn";
        public static readonly string LineSep = Environment.NewLine;
        public const string NnamesFile = "nicknames.txt";

        public const string TclassesFile = "trainerclasses.txt";

        // OLD custom names files
        public const string TnamesFile = "trainernames.txt";

        public const int UpdateVersion = 1721;
        public const string WebsiteUrl = "http://pokehacks.dabomstew.com/randomizer/";
    }
}