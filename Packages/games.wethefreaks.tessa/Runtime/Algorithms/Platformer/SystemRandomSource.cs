using System;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random random;

        public SystemRandomSource(int seed)
        {
            random = new Random(seed);
        }

        public SystemRandomSource(Random random)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            return random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat()
        {
            return (float)random.NextDouble();
        }
    }
}
