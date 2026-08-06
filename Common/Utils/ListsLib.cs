using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rogue.RNG;
using UnityEngine;

namespace Rogue.Helpers
{
    public static class ListsLib
    {
        public static T Extract<T>(this IList<T> list, int index = 0)
        {
            var item = list[index];
            list.RemoveAt(index);

            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ExtractLast<T>(this IList<T> list) => Extract(list, list.Count - 1);

        public static void MoveIndex<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            var element = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, element);
        }

        public static int GetFreeIndex<T>(this IReadOnlyList<T> list)
        {
            for (var index = 0; index < list.Count; index++)
            {
                if (list[index] == null) return index;
            }

            return -1;
        }

        public static bool HasSameItems<T>(this IList<T> list1, IList<T> list2)
            => list1 != null && new HashSet<T>(list1).SetEquals(list2);

        public static string ToDebugStr<T>(this IEnumerable<T> list) => string.Join(", ", list);

        #region Random Related

        private static readonly HashSet<int> sharedUsedIndexes = new();

        public static IList<T> Shuffle<T>(this IList<T> list, RandomSource rng) => list.Shuffle(list.Count, rng);

        public static IList<T> Shuffle<T>(this IList<T> list, int count, RandomSource rng)
        {
            for (var n = count - 1; n > 0; n--)
            {
                var k = RandomLib.NextInt(rng, 0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }

            return list;
        }

        public static T PickRandom<T>(this IList<T> list, RandomSource rng)
            => list[PickRandomIndex(list, rng)];

        public static int PickRandomIndex<T>(this IList<T> list, RandomSource rng) => list.Count switch
        {
            1 => 0,
            > 1 => RandomLib.NextInt(rng, 0, list.Count),
            _ => -1
        };

        public static List<T> PickNRandom<T>(this IList<T> list, int num, RandomSource rng)
        {
            var picked = new List<T>(num);

            for (var i = 0; i < num; i++)
                picked.Add(list.PickRandom(rng));

            return picked;
        }

        public static List<T> PickNUniqueRandom<T>(this IList<T> list, int num, RandomSource rng)
        {
            sharedUsedIndexes.Clear();

            var picked = new List<T>(num);

            for (var i = 0; i < num; i++)
                picked.Add(list.PickUniqueRandom(sharedUsedIndexes, rng));

            return picked;
        }

        public static void PickNUniqueRandom<T>(this IList<T> list, List<T> aggregator, int num, RandomSource rng)
        {
            sharedUsedIndexes.Clear();

            for (var i = 0; i < num; i++)
                aggregator.Add(list.PickUniqueRandom(sharedUsedIndexes, rng));
        }

        public static List<int> PickNRandomIndexes<T>(this IList<T> list, int num, RandomSource rng)
        {
            var picked = new List<int>(num);

            for (int i = 0; i < num; i++)
                picked.Add(RandomLib.NextInt(rng, 0, list.Count));

            return picked;
        }

        public static T PickUniqueRandom<T>(this IList<T> list, HashSet<int> usedIndexes, RandomSource rng)
        {
            var selected = rng.PickUniqueIndex(list.Count, usedIndexes);
            return list[selected];
        }

        public static int PickUniqueRandomIndex<T>(this IList<T> list, HashSet<int> usedIndexes, RandomSource rng)
            => rng.PickUniqueIndex(list.Count, usedIndexes);

        public static T PickValidRandom<T>(this IList<T> list, Func<int, bool> predicate, RandomSource rng)
        {
            var selected = rng.PickValidIndex(list.Count, predicate);
            return list[selected];
        }

        public static IEnumerable<T> IterateFromRandomIndex<T>(this IList<T> list, RandomSource rng)
        {
            var count = list.Count;
            var startIndex = PickRandomIndex(list, rng);

            for (var i = 0; i < count; i++)
            {
                var realIndex = (i + startIndex) % count;
                yield return list[realIndex];
            }
        }

        #endregion
    }
}