using System;
using System.Collections.Generic;

namespace RandomizerSharp.PokemonModel
{
    public sealed class EvolutionType
    {
        public static EvolutionType Happiness { get; } =
            new EvolutionType("HAPPINESS", EvolutionKind.Happiness, -1, 4, 1, 1, 1);

        public static EvolutionType HappinessDay { get; } =
            new EvolutionType("HAPPINESS_DAY", EvolutionKind.HappinessDay, -1, 4, 2, 2, 2);

        public static EvolutionType HappinessNight { get; } =
            new EvolutionType("HAPPINESS_NIGHT", EvolutionKind.HappinessNight, -1, 4, 3, 3, 3);

        public static EvolutionType Level { get; } = new EvolutionType("LEVEL", EvolutionKind.Level, 1, 1, 4, 4, 4);

        public static EvolutionType LevelAtkDefSame { get; } = new EvolutionType(
            "LEVEL_ATK_DEF_SAME",
            EvolutionKind.LevelAtkDefSame,
            -1,
            5,
            9,
            9,
            10);

        public static EvolutionType LevelAttackHigher { get; } =
            new EvolutionType("LEVEL_ATTACK_HIGHER", EvolutionKind.LevelAttackHigher, -1, 5, 8, 8, 9);

        public static EvolutionType LevelCreateExtra { get; } = new EvolutionType(
            "LEVEL_CREATE_EXTRA",
            EvolutionKind.LevelCreateExtra,
            -1,
            -1,
            13,
            13,
            14);

        public static EvolutionType LevelDefenseHigher { get; } =
            new EvolutionType("LEVEL_DEFENSE_HIGHER", EvolutionKind.LevelDefenseHigher, -1, 5, 10, 10, 11);

        public static EvolutionType LevelElectrifiedArea { get; } = new EvolutionType(
            "LEVEL_ELECTRIFIED_AREA",
            EvolutionKind.LevelElectrifiedArea,
            -1,
            -1,
            -1,
            24,
            25);

        public static EvolutionType LevelFemaleOnly { get; } = new EvolutionType(
            "LEVEL_FEMALE_ONLY",
            EvolutionKind.LevelFemaleOnly,
            -1,
            -1,
            -1,
            23,
            24);

        public static EvolutionType LevelHighBeauty { get; } = new EvolutionType(
            "LEVEL_HIGH_BEAUTY",
            EvolutionKind.LevelHighBeauty,
            -1,
            -1,
            15,
            15,
            16);

        public static EvolutionType LevelHighPv { get; } =
            new EvolutionType("LEVEL_HIGH_PV", EvolutionKind.LevelHighPv, -1, -1, 12, 12, 13);

        public static EvolutionType LevelIcyRock { get; } =
            new EvolutionType("LEVEL_ICY_ROCK", EvolutionKind.LevelIcyRock, -1, -1, -1, 26, 27);

        public static EvolutionType LevelIsExtra { get; } =
            new EvolutionType("LEVEL_IS_EXTRA", EvolutionKind.LevelIsExtra, -1, -1, 14, 14, 15);

        public static EvolutionType LevelItemDay { get; } =
            new EvolutionType("LEVEL_ITEM_DAY", EvolutionKind.LevelItemDay, -1, -1, -1, 18, 19);

        public static EvolutionType LevelItemNight { get; } = new EvolutionType(
            "LEVEL_ITEM_NIGHT",
            EvolutionKind.LevelItemNight,
            -1,
            -1,
            -1,
            19,
            20);

        public static EvolutionType LevelLowPv { get; } =
            new EvolutionType("LEVEL_LOW_PV", EvolutionKind.LevelLowPv, -1, -1, 11, 11, 12);

        public static EvolutionType LevelMaleOnly { get; } = new EvolutionType(
            "LEVEL_MALE_ONLY",
            EvolutionKind.LevelMaleOnly,
            -1,
            -1,
            -1,
            22,
            23);

        public static EvolutionType LevelMossRock { get; } = new EvolutionType(
            "LEVEL_MOSS_ROCK",
            EvolutionKind.LevelMossRock,
            -1,
            -1,
            -1,
            25,
            26);

        public static EvolutionType LevelWithMove { get; } = new EvolutionType(
            "LEVEL_WITH_MOVE",
            EvolutionKind.LevelWithMove,
            -1,
            -1,
            -1,
            20,
            21);

        public static EvolutionType LevelWithOther { get; } = new EvolutionType(
            "LEVEL_WITH_OTHER",
            EvolutionKind.LevelWithOther,
            -1,
            -1,
            -1,
            21,
            22);

        public static EvolutionType None { get; } = new EvolutionType("NONE", EvolutionKind.None, -1, -1, -1, -1, -1);

        public static EvolutionType Stone { get; } = new EvolutionType("STONE", EvolutionKind.Stone, 2, 2, 7, 7, 8);

        public static EvolutionType StoneFemaleOnly { get; } = new EvolutionType(
            "STONE_FEMALE_ONLY",
            EvolutionKind.StoneFemaleOnly,
            -1,
            -1,
            -1,
            17,
            18);

        public static EvolutionType StoneMaleOnly { get; } = new EvolutionType(
            "STONE_MALE_ONLY",
            EvolutionKind.StoneMaleOnly,
            -1,
            -1,
            -1,
            16,
            17);

        public static EvolutionType Trade { get; } = new EvolutionType("TRADE", EvolutionKind.Trade, 3, 3, 5, 5, 5);

        public static EvolutionType TradeItem { get; } =
            new EvolutionType("TRADE_ITEM", EvolutionKind.TradeItem, -1, 3, 6, 6, 6);

        public static EvolutionType TradeSpecial { get; } =
            new EvolutionType("TRADE_SPECIAL", EvolutionKind.TradeSpecial, -1, -1, -1, -1, 7);

        private static readonly EvolutionType[,] ReverseIndexes = new EvolutionType[5, 30];

        private static readonly IList<EvolutionType> ValueList = new List<EvolutionType>();
        private static int _nextOrdinal;


        private readonly int[] _indexNumbers;
        private readonly string _nameValue;
        private readonly int _ordinalValue;

        public EvolutionKind EvolutionKindValue { get; }

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
            {
                for (var i = 0; i < et._indexNumbers.Length; i++)
                {
                    if (et._indexNumbers[i] > 0 && ReverseIndexes[i, et._indexNumbers[i]] == null)
                        ReverseIndexes[i, et._indexNumbers[i]] = et;
                }
            }
        }

        private EvolutionType(string name, EvolutionKind evolutionKind, params int[] indexes)
        {
            _indexNumbers = indexes;

            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            EvolutionKindValue = evolutionKind;
        }

        public int ToIndex(int generation) => _indexNumbers[generation - 1];

        public static EvolutionType FromIndex(int generation, int index) => ReverseIndexes[generation - 1, index];

        public bool UsesLevel() => this == Level ||
                                   this == LevelAttackHigher ||
                                   this == LevelDefenseHigher ||
                                   this == LevelAtkDefSame ||
                                   this == LevelLowPv ||
                                   this == LevelHighPv ||
                                   this == LevelCreateExtra ||
                                   this == LevelIsExtra ||
                                   this == LevelMaleOnly ||
                                   this == LevelFemaleOnly;

        public static IList<EvolutionType> Values() => ValueList;

        public int Ordinal() => _ordinalValue;

        public override string ToString() => _nameValue;

        public static EvolutionType ValueOf(string name)
        {
            foreach (var enumInstance in ValueList)
            {
                if (enumInstance._nameValue == name)
                    return enumInstance;
            }

            throw new ArgumentException(name);
        }
    }
}