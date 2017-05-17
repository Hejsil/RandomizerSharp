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

        public static readonly Typing Normal = new Typing("NORMAL", InnerEnum.Normal);
        public static readonly Typing Fighting = new Typing("FIGHTING", InnerEnum.Fighting);
        public static readonly Typing Flying = new Typing("FLYING", InnerEnum.Flying);
        public static readonly Typing Grass = new Typing("GRASS", InnerEnum.Grass);
        public static readonly Typing Water = new Typing("WATER", InnerEnum.Water);
        public static readonly Typing Fire = new Typing("FIRE", InnerEnum.Fire);
        public static readonly Typing Rock = new Typing("ROCK", InnerEnum.Rock);
        public static readonly Typing Ground = new Typing("GROUND", InnerEnum.Ground);
        public static readonly Typing Psychic = new Typing("PSYCHIC", InnerEnum.Psychic);
        public static readonly Typing Bug = new Typing("BUG", InnerEnum.Bug);
        public static readonly Typing Dragon = new Typing("DRAGON", InnerEnum.Dragon);
        public static readonly Typing Electric = new Typing("ELECTRIC", InnerEnum.Electric);
        public static readonly Typing Ghost = new Typing("GHOST", InnerEnum.Ghost);
        public static readonly Typing Poison = new Typing("POISON", InnerEnum.Poison);
        public static readonly Typing Ice = new Typing("ICE", InnerEnum.Ice);
        public static readonly Typing Steel = new Typing("STEEL", InnerEnum.Steel);
        public static readonly Typing Dark = new Typing("DARK", InnerEnum.Dark);
        public static readonly Typing Gas = new Typing("GAS", InnerEnum.Gas, true);
        public static readonly Typing Fairy = new Typing("FAIRY", InnerEnum.Fairy, true);
        public static readonly Typing Wood = new Typing("WOOD", InnerEnum.Wood, true);
        public static readonly Typing Abnormal = new Typing("ABNORMAL", InnerEnum.Abnormal, true);
        public static readonly Typing Wind = new Typing("WIND", InnerEnum.Wind, true);
        public static readonly Typing Sound = new Typing("SOUND", InnerEnum.Sound, true);
        public static readonly Typing Light = new Typing("LIGHT", InnerEnum.Light, true);
        public static readonly Typing Tri = new Typing("TRI", InnerEnum.Tri, true);

        private static readonly IList<Typing> ValueList = new List<Typing>();
        private static int _nextOrdinal;

        public readonly InnerEnum InnerEnumValue;
        private readonly string _nameValue;
        private readonly int _ordinalValue;

        public bool IsHackOnly;

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


        public static Typing RandomType(Random random)
        {
            return ValueList[random.Next(ValueList.Count)];
        }

        public string CamelCase()
        {
            return RomFunctions.CamelCase(ToString());
        }


        public static IList<Typing> Values()
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

        public static Typing ValueOf(string name)
        {
            foreach (var enumInstance in ValueList)
                if (enumInstance._nameValue == name)
                    return enumInstance;
            throw new ArgumentException(name);
        }
    }
}