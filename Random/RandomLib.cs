using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rogue.Helpers;

namespace Rogue.RNG
{
    public static class RandomLib
    {
        public static uint GenerateSeed()
        {
            while (true)
            {
                var candidate = (uint) Guid.NewGuid().GetHashCode();
                if (candidate != 0) return candidate;
            }
        }

        #region Common Picks

        public static int PickUniqueIndex(this RandomSource rng, int maxIndexExclusive, HashSet<int> usedIndexes)
        {
            var attempts = 0;
            var selected = rng.NextInt(0, maxIndexExclusive);

            while (usedIndexes.Contains(selected))
            {
                selected = selected.AddCircular(maxIndexExclusive);

                if (++attempts > maxIndexExclusive)
                    throw new RandomException("Trying to pick valid index, but no index is valid");
            }

            usedIndexes.Add(selected);

            return selected;
        }

        public static int PickValidIndex(this RandomSource rng, int maxIndexExclusive, Func<int, bool> predicate)
        {
            var attempts = 0;
            var selected = rng.NextInt(0, maxIndexExclusive);

            while (!predicate(selected))
            {
                selected = selected.AddCircular(maxIndexExclusive);

                if (++attempts > maxIndexExclusive)
                    throw new RandomException("Trying to pick valid index, but no index is valid");
            }

            return selected;
        }

        #endregion

        #region Default Source

        public static readonly RandomSource DefaultSource = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat() => DefaultSource.NextFloat();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(RandomSource rng) => (rng ?? DefaultSource).NextFloat();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(float min, float max) => DefaultSource.NextFloat(min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloat(RandomSource rng, float min, float max) => (rng ?? DefaultSource).NextFloat(min, max);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static int NextInt() => DefaultSource.NextInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static int NextInt(RandomSource rng) => (rng ?? DefaultSource).NextInt();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextInt(int minInclusive, int maxExclusive) => DefaultSource.NextInt(minInclusive, maxExclusive);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextInt(RandomSource rng, int minInclusive, int maxExclusive) => (rng ?? DefaultSource).NextInt(minInclusive, maxExclusive);

        #endregion
    }
}