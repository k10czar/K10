using System;
using Random = Unity.Mathematics.Random;

namespace Rogue.RNG
{
    [Serializable]
    public class RandomSource
    {
        private Random rng;

        public uint Seed { get; private set; }

        public float NextFloat() => rng.NextFloat();
        public float NextFloat(float min, float max) => rng.NextFloat(min, max);

        public int NextInt() => rng.NextInt();
        public int NextInt(int minInclusive, int maxExclusive) => rng.NextInt(minInclusive, maxExclusive);

        public double NextDouble() => rng.NextDouble();

        public void ResetSeed(uint newSeed)
        {
            Seed = newSeed;
            rng.InitState(newSeed);
        }

        public RandomSource(uint seed)
        {
            Seed = seed;
            rng.InitState(seed);
        }

        public RandomSource() : this(RandomLib.GenerateSeed()) {}
    }
}