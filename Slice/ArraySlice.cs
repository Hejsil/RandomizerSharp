using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Slice
{
    public class Slice<T> : IReadOnlyList<T>
    {
        public Slice(T[] elements)
            : this(elements, 0, elements.Length)
        { }

        public Slice(T[] elements, int offset)
            : this(elements, offset, elements.Length - offset)
        { }

        public Slice(T[] elements, int offset, int count)
        {
            Elements = elements;
            Offset = offset;
            Count = count;
        }

        public T[] Elements { get; }
        public int Offset { get; }
        public int Count { get; }

        public T this[int i]
        {
            get => Elements[i];
            set => Elements[i] = value;
        }


        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) Elements).GetEnumerator();
    
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }

    public static class SliceExtensions
    {

        public static Slice<T> ToSlice<T>(this Slice<T> slice) => slice;
        public static Slice<T> ToSlice<T>(this T[] array) => new Slice<T>(array);
        public static Slice<T> ToSlice<T>(this IEnumerable<T> enu)
        {
            if (enu is T[] array)
                array.ToSlice();

            if (enu is Slice<T> slice)
                return slice;

            return enu.ToArray().ToSlice();
        }
    }
}
