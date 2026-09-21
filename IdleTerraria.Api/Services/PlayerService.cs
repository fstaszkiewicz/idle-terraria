using IdleTerraria.Api.Data;
using IdleTerraria.Api.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlayerProfileResponse?> GetPlayerProfileAsync(Guid playerId)
        {
            var player = await _dbContext.Players
                .Include(p => p.Stats)
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null) return null;

            // Mapowanie Entity na DTO
            return new PlayerProfileResponse
            {
                Id = player.Id,
                Username = player.Username,
                Level = player.Level,
                Experience = player.Experience,
                Gold = player.Gold,
                Stardust = player.Stardust,
                Energy = player.Energy,
                ArenaElo = player.ArenaElo,

                // Zabezpieczenie gdyby gracz nie miał jeszcze rekordu statystyk
                Strength = player.Stats?.StatStrength ?? 0,
                Dexterity = player.Stats?.StatDexterity ?? 0,
                Luck = player.Stats?.StatLuck ?? 0
            };
        }
    }
}