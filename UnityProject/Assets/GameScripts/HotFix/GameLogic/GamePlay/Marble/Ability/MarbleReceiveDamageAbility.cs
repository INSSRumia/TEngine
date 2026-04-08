using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleReceiveDamageAbility : Ability<MarbleRuntimeData>, IReceiveDamage
    {
        public void ReceiveDamage(int value)
        {
            if (Owner == null || Owner.RuntimeData == null || value <= 0)
                return;

            Owner.RuntimeData.PendingDamage += value;
            Owner.GetAbility<MarbleHandleDamageAbility>()?.Resolve();
            Owner.GetAbility<MarbleDeadAbility>()?.Resolve();
        }
    }
}
