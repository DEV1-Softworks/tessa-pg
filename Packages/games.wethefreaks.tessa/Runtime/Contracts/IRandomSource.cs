namespace Games.Wethefreaks.Tessa.Runtime.Algorithms.Platformer
{
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }
}
