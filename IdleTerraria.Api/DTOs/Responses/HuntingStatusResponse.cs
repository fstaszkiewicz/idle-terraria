namespace IdleTerraria.Api.DTOs.Responses
{
    public class HuntingStatusResponse
    {
        public bool IsHunting { get; set; }
        public int? ZoneId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? LastClaimedAt { get; set; }

    
        public double SecondsSinceLastClaim { get; set; }
    }
}