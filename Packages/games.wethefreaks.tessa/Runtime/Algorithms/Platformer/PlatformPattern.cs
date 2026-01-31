using System.Collections.Generic;
using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public sealed class PlatformPattern
    {
        public string Name { get; }
        public Vector2Int Size { get; }
        public IReadOnlyList<PlatformSegment> RelativeSegments { get; }

        public PlatformPattern(string name, Vector2Int size, IReadOnlyList<PlatformSegment> relativeSegments)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Pattern" : name;
            Size = size;
            RelativeSegments = relativeSegments ?? new List<PlatformSegment>();
        }

        public IEnumerable<PlatformSegment> PlaceAt(Vector2Int origin)
        {
            foreach (var segment in RelativeSegments)
            {
                yield return new PlatformSegment(origin.x + segment.StartX, segment.Length, origin.y + segment.Y);
            }
        }
    }
}
