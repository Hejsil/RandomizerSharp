using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomizerSharp.PokemonModel;

namespace RandomizerSharp
{
    public static class Extensions
    {
        public static int Generation(this Game game)
        {
            switch (game)
            {
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                    return 1;

                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return 2;

                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                    return 3;

                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                    return 4;

                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                    return 5;

                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                    return 6;

                case Game.Sun:
                case Game.Moon:
                    return 7;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }

        public static void RemoveAll<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            var table = new HashSet<T>(items);
            
            foreach (var item in col.ToArray())
            {
                if (table.Contains(item))
                    col.Remove(item);
            }
        }

        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Populate<T>(this T[] arr, T value)
        {
            for (var i = 0; i < arr.Length; i++)
                arr[i] = value;
        }

        public static int ReadFully(this Stream stream, byte[] buffer) => stream.Read(buffer, 0, buffer.Length);

        public static long Seek(this Stream stream, int offset) => stream.Seek(offset, SeekOrigin.Begin);

        public static bool IsEmpty(this string str) => str.Length == 0;

        public static bool IsEmpty<T>(this T[] arr) => arr.Length == 0;

        public static bool IsEmpty<T>(this ICollection<T> col) => col.Count == 0;

        public static void AddAll<T1, T2>(this IDictionary<T1, T2> thisDictionary, IDictionary<T1, T2> other)
        {
            foreach (var pair in other)
                thisDictionary.Add(pair);
        }

        public static void RetainAll<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            var table = new HashSet<T>(items);
            foreach (var item in col.ToArray())
            {
                if (!table.Contains(item))
                    col.Remove(item);
            }
        }
    }
}