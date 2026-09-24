using System.IdentityModel.Tokens.Jwt;
using IdleTerraria.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdleTerraria.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentProfile()
        {
            var playerIdClaim = User.FindFirst("player_id")?.Value;

            if (!Guid.TryParse(playerIdClaim, out var playerId))
            {
                return Unauthorized(new
                {
                    Message = "Token nie zawiera poprawnego identyfikatora gracza."
                });
            }

            var profile = await _playerService
                .GetPlayerProfileAsync(playerId);

            if (profile == null)
            {
                return NotFound(new
                {
                    Message = "Profil gracza nie został znaleziony."
                });
            }

            return Ok(profile);
        }
    }
}