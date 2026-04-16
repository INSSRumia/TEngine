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
                    new FixedDurationAbilityTiming(fixedConfig),
                RandomRangeAbilityTimingConfig randomConfig =>
                    new RandomRangeAbilityTiming(randomConfig),
                null => null,
                _ => null,
            };
        }
    }
}
