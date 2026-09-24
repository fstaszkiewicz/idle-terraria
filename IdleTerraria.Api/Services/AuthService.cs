using IdleTerraria.Api.Data;
using IdleTerraria.Api.DTOs.Requests;
using IdleTerraria.Api.DTOs.Responses;
using IdleTerraria.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdleTerraria.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            ApplicationDbContext dbContext,
            IPasswordHasher<Account> passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse?> RegisterAsync(
            RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var username = request.Username.Trim();

            var emailExists = await _dbContext.Accounts
                .AnyAsync(a => a.Email == email);

            if (emailExists)
            {
                return null;
            }

            var usernameExists = await _dbContext.Players
                .AnyAsync(p => p.Username.ToLower() == username.ToLower());

            if (usernameExists)
            {
                return null;
            }

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var account = new Account
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                account.PasswordHash = _passwordHasher.HashPassword(
                    account,
                    request.Password);

                var player = new Player
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    Account = account,
                    Username = username,
                    Level = 1,
                    Experience = 0,
                    Gold = 0,
                    Stardust = 0,
                    Energy = 100,
                    ArenaElo = 1200,
                    CurrentBiomeId = null,
                    StatsBoughtN = 0
                };

                var playerStats = new PlayerStats
                {
                    PlayerId = player.Id,
                    Player = player,
                    StatStrength = 0,
                    StatDexterity = 0,
                    StatLuck = 0,
                    StatVitality = 0
                };

                _dbContext.Accounts.Add(account);
                _dbContext.Players.Add(player);
                _dbContext.PlayerStats.Add(playerStats);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return _jwtTokenService.CreateToken(account, player);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AuthResponse?> LoginAsync(
            LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var account = await _dbContext.Accounts
                .Include(a => a.Player)
                .FirstOrDefaultAsync(a => a.Email == email);

            if (account == null || account.Player == null)
            {
                return null;
            }

            var verificationResult =
                _passwordHasher.VerifyHashedPassword(
                    account,
                    account.PasswordHash,
                    request.Password);

            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return null;
            }

            return _jwtTokenService.CreateToken(
                account,
                account.Player);
        }
    }
}