using System.Collections.Generic;

namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public interface IPlatformPlacementAlgorithm
    {
        IReadOnlyList<PlatformSegment> GeneratePlatforms(PlatformPlacementContext context, IRandomSource randomSource);
    }
}
