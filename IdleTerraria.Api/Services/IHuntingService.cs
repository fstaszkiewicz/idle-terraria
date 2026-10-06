using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;

namespace IdleTerraria.Api.Services;

public interface IHuntingService
{
    Task<HuntingStatusResponse> GetStatusAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);

    Task<HuntingStatusResponse> StartHuntingAsync(
        Guid playerId,
        StartHuntingRequest request,
        CancellationToken cancellationToken = default);

    Task<HuntingClaimResponse> ClaimRewardsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);

    Task<HuntingClaimResponse> StopHuntingAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);
}