namespace IdleTerraria.Api.Options
{
    public class GameRulesOptions
    {
        public const string SectionName = "GameRules";

        public int MaxIdleHours { get; set; }
        public int BaseHuntingCycleSeconds { get; set; }
        public int BaseExpPerCycle { get; set; }
        public int BaseExpRequirementForLevelUp { get; set; }
        public double ExpRequirementMultiplier { get; set; }
    }
}