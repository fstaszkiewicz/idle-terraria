using IdleTerraria.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdleTerraria.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        // GET: /api/player/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var profile = await _playerService.GetPlayerProfileAsync(id);

            if (profile == null)
            {
                return NotFound(new { Message = "Gracz nie został znaleziony." });
            }

            return Ok(profile); // Zwraca HTTP 200 OK wraz z JSONem
        }
    }
}