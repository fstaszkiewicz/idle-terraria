using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;

namespace IdleTerraria.Api.Services
{
    public interface IJwtTokenService
    {
        AuthResponse CreateToken(Account account, Player player);
    }
}