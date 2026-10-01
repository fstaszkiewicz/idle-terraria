using IdleTerraria.Api.Entities;
using IdleTerraria.Api.Options;
using Microsoft.Extensions.Options;
using MediatR;
using IdleTerraria.Api.Events;

namespace IdleTerraria.Api.Services
{
    public class PlayerProgressionService : IPlayerProgressionService
    {
        private readonly GameRulesOptions _gameRules;
        private readonly IPublisher _publisher;

        public PlayerProgressionService(
            IOptions<GameRulesOptions> gameRulesOptions,
            IPublisher publisher)
        {
            _gameRules = gameRulesOptions.Value;
            _publisher = publisher;
        }

        public async Task AddExperienceAsync(Player player, int expAmount, CancellationToken cancellationToken = default)
        {
            if (expAmount <= 0) return;

            player.Experience += expAmount;

            long requiredExp = CalculateRequiredExpForLevel(player.Level);

            while (player.Experience >= requiredExp)
            {
                player.Experience -= requiredExp;
                player.Level++;

                requiredExp = CalculateRequiredExpForLevel(player.Level);

                await _publisher.Publish(new PlayerLeveledUpEvent(player, player.Level), cancellationToken);
            }
        }

        public int CalculateRequiredExpForLevel(int level)
        {
            return (int)(_gameRules.BaseExpRequirementForLevelUp * Math.Pow(_gameRules.ExpRequirementMultiplier, level - 1));
        }
    }
}