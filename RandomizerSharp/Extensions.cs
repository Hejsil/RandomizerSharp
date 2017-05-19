using System;
using System.Collections.Generic;
using System.IO;

namespace RandomizerSharp
{
    public static class Extensions
    {
        public static void RemoveAll<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            var table = new HashSet<T>(items);
            foreach (var item in col)
                if (table.Contains(item))
                    col.Remove(item);
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

        public static int ReadFully(this Stream stream, byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public static long Seek(this Stream stream, int offset)
        {
            return stream.Seek(offset, SeekOrigin.Begin);
        }

        public static bool IsEmpty(this string str)
        {
            return str.Length == 0;
        }

        public static bool IsEmpty<T>(this T[] arr)
        {
            return arr.Length == 0;
        }

        public static bool IsEmpty<T>(this ICollection<T> col)
        {
            return col.Count == 0;
        }

        public static void AddAll<T1, T2>(this IDictionary<T1, T2> thisDictionary, IDictionary<T1, T2> other)
        {
            foreach (var pair in other)
                thisDictionary.Add(pair);
        }

        public static void RetainAll<T>(this ICollection<T> col, IEnumerable<T> items)
        {
            var table = new HashSet<T>(items);
            foreach (var item in col)
                if (!table.Contains(item))
                    col.Remove(item);
        }
    }
}