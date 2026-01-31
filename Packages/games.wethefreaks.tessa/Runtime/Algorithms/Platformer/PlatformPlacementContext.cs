using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public readonly struct PlatformPlacementContext
    {
        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }

        public Vector2Int EntryPoint { get; }
        public Vector2Int ExitPoint { get; }
        public bool HasEntryExit { get; }

        public PlatformPlacementContext(int minX, int maxX, int minY, int maxY)
            : this(minX, maxX, minY, maxY, Vector2Int.zero, Vector2Int.zero, false)
        {
        }

        public PlatformPlacementContext(int minX, int maxX, int minY, int maxY, Vector2Int entryPoint, Vector2Int exitPoint)
            : this(minX, maxX, minY, maxY, entryPoint, exitPoint, true)
        {
        }

        private PlatformPlacementContext(int minX, int maxX, int minY, int maxY, Vector2Int entryPoint, Vector2Int exitPoint, bool hasEntryExit)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            EntryPoint = entryPoint;
            ExitPoint = exitPoint;
            HasEntryExit = hasEntryExit;
        }

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;

        public bool IsValid => Width > 0 && Height > 0;

        public PlatformPlacementContext ClampToBounds(int minX, int maxX, int minY, int maxY)
        {
            return new PlatformPlacementContext(
                Mathf.Max(MinX, minX),
                Mathf.Min(MaxX, maxX),
                Mathf.Max(MinY, minY),
                Mathf.Min(MaxY, maxY),
                EntryPoint,
                ExitPoint,
                HasEntryExit
            );
        }

        public static PlatformPlacementContext FromRoom(Vector2Int roomOrigin, Vector2Int roomSize, int horizontalPadding, int verticalPadding)
        {
            int minX = roomOrigin.x + 1 + Mathf.Max(0, horizontalPadding);
            int maxX = roomOrigin.x + roomSize.x - 2 - Mathf.Max(0, horizontalPadding);
            int minY = roomOrigin.y + 1 + Mathf.Max(0, verticalPadding);
            int maxY = roomOrigin.y + roomSize.y - 2 - Mathf.Max(0, verticalPadding);
            return new PlatformPlacementContext(minX, maxX, minY, maxY);
        }
    }
}
