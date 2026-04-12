using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleAddHealAbility : MarbleAbility, IReceiveHeal
    {
        public void ReceiveHeal(int value, ASC source = null)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.GetAbility<MarbleHealPipelineAbility>()?.Execute(value, source);
        }
    }
}
