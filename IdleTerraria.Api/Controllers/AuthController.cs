using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdleTerraria.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return Conflict(new
                {
                    Message = "Email lub username jest już zajęty."
                });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new
                {
                    Message = "Nieprawidłowy email lub hasło."
                });
            }

            return Ok(result);
        }
    }
}