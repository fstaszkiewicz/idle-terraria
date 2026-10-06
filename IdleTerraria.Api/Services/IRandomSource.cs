namespace IdleTerraria.Api.Services;

public interface IRandomSource
{
    int NextInt(int minInclusive, int maxExclusive);

    long NextLong(long minInclusive, long maxExclusive);

    double NextDouble();
}