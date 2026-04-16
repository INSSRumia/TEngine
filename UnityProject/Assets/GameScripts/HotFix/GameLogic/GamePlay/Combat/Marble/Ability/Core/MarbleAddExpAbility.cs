using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleAddExpAbility : MarbleAbility
    {
        public MarbleAddExpAbility(MarbleAddExpAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void AddExp(int value)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.RuntimeData.State.Exp += value;
            Owner.GetAbility<MarbleLevelUpAbility>()?.Resolve();
        }
    }
}
