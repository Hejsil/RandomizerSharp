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
        public static IList<MiscTweak> AllTweaks { get; private set; }

        public static MiscTweak AllowPikachuEvolution { get; } = new MiscTweak(64, "pikachuEvo", 0);
        public static MiscTweak BanLuckyEgg { get; } = new MiscTweak(4096, "luckyEgg", 1);

        /* @formatter:off */
        // Higher priority value (third argument) = run first

        public static MiscTweak BwExpPatch { get; } = new MiscTweak(1, "bwPatch", 0);
        public static MiscTweak FastestText { get; } = new MiscTweak(8, "fastestText", 0);
        public static MiscTweak FixCritRate { get; } = new MiscTweak(4, "critRateFix", 0);
        public static MiscTweak LowerCasePokemonNames { get; } = new MiscTweak(1024, "lowerCaseNames", 0);
        public static MiscTweak NationalDexAtStart { get; } = new MiscTweak(128, "nationalDex", 0);

        public static MiscTweak NerfXAccuracy { get; } = new MiscTweak(2, "nerfXAcc", 0);
        public static MiscTweak RandomizeCatchingTutorial { get; } = new MiscTweak(2048, "catchingTutorial", 0);
        public static MiscTweak RandomizeHiddenHollows { get; } = new MiscTweak(512, "hiddenHollows", 0);
        public static MiscTweak RandomizePcPotion { get; } = new MiscTweak(32, "pcPotion", 0);
        public static MiscTweak RunningShoesIndoors { get; } = new MiscTweak(16, "runningShoes", 0);
        public static MiscTweak UpdateTypeEffectiveness { get; } = new MiscTweak(256, "typeEffectiveness", 0);

        private readonly int _priority;

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string TooltipText { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string TweakName { get; }

        public int Value { get; }

        /* @formatter:on */

        // ReSharper disable once UnusedParameter.Local
        private MiscTweak(int value, string tweakId, int priority)
        {
            if (AllTweaks == null)
                AllTweaks = new List<MiscTweak>();

            Value = value;
            TweakName = tweakId;
            TooltipText = tweakId;
            _priority = priority;
            AllTweaks.Add(this);
        }

        public int CompareTo(MiscTweak o) => o._priority - _priority;
    }
}