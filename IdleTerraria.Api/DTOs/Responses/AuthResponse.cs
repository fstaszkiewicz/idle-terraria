namespace IdleTerraria.Api.DTOs.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public Guid AccountId { get; set; }

        public Guid PlayerId { get; set; }

        public string Username { get; set; } = string.Empty;
    }
}