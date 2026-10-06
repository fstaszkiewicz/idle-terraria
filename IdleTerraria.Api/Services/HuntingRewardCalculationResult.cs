namespace IdleTerraria.Api.Services;

public sealed class HuntingRewardCalculationResult
{
    public int ExperienceGained { get; init; }

    public long GoldGained { get; init; }

    public int DefeatedEnemies { get; init; }

    public IReadOnlyCollection<HuntingLootReward> Loot { get; init; } =
        Array.Empty<HuntingLootReward>();
}

public sealed class HuntingLootReward
{
    public int ItemTemplateId { get; init; }

    public string ItemCode { get; init; } = string.Empty;

    public string ItemName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public int MaxStackSize { get; init; }
}