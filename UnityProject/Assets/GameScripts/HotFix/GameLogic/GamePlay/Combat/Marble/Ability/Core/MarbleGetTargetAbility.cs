using GameLogic.GamePlay.Combat;

namespace GameLogic.Gameplay.Combat.Marble
{
    public class MarbleGetTargetAbility : MarbleAbility, IAbilityFixedUpdate
    {
        public void OnAbilityFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if(Owner == null || Owner.RuntimeData == null || Owner.RuntimeData.IsAlive == false)
                return;

            // TODO: 从战斗管理器中获取最近的敌人
            Marble target = Owner.CombatManager?.GetNearestEnemy(Owner);

            if(target == null)
                return;

            Owner.RuntimeData.TargetMarbleInstId = target.RuntimeData.InstId;
        }

    }
}
