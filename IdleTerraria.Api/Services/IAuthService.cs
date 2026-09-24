using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;

namespace IdleTerraria.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);

        Task<AuthResponse?> LoginAsync(LoginRequest request);
    }
}