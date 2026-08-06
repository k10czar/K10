using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rogue.Helpers
{
    public interface IFetchable<T>
    {
        public T Key { get; }
    }

    public static class FetchableLib
    {
        public static bool ContainsFetchable<T, U>(this IEnumerable<U> list, T target) where U : class, IFetchable<T>
            => list.Any(fetchable => fetchable.Key.Equals(target));

        public static U Fetch<T, U>(this IEnumerable<U> list, T target) where U : class, IFetchable<T>
        {
            foreach (var fetchable in list)
            {
                if (fetchable.Key.Equals(target))
                    return fetchable;
            }

            return null;
        }

        public static int FetchIndex<T, U>(this IEnumerable<U> list, T target) where U : class, IFetchable<T>
        {
            var index = 0;
            foreach (var fetchable in list)
            {
                if (fetchable.Key.Equals(target))
                    return index;

                index++;
            }

            return -1;
        }

        public static Dictionary<T, U> ToDict<T, U>(this IEnumerable<U> list) where U : class, IFetchable<T>
            => list.ToDictionary(entry => entry.Key, entry => entry);

        public static bool RemoveFetchable<T, U>(this IList<U> list, T target) where U : class, IFetchable<T>
        {
            var index = list.FetchIndex(target);
            if (index == -1) return false;

            list.RemoveAt(index);
            return true;
        }

        private static T[] Replace<T, U>(T[] source, T[] toReplace) where T : class, IFetchable<U>
        {
            var array = source.ToArray();

            foreach (var entry in toReplace)
            {
                var index = array.FetchIndex(entry.Key);

                if (index == -1) Debug.LogError($"Failed to find {entry.Key} to replace!");
                else array[index] = entry;
            }

            return array;
        }

        private static T[] Remove<T, U>(T[] source, T[] toRemove) where T : class, IFetchable<U>
        {
            var list = source.ToList();

            foreach (var entry in toRemove)
            {
                var index = list.FetchIndex(entry.Key);

                if (index == -1) Debug.LogError($"Failed to find {entry.Key} to remove!");
                else list.RemoveAt(index);
            }

            return list.ToArray();
        }
    }
}