using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleAddHealAbility : Ability<MarbleRuntimeData>
    {
        public void AddHeal(int value)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.RuntimeData.PendingHeal += value;
            Owner.GetAbility<MarbleHandleDamageAbility>()?.Resolve();
            Owner.GetAbility<MarbleDeadAbility>()?.Resolve();
        }
    }
}
