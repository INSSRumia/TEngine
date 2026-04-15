using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public static class AbilityTimingFactory
    {
        public static IAbilityTiming CreateTiming(AbilityTimingConfig config)
        {
            return config switch
            {
                FixedAbilityTimingConfig fixedConfig =>
                    new FixedDurationAbilityTiming(fixedConfig.Duration, fixedConfig.Cooldown, fixedConfig.AutoActivate),
                RandomRangeAbilityTimingConfig randomConfig =>
                    new RandomRangeAbilityTiming(
                        randomConfig.MinDuration,
                        randomConfig.MaxDuration,
                        randomConfig.MinCooldown,
                        randomConfig.MaxCooldown,
                        randomConfig.AutoActivate),
                null => null,
                _ => null,
            };
        }
    }
}
