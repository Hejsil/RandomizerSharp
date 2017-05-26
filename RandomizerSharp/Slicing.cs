using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RandomizerSharp
{
    public class ArraySlice<T> : IList<T>
    {
        public ArraySlice(T[] pointer)
            : this(pointer, pointer.Length)
        { }

        public ArraySlice(ArraySlice<T> slice)
            : this(slice.Pointer)
        { }

        public ArraySlice(ArraySlice<T> slice, int length, int offset = 0)
            : this(slice.Pointer, length, offset + slice.Offset)
        { }

        public ArraySlice(T[] pointer, int length, int offset = 0)
        {
            if (pointer == null)
                throw new ArgumentNullException();
            if (pointer.Length <= offset)
                throw new ArgumentOutOfRangeException();
            if (pointer.Length < length + offset)
                throw new ArgumentOutOfRangeException();

            Pointer = pointer;
            Length = length;
            Offset = offset;
        }

        public T[] Pointer { get; }
        public int Length { get; }
        public int Offset { get; }

        public static implicit operator ArraySlice<T>(T[] array) => new ArraySlice<T>(array);

        public IEnumerator<T> GetEnumerator() => Pointer.GetEnumerator<T>();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public void Insert(int index, T item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        public bool Contains(T item) => ((IEnumerable<T>) this).Contains(item);

        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            var index = 0;

            foreach (var element in this)
            {
                if (comparer.Equals(item, element))
                    return index;

                index++;
            }

            return -1;
        }


        public int Count => Length;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => Pointer[index + Offset];
            set => Pointer[index + Offset] = value;
        }

        public ArraySlice<T> Slice() => this;
        public ArraySlice<T> Slice(int count, int offset = 0) => new ArraySlice<T>(this, count, offset);
        public ArraySlice<T> SliceFrom(int from) => Slice(Length - from, from);
        public ArraySlice<T> SliceFrom(int from, int to) => Slice(to - from, from);

        public void CopyTo(T[] array, int arrayIndex = 0) => ArraySlice.Copy(this, 0, array, arrayIndex, Length);
    }

    public static class ArraySlice
    {
        public static ArraySlice<T> Slice<T>(this T[] array) => new ArraySlice<T>(array);
        public static ArraySlice<T> Slice<T>(this T[] array, int count, int offset = 0) => new ArraySlice<T>(array, count, offset);
        public static ArraySlice<T> SliceFrom<T>(this T[] array, int from) => array.Slice(array.Length - from, from);
        public static ArraySlice<T> SliceFrom<T>(this T[] array, int from, int to) => array.Slice(to - from, from);

        public static ArraySlice<T> SliceFrom<T>(this IEnumerable<T> enu, int from, int to) => enu.Slice(to - from, from);
        public static ArraySlice<T> Slice<T>(this IEnumerable<T> enu)
        {
            if (enu is ArraySlice<T> slice)
                return slice;

            if (enu is T[] array)
                return array.Slice();

            return enu.ToArray().Slice();
        }

        public static ArraySlice<T> Slice<T>(this IEnumerable<T> enu, int count, int offset = 0)
        {
            if (enu is ArraySlice<T> slice)
                return slice.Slice(count, offset);

            if (enu is T[] array)
                return array.Slice(count, offset);

            return enu.ToArray().Slice(count, offset);
        }

        public static ArraySlice<T> SliceFrom<T>(this IEnumerable<T> enu, int from)
        {
            if (enu is ArraySlice<T> slice)
                return slice.SliceFrom(from);

            if (enu is T[] array)
                return array.SliceFrom(from);

            return enu.ToArray().SliceFrom(from);
        }

        public static void Copy<T>(ArraySlice<T> source, ArraySlice<T> destination, int length) => Copy(source, 0, destination, 0, length);
        public static void Copy<T>(ArraySlice<T> source, int sourceOffset, ArraySlice<T> destination, int destinationOffset, int length)
        {
            Array.Copy(
                source.Pointer, 
                source.Offset + sourceOffset, 
                destination.Pointer, 
                destination.Offset + destinationOffset, 
                length
            );
        }

        public static void Copy<T>(T[] source, ArraySlice<T> destination, int length) => Copy(source, 0, destination, 0, length);
        public static void Copy<T>(T[] source, int sourceOffset, ArraySlice<T> destination, int destinationOffset, int length)
        {
            Array.Copy(
                source,
                sourceOffset,
                destination.Pointer,
                destination.Offset + destinationOffset,
                length
            );
        }

        public static void Copy<T>(ArraySlice<T> source, T[] destination, int length) => Copy(source, 0, destination, 0, length);
        public static void Copy<T>(ArraySlice<T> source, int sourceOffset, T[] destination, int destinationOffset, int length)
        {
            Array.Copy(
                source.Pointer,
                source.Offset + sourceOffset,
                destination,
                destinationOffset,
                length
            );
        }
    }
}