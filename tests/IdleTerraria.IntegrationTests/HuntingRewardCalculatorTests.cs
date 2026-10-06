using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Services;
using Xunit;

namespace IdleTerraria.IntegrationTests;

public sealed class HuntingRewardCalculatorTests
{
    [Fact]
    public void Calculate_should_return_exp_gold_enemy_count_and_loot()
    {
        var item = new ItemTemplate
        {
            Id = 10,
            Code = "SLIME_GEL",
            Name = "Slime Gel",
            MaxStackSize = 999
        };

        var mob = new MobTemplate
        {
            Id = 1,
            Code = "GREEN_SLIME",
            Name = "Green Slime",
            BaseExperience = 15,
            MinimumGold = 4,
            MaximumGold = 4,
            LootDrops =
            [
                new MobLootDrop
                {
                    Id = 1,
                    MobTemplateId = 1,
                    ItemTemplateId = 10,
                    DropChance = 1.0m,
                    MinimumQuantity = 1,
                    MaximumQuantity = 2,
                    ItemTemplate = item
                }
            ]
        };

        var biomeMob = new BiomeMob
        {
            BiomeId = 1,
            MobTemplateId = 1,
            SpawnWeight = 100,
            MobTemplate = mob
        };

        var random = new SequenceRandomSource(
            ints:
            [
                0, 1, // cykl 1: mob (indeks/waga)
                0, 1, // cykl 2: mob (indeks/waga)
                0, 1  // cykl 3: mob (indeks/waga)
            ],
            longs: [4, 4, 4],
            doubles: [0.0, 0.0, 0.0]);

        var calculator = new HuntingRewardCalculator(random);

        var result = calculator.Calculate(
            [biomeMob],
            completedCycles: 3);

        Assert.Equal(45, result.ExperienceGained);
        Assert.Equal(12, result.GoldGained);
        Assert.Equal(3, result.DefeatedEnemies);

        var loot = Assert.Single(result.Loot);

        Assert.Equal(10, loot.ItemTemplateId);
        Assert.Equal("SLIME_GEL", loot.ItemCode);
        Assert.Equal(3, loot.Quantity);
    }

    private sealed class SequenceRandomSource : IRandomSource
    {
        private readonly Queue<int> _ints;
        private readonly Queue<long> _longs;
        private readonly Queue<double> _doubles;

        public SequenceRandomSource(
            IEnumerable<int> ints,
            IEnumerable<long> longs,
            IEnumerable<double> doubles)
        {
            _ints = new Queue<int>(ints);
            _longs = new Queue<long>(longs);
            _doubles = new Queue<double>(doubles);
        }

        public int NextInt(
    int minInclusive,
    int maxExclusive)
        {
            if (_ints.Count == 0)
            {
                throw new InvalidOperationException(
                    "Brak zaplanowanej wartości int w SequenceRandomSource.");
            }

            var value = _ints.Dequeue();

            Assert.InRange(
                value,
                minInclusive,
                maxExclusive - 1);

            return value;
        }

        public long NextLong(
            long minInclusive,
            long maxExclusive)
        {
            if (_longs.Count == 0)
            {
                throw new InvalidOperationException(
                    "Brak zaplanowanej wartości long w SequenceRandomSource.");
            }

            var value = _longs.Dequeue();

            Assert.InRange(
                value,
                minInclusive,
                maxExclusive - 1);

            return value;
        }

        public double NextDouble()
        {
            if (_doubles.Count == 0)
            {
                throw new InvalidOperationException(
                    "Brak zaplanowanej wartości double w SequenceRandomSource.");
            }

            return _doubles.Dequeue();
        }
    }
}