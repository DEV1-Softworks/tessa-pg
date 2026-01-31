using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class TieredPlatformAlgorithm : IPlatformPlacementAlgorithm
    {
        public int MinPlatforms { get; }
        public int MaxPlatforms { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public int TierCount { get; }
        public int MinVerticalSpacing { get; }

        public TieredPlatformAlgorithm(int minPlatforms, int maxPlatforms, int minLength, int maxLength, int tierCount, int minVerticalSpacing)
        {
            MinPlatforms = Mathf.Max(0, minPlatforms);
            MaxPlatforms = Mathf.Max(MinPlatforms, maxPlatforms);
            MinLength = Mathf.Max(1, minLength);
            MaxLength = Mathf.Max(MinLength, maxLength);
            TierCount = Mathf.Max(1, tierCount);
            MinVerticalSpacing = Mathf.Max(0, minVerticalSpacing);
        }

        public IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource)
        {
            var results = new List<PlatformSegment>();
            if (!context.IsValid || context.Width < MinLength) return results;

            int platformsToPlace = randomSource.NextInt(MinPlatforms, MaxPlatforms + 1);
            if (platformsToPlace == 0) return results;

            int tierHeight = Mathf.Max(1, context.Height / TierCount);
            var usedRows = new HashSet<int>();

            for (int i = 0; i < platformsToPlace; i++)
            {
                int tierIndex = i % TierCount;
                int tierMin = context.MinY + tierIndex * tierHeight;
                int tierMax = Mathf.Min(context.MaxY, tierMin + tierHeight - 1);

                int rowY = PickRow(tierMin, tierMax, usedRows, randomSource);
                usedRows.Add(rowY);

                int length = randomSource.NextInt(MinLength, MaxLength + 1);
                length = Mathf.Min(length, context.Width);
                int startX = randomSource.NextInt(context.MinX, context.MaxX - length + 2);
                results.Add(new PlatformSegment(startX, length, rowY));
            }

            return results;
        }

        private int PickRow(int minY, int maxY, HashSet<int> usedRows, IRandomSource randomSource)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                int candidate = randomSource.NextInt(minY, maxY + 1);
                if (!IsTooClose(candidate, usedRows)) return candidate;
                attempts++;
            }

            return minY;
        }

        private bool IsTooClose(int rowY, HashSet<int> usedRows)
        {
            foreach (int used in usedRows)
            {
                if (Mathf.Abs(used - rowY) <= MinVerticalSpacing) return true;
            }

            return false;
        }
    }
}
