using UnityEngine;
using GameLogic.GamePlay.Combat;

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
            if (!Owner.RuntimeData.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            var finalDamage = Mathf.Max(context.FinalValue, 0);
            if (finalDamage <= 0)
                return;

            if (runtimeData.Shield > 0)
            {
                runtimeData.Shield = Mathf.Clamp(runtimeData.Shield - finalDamage, 0, runtimeData.MaxShield);
                return;
            }

            runtimeData.Hp = Mathf.Clamp(runtimeData.Hp - finalDamage, 0, runtimeData.MaxHp);
        }

        public void AfterApplyHeal(IAbility ability)
        {
            var context = (ability as MarbleHealPipelineAbility)?.CurrentContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            runtimeData.Hp = Mathf.Clamp(runtimeData.Hp + Mathf.Max(context.FinalValue, 0), 0, runtimeData.MaxHp);
        }

        public void AfterApplyShield(IAbility ability)
        {
            var context = (ability as MarbleShieldHealPipelineAbility)?.CurrentHealContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            runtimeData.Shield = Mathf.Clamp(runtimeData.Shield + Mathf.Max(context.FinalValue, 0), 0, runtimeData.MaxShield);
        }
    }
}
