using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class UnityRandomSource : IRandomSource
    {
        public int NextInt(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }

        public float NextFloat()
        {
            return Random.value;
        }
    }
}
