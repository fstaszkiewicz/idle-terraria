using MediatR;
using IdleTerraria.Api.Events;
using IdleTerraria.Api.Data;
using Microsoft.Extensions.Logging;

namespace IdleTerraria.Api.Handlers
{
    public class GrantSkillPointsOnLevelUpHandler : INotificationHandler<PlayerLeveledUpEvent>
    {
        private readonly ILogger<GrantSkillPointsOnLevelUpHandler> _logger;
        private readonly ApplicationDbContext _dbContext;

        public GrantSkillPointsOnLevelUpHandler(
            ILogger<GrantSkillPointsOnLevelUpHandler> logger,
            ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Handle(PlayerLeveledUpEvent notification, CancellationToken cancellationToken)
        {
            notification.Player.SkillPoints += 1;

            _logger.LogInformation(
                "Gracz {PlayerId} osiągnął {NewLevel} poziom i otrzymał 1 punkt umiejętności. (Obecnie: {SkillPoints})",
                notification.Player.Id,
                notification.NewLevel,
                notification.Player.SkillPoints);
            await Task.CompletedTask;
        }
    }
}