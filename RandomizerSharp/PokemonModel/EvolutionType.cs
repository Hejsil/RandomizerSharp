using System;
using System.Collections.Generic;

namespace RandomizerSharp.PokemonModel
{
    public sealed class EvolutionType
    {
        public enum InnerEnum
        {
            Level,
            Stone,
            Trade,
            TradeItem,
            Happiness,
            HappinessDay,
            HappinessNight,
            LevelAttackHigher,
            LevelDefenseHigher,
            LevelAtkDefSame,
            LevelLowPv,
            LevelHighPv,
            LevelCreateExtra,
            LevelIsExtra,
            LevelHighBeauty,
            StoneMaleOnly,
            StoneFemaleOnly,
            LevelItemDay,
            LevelItemNight,
            LevelWithMove,
            LevelWithOther,
            LevelMaleOnly,
            LevelFemaleOnly,
            LevelElectrifiedArea,
            LevelMossRock,
            LevelIcyRock,
            TradeSpecial,
            None
        }

        /* @formatter:off */
        public static readonly EvolutionType Level = new EvolutionType("LEVEL", InnerEnum.Level, 1, 1, 4, 4, 4);

        public static readonly EvolutionType Stone = new EvolutionType("STONE", InnerEnum.Stone, 2, 2, 7, 7, 8);
        public static readonly EvolutionType Trade = new EvolutionType("TRADE", InnerEnum.Trade, 3, 3, 5, 5, 5);

        public static readonly EvolutionType TradeItem =
            new EvolutionType("TRADE_ITEM", InnerEnum.TradeItem, -1, 3, 6, 6, 6);

        public static readonly EvolutionType Happiness =
            new EvolutionType("HAPPINESS", InnerEnum.Happiness, -1, 4, 1, 1, 1);

        public static readonly EvolutionType HappinessDay =
            new EvolutionType("HAPPINESS_DAY", InnerEnum.HappinessDay, -1, 4, 2, 2, 2);

        public static readonly EvolutionType HappinessNight =
            new EvolutionType("HAPPINESS_NIGHT", InnerEnum.HappinessNight, -1, 4, 3, 3, 3);

        public static readonly EvolutionType LevelAttackHigher =
            new EvolutionType("LEVEL_ATTACK_HIGHER", InnerEnum.LevelAttackHigher, -1, 5, 8, 8, 9);

        public static readonly EvolutionType LevelDefenseHigher =
            new EvolutionType("LEVEL_DEFENSE_HIGHER", InnerEnum.LevelDefenseHigher, -1, 5, 10, 10, 11);

        public static readonly EvolutionType LevelAtkDefSame =
            new EvolutionType("LEVEL_ATK_DEF_SAME", InnerEnum.LevelAtkDefSame, -1, 5, 9, 9, 10);

        public static readonly EvolutionType LevelLowPv =
            new EvolutionType("LEVEL_LOW_PV", InnerEnum.LevelLowPv, -1, -1, 11, 11, 12);

        public static readonly EvolutionType LevelHighPv =
            new EvolutionType("LEVEL_HIGH_PV", InnerEnum.LevelHighPv, -1, -1, 12, 12, 13);

        public static readonly EvolutionType LevelCreateExtra =
            new EvolutionType("LEVEL_CREATE_EXTRA", InnerEnum.LevelCreateExtra, -1, -1, 13, 13, 14);

        public static readonly EvolutionType LevelIsExtra =
            new EvolutionType("LEVEL_IS_EXTRA", InnerEnum.LevelIsExtra, -1, -1, 14, 14, 15);

        public static readonly EvolutionType LevelHighBeauty =
            new EvolutionType("LEVEL_HIGH_BEAUTY", InnerEnum.LevelHighBeauty, -1, -1, 15, 15, 16);

        public static readonly EvolutionType StoneMaleOnly =
            new EvolutionType("STONE_MALE_ONLY", InnerEnum.StoneMaleOnly, -1, -1, -1, 16, 17);

        public static readonly EvolutionType StoneFemaleOnly =
            new EvolutionType("STONE_FEMALE_ONLY", InnerEnum.StoneFemaleOnly, -1, -1, -1, 17, 18);

        public static readonly EvolutionType LevelItemDay =
            new EvolutionType("LEVEL_ITEM_DAY", InnerEnum.LevelItemDay, -1, -1, -1, 18, 19);

        public static readonly EvolutionType LevelItemNight =
            new EvolutionType("LEVEL_ITEM_NIGHT", InnerEnum.LevelItemNight, -1, -1, -1, 19, 20);

        public static readonly EvolutionType LevelWithMove =
            new EvolutionType("LEVEL_WITH_MOVE", InnerEnum.LevelWithMove, -1, -1, -1, 20, 21);

        public static readonly EvolutionType LevelWithOther =
            new EvolutionType("LEVEL_WITH_OTHER", InnerEnum.LevelWithOther, -1, -1, -1, 21, 22);

        public static readonly EvolutionType LevelMaleOnly =
            new EvolutionType("LEVEL_MALE_ONLY", InnerEnum.LevelMaleOnly, -1, -1, -1, 22, 23);

        public static readonly EvolutionType LevelFemaleOnly =
            new EvolutionType("LEVEL_FEMALE_ONLY", InnerEnum.LevelFemaleOnly, -1, -1, -1, 23, 24);

        public static readonly EvolutionType LevelElectrifiedArea =
            new EvolutionType("LEVEL_ELECTRIFIED_AREA", InnerEnum.LevelElectrifiedArea, -1, -1, -1, 24, 25);

        public static readonly EvolutionType LevelMossRock =
            new EvolutionType("LEVEL_MOSS_ROCK", InnerEnum.LevelMossRock, -1, -1, -1, 25, 26);

        public static readonly EvolutionType LevelIcyRock =
            new EvolutionType("LEVEL_ICY_ROCK", InnerEnum.LevelIcyRock, -1, -1, -1, 26, 27);

        public static readonly EvolutionType TradeSpecial =
            new EvolutionType("TRADE_SPECIAL", InnerEnum.TradeSpecial, -1, -1, -1, -1, 7);

        public static readonly EvolutionType None = new EvolutionType("NONE", InnerEnum.None, -1, -1, -1, -1, -1);

        private static readonly IList<EvolutionType> ValueList = new List<EvolutionType>();
        private static int _nextOrdinal;
        
        private static readonly EvolutionType[,] ReverseIndexes = new EvolutionType[5, 30];

        public readonly InnerEnum InnerEnumValue;
        private readonly string _nameValue;

        private readonly int _ordinalValue;
        /* @formatter:on */

        private readonly int[] _indexNumbers;

        static EvolutionType()
        {
            ValueList.Add(Level);
            ValueList.Add(Stone);
            ValueList.Add(Trade);
            ValueList.Add(TradeItem);
            ValueList.Add(Happiness);
            ValueList.Add(HappinessDay);
            ValueList.Add(HappinessNight);
            ValueList.Add(LevelAttackHigher);
            ValueList.Add(LevelDefenseHigher);
            ValueList.Add(LevelAtkDefSame);
            ValueList.Add(LevelLowPv);
            ValueList.Add(LevelHighPv);
            ValueList.Add(LevelCreateExtra);
            ValueList.Add(LevelIsExtra);
            ValueList.Add(LevelHighBeauty);
            ValueList.Add(StoneMaleOnly);
            ValueList.Add(StoneFemaleOnly);
            ValueList.Add(LevelItemDay);
            ValueList.Add(LevelItemNight);
            ValueList.Add(LevelWithMove);
            ValueList.Add(LevelWithOther);
            ValueList.Add(LevelMaleOnly);
            ValueList.Add(LevelFemaleOnly);
            ValueList.Add(LevelElectrifiedArea);
            ValueList.Add(LevelMossRock);
            ValueList.Add(LevelIcyRock);
            ValueList.Add(TradeSpecial);
            ValueList.Add(None);

            foreach (var et in Values())
                for (var i = 0; i < et._indexNumbers.Length; i++)
                    if (et._indexNumbers[i] > 0 && ReverseIndexes[i, et._indexNumbers[i]] == null)
                        ReverseIndexes[i, et._indexNumbers[i]] = et;
        }

        private EvolutionType(string name, InnerEnum innerEnum, params int[] indexes)
        {
            _indexNumbers = indexes;

            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        public int ToIndex(int generation)
        {
            return _indexNumbers[generation - 1];
        }

        public static EvolutionType FromIndex(int generation, int index)
        {
            return ReverseIndexes[generation - 1, index];
        }

        public bool UsesLevel()
        {
            return this == Level || this == LevelAttackHigher || this == LevelDefenseHigher ||
                   this == LevelAtkDefSame || this == LevelLowPv || this == LevelHighPv ||
                   this == LevelCreateExtra || this == LevelIsExtra || this == LevelMaleOnly ||
                   this == LevelFemaleOnly;
        }

        public static IList<EvolutionType> Values()
        {
            return ValueList;
        }

        public int Ordinal()
        {
            return _ordinalValue;
        }

        public override string ToString()
        {
            return _nameValue;
        }

        public static EvolutionType ValueOf(string name)
        {
            foreach (var enumInstance in ValueList)
                if (enumInstance._nameValue == name)
                    return enumInstance;
            throw new ArgumentException(name);
        }
    }
}