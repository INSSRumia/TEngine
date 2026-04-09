using UnityEngine;
using GameLogic.GamePlay.Common;

namespace GameLogic.Marble
{
    public class MarbleHandleDamageAbility : Ability<MarbleRuntimeData>,
        IAfterApplyDamage,
        IAfterApplyHeal,
        IAfterApplyShield
    {
        public override int Priority => 9900;

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
            var context = (ability as MarbleShieldPipelineAbility)?.CurrentContext;
            if (Owner == null || Owner.RuntimeData == null || context == null)
                return;
            if (!Owner.RuntimeData.IsAlive)
                return;

            var runtimeData = Owner.RuntimeData;
            runtimeData.Shield = Mathf.Clamp(runtimeData.Shield + Mathf.Max(context.FinalValue, 0), 0, runtimeData.MaxShield);
        }
    }
}
