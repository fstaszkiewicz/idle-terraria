namespace IdleTerraria.Api.DTOs.Responses;

public sealed class HuntingClaimResponse
{
    public int ExperienceGained { get; init; }

    public long GoldGained { get; init; }

    public int DefeatedEnemies { get; init; }

    public IReadOnlyCollection<HuntingLootResponse> Loot { get; init; } =
        Array.Empty<HuntingLootResponse>();

    public HuntingStatusResponse CurrentStatus { get; init; } = null!;
}