using IdleTerraria.Api.Entities;

namespace IdleTerraria.Api.Services;

public interface IPlayerProgressionService
{
    Task AddExperienceAsync(
        Player player,
        int expAmount,
        CancellationToken cancellationToken = default);

    int CalculateRequiredExpForLevel(int level);
}