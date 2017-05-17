using System;
using System.Collections.Generic;

namespace RandomizerSharp
{
    /*----------------------------------------------------------------------------*/
    /*--  MiscTweak.java - represents a miscellaneous tweak that can be applied --*/
    /*--                   to some or all games that the randomizer supports.   --*/
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


    public class MiscTweak : IComparable<MiscTweak>
    {
        public static IList<MiscTweak> AllTweaks = new List<MiscTweak>();

        /* @formatter:off */
        // Higher priority value (third argument) = run first
        public static readonly MiscTweak BwExpPatch = new MiscTweak(1, "bwPatch", 0);

        public static readonly MiscTweak NerfXAccuracy = new MiscTweak(2, "nerfXAcc", 0);
        public static readonly MiscTweak FixCritRate = new MiscTweak(4, "critRateFix", 0);
        public static readonly MiscTweak FastestText = new MiscTweak(8, "fastestText", 0);
        public static readonly MiscTweak RunningShoesIndoors = new MiscTweak(16, "runningShoes", 0);
        public static readonly MiscTweak RandomizePcPotion = new MiscTweak(32, "pcPotion", 0);
        public static readonly MiscTweak AllowPikachuEvolution = new MiscTweak(64, "pikachuEvo", 0);
        public static readonly MiscTweak NationalDexAtStart = new MiscTweak(128, "nationalDex", 0);
        public static readonly MiscTweak UpdateTypeEffectiveness = new MiscTweak(256, "typeEffectiveness", 0);
        public static readonly MiscTweak RandomizeHiddenHollows = new MiscTweak(512, "hiddenHollows", 0);
        public static readonly MiscTweak LowerCasePokemonNames = new MiscTweak(1024, "lowerCaseNames", 0);
        public static readonly MiscTweak RandomizeCatchingTutorial = new MiscTweak(2048, "catchingTutorial", 0);
        public static readonly MiscTweak BanLuckyEgg = new MiscTweak(4096, "luckyEgg", 1);
        private readonly int _priority;

        /* @formatter:on */

        private MiscTweak(int value, string tweakId, int priority)
        {
            Value = value;
            _priority = priority;
            AllTweaks.Add(this);
        }

        public int Value { get; }

        public string TweakName { get; }

        public string TooltipText { get; }

        public int CompareTo(MiscTweak o)
        {
            // Order according to reverse priority, so higher priority = earlier in
            // ordering
            return o._priority - _priority;
        }
    }
}