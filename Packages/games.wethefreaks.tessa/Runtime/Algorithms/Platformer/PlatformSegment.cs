using UnityEngine;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public readonly struct PlatformSegment
    {
        public int StartX { get; }
        public int Length { get; }
        public int Y { get; }

        public PlatformSegment(int startX, int length, int y)
        {
            StartX = startX;
            Length = length;
            Y = y;
        }

        public int EndX => StartX + Length - 1;

        public bool IsValid => Length > 0;
    }
}
