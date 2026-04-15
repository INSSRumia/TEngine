using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleReceiveDamageAbility : MarbleAbility, IReceiveDamage
    {
        public MarbleReceiveDamageAbility(MarbleReceiveDamageAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void ReceiveDamage(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.GetAbility<MarbleDamagePipelineAbility>()?.Execute(value, source);
        }
    }
}
