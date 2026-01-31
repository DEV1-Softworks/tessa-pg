using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class NoisePlatformAlgorithm : IPlatformPlacementAlgorithm
    {
        public float NoiseScale { get; }
        public float Threshold { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public int MaxPlatforms { get; }
        public int Seed { get; }

        public NoisePlatformAlgorithm(float noiseScale, float threshold, int minLength, int maxLength, int maxPlatforms, int seed)
        {
            NoiseScale = Mathf.Max(0.0001f, noiseScale);
            Threshold = Mathf.Clamp01(threshold);
            MinLength = Mathf.Max(1, minLength);
            MaxLength = Mathf.Max(MinLength, maxLength);
            MaxPlatforms = Mathf.Max(0, maxPlatforms);
            Seed = seed;
        }

        public IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource)
        {
            var results = new List<PlatformSegment>();
            if (!context.IsValid || context.Width < MinLength) return results;

            int rowsChecked = 0;
            for (int y = context.MinY; y <= context.MaxY && results.Count < MaxPlatforms; y++)
            {
                float noiseValue = SampleNoise(Seed, y);
                if (noiseValue < Threshold) continue;

                float lengthLerp = Mathf.Lerp(MinLength, MaxLength, noiseValue);
                int length = Mathf.Clamp(Mathf.RoundToInt(lengthLerp), MinLength, MaxLength);
                length = Mathf.Min(length, context.Width);
                int startX = randomSource.NextInt(context.MinX, context.MaxX - length + 2);

                results.Add(new PlatformSegment(startX, length, y));
                rowsChecked++;
            }

            return results;
        }

        private float SampleNoise(int seed, int row)
        {
            float x = (seed * 0.013f + row) * NoiseScale;
            return Mathf.PerlinNoise(x, seed * 0.017f);
        }
    }
}
