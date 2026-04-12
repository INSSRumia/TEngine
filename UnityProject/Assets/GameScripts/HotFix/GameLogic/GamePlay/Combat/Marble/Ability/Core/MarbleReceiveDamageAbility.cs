using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleReceiveDamageAbility : MarbleAbility, IReceiveDamage
    {
        public void ReceiveDamage(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.GetAbility<MarbleDamagePipelineAbility>()?.Execute(value, source);
        }
    }
}
