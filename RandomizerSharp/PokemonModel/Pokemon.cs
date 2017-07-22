using System;
using System.Collections.Generic;
using System.Drawing;

namespace RandomizerSharp.PokemonModel
{
    public class Pokemon : IComparable<Pokemon>, IEquatable<Pokemon>
    {
        public const int ShedinjaNumber = 292;

        private static readonly HashSet<int> Legendaries =
            new HashSet<int>
            {
                144,
                145,
                146,
                150,
                151,
                243,
                244,
                245,
                249,
                250,
                251,
                377,
                378,
                379,
                380,
                381,
                382,
                383,
                384,
                385,
                386,
                479,
                480,
                481,
                482,
                483,
                484,
                485,
                486,
                487,
                488,
                489,
                490,
                491,
                492,
                493,
                494,
                638,
                639,
                640,
                641,
                642,
                643,
                644,
                645,
                646,
                647,
                648,
                649
            };

        public List<Evolution> EvolutionsFrom { get; } = new List<Evolution>();
        public List<Evolution> EvolutionsTo { get; } = new List<Evolution>();

        public bool Legendary => Legendaries.Contains(Id);
        public List<MoveLearnt> MovesLearnt { get; } = new List<MoveLearnt>();

        public int Id { get; }
        public string Name { get; set; }

        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Spatk { get; set; }
        public int Spdef { get; set; }
        public int Special => (Spatk + Spdef) / 2;
        public int Speed { get; set; }

        public Ability Ability1 { get; set; }
        public Ability Ability2 { get; set; }
        public Ability Ability3 { get; set; }

        public int CatchRate { get; set; }

        public Item CommonHeldItem { get; set; }
        public Item RareHeldItem { get; set; }
        public Item DarkGrassHeldItem { get; set; }
        
        public ExpCurve GrowthExpCurve { get; set; }
        public Typing PrimaryType { get; set; }
        public Typing SecondaryType { get; set; }

        public Bitmap Sprite { get; set; }

        // ReSharper disable once InconsistentNaming
        public MachineLearnt[] TMHMCompatibility { get; internal set; } = Array.Empty<MachineLearnt>();
        public bool[] MoveTutorCompatibility { get; internal set; } = Array.Empty<bool>();

        public bool TemporaryFlag { get; set; }
        public List<int> ShuffledStatsOrder { get; set; } = new List<int> { 0, 1, 2, 3, 4, 5 };


        public Pokemon(int id) => Id = id;

        public int CompareTo(Pokemon o) => Id - o.Id;

        public virtual void ShuffleStats(Random random)
        {
            ShuffledStatsOrder.Shuffle(random);
            ApplyShuffledOrderToStats();
        }

        public virtual void CopyShuffledStatsUpEvolution(Pokemon evolvesFrom)
        {
            ShuffledStatsOrder = evolvesFrom.ShuffledStatsOrder;
            ApplyShuffledOrderToStats();
        }

        private void ApplyShuffledOrderToStats()
        {
            IList<int> stats = new List<int> { Hp, Attack, Defense, Spatk, Spdef, Speed };
            Hp = stats[ShuffledStatsOrder[0]];
            Attack = stats[ShuffledStatsOrder[1]];
            Defense = stats[ShuffledStatsOrder[2]];
            Spatk = stats[ShuffledStatsOrder[3]];
            Spdef = stats[ShuffledStatsOrder[4]];
            Speed = stats[ShuffledStatsOrder[5]];
        }

        public int Bst() => Hp + Attack + Defense + Spatk + Spdef + Speed;

        public int BstForPowerLevels()
        {
            if (Id == ShedinjaNumber)
                return (Attack + Defense + Spatk + Spdef + Speed) * 6 / 5;

            return Hp + Attack + Defense + Spatk + Spdef + Speed;
        }

        public override string ToString() => $"Pokemon[" +
                                             $"name={Name}, " +
                                             $"number={Id}, " +
                                             $"primaryType={PrimaryType}, " +
                                             $"secondaryType={SecondaryType}, " +
                                             $"hp={Hp}, " +
                                             $"attack={Attack}, " +
                                             $"defense={Defense}, " +
                                             $"spatk={Spatk}, " +
                                             $"spdef={Spdef}, " +
                                             $"speed={Speed}" +
                                             $"]";

        public string ToStringRby() => $"Pokemon[" +
                                       $"name={Name}, " +
                                       $"number={Id}, " +
                                       $"primaryType={PrimaryType}, " +
                                       $"secondaryType={SecondaryType}, " +
                                       $"hp={Hp}, " +
                                       $"attack={Attack}, " +
                                       $"defense={Defense}, " +
                                       $"special={Special}, " +
                                       $"speed={Speed}" +
                                       $"]";

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            result = prime * result + Id;

            return result;
        }

        public bool Equals(Pokemon poke)
        {
            if (ReferenceEquals(this, poke))
                return true;

            return Id == poke?.Id;
        }

        public override bool Equals(object obj) => Equals(obj as Pokemon);
    }
}