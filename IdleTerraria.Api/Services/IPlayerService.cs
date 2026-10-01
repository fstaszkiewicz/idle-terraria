using System;
using System.Threading.Tasks;
using IdleTerraria.Api.DTOs.Responses;

namespace IdleTerraria.Api.Services
{
    public interface IPlayerService
    {
        Task<PlayerProfileResponse?> GetPlayerProfileAsync(Guid playerId);
    }
}