using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class CriticalPathPlatformAlgorithm : IPlatformPlacementAlgorithm
    {
        public int MinPlatformLength { get; }
        public int MaxPlatformLength { get; }
        public int MinStepX { get; }
        public int MaxStepX { get; }
        public int MaxStepY { get; }
        public int ExtraPlatforms { get; }

        public CriticalPathPlatformAlgorithm(int minPlatformLength, int maxPlatformLength, int minStepX, int maxStepX, int maxStepY, int extraPlatforms)
        {
            MinPlatformLength = Mathf.Max(1, minPlatformLength);
            MaxPlatformLength = Mathf.Max(MinPlatformLength, maxPlatformLength);
            MinStepX = Mathf.Max(1, minStepX);
            MaxStepX = Mathf.Max(MinStepX, maxStepX);
            MaxStepY = Mathf.Max(0, maxStepY);
            ExtraPlatforms = Mathf.Max(0, extraPlatforms);
        }

        public IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource)
        {
            var results = new List<PlatformSegment>();
            if (!context.IsValid || context.Width < MinPlatformLength) return results;

            Vector2Int entry = context.HasEntryExit ? context.EntryPoint : new Vector2Int(context.MinX, context.MinY + context.Height / 2);
            Vector2Int exit = context.HasEntryExit ? context.ExitPoint : new Vector2Int(context.MaxX, context.MinY + context.Height / 2);

            entry = ClampPoint(entry, context);
            exit = ClampPoint(exit, context);

            int currentX = entry.x;
            int currentY = entry.y;

            while (currentX < exit.x)
            {
                int length = randomSource.NextInt(MinPlatformLength, MaxPlatformLength + 1);
                length = Mathf.Min(length, context.MaxX - currentX + 1);
                results.Add(new PlatformSegment(currentX, length, currentY));

                int nextX = currentX + length + randomSource.NextInt(MinStepX, MaxStepX + 1);
                int deltaY = randomSource.NextInt(-MaxStepY, MaxStepY + 1);
                int nextY = Mathf.Clamp(currentY + deltaY, context.MinY, context.MaxY);

                currentX = Mathf.Min(nextX, exit.x);
                currentY = nextY;
            }

            AddExtraPlatforms(context, randomSource, results);
            return results;
        }

        private void AddExtraPlatforms(PlatformPlacementContext context, IRandomSource randomSource, List<PlatformSegment> results)
        {
            for (int i = 0; i < ExtraPlatforms; i++)
            {
                int length = randomSource.NextInt(MinPlatformLength, MaxPlatformLength + 1);
                length = Mathf.Min(length, context.Width);
                int startX = randomSource.NextInt(context.MinX, context.MaxX - length + 2);
                int rowY = randomSource.NextInt(context.MinY, context.MaxY + 1);
                results.Add(new PlatformSegment(startX, length, rowY));
            }
        }

        private Vector2Int ClampPoint(Vector2Int point, PlatformPlacementContext context)
        {
            int x = Mathf.Clamp(point.x, context.MinX, context.MaxX);
            int y = Mathf.Clamp(point.y, context.MinY, context.MaxY);
            return new Vector2Int(x, y);
        }
    }
}
