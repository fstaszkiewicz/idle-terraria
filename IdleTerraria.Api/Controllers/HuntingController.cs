using System.Security.Claims;
using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleTerraria.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class HuntingController : ControllerBase
{
    private const string PlayerIdClaimType = "player_id";

    private readonly IHuntingService _huntingService;

    public HuntingController(IHuntingService huntingService)
    {
        _huntingService = huntingService;
    }

    [HttpGet("status")]
    [ProducesResponseType<HuntingStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HuntingStatusResponse>> GetStatus(
        CancellationToken cancellationToken)
    {
        var playerId = GetPlayerIdFromToken();

        var status = await _huntingService.GetStatusAsync(
            playerId,
            cancellationToken);

        return Ok(status);
    }

    [HttpPost("start")]
    [ProducesResponseType<HuntingStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HuntingStatusResponse>> StartHunting(
        [FromBody] StartHuntingRequest request,
        CancellationToken cancellationToken)
    {
        var playerId = GetPlayerIdFromToken();

        var status = await _huntingService.StartHuntingAsync(
            playerId,
            request,
            cancellationToken);

        return Ok(status);
    }

    [HttpPost("claim")]
    [ProducesResponseType<HuntingClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HuntingClaimResponse>> ClaimRewards(
        CancellationToken cancellationToken)
    {
        var playerId = GetPlayerIdFromToken();

        var response = await _huntingService.ClaimRewardsAsync(
            playerId,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("stop")]
    [ProducesResponseType<HuntingClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HuntingClaimResponse>> StopHunting(
        CancellationToken cancellationToken)
    {
        var playerId = GetPlayerIdFromToken();

        var response = await _huntingService.StopHuntingAsync(
            playerId,
            cancellationToken);

        return Ok(response);
    }

    private Guid GetPlayerIdFromToken()
    {
        var playerIdClaim = User.FindFirstValue(
            PlayerIdClaimType);

        if (!Guid.TryParse(playerIdClaim, out var playerId))
        {
            throw new UnauthorizedAccessException(
                "Token nie zawiera poprawnego identyfikatora gracza.");
        }

        return playerId;
    }
}