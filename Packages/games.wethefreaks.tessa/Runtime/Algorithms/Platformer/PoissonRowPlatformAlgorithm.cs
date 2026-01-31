using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class PoissonRowPlatformAlgorithm : IPlatformPlacementAlgorithm
    {
        public int MinLength { get; }
        public int MaxLength { get; }
        public int MinRowSpacing { get; }
        public int MaxPlatforms { get; }
        public int MaxPlacementAttempts { get; }

        public PoissonRowPlatformAlgorithm(int minLength, int maxLength, int minRowSpacing, int maxPlatforms, int maxPlacementAttempts)
        {
            MinLength = Mathf.Max(1, minLength);
            MaxLength = Mathf.Max(MinLength, maxLength);
            MinRowSpacing = Mathf.Max(0, minRowSpacing);
            MaxPlatforms = Mathf.Max(0, maxPlatforms);
            MaxPlacementAttempts = Mathf.Max(1, maxPlacementAttempts);
        }

        public IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource)
        {
            var results = new List<PlatformSegment>();
            if (!context.IsValid || context.Width < MinLength) return results;

            var usedRows = new HashSet<int>();
            int attempts = 0;

            while (results.Count < MaxPlatforms && attempts < MaxPlacementAttempts)
            {
                attempts++;
                int rowY = randomSource.NextInt(context.MinY, context.MaxY + 1);
                if (IsTooClose(rowY, usedRows)) continue;

                int length = randomSource.NextInt(MinLength, MaxLength + 1);
                length = Mathf.Min(length, context.Width);
                int startX = randomSource.NextInt(context.MinX, context.MaxX - length + 2);

                usedRows.Add(rowY);
                results.Add(new PlatformSegment(startX, length, rowY));
            }

            return results;
        }

        private bool IsTooClose(int rowY, HashSet<int> usedRows)
        {
            foreach (int used in usedRows)
            {
                if (Mathf.Abs(used - rowY) <= MinRowSpacing) return true;
            }

            return false;
        }
    }
}
