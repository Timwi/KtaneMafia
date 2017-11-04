using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rnd = UnityEngine.Random;

namespace Mafia
{
    static class Ut
    {
        public static T[] NewArray<T>(params T[] array) { return array; }

        public static int IndexOf(this Suspect[] suspects, Suspect name)
        {
            for (int i = 0; i < suspects.Length; i++)
                if (suspects[i] == name)
                    return i;
            return -1;
        }

        public static Suspect FindOrDefault(this Suspect[] suspects, Suspect name, Suspect otherwise)
        {
            var ix = suspects.IndexOf(name);
            return ix == -1 ? otherwise : suspects[ix];
        }

        public static Suspect After(this Suspect[] suspects, Suspect name, Suspect skip)
        {
            var ix = (suspects.IndexOf(name) + 1) % suspects.Length;
            if (suspects[ix] == skip)
                ix = (ix + 1) % suspects.Length;
            return suspects[ix];
        }

        public static Suspect After(this Suspect[] suspects, Suspect name, int number = 1)
        {
            return suspects[(suspects.IndexOf(name) + number) % suspects.Length];
        }

        public static Suspect FirstAfter(this Suspect[] suspects, Suspect startAfter, Func<Suspect, bool> predicate)
        {
            var ix = (suspects.IndexOf(startAfter) + 1) % suspects.Length;
            for (int i = 0; i < suspects.Length; i++)
            {
                var thisOne = suspects[(i + ix) % suspects.Length];
                if (predicate(thisOne))
                    return thisOne;
            }
            return suspects[ix];
        }

        public static bool ContainsNoCase(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        /// <summary>
        ///     Returns an enumeration of tuples containing all unique pairs of distinct elements from the source collection.
        ///     For example, the input sequence 1, 2, 3 yields the pairs [1,2], [1,3] and [2,3] only.</summary>
        public static IEnumerable<Pair<T>> UniquePairs<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return uniquePairsIterator(source);
        }
        private static IEnumerable<Pair<T>> uniquePairsIterator<T>(IEnumerable<T> source)
        {
            // Make sure that ‘source’ is evaluated only once
            IList<T> arr = source as IList<T> ?? source.ToArray();
            for (int i = 0; i < arr.Count - 1; i++)
                for (int j = i + 1; j < arr.Count; j++)
                    yield return new Pair<T>(arr[i], arr[j]);
        }

        public static bool AnyDuplicates<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            var hashSet = new HashSet<T>();
            foreach (var elem in source)
                if (!hashSet.Add(elem))
                    return true;
            return false;
        }

        /// <summary>
        ///     Returns the first element from the input sequence for which the value selector returns the largest value.</summary>
        /// <exception cref="InvalidOperationException">
        ///     The input collection is empty.</exception>
        public static T MaxElement<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where TValue : IComparable<TValue>
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("source contains no elements.");
                T minMaxElem = enumerator.Current;
                TValue minMaxValue = valueSelector(minMaxElem);
                int curIndex = 0;
                while (enumerator.MoveNext())
                {
                    curIndex++;
                    TValue value = valueSelector(enumerator.Current);
                    if (value.CompareTo(minMaxValue) > 0)
                    {
                        minMaxValue = value;
                        minMaxElem = enumerator.Current;
                    }
                }
                return minMaxElem;
            }
        }

        /// <summary>
        ///     Brings the elements of the given list into a random order.</summary>
        /// <typeparam name="T">
        ///     Type of the list.</typeparam>
        /// <param name="list">
        ///     List to shuffle.</param>
        /// <param name="rnd">
        ///     Random number generator, or null to use <see cref="Rnd"/>.</param>
        /// <returns>
        ///     The list operated on.</returns>
        public static T Shuffle<T>(this T list) where T : IList
        {
            if (list == null)
                throw new ArgumentNullException("list");
            for (int j = list.Count; j >= 1; j--)
            {
                int item = Rnd.Range(0, j);
                if (item < j - 1)
                {
                    var t = list[item];
                    list[item] = list[j - 1];
                    list[j - 1] = t;
                }
            }
            return list;
        }
    }
}
