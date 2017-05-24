using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomizerSharp
{
    public static class SliceExtensions
    {
        public static ArraySlice<T> Slice<T>(this IEnumerable<T> enu)
        {
            if (enu is ArraySlice<T> slice)
                return slice;

            if (enu is T[] array)
                return array.Slice();

            return enu.ToArray().Slice();
        }

        public static ArraySlice<T> Slice<T>(this T[] array) => new ArraySlice<T>(array);
        public static ArraySlice<T> Slice<T>(this ArraySlice<T> slice) => slice;


        public static ArraySlice<T> Slice<T>(this IEnumerable<T> enu, int count, int offset = 0)
        {
            if (enu is ArraySlice<T> slice)
                return slice.Slice(count, offset);

            if (enu is T[] array)
                return array.Slice(count, offset);

            return enu.ToArray().Slice(count, offset);
        }

        public static ArraySlice<T> Slice<T>(this T[] array, int count, int offset = 0) => new ArraySlice<T>(
            array,
            offset,
            count);

        public static ArraySlice<T> Slice<T>(this ArraySlice<T> slice, int count, int offset = 0) => new ArraySlice<T>(
            slice,
            offset,
            count);


        public static ArraySlice<T> SliceFrom<T>(this IEnumerable<T> enu, int from)
        {
            if (enu is ArraySlice<T> slice)
                return slice.SliceFrom(from);

            if (enu is T[] array)
                return array.SliceFrom(from);

            return enu.ToArray().SliceFrom(from);
        }

        public static ArraySlice<T> SliceFrom<T>(this T[] array, int from) => array.Slice(array.Length - from, from);

        public static ArraySlice<T> SliceFrom<T>(this ArraySlice<T> slice, int from) => slice.Slice(
            slice.Length - from,
            from);


        public static ArraySlice<T> SliceFrom<T>(this IEnumerable<T> enu, int from, int to)
        {
            if (enu is ArraySlice<T> slice)
                return slice.SliceFrom(from, to);

            if (enu is T[] array)
                return array.SliceFrom(from, to);

            return enu.ToArray().SliceFrom(from, to);
        }

        public static ArraySlice<T> SliceFrom<T>(this T[] array, int from, int to) => array.Slice(to - from, from);

        public static ArraySlice<T> SliceFrom<T>(this ArraySlice<T> slice, int from, int to) => slice.Slice(
            to - from,
            from);

        public static T[] CopyToArray<T>(this ArraySlice<T> slice, int count, int offset = 0)
        {
            var result = new T[count];
            Buffer.BlockCopy(slice.Array, slice.Offset + offset, result, 0, count);

            return result;
        }
    }
}