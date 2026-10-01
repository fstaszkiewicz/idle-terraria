using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;

namespace IdleTerraria.Api.Services
{
    public interface IHuntingService
    {
        Task<HuntingStatusResponse> GetStatusAsync(System.Guid playerId);

        Task<HuntingStatusResponse> StartHuntingAsync(System.Guid playerId, StartHuntingRequest request);

        Task<HuntingClaimResponse> ClaimRewardsAsync(System.Guid playerId);

        Task<HuntingClaimResponse> StopHuntingAsync(System.Guid playerId);
    }
}