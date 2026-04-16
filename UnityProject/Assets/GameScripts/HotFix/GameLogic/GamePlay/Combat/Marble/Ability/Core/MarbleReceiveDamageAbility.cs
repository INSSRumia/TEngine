using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    /// <summary>
    /// Marble 受伤入口能力。
    /// 该类本身不直接结算伤害，而是把“收到伤害请求”统一转交给 DamagePipeline，
    /// 让所有阶段性修正能力都围绕同一上下文执行。
    /// </summary>
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
