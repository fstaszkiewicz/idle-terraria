namespace IdleTerraria.Api.Services;

public sealed class GameRandomSource : IRandomSource
{
    public int NextInt(
        int minInclusive,
        int maxExclusive)
    {
        return Random.Shared.Next(
            minInclusive,
            maxExclusive);
    }

    public long NextLong(
        long minInclusive,
        long maxExclusive)
    {
        return Random.Shared.NextInt64(
            minInclusive,
            maxExclusive);
    }

    public double NextDouble()
    {
        return Random.Shared.NextDouble();
    }
}