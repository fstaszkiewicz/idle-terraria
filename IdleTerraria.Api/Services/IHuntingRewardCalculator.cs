using IdleTerraria.Api.Entities;

namespace IdleTerraria.Api.Services;

public interface IHuntingRewardCalculator
{
    HuntingRewardCalculationResult Calculate(
        IReadOnlyCollection<BiomeMob> mobPool,
        long completedCycles,
        CancellationToken cancellationToken = default);
}