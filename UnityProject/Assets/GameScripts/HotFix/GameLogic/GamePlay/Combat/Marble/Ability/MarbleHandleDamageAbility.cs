using UnityEngine;
using GameLogic.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    [System.Obsolete]
    public class MarbleHandleDamageAbility : MarbleAbility,
        IAfterApplyDamage,
        IAfterApplyHeal,
        IAfterApplyShield
    {
        public void AfterApplyDamage(IAbility ability)
        {
            var context = (ability as MarbleDamagePipelineAbility)?.CurrentContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.State.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            var finalDamage = Mathf.Max(context.FinalValue, 0);
            if (finalDamage <= 0)
                return;

            if (runtimeData.State.Shield > 0)
            {
                runtimeData.State.Shield = Mathf.Clamp(runtimeData.State.Shield - finalDamage, 0, runtimeData.State.MaxShield);
                return;
            }

            runtimeData.State.Hp = Mathf.Clamp(runtimeData.State.Hp - finalDamage, 0, runtimeData.State.MaxHp);
        }

        public void AfterApplyHeal(IAbility ability)
        {
            var context = (ability as MarbleHealPipelineAbility)?.CurrentContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.State.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            runtimeData.State.Hp = Mathf.Clamp(runtimeData.State.Hp + Mathf.Max(context.FinalValue, 0), 0, runtimeData.State.MaxHp);
        }

        public void AfterApplyShield(IAbility ability)
        {
            var context = (ability as MarbleShieldHealPipelineAbility)?.CurrentHealContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.State.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            runtimeData.State.Shield = Mathf.Clamp(runtimeData.State.Shield + Mathf.Max(context.FinalValue, 0), 0, runtimeData.State.MaxShield);
        }
    }
}
