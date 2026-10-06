using IdleTerraria.Api.Entities;

namespace IdleTerraria.Api.Services;

public sealed class HuntingRewardCalculator : IHuntingRewardCalculator
{
    private const string DefaultLootPrefix = "Normal";

    private readonly IRandomSource _randomSource;

    public HuntingRewardCalculator(IRandomSource randomSource)
    {
        _randomSource = randomSource;
    }

    public HuntingRewardCalculationResult Calculate(
        IReadOnlyCollection<BiomeMob> mobPool,
        long completedCycles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mobPool);

        if (completedCycles < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedCycles),
                "Liczba cykli nie może być ujemna.");
        }

        if (completedCycles == 0)
        {
            return new HuntingRewardCalculationResult();
        }

        if (mobPool.Count == 0)
        {
            throw new InvalidOperationException(
                "Wybrany biom nie posiada puli przeciwników.");
        }

        ValidateMobPool(mobPool);

        var experience = 0;
        var gold = 0L;
        var defeatedEnemies = 0;

        var loot = new Dictionary<int, HuntingLootAccumulator>();

        for (var cycle = 0L; cycle < completedCycles; cycle++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var biomeMob = SelectMob(mobPool);

            experience = checked(
                experience +
                biomeMob.MobTemplate.BaseExperience);

            gold = checked(
                gold +
                CalculateGold(biomeMob.MobTemplate));

            defeatedEnemies = checked(defeatedEnemies + 1);

            CalculateLoot(
                biomeMob.MobTemplate,
                loot);
        }

        return new HuntingRewardCalculationResult
        {
            ExperienceGained = experience,
            GoldGained = gold,
            DefeatedEnemies = defeatedEnemies,
            Loot = loot.Values
                .Select(item => new HuntingLootReward
                {
                    ItemTemplateId = item.ItemTemplateId,
                    ItemCode = item.ItemCode,
                    ItemName = item.ItemName,
                    Quantity = item.Quantity,
                    MaxStackSize = item.MaxStackSize
                })
                .ToArray()
        };
    }

    private BiomeMob SelectMob(
        IReadOnlyCollection<BiomeMob> mobPool)
    {
        var totalWeight = 0;

        foreach (var biomeMob in mobPool)
        {
            totalWeight = checked(
                totalWeight +
                biomeMob.SpawnWeight);
        }

        var roll = _randomSource.NextInt(
            0,
            totalWeight);

        var cumulativeWeight = 0;

        foreach (var biomeMob in mobPool)
        {
            cumulativeWeight += biomeMob.SpawnWeight;

            if (roll < cumulativeWeight)
            {
                return biomeMob;
            }
        }

        throw new InvalidOperationException(
            "Nie udało się wybrać przeciwnika z puli biomu.");
    }

    private long CalculateGold(MobTemplate mob)
    {
        if (mob.MinimumGold < 0 ||
            mob.MaximumGold < mob.MinimumGold)
        {
            throw new InvalidOperationException(
                $"Nieprawidłowy zakres złota dla moba {mob.Code}.");
        }

        if (mob.MinimumGold == mob.MaximumGold)
        {
            return mob.MinimumGold;
        }

        return _randomSource.NextLong(
            mob.MinimumGold,
            checked(mob.MaximumGold + 1));
    }

    private void CalculateLoot(
        MobTemplate mob,
        Dictionary<int, HuntingLootAccumulator> loot)
    {
        foreach (var drop in mob.LootDrops)
        {
            if (drop.DropChance < 0 ||
                drop.DropChance > 1)
            {
                throw new InvalidOperationException(
                    $"Nieprawidłowa szansa dropu dla " +
                    $"{mob.Code} -> {drop.ItemTemplate.Code}.");
            }

            if (drop.MinimumQuantity <= 0 ||
                drop.MaximumQuantity < drop.MinimumQuantity)
            {
                throw new InvalidOperationException(
                    $"Nieprawidłowa ilość dropu dla " +
                    $"{mob.Code} -> {drop.ItemTemplate.Code}.");
            }

            if (_randomSource.NextDouble() >=
                (double)drop.DropChance)
            {
                continue;
            }

            var quantity = drop.MinimumQuantity;

            if (drop.MinimumQuantity != drop.MaximumQuantity)
            {
                quantity = _randomSource.NextInt(
                    drop.MinimumQuantity,
                    checked(drop.MaximumQuantity + 1));
            }

            var item = drop.ItemTemplate;

            if (item.MaxStackSize <= 0)
            {
                throw new InvalidOperationException(
                    $"Przedmiot {item.Code} ma nieprawidłowy " +
                    $"MaxStackSize.");
            }

            if (loot.TryGetValue(
                    item.Id,
                    out var existing))
            {
                existing.Quantity = checked(
                    existing.Quantity + quantity);
            }
            else
            {
                loot.Add(
                    item.Id,
                    new HuntingLootAccumulator
                    {
                        ItemTemplateId = item.Id,
                        ItemCode = item.Code,
                        ItemName = item.Name,
                        Quantity = quantity,
                        MaxStackSize = item.MaxStackSize
                    });
            }
        }
    }

    private static void ValidateMobPool(
        IReadOnlyCollection<BiomeMob> mobPool)
    {
        foreach (var biomeMob in mobPool)
        {
            if (biomeMob.SpawnWeight <= 0)
            {
                throw new InvalidOperationException(
                    "Waga występowania moba musi być większa od zera.");
            }

            if (biomeMob.MobTemplate is null)
            {
                throw new InvalidOperationException(
                    "Pula biomu zawiera moba bez definicji.");
            }

            if (biomeMob.MobTemplate.BaseExperience < 0)
            {
                throw new InvalidOperationException(
                    "EXP moba nie może być ujemny.");
            }

            if (biomeMob.MobTemplate.LootDrops is null)
            {
                throw new InvalidOperationException(
                    "Mob nie posiada załadowanej tabeli dropów.");
            }
        }
    }

    private sealed class HuntingLootAccumulator
    {
        public int ItemTemplateId { get; init; }

        public string ItemCode { get; init; } = string.Empty;

        public string ItemName { get; init; } = string.Empty;

        public int Quantity { get; set; }

        public int MaxStackSize { get; init; }
    }
}