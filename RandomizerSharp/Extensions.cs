using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomizerSharp
{
    public static class Extensions
    {

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

        public static int IndexOf<T>(this IEnumerable<T> enu, T item)
        {
            var eq = EqualityComparer<T>.Default;
            var index = 0;

            foreach (var element in enu)
            {
                if (eq.Equals(element, item))
                    return index;

                index++;
            }

            return -1;
        }

        public static T RandomItem<T>(this IList<T> list, Random random) => list[random.Next(list.Count)];
        public static T RandomItem<T>(this IReadOnlyList<T> list, Random random) => list[random.Next(list.Count)];

        public static void Populate<T>(this T[] arr, T value)
        {
            for (var i = 0; i < arr.Length; i++)
                arr[i] = value;
        }

        public static void Populate<T>(this T[] arr, Func<T> valueGetter)
        {
            for (var i = 0; i < arr.Length; i++)
                arr[i] = valueGetter();
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

        public static IEnumerable<(T1, T2)> Zip<T1, T2>(this IEnumerable<T1> enu1, IEnumerable<T2> enu2) => 
            enu1.Zip(enu2, (arg1, arg2) => (arg1, arg2));

        public static IEnumerator<T> GetEnumerator<T>(this T[] arr) => ((IEnumerable<T>) arr).GetEnumerator();

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