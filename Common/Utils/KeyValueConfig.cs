using System;
using System.Collections.Generic;
using System.Linq;
using Rogue.RNG;

namespace Rogue.Helpers
{
    [Serializable]
    public class KeyValueConfig<T,U>
    {
        public T key;
        public U value;

        public void Deconstruct(out T dKey, out U dValue)
        {
            dKey = key;
            dValue = value;
        }

        public KeyValueConfig() {}

        public KeyValueConfig(T key, U value)
        {
            this.key = key;
            this.value = value;
        }

        public override string ToString() => key.ToString();
    }

    [Serializable]
    public class KeyValueConfig<T,U,V>
    {
        public T key;
        public U value1;
        public V value2;

        public KeyValueConfig() {}

        public KeyValueConfig(T key, U value1, V value2)
        {
            this.key = key;
            this.value1 = value1;
            this.value2 = value2;
        }

        public void Deconstruct(out T dKey, out U dValue1, out V dValue2)
        {
            dKey = key;
            dValue1 = value1;
            dValue2 = value2;
        }

        public override string ToString() => key.ToString();
    }

    [Serializable]
    public class KeyValueConfig<T,U,V,W>
    {
        public T key;
        public U value1;
        public V value2;
        public W value3;

        public void Deconstruct(out T dKey, out U dValue1, out V dValue2, out W dValue3)
        {
            dKey = key;
            dValue1 = value1;
            dValue2 = value2;
            dValue3 = value3;
        }

        public override string ToString() => key.ToString();
    }

    public static  class KeyValueConfigExtensions
    {
        public static KeyValueConfig<T,U> Fetch<T,U>(this IList<KeyValueConfig<T,U>> source, T target)
            => source.FirstOrDefault(entry => entry.key.Equals(target));

        public static KeyValueConfig<T,U,V> Fetch<T,U,V>(this IList<KeyValueConfig<T,U,V>> source, T target)
            => source.FirstOrDefault(entry => entry.key.Equals(target));

        public static KeyValueConfig<T,U,V,W> Fetch<T,U,V,W>(this IList<KeyValueConfig<T,U,V,W>> source, T target)
            => source.FirstOrDefault(entry => entry.key.Equals(target));

        public static Dictionary<T, U> ToDictionary<T, U>(this IList<KeyValueConfig<T, U>> source)
            => source.ToDictionary(entry => entry.key, entry => entry.value);

        public static T PickValid<T>(this KeyValueConfig<T, bool>[] source)
        {
            var start = UnityEngine.Random.Range(0, source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                var index = (start + i) % source.Length;
                var entry = source[index];
                if (entry.value) continue;

                return entry.key;
            }

            throw new Exception("All values are disabled!");
        }

        public static T WeightedRandom<T>(this KeyValueConfig<T, float>[] source, RandomSource rng)
        {
            var sum = source.Sum(entry => entry.value);
            var random = RandomLib.NextFloat(rng, 0, sum);

            foreach (var (key, chance) in source)
            {
                if (random <= chance) return key;
                random -= chance;
            }

            throw new Exception("Weights misconfigured!");
        }

        public static T WeightedRandom<T>(this KeyValueConfig<T, float>[] source, HashSet<T> validSet, RandomSource rng)
        {
            var sum = source.Sum(entry => validSet.Contains(entry.key) ? entry.value : 0);
            var random = UnityEngine.Random.Range(0, sum);

            foreach (var (key, chance) in source)
            {
                if (!validSet.Contains(key)) continue;
                if (random <= chance) return key;
                random -= chance;
            }

            throw new Exception("Weights misconfigured!");
        }
    }
}