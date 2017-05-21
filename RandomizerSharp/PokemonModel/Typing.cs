using System;
using System.Collections.Generic;

namespace RandomizerSharp.PokemonModel
{
    public sealed class Typing
    {
        public enum InnerEnum
        {
            Normal,
            Fighting,
            Flying,
            Grass,
            Water,
            Fire,
            Rock,
            Ground,
            Psychic,
            Bug,
            Dragon,
            Electric,
            Ghost,
            Poison,
            Ice,
            Steel,
            Dark,
            Gas,
            Fairy,
            Wood,
            Abnormal,
            Wind,
            Sound,
            Light,
            Tri
        }

        public static Typing Abnormal { get; } = new Typing("ABNORMAL", InnerEnum.Abnormal, true);
        public static Typing Bug { get; } = new Typing("BUG", InnerEnum.Bug);
        public static Typing Dark { get; } = new Typing("DARK", InnerEnum.Dark);
        public static Typing Dragon { get; } = new Typing("DRAGON", InnerEnum.Dragon);
        public static Typing Electric { get; } = new Typing("ELECTRIC", InnerEnum.Electric);
        public static Typing Fairy { get; } = new Typing("FAIRY", InnerEnum.Fairy, true);
        public static Typing Fighting { get; } = new Typing("FIGHTING", InnerEnum.Fighting);
        public static Typing Fire { get; } = new Typing("FIRE", InnerEnum.Fire);
        public static Typing Flying { get; } = new Typing("FLYING", InnerEnum.Flying);
        public static Typing Gas { get; } = new Typing("GAS", InnerEnum.Gas, true);
        public static Typing Ghost { get; } = new Typing("GHOST", InnerEnum.Ghost);
        public static Typing Grass { get; } = new Typing("GRASS", InnerEnum.Grass);
        public static Typing Ground { get; } = new Typing("GROUND", InnerEnum.Ground);
        public static Typing Ice { get; } = new Typing("ICE", InnerEnum.Ice);
        public static Typing Light { get; } = new Typing("LIGHT", InnerEnum.Light, true);

        public static Typing Normal { get; } = new Typing("NORMAL", InnerEnum.Normal);
        public static Typing Poison { get; } = new Typing("POISON", InnerEnum.Poison);
        public static Typing Psychic { get; } = new Typing("PSYCHIC", InnerEnum.Psychic);
        public static Typing Rock { get; } = new Typing("ROCK", InnerEnum.Rock);
        public static Typing Sound { get; } = new Typing("SOUND", InnerEnum.Sound, true);
        public static Typing Steel { get; } = new Typing("STEEL", InnerEnum.Steel);
        public static Typing Tri { get; } = new Typing("TRI", InnerEnum.Tri, true);
        public static Typing Water { get; } = new Typing("WATER", InnerEnum.Water);
        public static Typing Wind { get; } = new Typing("WIND", InnerEnum.Wind, true);
        public static Typing Wood { get; } = new Typing("WOOD", InnerEnum.Wood, true);

        private static readonly IList<Typing> ValueList = new List<Typing>();
        private static int _nextOrdinal;
        private readonly string _nameValue;
        private readonly int _ordinalValue;

        public InnerEnum InnerEnumValue { get; }

        public bool IsHackOnly { get; }

        static Typing()
        {
            ValueList.Add(Normal);
            ValueList.Add(Fighting);
            ValueList.Add(Flying);
            ValueList.Add(Grass);
            ValueList.Add(Water);
            ValueList.Add(Fire);
            ValueList.Add(Rock);
            ValueList.Add(Ground);
            ValueList.Add(Psychic);
            ValueList.Add(Bug);
            ValueList.Add(Dragon);
            ValueList.Add(Electric);
            ValueList.Add(Ghost);
            ValueList.Add(Poison);
            ValueList.Add(Ice);
            ValueList.Add(Steel);
            ValueList.Add(Dark);
            ValueList.Add(Gas);
            ValueList.Add(Fairy);
            ValueList.Add(Wood);
            ValueList.Add(Abnormal);
            ValueList.Add(Wind);
            ValueList.Add(Sound);
            ValueList.Add(Light);
            ValueList.Add(Tri);
        }

        private Typing(string name, InnerEnum innerEnum)
        {
            IsHackOnly = false;

            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        private Typing(string name, InnerEnum innerEnum, bool isHackOnly)
        {
            IsHackOnly = isHackOnly;

            _nameValue = name;
            _ordinalValue = _nextOrdinal++;
            InnerEnumValue = innerEnum;
        }


        public static Typing RandomType(Random random) => ValueList[random.Next(ValueList.Count)];

        public string CamelCase() => RomFunctions.CamelCase(ToString());


        public static IList<Typing> Values() => ValueList;

        public int Ordinal() => _ordinalValue;

        public override string ToString() => _nameValue;

        public static Typing ValueOf(string name)
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