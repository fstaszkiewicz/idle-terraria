namespace IdleTerraria.Api.DTOs.Responses
{
    public class PlayerProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Level { get; set; }
        public long Experience { get; set; }
        public long Gold { get; set; }
        public int Stardust { get; set; }
        public int Energy { get; set; }
        public int ArenaElo { get; set; }


        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Luck { get; set; }
    }
}