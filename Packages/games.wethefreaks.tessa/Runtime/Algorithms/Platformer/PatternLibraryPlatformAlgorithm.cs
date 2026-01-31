using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class PatternLibraryPlatformAlgorithm : IPlatformPlacementAlgorithm
    {
        private readonly IReadOnlyList<PlatformPattern> patterns;
        public int MaxPatternsPerRoom { get; }

        public PatternLibraryPlatformAlgorithm(IReadOnlyList<PlatformPattern> patterns, int maxPatternsPerRoom)
        {
            this.patterns = patterns ?? new List<PlatformPattern>();
            MaxPatternsPerRoom = Mathf.Max(0, maxPatternsPerRoom);
        }

        public IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource)
        {
            var results = new List<PlatformSegment>();
            if (!context.IsValid || patterns.Count == 0 || MaxPatternsPerRoom == 0) return results;

            int patternsToPlace = randomSource.NextInt(1, MaxPatternsPerRoom + 1);
            for (int i = 0; i < patternsToPlace; i++)
            {
                PlatformPattern pattern = patterns[randomSource.NextInt(0, patterns.Count)];
                if (pattern.Size.x <= 0 || pattern.Size.y <= 0) continue;
                if (pattern.Size.x > context.Width || pattern.Size.y > context.Height) continue;

                int originX = randomSource.NextInt(context.MinX, context.MaxX - pattern.Size.x + 2);
                int originY = randomSource.NextInt(context.MinY, context.MaxY - pattern.Size.y + 2);
                var origin = new Vector2Int(originX, originY);

                foreach (var segment in pattern.PlaceAt(origin))
                {
                    results.Add(segment);
                }
            }

            return results;
        }
    }
}
