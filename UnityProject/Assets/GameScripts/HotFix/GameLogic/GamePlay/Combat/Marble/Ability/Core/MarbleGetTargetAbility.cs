using GameLogic.Gameplay.Combat;
using GameConfig.Gameplay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleGetTargetAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public MarbleGetTargetAbility(MarbleGetTargetAbilityConfig config)
        {
            Priority = config.Priority;
        }

        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.State.IsAlive == false)
                return;

            // TODO: 从战斗管理器中获取最近的敌人
            Marble target = Owner.CombatManager?.GetNearestEnemy(Owner);

            if(target == null)
                return;

            Owner.RuntimeData.State.TargetMarbleInstId = target.RuntimeData.InstId;
        }

    }
}
