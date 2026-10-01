using IdleTerraria.Api.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace IdleTerraria.Api.Services
{
    public interface IPlayerProgressionService
    {
        Task AddExperienceAsync(Player player, int expAmount, CancellationToken cancellationToken = default);
        int CalculateRequiredExpForLevel(int level);
    }
}
