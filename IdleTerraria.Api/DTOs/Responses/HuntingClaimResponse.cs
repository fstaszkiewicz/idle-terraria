namespace IdleTerraria.Api.DTOs.Responses
{
    public class HuntingClaimResponse
    {
        public int ExperienceGained { get; set; }

        public HuntingStatusResponse CurrentStatus { get; set; } = null!;
    }
}