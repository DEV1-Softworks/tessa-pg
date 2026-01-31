using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public static class DefaultPlatformPatterns
    {
        public static IReadOnlyList<PlatformPattern> Create()
        {
            return new List<PlatformPattern>
            {
                new PlatformPattern(
                    "StairsUp",
                    new Vector2Int(10, 6),
                    new List<PlatformSegment>
                    {
                        new PlatformSegment(0, 4, 0),
                        new PlatformSegment(3, 4, 2),
                        new PlatformSegment(6, 4, 4)
                    }
                ),
                new PlatformPattern(
                    "ZigZag",
                    new Vector2Int(12, 6),
                    new List<PlatformSegment>
                    {
                        new PlatformSegment(0, 5, 1),
                        new PlatformSegment(5, 5, 3),
                        new PlatformSegment(2, 4, 5)
                    }
                ),
                new PlatformPattern(
                    "TwoTiers",
                    new Vector2Int(12, 6),
                    new List<PlatformSegment>
                    {
                        new PlatformSegment(0, 6, 1),
                        new PlatformSegment(6, 6, 4)
                    }
                ),
                new PlatformPattern(
                    "CentralIslands",
                    new Vector2Int(12, 6),
                    new List<PlatformSegment>
                    {
                        new PlatformSegment(2, 3, 2),
                        new PlatformSegment(7, 3, 2),
                        new PlatformSegment(4, 4, 4)
                    }
                )
            };
        }
    }
}
