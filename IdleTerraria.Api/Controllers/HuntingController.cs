using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Services;
using System.Security.Claims;

namespace IdleTerraria.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HuntingController : ControllerBase
    {
        private readonly IHuntingService _huntingService;
        private readonly ILogger<HuntingController> _logger;

        public HuntingController(IHuntingService huntingService, ILogger<HuntingController> logger)
        {
            _huntingService = huntingService;
            _logger = logger;
        }

        private Guid GetPlayerIdFromToken()
        {
            var playerIdClaim = User.FindFirst("PlayerId")?.Value;

            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out Guid playerId))
            {
                throw new UnauthorizedAccessException("Brak poprawnego ID Gracza w tokenie.");
            }
            return playerId;
        }

        [HttpGet("status")]
        public async Task<ActionResult<HuntingStatusResponse>> GetStatus()
        {
            try
            {
                var playerId = GetPlayerIdFromToken();
                var status = await _huntingService.GetStatusAsync(playerId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd pobierania statusu polowania.");
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }

        [HttpPost("start")]
        public async Task<ActionResult<HuntingStatusResponse>> StartHunting([FromBody] StartHuntingRequest request)
        {
            try
            {
                var playerId = GetPlayerIdFromToken();
                var status = await _huntingService.StartHuntingAsync(playerId, request);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas rozpoczynania polowania.");
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }

        [HttpPost("claim")]
        public async Task<ActionResult<HuntingClaimResponse>> ClaimRewards()
        {
            try
            {
                var playerId = GetPlayerIdFromToken();
                var response = await _huntingService.ClaimRewardsAsync(playerId);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas odbierania nagród.");
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }

        [HttpPost("stop")]
        public async Task<ActionResult<HuntingClaimResponse>> StopHunting()
        {
            try
            {
                var playerId = GetPlayerIdFromToken();
                var response = await _huntingService.StopHuntingAsync(playerId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zatrzymywania polowania.");
                return StatusCode(500, "Wewnętrzny błąd serwera.");
            }
        }
    }
}